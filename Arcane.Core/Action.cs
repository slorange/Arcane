using Arcane.Core.Cards;
using Arcane.Core.Events;
using System.Collections;
using System.Runtime.InteropServices.Marshalling;
using static System.Net.Mime.MediaTypeNames;

namespace Arcane.Core;

public abstract class PlayerAction
{
	public string Name { get; }

	protected PlayerAction(string name)
	{
		Name = name;
	}

	public abstract bool CanExecute(State state, Player player);

	public abstract void Execute(State state, Player player, List<GameEvent> events, string parameters);
}

public class SpellCastAction : PlayerAction
{
	private readonly Spell _spell;

	public SpellCastAction(Spell spell) : base($"Cast {spell.Name}")
	{
		_spell = spell;
	}

	public override bool CanExecute(State state, Player player)
	{
		return (state.CurrentPhase == Phase.Battle || _spell.Target == TargetType.Self)
				&& player.Resources.HasMana(_spell.ManaCost)
				&& !(_spell.OncePerBattle && _spell.UsedThisBattle);
	}

	public override void Execute(State state, Player player, List<GameEvent> events, string parameters)
	{
		if (!player.Resources.SpendMana(_spell.ManaCost))
		{
			events.Add(new ErrorOccurred("Not enough mana."));
			return;
		}

		if (_spell.OncePerBattle) _spell.UsedThisBattle = true;

		var monsters = state.Monsters.Where(m => m.IsAlive).ToList();

		if (!monsters.Any())
		{
			events.Add(new ErrorOccurred("No valid targets."));
			return;
		}

		if (_spell.Target == TargetType.Enemy)
		{
			Monster target = null;
			if (!string.IsNullOrEmpty(parameters))
				target = monsters.FirstOrDefault(m => m.Name.Equals(parameters, StringComparison.OrdinalIgnoreCase));
			target??= monsters.First();
			monsters.Clear();
			monsters.Add(target);
		}

		int baseDamage = 0;
		switch (_spell.Target)
		{
			case TargetType.Enemy:
				baseDamage = CastSingleTarget(monsters.First(), player, events);
				break;

			case TargetType.Cleave:
				baseDamage = CastCleave(monsters, player, events);
				break;

			case TargetType.AllEnemies:
				baseDamage = CastAOE(monsters, player, events);
				break;
		}
		ApplyAfterEffects(player, monsters, baseDamage, events);

		player.TickEffects(events);
	}

	private int CastSingleTarget(Monster target, Player player, List<GameEvent> events)
	{
		int damage = ResolveDamage(player, target, events);

		target.TakeDamage(damage);

		events.Add(new MonsterTookDamage(target.Name, damage, target.Health));

		if (!target.IsAlive) events.Add(new MonsterDefeated(target.Name));

		return damage;
	}

	private int CastCleave(List<Monster> monsters, Player player, List<GameEvent> events)
	{
		var primary = monsters.First();

		int damage = ResolveDamage(player, primary, events);

		primary.TakeDamage(damage);

		events.Add(new MonsterTookDamage(primary.Name, damage, primary.Health));

		if (!primary.IsAlive) events.Add(new MonsterDefeated(primary.Name));

		foreach (var monster in monsters.Skip(1))
		{
			int splash = _spell.SplashDamage.Resolve(damage);

			monster.TakeDamage(splash);

			events.Add(new MonsterTookDamage(monster.Name, splash, monster.Health));

			if (!monster.IsAlive) events.Add(new MonsterDefeated(monster.Name));
		}

		return damage;
	}

	private int CastAOE(List<Monster> monsters, Player player, List<GameEvent> events)
	{
		int totalDamage = 0;
		foreach (var monster in monsters)
		{
			int damage = ResolveDamage(player, monster, events);

			monster.TakeDamage(damage);

			events.Add(new MonsterTookDamage(monster.Name, damage, monster.Health));

			if (!monster.IsAlive)
				events.Add(new MonsterDefeated(monster.Name));

			totalDamage += damage;
		}

		return totalDamage;
	}

	private int ResolveDamage(Player player, Monster monster, List<GameEvent> events)
	{
		var value = _spell.Damage;

		// Percent or Flat shouldn't happen here
		if (value.Type == ValueKind.Percent) return 0;
		if (value.Type == ValueKind.Flat) return value.Flat;

		// For Dice:
		if (monster.IsImmune(_spell))
		{
			events.Add(new GameEventMessage($"{monster.Name} is immune to {_spell.School}!"));
			return 0;
		}

		var reasons = "";
		var modifier = monster.ComputeModifier(_spell, ref reasons);
		modifier += player.ComputeModifier(_spell, ref reasons);

		var dice = value.Dice.Modify(modifier, events, reasons);

		return dice.Roll(events, _spell.Name);
	}

	private void ApplyAfterEffects(Player player, List<Monster> monsters, int damageDealt, List<GameEvent> events)
	{
		if (_spell.Lifesteal.Type != ValueKind.Flat || _spell.Lifesteal.Flat != 0)
		{
			int heal = _spell.Lifesteal.Resolve(damageDealt);

			if (heal > 0)
			{
				player.Heal(heal);
				events.Add(new GameEventMessage($"{player.Name} steals {heal} health."));
			}
		}

		if (_spell.Heal.Type != ValueKind.Flat || _spell.Heal.Flat != 0)
		{
			int heal = _spell.Heal.Resolve(events, $"{_spell.Name}");

			player.Heal(heal);
			events.Add(new GameEventMessage($"{player.Name} heals {heal} HP."));
		}

		if (_spell.Shield.Type != ValueKind.Flat || _spell.Shield.Flat != 0)
		{
			int shield = _spell.Shield.Resolve(events, $"{_spell.Name}");

			player.AddShield(shield);
			events.Add(new GameEventMessage($"{player.Name} gains {shield} shield."));
		}

		if (_spell.ManaGain.Type != ValueKind.Flat || _spell.ManaGain.Flat != 0)
		{
			int mana = _spell.ManaGain.Resolve(events, $"{_spell.Name}");

			player.Resources.AddMana(mana);
			events.Add(new PlayerGainedMana(player.Name, mana));
		}

		var effect = _spell.StatusEffect;

		if (effect.Type != StatusEffectType.None)
		{
			foreach (var m in monsters)
			{
				m.GiveEffect(effect, events);
			}
		}

		if (_spell.PlayerEffect != null)
		{
			var peffect = _spell.PlayerEffect.Clone();
			player.Effects.Add(peffect);
			//events.Add(new GameEventMessage($"{player.Name} gains a magical buff."));
		}
	}
}

public class BuyCardAction : PlayerAction
{
	private readonly Card _card;

	public BuyCardAction(Card card) : base($"Buy {card.Name}")
	{
		_card = card;
	}

	public override bool CanExecute(State state, Player player)
	{
		return state.CurrentPhase == Phase.Prep
			   && player.Resources.Knowledge >= _card.KnowledgeCost;
	}

	public override void Execute(State state, Player player, List<GameEvent> events, string parameters)
	{
		if (!player.Resources.SpendKnowledge(_card.KnowledgeCost))
		{
			events.Add(new ErrorOccurred("Not enough knowledge."));
			return;
		}

		state.Market.Purchase(_card);

		if (_card is Spell spell)
		{
			player.Spells.Add(spell);
			events.Add(new SpellPurchased(player.Name, spell.Name));
		}
		else if (_card is Passive passive)
		{
			passive.Apply(player);
			events.Add(new GameEventMessage($"{player.Name} gains {passive.Name}."));
		}
	}
}

public class ChannelAction : PlayerAction
{
	private Dice ManaGain = new Dice("1d6");

	public ChannelAction() : base("Channel") { }

	public override bool CanExecute(State state, Player player)
	{
		return state.CurrentPhase == Phase.Battle;
	}

	public override void Execute(State state, Player player, List<GameEvent> events, string parameters)
	{
		var mana = ManaGain.Roll(events, "Channel");
		player.Resources.AddMana(mana);
		events.Add(new PlayerGainedMana(player.Name, mana));
	}
}

public class TrainAction : PlayerAction
{
	public TrainAction(int AdvancedTraining) : base($"Train lv{AdvancedTraining + 1}") { }

	public override bool CanExecute(State state, Player player)
	{
		int cost = 1 + player.AdvancedTraining;

		return state.CurrentPhase == Phase.Prep
			&& player.Resources.HasMana(cost);
	}

	public override void Execute(State state, Player player, List<GameEvent> events, string parameters)
	{
		int cost = 1 + player.AdvancedTraining;
		int progress = 1 + player.AdvancedTraining;

		if (!player.Resources.Train(cost, progress))
		{
			events.Add(new GameEventMessage($"{player.Name} trains."));
		}
		else
		{
			events.Add(new GameEventMessage($"{player.Name} has increased Max Mana to {player.Resources.MaxMana}!"));
		}
	}
}

public class EndTurnAction : PlayerAction
{
	public EndTurnAction() : base("End Turn") { }

	public override bool CanExecute(State state, Player player)
	{
		return true;
	}

	public override void Execute(State state, Player player, List<GameEvent> events, string parameters)
	{
		events.Add(new GameEventMessage("Turn ended."));
		state.EndPrepRound();
	}
}

public class RestAction : PlayerAction
{
	private const int HealAmount = 5;

	public RestAction() : base("Rest") { }

	public override bool CanExecute(State state, Player player)
	{
		return state.CurrentPhase == Phase.Prep
			   && player.Health < player.MaxHealth;
	}

	public override void Execute(State state, Player player, List<GameEvent> events, string parameters)
	{
		int before = player.Health;
		player.Heal(HealAmount);
		int healed = player.Health - before;

		events.Add(new GameEventMessage($"{player.Name} rests and heals {healed} HP."));
	}
}

public class LearnAction : PlayerAction
{
	private const int KnowledgeGain = 2;

	public LearnAction() : base("Learn") { }

	public override bool CanExecute(State state, Player player)
	{
		return state.CurrentPhase == Phase.Prep;
	}

	public override void Execute(State state, Player player, List<GameEvent> events, string parameters)
	{
		player.Resources.AddKnowledge(KnowledgeGain);
		events.Add(new PlayerGainedKnowledge(player.Name, KnowledgeGain));
	}
}
using Arcane.Core.Cards;
using Arcane.Core.Events;
using System.Collections;
using System.Runtime.InteropServices.Marshalling;
using static Arcane.Core.Cards.Artifact;
using static System.Net.Mime.MediaTypeNames;

namespace Arcane.Core;

public abstract class PlayerAction
{
	public string Name { get; }
	public Game Game { get; }

	protected PlayerAction(string name, Game game)
	{
		Name = name;
		Game = game;
	}

	public abstract bool CanExecute();

	public abstract void Execute(string parameters);
}

public class SpellCastAction : PlayerAction
{
	private readonly Spell _spell;

	public SpellCastAction(Spell spell, Game game) : base($"Cast {spell.Name}", game)
	{
		_spell = spell;
	}

	public override bool CanExecute()
	{
		return (Game.State.CurrentPhase == Phase.Battle)
				&& Game.CurrentPlayer().Resources.HasMana(_spell.ManaCost)
				&& !(_spell.OncePerBattle && _spell.UsedThisBattle);
	}

	public override void Execute(string parameters)
	{
		var player = Game.CurrentPlayer();
		var events = Game.Events;

		if (!player.Resources.SpendMana(_spell.ManaCost))
		{
			events.Add(new ErrorOccurred("Not enough mana."));
			return;
		}

		if (_spell.OncePerBattle) _spell.UsedThisBattle = true;

		var monsters = Game.State.Monsters.Where(m => m.IsAlive).ToList();

		if (!monsters.Any())
		{
			events.Add(new ErrorOccurred("No valid targets."));
			return;
		}

		Monster target = null;
		if (!string.IsNullOrEmpty(parameters))
			target = monsters.FirstOrDefault(m => m.Name.Equals(parameters, StringComparison.OrdinalIgnoreCase));
		target ??= monsters.First();

		if (_spell.Target == TargetType.Enemy)
		{
			monsters.Clear();
			monsters.Add(target);
		}

		if (_spell.Target == TargetType.Cleave)
		{
			monsters.Remove(target);
			monsters.Insert(0, target);
		}

		if (_spell.Target == TargetType.Self || _spell.Target == TargetType.Ally || _spell.Target == TargetType.AllAllies)
		{
			monsters.Clear();
		}

		Game.RaiseSpellCast(_spell, monsters);

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

		target.TakeDamage(damage, _spell);

		events.Add(new MonsterTookDamage(target.Name, damage, target.Health));

		if (!target.IsAlive) events.Add(new MonsterDefeated(target.Name));

		return damage;
	}

	private int CastCleave(List<Monster> monsters, Player player, List<GameEvent> events)
	{
		var primary = monsters.First();

		int damage = ResolveDamage(player, primary, events);

		primary.TakeDamage(damage, _spell);

		events.Add(new MonsterTookDamage(primary.Name, damage, primary.Health));

		if (!primary.IsAlive) events.Add(new MonsterDefeated(primary.Name));

		foreach (var monster in monsters.Skip(1))
		{
			int splash = _spell.SplashDamage.Resolve(damage);

			monster.TakeDamage(splash, _spell);

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

			monster.TakeDamage(damage, _spell);

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

		if (damageDealt > 0)
		{
			Game.RaiseDamageDealt(_spell, damageDealt, monsters);
		}

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

	public BuyCardAction(Card card, Game game) : base($"Buy {card.Name}", game)
	{
		_card = card;
	}

	public override bool CanExecute()
	{
		return Game.State.CurrentPhase == Phase.Prep
			   && Game.CurrentPlayer().Resources.Knowledge >= _card.KnowledgeCost;
	}

	public override void Execute(string parameters)
	{
		var player = Game.CurrentPlayer();
		var events = Game.Events;

		if (!player.Resources.SpendKnowledge(_card.KnowledgeCost))
		{
			events.Add(new ErrorOccurred("Not enough knowledge."));
			return;
		}

		Game.State.Market.Purchase(_card);

		if (_card is Spell spell)
		{
			player.Spells.Add(spell);
			events.Add(new SpellPurchased(player.Name, spell.Name));
		}
		else if (_card is Upgrade passive)
		{
			passive.Apply(player);
			events.Add(new GameEventMessage($"{player.Name} gains {passive.Name}."));
		}

		Game.RegisterListener(_card);
	}
}

public class ChannelAction : PlayerAction
{
	private Dice ManaGain = new Dice("2d4");

	public ChannelAction(Game game) : base("Channel", game) { }

	public override bool CanExecute()
	{
		return Game.State.CurrentPhase == Phase.Battle;
	}

	public override void Execute(string parameters)
	{
		var player = Game.CurrentPlayer();
		var events = Game.Events;
		var dice = ManaGain.Modify(player.ChannelBonus);
		var mana = dice.Roll(events, "Channel");
		player.Resources.AddMana(mana);
		events.Add(new PlayerGainedMana(player.Name, mana));
	}
}

public class TrainAction : PlayerAction
{
	public TrainAction(int AdvancedTraining, Game game) : base($"Train lv{AdvancedTraining + 1}", game) { }

	public override bool CanExecute()
	{
		var player = Game.CurrentPlayer();
		int cost = 1 + player.TrainingBonus;

		return Game.State.CurrentPhase == Phase.Prep
			&& player.Resources.HasMana(cost);
	}

	public override void Execute(string parameters)
	{
		var player = Game.CurrentPlayer();
		var events = Game.Events;
		int cost = 1 + player.TrainingBonus;
		int progress = 1 + player.TrainingBonus;

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
	public EndTurnAction(Game game) : base("End Turn", game) { }

	public override bool CanExecute()
	{
		return true;
	}

	public override void Execute(string parameters)
	{
		Game.Events.Add(new GameEventMessage("Turn ended."));
		Game.State.EndPrepRound();
	}
}

public class RestAction : PlayerAction
{
	private const int HealAmount = 5;

	public RestAction(Game game) : base("Rest", game) { }

	public override bool CanExecute()
	{
		var player = Game.CurrentPlayer();
		return Game.State.CurrentPhase == Phase.Prep
			   && player.Health < player.MaxHealth;
	}

	public override void Execute(string parameters)
	{
		var player = Game.CurrentPlayer();
		int before = player.Health;
		player.Heal(HealAmount);
		int healed = player.Health - before;

		Game.Events.Add(new GameEventMessage($"{player.Name} rests and heals {healed} HP."));
	}
}

public class LearnAction : PlayerAction
{
	private const int KnowledgeGain = 2;

	public LearnAction(Game game) : base("Learn", game) { }

	public override bool CanExecute()
	{
		return Game.State.CurrentPhase == Phase.Prep;
	}

	public override void Execute(string parameters)
	{
		var player = Game.CurrentPlayer();
		player.Resources.AddKnowledge(KnowledgeGain);
		Game.Events.Add(new PlayerGainedKnowledge(player.Name, KnowledgeGain));
	}
}

public class ArtifactSelectAction : PlayerAction
{
	Artifact artifact;
	public ArtifactSelectAction(Game game, Artifact artifact) : base("Choose " + artifact.Name, game) 
	{
		this.artifact = artifact;
	}

	public override bool CanExecute()
	{
		return Game.State.CurrentPhase == Phase.ArtifactChoice;
	}

	public override void Execute(string parameters)
	{
		var player = Game.CurrentPlayer();

		Game.State.ArtifactDeck.Choose(artifact);

		player.Artifacts.Add(artifact);
		artifact.Game = Game;
		Game.RegisterListener(artifact);

		Game.Events.Add(new GameEventMessage($"You chose {artifact.Name}!"));
	}
}
public class ActivateImplementAction : PlayerAction
{
	private readonly Implement _implement;

	public ActivateImplementAction(Implement implement, Game game)
		: base($"Activate {implement.Name}", game)
	{
		_implement = implement;
	}

	public override bool CanExecute()
	{
		return Game.State.CurrentPhase == Phase.Prep
			&& !_implement.IsActive
			&& Game.CurrentPlayer().Resources.CurrentMana >= _implement.ManaCost;
	}

	public override void Execute(string parameters)
	{
		var player = Game.CurrentPlayer();
		_implement.Activate(player);
	}
}
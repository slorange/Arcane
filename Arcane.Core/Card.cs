using Arcane.Core.Events;

namespace Arcane.Core.Cards;

public abstract class Card : IGameEventListener
{
	public Game Game;
	public Guid Id { get; set; } = Guid.NewGuid();
	public string Name { get; set; }
	public int KnowledgeCost { get; set; }

	// Delegates
	public Action? OnBattleStartAction;
	public Action<Spell?>? OnBattleEndAction;
	public Action<Spell, List<Monster>>? OnSpellCastAction;
	public Action<List<Monster>, int, Spell?>? OnDamageDealtAction;
	public Action<Monster, Spell?>? OnEnemyKilledAction;
	public Action? OnTurnStartAction;
	public Action? OnTurnEndAction;

	protected Card(string name, int knowledgeCost)
	{
		Name = name;
		KnowledgeCost = knowledgeCost;
	}
	public virtual string GetDisplay(bool market = false)
	{
		if (market)
			return $"{Name,-26} — {KnowledgeCost} Knowledge";
		else
			return $"{Name,-26}"; 
	}

	// Interface implementations
	public virtual void OnBattleStart()
		=> OnBattleStartAction?.Invoke();

	public virtual void OnBattleEnd(Spell? killingBlow)
		=> OnBattleEndAction?.Invoke(killingBlow);

	public virtual void OnSpellCast(Spell spell, List<Monster> targets)
		=> OnSpellCastAction?.Invoke(spell, targets);

	public virtual void OnDamageDealt(List<Monster> targets, int amount, Spell? source)
		=> OnDamageDealtAction?.Invoke(targets, amount, source);

	public virtual void OnEnemyKilled(Monster enemy, Spell? killingBlow)
		=> OnEnemyKilledAction?.Invoke(enemy, killingBlow);

	public virtual void OnTurnStart()
		=> OnTurnStartAction?.Invoke();

	public virtual void OnTurnEnd()
		=> OnTurnEndAction?.Invoke();
}

// Used within the game for different components to handle special cases
public interface IGameEventListener
{
	void OnBattleStart() { }
	void OnBattleEnd(Spell? killingBlow) { }
	void OnSpellCast(Spell spell, List<Monster> targets) { }
	void OnDamageDealt(List<Monster> targets, int amount, Spell? source) { }
	void OnEnemyKilled(Monster enemy, Spell? killingBlow) { }
	void OnTurnStart() { }
	void OnTurnEnd() { }
}

public class Upgrade : Card
{
	public string Description { get; set; }
	public Action<Player> Apply { get; set; }

	public Upgrade(string name, int knowledgeCost, string description, Action<Player> apply)
		: base(name, knowledgeCost)
	{
		Description = description;
		Apply = apply;
	}

	public override string GetDisplay(bool market = false)
	{
		if (market)
			return $"{Name,-26} — {KnowledgeCost} Knowledge - {Description}";
		else
			return $"{Name,-26} - {Description}";
	}
}

public class Artifact : Card
{
	public string Description { get; set; }
	public Action<Player> Apply { get; set; }
	public MonsterTheme? Theme { get; set; }

	public Artifact(string name, string description, Action<Player> apply, MonsterTheme? theme)
		: base(name, 0)
	{
		Description = description;
		Apply = apply;
		Theme = theme;
	}

	public override string GetDisplay(bool market = false)
	{
		if (market)
			return $"{Name,-26} — {KnowledgeCost} Knowledge - {Description}";
		else
			return $"{Name,-26} - {Description}";
	}

	public class Implement : Card
	{
		public int ManaCost { get; set; }
		public bool IsActive { get; private set; }

		public Implement(
			string name,
			int knowledgeCost,
			int manaCost,
			string description,
		) : base(name, knowledgeCost)
		{
			ManaCost = manaCost;
			Description = description;
		}

		public string Description { get; set; }

		public void Activate(Player player)
		{
			if (IsActive) return;

			player.Resources.SpendMana(ManaCost);
			IsActive = true;

			Game.RegisterListener(this);

			Game.Events.Add(new GameEventMessage($"{player.Name} activates {Name}."));
		}

		public void Deactivate()
		{
			IsActive = false;
		}
	}
}
using Arcane.Core.Cards;
using Arcane.Core.Events;

namespace Arcane.Core;

public enum MonsterTheme
{
	Beasts,
	Goblins,
	Ogres,
	Undead,
	Elemental,
	Warlocks,
	Demons,
	Vampires,
}

public class Monster
{
	public string Name { get; }
	public int Health { get; private set; }
	public Value AttackDamage { get; }
	public int Threat { get; }
	public MonsterTheme Theme { get; }
	public List<StatusEffect> Effects { get; } = new();

	public Monster(string name, int health, Value attackDamage, int threat, MonsterTheme theme)
	{
		Name = name;
		Health = health;
		AttackDamage = attackDamage;
		Threat = threat;
		Theme = theme;
	}

	public Monster Clone()
	{
		return new Monster(Name, Health, AttackDamage, Threat, Theme);
	}

	public void TakeDamage(int amount)
	{
		Health -= amount;
		if (Health < 0) Health = 0;
	}

	public bool IsAlive => Health > 0;

	public bool HasEffect(StatusEffectType type)
	{
		return Effects.Any(e => e.Type == type && e.Duration > 0);
	}
	public void TickEffects(List<GameEvent> events)
	{
		foreach (var e in Effects.ToList())
		{
			e.Duration--;
			if (e.Duration <= 0)
			{
				events.Add(new GameEventMessage($"{Name} is no longer {e.Type.ToString().ToLower()}"));
				Effects.Remove(e);
			}
		}
	}
}
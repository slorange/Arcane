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
	public HashSet<SpellSchool> Immunities { get; } = new();
	public HashSet<SpellSchool> Resistances { get; } = new();
	public HashSet<SpellSchool> Weaknesses { get; } = new();
	public HashSet<StatusEffectType> StatusImmunities { get; } = new();

	public Monster(string name, int health, Value attackDamage, int threat, MonsterTheme theme)
	{
		Name = name;
		Health = health;
		AttackDamage = attackDamage;
		Threat = threat;
		Theme = theme;

		MonsterLibrary.ApplyThemeDefaults(this);
	}

	public Monster Clone()
	{
		var m = new Monster(Name, Health, AttackDamage, Threat, Theme);

		m.Immunities.UnionWith(Immunities);
		m.Resistances.UnionWith(Resistances);
		m.Weaknesses.UnionWith(Weaknesses);
		m.StatusImmunities.UnionWith(StatusImmunities);

		return m;
	}

	public void TakeDamage(int amount)
	{
		Health -= amount;
		if (Health < 0) Health = 0;
	}

	public bool IsAlive => Health > 0;

	public void GiveEffect(StatusEffect effect, List<GameEvent> events)
	{
		if (StatusImmunities.Contains(effect.Type))
		{
			events.Add(new GameEventMessage($"{Name} is immune to {effect.Type.ToString().ToLower()}!"));
			return;
		}

		var effectClone = effect.Clone();

		if (effect.Type == StatusEffectType.Burn)
		{
			if (HasEffect(StatusEffectType.Burn))
			{
				var burn = Effects.First(e => e.Type == StatusEffectType.Burn);
				if (burn.BurnDice.Value.Sides == 20) return;
				burn.BurnDice = burn.BurnDice.Value.Modify(1);
			}
			else
			{
				Effects.Add(effectClone);
				events.Add(new GameEventMessage($"{Name} is {effect.Type.ToString().ToLower()} with {effect.BurnDice}!"));
			}
		}
		else
		{
			Effects.Add(effectClone);
			events.Add(new GameEventMessage($"{Name} is {effect.Type.ToString().ToLower()} for {effect.Duration} turn(s)!"));
		}
	}

	public bool HasEffect(StatusEffectType type)
	{
		return Effects.Any(e => e.Type == type);
	}
	public void TickEffects(List<GameEvent> events)
	{
		foreach (var e in Effects.ToList())
		{
			if (e.Type == StatusEffectType.Burn)
			{
				if (e.BurnDice.Value.Sides == 4)
				{
					events.Add(new GameEventMessage($"{Name} is no longer {e.Type.ToString().ToLower()}"));
					Effects.Remove(e);
				}
				e.BurnDice = e.BurnDice.Value.Modify(-1);
			}
			else
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

	public bool IsImmune(Spell spell)
	{
		return Immunities.Contains(spell.School);
	}

	public int ComputeModifier(Spell spell, ref string reasons)
	{
		int modifier = 0;

		if (Resistances.Contains(spell.School))
		{
			modifier--;
			reasons += $"{spell.School} resistance, ";
		}

		if (Weaknesses.Contains(spell.School))
		{
			modifier++;
			reasons += $"{spell.School} weakness, ";
		}

		if (HasEffect(StatusEffectType.Shock) && spell.School == SpellSchool.Lightning)
		{
			modifier += 2;
			reasons += "Shock, ";
		}

		if (HasEffect(StatusEffectType.Brittle))
		{
			modifier++;
			reasons += "Brittle, ";
		}

		return modifier;
	}
}
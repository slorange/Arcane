using Arcane.Core.Cards;
using Arcane.Core.Events;
using System.Numerics;

namespace Arcane.Core;

public class Player
{
	public string Name { get; }
	public int Health { get; private set; }
	public int Shield { get; private set; }
	public Resources Resources { get; } = new();
	public List<Spell> Spells { get; } = new();

	public List<PlayerEffect> Effects = new List<PlayerEffect>();
	public int AdvancedTraining { get; set; }

	public int MaxHealth = 25;
	public Player(string name)
	{
		Name = name;
		Health = MaxHealth;
		Spells.Add(SpellLibrary.MagicMissile());
	}

	public void TakeDamage(int amount)
	{
		if (Shield > 0)
		{
			int blocked = Math.Min(Shield, amount);
			Shield -= blocked;
			amount -= blocked;
		}

		if (amount > 0)
		{
			Health -= amount;
			if (Health < 0) Health = 0;
		}
	}

	public void Heal(int amount)
	{
		Health += amount;
		if (Health > MaxHealth) Health = MaxHealth;
	}

	public void FullHeal()
	{
		Health = MaxHealth;
	}

	public void AddShield(int amount)
	{
		if (amount <= 0) return;

		Shield += amount;
	}

	public void ResetShield()
	{
		Shield = 0;
	}

	public bool IsAlive => Health > 0;
	public List<Card> Actions { get; } = new();

	public int ComputeModifier(Spell spell, ref string reasons)
	{
		int modifier = 0;

		foreach (var effect in Effects)
		{
			if (effect.School == SpellSchool.None || effect.School == spell.School)
			{
				modifier += effect.Modifier;

				reasons += $"{effect.Modifier:+#;-#} {effect.School} buff, ";

				if (effect.ConsumeOnUse)
					effect.Duration = 0;
			}
		}

		return modifier;
	}

	public void TickEffects(List<GameEvent> events)
	{
		foreach (var e in Effects.ToList())
		{
			if (e.Duration == null) continue;

			e.Duration--;

			if (e.Duration <= 0)
			{
				Effects.Remove(e);
				events.Add(new GameEventMessage($"{Name}'s {e.School} spell buff fades."));
			}
		}
	}
}

public class PlayerEffect
{
	public SpellSchool School { get; }
	public int Modifier { get; }
	public int? Duration { get; set; }
	public bool ConsumeOnUse { get; set; }

	public PlayerEffect(SpellSchool school, int modifier, int? duration, bool consumeOnUse = false)
	{
		School = school;
		Modifier = modifier;
		Duration = duration;
		ConsumeOnUse = consumeOnUse;
	}

	public PlayerEffect Clone() => new PlayerEffect(School, Modifier, Duration, ConsumeOnUse );
}


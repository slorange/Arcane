using Arcane.Core.Events;
using System.Reflection.Emit;

namespace Arcane.Core.Cards;

public abstract class Card
{
	public Guid Id { get; } = Guid.NewGuid();
	public string Name { get; }
	public int KnowledgeCost { get; }

	protected Card(string name, int knowledgeCost)
	{
		Name = name;
		KnowledgeCost = knowledgeCost;
	}
}

public class Spell : Card
{
	// Static Values
	public SpellSchool School { get; }
	public TargetType Target { get; }
	public int ManaCost { get; }
	public Value Damage { get; }
	public Value SplashDamage { get; }
	public Value Heal { get; }
	public Value Shield { get; }
	public Value Lifesteal { get; }
	public Value ManaGain { get; }
	public StatusEffect StatusEffect { get; }
	public bool OncePerBattle { get; }

	// Game state
	public bool UsedThisBattle { get; set; } = false;

	public Spell(
		string name,
		SpellSchool school,
		int knowledgeCost,
		int manaCost,
		TargetType target,
		Value damage = default,
		Value splashDamage = default,
		Value heal = default,
		Value shield = default,
		Value lifesteal = default,
		Value manaGain = default,
		StatusEffect statusEffect = default,
		bool oncePerBattle = false
	) : base(name, knowledgeCost)
	{
		School = school;
		ManaCost = manaCost;
		Target = target;

		Damage = damage;
		SplashDamage = splashDamage;
		Heal = heal;

		Shield = shield;
		Lifesteal = lifesteal;

		ManaGain = manaGain;

		StatusEffect = statusEffect;

		OncePerBattle = oncePerBattle;

		StatusEffect ??= new StatusEffect(StatusEffectType.None, 0);
	}

	public string GetDescription()
	{
		var parts = new List<string>();

		if (Damage.Type != ValueKind.Flat || Damage.Flat != 0)
		{
			if (Target == TargetType.AllEnemies)
				parts.Add($"{Damage} damage to all enemies");
			else if (Target == TargetType.Cleave)
				parts.Add($"{Damage} damage, {SplashDamage} splash");
			else
				parts.Add($"{Damage} damage");
		}

		if (Heal.Type != ValueKind.Flat || Heal.Flat != 0)
		{
			parts.Add($"heal {Heal}");
		}

		if (Shield.Type != ValueKind.Flat || Shield.Flat != 0)
		{
			parts.Add($"gain {Shield} shield");
		}

		if (Lifesteal.Type != ValueKind.Flat || Lifesteal.Flat != 0)
		{
			parts.Add($"lifesteal {Lifesteal}");
		}

		if (ManaGain.Type != ValueKind.Flat || ManaGain.Flat != 0)
		{
			parts.Add($"gain {ManaGain} mana");
		}

		if (StatusEffect.Type != StatusEffectType.None)
		{
			switch (StatusEffect.Type)
			{
				case StatusEffectType.Burn:
					parts.Add($"burn {StatusEffect.DamagePerTurn} for {StatusEffect.Duration} turns");
					break;

				case StatusEffectType.Freeze:
					parts.Add($"freeze for {StatusEffect.Duration} turn");
					break;
			}
		}

		if (OncePerBattle)
		{
			parts.Add("once per battle");
		}

		return string.Join(", ", parts);
	}

	public string GetPlayerDisplay()
	{
		return $"{Name} ({School}) — {ManaCost} Mana — {GetDescription()}";
	}

	public string GetMarketDisplay()
	{
		return $"{Name} ({School}) — {KnowledgeCost} Knowledge — {ManaCost} Mana — {GetDescription()}";
	}
}

public enum SpellSchool
{
	None,
	Arcane,
	Fire,
	Ice,
	Lightning,
	Earth,
	Holy,
	Blood,
}

public enum TargetType
{
	Enemy,
	AllEnemies,
	Cleave,
	Self,
	Ally,
	AllAllies
}

public enum StatusEffectType
{
	None,
	Burn,
	Freeze
}

public class StatusEffect
{
	public StatusEffectType Type { get; }
	public int Duration { get; set; }
	public Value? DamagePerTurn { get; }

	public StatusEffect(StatusEffectType type, int duration, Value? damagePerTurn = null)
	{
		Type = type;
		Duration = duration;
		DamagePerTurn = damagePerTurn;
	}

	public StatusEffect Clone()
	{
		return new StatusEffect(Type, Duration, DamagePerTurn);
	}
}

public enum ValueKind
{
	Flat,
	Dice,
	Percent
}

public struct Value
{
	public ValueKind Type { get; }

	public int Flat { get; }

	public Dice Dice { get; }

	public double Percent { get; }

	public Value(int flat)
	{
		Type = ValueKind.Flat;
		Flat = flat;
		Dice = default;
		Percent = 0;
	}

	public Value(Dice dice)
	{
		Type = ValueKind.Dice;
		Dice = dice;
		Flat = 0;
		Percent = 0;
	}

	public Value(double percent)
	{
		Type = ValueKind.Percent;
		Percent = percent;
		Flat = 0;
		Dice = default;
	}

	public Value(string text)
	{
		text = text.Trim().ToLower();

		Flat = 0;
		Dice = default;
		Percent = 0;

		// Percent
		if (text.EndsWith("%"))
		{
			Type = ValueKind.Percent;

			var number = text.Substring(0, text.Length - 1);
			Percent = double.Parse(number) / 100.0;

			return;
		}

		// Dice (XdY)
		if (text.Contains("d"))
		{
			Type = ValueKind.Dice;

			var parts = text.Split('d');

			if (parts.Length != 2)
				throw new ArgumentException($"Invalid dice format: {text}");

			int count = int.Parse(parts[0]);
			int sides = int.Parse(parts[1]);

			Dice = new Dice(count, sides);

			return;
		}

		// Flat number
		if (int.TryParse(text, out int value))
		{
			Type = ValueKind.Flat;
			Flat = value;
			return;
		}

		throw new ArgumentException($"Invalid Value format: {text}");
	}

	public int Resolve(int baseValue)
	{
		switch (Type)
		{
			case ValueKind.Percent:
				return (int)Math.Round(baseValue * Percent);
			case ValueKind.Flat:
			case ValueKind.Dice:
			default:
				return 0; // Shouldn't be called with these parameters
		}
	}

	public int Resolve(List<GameEvent> events, string label = "")
	{
		switch (Type)
		{
			case ValueKind.Flat:
				return Flat;

			case ValueKind.Percent:
				return 0; // Shouldn't be called with these parameters

			case ValueKind.Dice:
				var (total, rolls) = Dice.Roll();

				if (label != "")
				{
					var rollText = string.Join(" + ", rolls);
					events.Add(new GameEventMessage($"{label} rolls {Dice} -> ({rollText}) = {total}"));
				}

				return total;

			default:
				return 0;
		}
	}

	public override string ToString()
	{
		return Type switch
		{
			ValueKind.Flat => Flat.ToString(),
			ValueKind.Dice => $"{Dice.Count}d{Dice.Sides}",
			ValueKind.Percent => $"{Percent * 100}%",
			_ => "?"
		};
	}
}

public struct Dice
{
	static Random rng = new Random();

	public int Count { get; }
	public int Sides { get; }

	public Dice(int count, int sides)
	{
		Count = count;
		Sides = sides;
	}

	public (int total, List<int> rolls) Roll()
	{
		var rolls = new List<int>();
		int total = 0;

		for (int i = 0; i < Count; i++)
		{
			int r = rng.Next(1, Sides + 1);
			rolls.Add(r);
			total += r;
		}

		return (total, rolls);
	}

	public double Average => Count * (Sides + 1) / 2.0;

	public override string ToString() => $"{Count}d{Sides}";
}


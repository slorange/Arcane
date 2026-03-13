using Arcane.Core.Cards;
using Arcane.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcane.Core;

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
				return Dice.Roll(events, label);

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

	public Dice(string dice)
	{
		var parts = dice.Split('d');

		if (parts.Length != 2)
			throw new ArgumentException($"Invalid dice format: {dice}");

		Count = int.Parse(parts[0]);
		Sides = int.Parse(parts[1]);
	}

	public Dice Modify(int modifier, List<GameEvent> events = null, string reasons = "")
	{
		int count = Count;
		int sides = Sides;

		int[] ladder = { 4, 6, 8, 10, 12, 20 };

		int index = Array.IndexOf(ladder, sides);
		if (index < 0) index = 1; // fallback to d6

		if (modifier > 0)
		{
			for (int i = 0; i < modifier; i++)
			{
				if (index < ladder.Length - 1)
				{
					index++;
				}
				else
				{
					count++;
				}
			}
		}
		else if (modifier < 0)
		{
			for (int i = 0; i < -modifier; i++)
			{
				if (count > 1)
				{
					count--;
				}
				else if (index > 0)
				{
					index--;
				}
			}
		}
		var newDice = new Dice(count, ladder[index]);
		if (events != null && !string.IsNullOrEmpty(reasons))
		{
			reasons = reasons.TrimEnd(' ', ',');
			events.Add(new GameEventMessage($"{this} -> {newDice} because of {reasons}"));
		}

		return newDice;
	}

	public int Roll(List<GameEvent> events, string label = "")
	{
		var rolls = new List<int>();
		int total = 0;

		for (int i = 0; i < Count; i++)
		{
			int r = rng.Next(1, Sides + 1);
			rolls.Add(r);
			total += r;
		}

		var rollText = string.Join(" + ", rolls);
		events.Add(new GameEventMessage($"{label} rolls {this} -> ({rollText}) = {total}"));

		return total;
	}

	public double Average => Count * (Sides + 1) / 2.0;

	public override string ToString() => $"{Count}d{Sides}";
}


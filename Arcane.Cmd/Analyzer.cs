using Arcane.Core;
using Arcane.Core.Cards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcane.Cmd;

public  class Analyzer
{

	public readonly struct SpellData : IComparable<SpellData>
	{
		public Spell Spell { get; }
		public string Name { get; }
		public double Power { get; }
		public double Cost { get; }
		public double Efficiency { get; }

		public SpellData(Spell spell, double power, double cost)
		{
			Spell = spell;
			Name = spell.Name;
			Power = power;
			Cost = cost;
			Efficiency = power / cost * 4 / 3.5; // Constant at the end to normalize Magic Missile as 1 efficiency
		}

		public int CompareTo(SpellData other)
		{
			return Efficiency.CompareTo(other.Efficiency);
		}
	}

	public static void AnalyzeSpells()
	{
		Console.WriteLine("=== SPELL BALANCE ESTIMATE ===\n");

		var data = new List<SpellData>();

		var spellList = SpellLibrary.AllSpells();

		foreach (var spell in spellList)
		{
			double power = EstimateSpellPower(spell);
			double cost = spell.ManaCost + spell.KnowledgeCost * 0.33 + 3; // +3 for actions

			data.Add(new SpellData(spell, power, cost));
		}

		//data.Sort();
		double avg = data.Average(s => s.Efficiency);
		double tolerance = 0.1;

		data = data.OrderBy(s => s.Cost).ToList();

		foreach (var spell in data)
		{
			if (avg * (1-tolerance) > spell.Efficiency)
				Console.ForegroundColor = ConsoleColor.Yellow;
			else if (avg * (1 + tolerance) < spell.Efficiency)
				Console.ForegroundColor = ConsoleColor.Red;
			else
				Console.ForegroundColor = ConsoleColor.Gray;

			double targetCost = spell.Power / avg * 4 / 3.5;
			double costDelta = targetCost - spell.Cost;

			double knowledgeDiffTotal = Math.Round(costDelta * 3);
			int manaDiff = (int)(knowledgeDiffTotal / 3);
			int knowledgeDiff = (int)(knowledgeDiffTotal % 3);

			Console.WriteLine($"{($"{spell.Name} ({spell.Spell.Target})"),-32} | Power: {spell.Power,6:F1} | Cost: {spell.Cost,5:F1} | Efficiency: {spell.Efficiency,5:F2} | Fix: {manaDiff} Mana {knowledgeDiff} Knowledge");
		}

		Console.ForegroundColor = ConsoleColor.Gray;
		Console.WriteLine();
		Console.WriteLine($"Average Efficiency: {avg}");
		data.Sort();
		var weakest = data.First();
		var strongest = data.Last();
		Console.WriteLine($"Weakest: {weakest.Spell.Name}");
		Console.WriteLine($"Strongest: {strongest.Spell.Name}");
		Console.WriteLine($"Ratio: {strongest.Efficiency / weakest.Efficiency}");
		Console.WriteLine();
	}

	private static double EstimateSpellPower(Spell s)
	{
		double power = 0;

		// DAMAGE
		if (s.Damage.Type == ValueKind.Dice)
			power += ExpectedDice(s.Damage.Dice);

		// HEAL
		if (s.Heal.Type == ValueKind.Dice)
			power += ExpectedDice(s.Heal.Dice);

		// SHIELD
		if (s.Shield.Type == ValueKind.Dice)
			power += ExpectedDice(s.Shield.Dice) * 1.5;

		// MANA
		if (s.ManaGain.Type == ValueKind.Dice)
			power += ExpectedDice(s.ManaGain.Dice);

		// LIFESTEAL
		if (s.Lifesteal.Type == ValueKind.Percent && s.Damage.Type == ValueKind.Dice)
		{
			var dmg = ExpectedDice(s.Damage.Dice);
			power += dmg * s.Lifesteal.Percent;
		}

		// Cleave is here because dots and other effects only apply to the primary target
		if (s.Target == TargetType.Cleave) power *= 1 + 2 * s.SplashDamage.Percent; 

		// BURN
		if (s.StatusEffect.Type == StatusEffectType.Burn)
		{
			var burn = ExpectedDice(s.StatusEffect.BurnDice.Value);
			power += burn * 2;
		}

		// OTHER STATUS
		if (s.StatusEffect.Type != StatusEffectType.None)
		{
			double multiplier = DiminishingSeries(s.StatusEffect.Duration);
			double baseValue = s.StatusEffect.Type == StatusEffectType.Freeze ? 10 : 4;
			power += baseValue * multiplier;
		}

		// TARGET MULTIPLIER
		if (s.Target == TargetType.AllEnemies) power *= 3;

		// PLAYER BUFFS
		if (s.PlayerEffect != null)
		{
			double duration = s.PlayerEffect.Duration ?? 5;
			if (s.PlayerEffect.ConsumeOnUse) duration = 1.5; // Better than 1 but not as good as 2

			double value = 3 * s.PlayerEffect.Modifier * duration;

			power += value;
		}

		if (s.OncePerBattle) power /= 2.5;

		return power;
	}

	private static double ExpectedDice(Dice dice)
	{
		return dice.Count * (dice.Sides + 1) / 2.0;
	}

	private static double DiminishingSeries(int duration, double r = 0.75)
	{
		return (1 - Math.Pow(r, duration)) / (1 - r);
	}
}

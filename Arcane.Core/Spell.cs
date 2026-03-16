using Arcane.Core.Cards;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Arcane.Core;

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
	public StatusEffect StatusEffect { get; set; }
	public PlayerEffect? PlayerEffect { get; set; }
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
					parts.Add($"burn {StatusEffect.BurnDice}");
					break;

				case StatusEffectType.Freeze:
					parts.Add($"freeze for {StatusEffect.Duration} turn(s)");
					break;

				case StatusEffectType.Shock:
					parts.Add($"shock for {StatusEffect.Duration} turn(s)");
					break;

				case StatusEffectType.Brittle:
					parts.Add($"brittle for {StatusEffect.Duration} turn(s)");
					break;

				case StatusEffectType.Weak:
					parts.Add($"weaken for {StatusEffect.Duration} turn(s)");
					break;

				case StatusEffectType.Blinded:
					parts.Add($"blind for {StatusEffect.Duration} turn(s)");
					break;

				case StatusEffectType.Marked:
					parts.Add($"mark for {StatusEffect.Duration} turn(s)");
					break;
			}
		}

		if (PlayerEffect != null)
		{
			var school = PlayerEffect.School == SpellSchool.None
				? "spells"
				: $"{PlayerEffect.School.ToString().ToLower()} spells";

			var duration = PlayerEffect.Duration == null
				? ""
				: $" for {PlayerEffect.Duration} turn(s)";

			if (PlayerEffect.ConsumeOnUse)
				duration = " on next cast";

			parts.Add($"{school} +{PlayerEffect.Modifier} die{duration}");
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

	public override string GetMarketDisplay()
	{
		return $"{$"{Name} ({School})", -26} — {KnowledgeCost} Knowledge — {ManaCost} Mana — {GetDescription()}";
	}
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

public enum StatusEffectType
{
	None,
	Marked,
	Burn,
	Freeze,
	Shock,
	Brittle,
	Blinded,
	Weak,
}

public class StatusEffect
{
	public StatusEffectType Type { get; }
	public int Duration { get; set; }
	public Dice? BurnDice { get; set; }

	public StatusEffect(StatusEffectType type, int duration, Dice? burnDice = null)
	{
		Type = type;
		Duration = duration;
		BurnDice = burnDice;
	}

	public StatusEffect Clone()
	{
		return new StatusEffect(Type, Duration, BurnDice);
	}
}
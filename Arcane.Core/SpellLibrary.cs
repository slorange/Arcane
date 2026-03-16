using Arcane.Core.Cards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcane.Core;

public static class SpellLibrary
{

	private static List<Spell>? _allSpells;
	public static List<Spell> AllSpells()
	{
		if (_allSpells != null) return _allSpells;

		var methods = typeof(SpellLibrary)
			.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
			.Where(m => m.ReturnType == typeof(Spell) &&
						m.Name != nameof(MagicMissile));

		_allSpells = methods.Select(m => (Spell)m.Invoke(null, null)!).ToList();

		return _allSpells;
	}


	// ===== ARCANE =====

	public static Spell MagicMissile()
	{
		var s = new Spell(
			name: "Magic Missile",
			school: SpellSchool.Arcane,
			knowledgeCost: 0,
			manaCost: 1,
			target: TargetType.Enemy,
			damage: new Value("1d12")
		);
		return s;
	}

	public static Spell ArcaneBarrage()
	{
		var s = new Spell(
			name: "Arcane Barrage",
			school: SpellSchool.Arcane,
			knowledgeCost: 4,
			manaCost: 4,
			target: TargetType.Enemy,
			damage: new Value("3d10")
		);
		return s;
	}

	public static Spell ChaosBolt()
	{
		var s = new Spell(
			name: "Chaos Bolt",
			school: SpellSchool.Arcane,
			knowledgeCost: 4,
			manaCost: 1,
			target: TargetType.Enemy,
			damage: new Value("1d20")
		);
		return s;
	}

	public static Spell Disintegrate()
	{
		var s = new Spell(
			name: "Disintegrate",
			school: SpellSchool.Arcane,
			knowledgeCost: 9,
			manaCost: 8,
			target: TargetType.Enemy,
			damage: new Value("5d10")
		);
		return s;
	}

	public static Spell ManaPulse()
	{
		var s = new Spell(
			name: "Mana Pulse",
			school: SpellSchool.Arcane,
			knowledgeCost: 1,
			manaCost: 2,
			target: TargetType.AllEnemies,
			damage: new Value("1d6")
		);
		return s;
	}

	public static Spell ArcaneMark()
	{
		var s = new Spell(
			name: "Arcane Mark",
			school: SpellSchool.Arcane,
			knowledgeCost: 2,
			manaCost: 1,
			target: TargetType.Enemy
		);

		s.StatusEffect = new StatusEffect(StatusEffectType.Marked, 3);

		return s;
	}

	public static Spell ArcaneFocus()
	{
		var s = new Spell(
			name: "Arcane Focus",
			school: SpellSchool.Arcane,
			knowledgeCost: 1,
			manaCost: 1,
			target: TargetType.Self
		);

		s.PlayerEffect = new PlayerEffect(
			school: SpellSchool.None,
			modifier: 1,
			duration: 3
		);

		return s;
	}

	public static Spell ArcaneDraw()
	{
		var s = new Spell(
			name: "Arcane Draw",
			school: SpellSchool.Arcane,
			knowledgeCost: 2,
			manaCost: 0,
			target: TargetType.Self,
			manaGain: new Value("2d6")
		);
		return s;
	}

	public static Spell ManaSurge()
	{
		var s = new Spell(
			name: "Mana Surge",
			school: SpellSchool.Arcane,
			knowledgeCost: 6,
			manaCost: 0,
			target: TargetType.Self,
			manaGain: new Value("3d6")
		);
		return s;
	}

	public static Spell LeylineTap()
	{
		var s = new Spell(
			name: "Leyline Tap",
			school: SpellSchool.Arcane,
			knowledgeCost: 10,
			manaCost: 0,
			target: TargetType.Self,
			manaGain: new Value("3d20"),
			oncePerBattle: true
		);
		return s;
	}


	// ===== FIRE =====

	public static Spell Firebolt()
	{
		var s = new Spell(
			name: "Firebolt",
			school: SpellSchool.Fire,
			knowledgeCost: 2,
			manaCost: 2,
			target: TargetType.Enemy,
			damage: new Value("2d10")
		);
		return s;
	}

	public static Spell BurningCurse()
	{
		var s = new Spell(
			name: "Burning Curse",
			school: SpellSchool.Fire,
			knowledgeCost: 4,
			manaCost: 2,
			target: TargetType.Enemy
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Burn,
			0,
			new Dice("1d12")
		);

		return s;
	}

	public static Spell MeteorShard()
	{
		var s = new Spell(
			name: "Meteor Shard",
			school: SpellSchool.Fire,
			knowledgeCost: 4,
			manaCost: 5,
			target: TargetType.Cleave,
			damage: new Value("2d8"),
			splashDamage: new Value("50%")
		);
		return s;
	}

	public static Spell CinderBurst()
	{
		var s = new Spell(
			name: "Cinder Burst",
			school: SpellSchool.Fire,
			knowledgeCost: 2,
			manaCost: 2,
			target: TargetType.AllEnemies,
			damage: new Value("1d6")
		);

		return s;
	}

	public static Spell Firestorm()
	{
		var s = new Spell(
			name: "Firestorm",
			school: SpellSchool.Fire,
			knowledgeCost: 7,
			manaCost: 7,
			target: TargetType.AllEnemies,
			damage: new Value("1d6")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Burn,
			0,
			new Dice("1d4")
		);

		return s;
	}

	public static Spell FlameEmpower()
	{
		var s = new Spell(
			name: "Heat Wave",
			school: SpellSchool.Fire,
			knowledgeCost: 4,
			manaCost: 0,
			target: TargetType.AllAllies
		);

		s.PlayerEffect = new PlayerEffect(
			school: SpellSchool.Fire,
			modifier: 2,
			duration: null,
			consumeOnUse: true
		);

		return s;
	}

	public static Spell Inferno()
	{
		var s = new Spell(
			name: "Inferno",
			school: SpellSchool.Fire,
			knowledgeCost: 12,
			manaCost: 17,
			target: TargetType.AllEnemies,
			damage: new Value("2d8")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Burn,
			0,
			new Dice("1d6")
		);

		return s;
	}

	public static Spell VolcanicLance()
	{
		var s = new Spell(
			name: "Volcanic Lance",
			school: SpellSchool.Fire,
			knowledgeCost: 11,
			manaCost: 14,
			target: TargetType.Enemy,
			damage: new Value("5d10")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Burn,
			0,
			new Dice("1d12")
		);
		return s;
	}


	// ===== ICE =====

	public static Spell Freeze()
	{
		var s = new Spell(
			name: "Freeze",
			school: SpellSchool.Ice,
			knowledgeCost: 1,
			manaCost: 2,
			target: TargetType.Enemy
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Freeze,
			1
		);

		return s;
	}

	public static Spell FrostNova()
	{
		var s = new Spell(
			name: "Frost Nova",
			school: SpellSchool.Ice,
			knowledgeCost: 5,
			manaCost: 6,
			target: TargetType.AllEnemies,
			oncePerBattle: true
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Freeze,
			2
		);

		return s;
	}

	public static Spell IceLance()
	{
		var s = new Spell(
			name: "Ice Lance",
			school: SpellSchool.Ice,
			knowledgeCost: 2,
			manaCost: 1,
			target: TargetType.Enemy,
			damage: new Value("2d8")
		);
		return s;
	}

	public static Spell Blizzard()
	{
		var s = new Spell(
			name: "Blizzard",
			school: SpellSchool.Ice,
			knowledgeCost: 9,
			manaCost: 10,
			target: TargetType.AllEnemies,
			damage: new Value("3d6")
		);

		return s;
	}

	public static Spell GlacialSpike()
	{
		var s = new Spell(
			name: "Glacial Spike",
			school: SpellSchool.Ice,
			knowledgeCost: 5,
			manaCost: 5,
			target: TargetType.Enemy,
			damage: new Value("4d8")
		);

		return s;
	}

	public static Spell FrozenPrison()
	{
		var s = new Spell(
			name: "Frozen Prison",
			school: SpellSchool.Ice,
			knowledgeCost: 6,
			manaCost: 4,
			target: TargetType.Enemy
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Freeze,
			2
		);

		return s;
	}

	public static Spell AbsoluteZero()
	{
		var s = new Spell(
			name: "Absolute Zero",
			school: SpellSchool.Ice,
			knowledgeCost: 9,
			manaCost: 8,
			target: TargetType.Enemy,
			damage: new Value("6d8")
		);
		return s;
	}


	// ===== LIGHTNING =====

	public static Spell LightningBolt()
	{
		var s = new Spell(
			name: "Lightning Bolt",
			school: SpellSchool.Lightning,
			knowledgeCost: 2,
			manaCost: 1,
			target: TargetType.Enemy,
			damage: new Value("2d8")
		);
		return s;
	}
	public static Spell StaticField()
	{
		var s = new Spell(
			name: "Static Field",
			school: SpellSchool.Lightning,
			knowledgeCost: 2,
			manaCost: 2,
			target: TargetType.AllEnemies,
			damage: new Value("1d6")
		);
		return s;
	}

	public static Spell Thunderstorm()
	{
		var s = new Spell(
			name: "Thunderstorm",
			school: SpellSchool.Lightning,
			knowledgeCost: 12,
			manaCost: 14,
			target: TargetType.AllEnemies,
			damage: new Value("4d6")
		);
		return s;
	}

	public static Spell StaticSurge()
	{
		var s = new Spell(
			name: "Static Surge",
			school: SpellSchool.Lightning,
			knowledgeCost: 2,
			manaCost: 2,
			target: TargetType.Enemy,
			damage: new Value("1d6")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Shock,
			2
		);

		return s;
	}

	public static Spell ChainLightning()
	{
		var s = new Spell(
			name: "Chain Lightning",
			school: SpellSchool.Lightning,
			knowledgeCost: 5,
			manaCost: 8,
			target: TargetType.Cleave,
			damage: new Value("2d8"),
			splashDamage: new Value("50%")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Shock,
			2
		);

		return s;
	}

	public static Spell StormStrike()
	{
		var s = new Spell(
			name: "Storm Strike",
			school: SpellSchool.Lightning,
			knowledgeCost: 3,
			manaCost: 3,
			target: TargetType.Enemy,
			damage: new Value("2d6")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Shock,
			2
		);

		return s;
	}

	public static Spell LightningSurge()
	{
		var s = new Spell(
			name: "Lightning Surge",
			school: SpellSchool.Lightning,
			knowledgeCost: 5,
			manaCost: 5,
			target: TargetType.Enemy,
			damage: new Value("4d8")
		);

		return s;
	}

	public static Spell Thunderbolt()
	{
		var s = new Spell(
			name: "Thunderbolt",
			school: SpellSchool.Lightning,
			knowledgeCost: 9,
			manaCost: 10,
			target: TargetType.Enemy,
			damage: new Value("5d12")
		);

		return s;
	}

	public static Spell StaticCharge()
	{
		var s = new Spell(
			name: "Static Charge",
			school: SpellSchool.Lightning,
			knowledgeCost: 5,
			manaCost: 0,
			target: TargetType.Self,
			manaGain: new Value("4d4")
		);

		return s;
	}

	public static Spell StormChannel()
	{
		var s = new Spell(
			name: "Storm Channel",
			school: SpellSchool.Lightning,
			knowledgeCost: 13,
			manaCost: 0,
			target: TargetType.Self,
			manaGain: new Value("6d12"),
			oncePerBattle: true
		);

		return s;
	}


	// ===== EARTH =====

	public static Spell StoneCannon()
	{
		var s = new Spell(
			name: "Stone Cannon",
			school: SpellSchool.Earth,
			knowledgeCost: 4,
			manaCost: 2,
			target: TargetType.Enemy,
			damage: new Value("1d8")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Brittle,
			2
		);

		return s;
	}

	public static Spell Earthquake()
	{
		var s = new Spell(
			name: "Earthquake",
			school: SpellSchool.Earth,
			knowledgeCost: 12,
			manaCost: 15,
			target: TargetType.AllEnemies,
			damage: new Value("3d6")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Brittle,
			1
		);

		return s;
	}

	public static Spell GravityWell()
	{
		var s = new Spell(
			name: "Gravity Well",
			school: SpellSchool.Earth,
			knowledgeCost: 10,
			manaCost: 14,
			target: TargetType.AllEnemies,
			damage: new Value("3d8")
		);

		return s;
	}

	public static Spell Stalagmite()
	{
		var s = new Spell(
			name: "Stalagmite",
			school: SpellSchool.Earth,
			knowledgeCost: 6,
			manaCost: 6,
			target: TargetType.Enemy,
			damage: new Value("4d8")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Brittle,
			1
		);

		return s;
	}

	public static Spell Singularity()
	{
		var s = new Spell(
			name: "Singularity",
			school: SpellSchool.Earth,
			knowledgeCost: 13,
			manaCost: 14,
			target: TargetType.Enemy,
			damage: new Value("5d12")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Brittle,
			3
		);

		return s;
	}

	public static Spell RockFall()
	{
		var s = new Spell(
			name: "Rock Fall",
			school: SpellSchool.Earth,
			knowledgeCost: 2,
			manaCost: 3,
			target: TargetType.Cleave,
			damage: new Value("2d6"),
			splashDamage: new Value("50%")
		);

		return s;
	}


	// ===== HOLY =====

	public static Spell HealingLight()
	{
		var s = new Spell(
			name: "Healing Light",
			school: SpellSchool.Holy,
			knowledgeCost: 3,
			manaCost: 1,
			target: TargetType.Ally,
			heal: new Value("2d8")
		);
		return s;
	}

	public static Spell GreaterHeal()
	{
		var s = new Spell(
			name: "Greater Heal",
			school: SpellSchool.Holy,
			knowledgeCost: 4,
			manaCost: 4,
			target: TargetType.Self,
			heal: new Value("3d10")
		);
		return s;
	}

	public static Spell RadiantFlash()
	{
		var s = new Spell(
			name: "Radiant Flash",
			school: SpellSchool.Holy,
			knowledgeCost: 4,
			manaCost: 2,
			target: TargetType.AllEnemies
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Blinded,
			1
		);

		return s;
	}

	public static Spell Smite()
	{
		var s = new Spell(
			name: "Smite",
			school: SpellSchool.Holy,
			knowledgeCost: 3,
			manaCost: 2,
			target: TargetType.Enemy,
			damage: new Value("2d10")
		);

		return s;
	}

	public static Spell SpearOfJudgment()
	{
		var s = new Spell(
			name: "Spear Of Judgment",
			school: SpellSchool.Holy,
			knowledgeCost: 10,
			manaCost: 12,
			target: TargetType.Enemy,
			damage: new Value("8d8")
		);

		return s;
	}

	public static Spell RadiantStorm()
	{
		var s = new Spell(
			name: "Radiant Storm",
			school: SpellSchool.Holy,
			knowledgeCost: 8,
			manaCost: 11,
			target: TargetType.AllEnemies,
			damage: new Value("2d6")
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Blinded,
			1
		);

		return s;
	}

	public static Spell SacredBlade()
	{
		var s = new Spell(
			name: "Sacred Blade",
			school: SpellSchool.Holy,
			knowledgeCost: 6,
			manaCost: 5,
			target: TargetType.Enemy,
			damage: new Value("4d8")
		);

		return s;
	}

	public static Spell DivinePrayer()
	{
		var s = new Spell(
			name: "Divine Prayer",
			school: SpellSchool.Holy,
			knowledgeCost: 7,
			manaCost: 0,
			target: TargetType.Self,
			manaGain: new Value("4d12"),
			oncePerBattle: true
		);

		return s;
	}


	// ===== BLOOD =====

	public static Spell DrainLife()
	{
		var s = new Spell(
			name: "Drain Life",
			school: SpellSchool.Blood,
			knowledgeCost: 4,
			manaCost: 1,
			target: TargetType.Enemy,
			damage: new Value("2d6"),
			lifesteal: new Value("50%")
		);
		return s;
	}

	public static Spell BloodSpike()
	{
		var s = new Spell(
			name: "Blood Spike",
			school: SpellSchool.Blood,
			knowledgeCost: 3,
			manaCost: 2,
			target: TargetType.Enemy,
			damage: new Value("2d10")
		);

		return s;
	}

	public static Spell Wither()
	{
		var s = new Spell(
			name: "Wither",
			school: SpellSchool.Blood,
			knowledgeCost: 2,
			manaCost: 1,
			target: TargetType.Enemy
		);

		s.StatusEffect = new StatusEffect(
			StatusEffectType.Weak,
			3
		);

		return s;
	}

	public static Spell SoulRend()
	{
		var s = new Spell(
			name: "Soul Rend",
			school: SpellSchool.Blood,
			knowledgeCost: 5,
			manaCost: 6,
			target: TargetType.Enemy,
			damage: new Value("4d6"),
			lifesteal: new Value("50%")
		);

		return s;
	}

	public static Spell BloodRitual()
	{
		var s = new Spell(
			name: "Blood Ritual",
			school: SpellSchool.Blood,
			knowledgeCost: 5,
			manaCost: 0,
			target: TargetType.Self
		);

		s.PlayerEffect = new PlayerEffect(
			school: SpellSchool.Blood,
			modifier: 2,
			duration: null,
			consumeOnUse: true
		);

		return s;
	}

	public static Spell DarkHarvest()
	{
		var s = new Spell(
			name: "Dark Harvest",
			school: SpellSchool.Blood,
			knowledgeCost: 9,
			manaCost: 11,
			target: TargetType.AllEnemies,
			damage: new Value("3d4"),
			lifesteal: new Value("50%")
		);

		return s;
	}

	public static Spell DeathSpike()
	{
		var s = new Spell(
			name: "Death Spike",
			school: SpellSchool.Blood,
			knowledgeCost: 13,
			manaCost: 13,
			target: TargetType.Enemy,
			damage: new Value("7d8"),
			lifesteal: new Value("25%")
		);

		return s;
	}

	public static Spell BloodTap()
	{
		var s = new Spell(
			name: "Blood Tap",
			school: SpellSchool.Blood,
			knowledgeCost: 6,
			manaCost: 0,
			target: TargetType.Self,
			manaGain: new Value("3d8"),
			damage: new Value(5)
		);
		return s;
	}

	public static Spell CrimsonOffering()
	{
		var s = new Spell(
			name: "Crimson Offering",
			school: SpellSchool.Blood,
			knowledgeCost: 7,
			manaCost: 0,
			target: TargetType.Self,
			manaGain: new Value("7d8"),
			damage: new Value(10),
			oncePerBattle: true
		);
		return s;
	}

	// ===== SHIELDS =====

	public static Spell ManaShield()
	{
		var s = new Spell(
			name: "Mana Shield",
			school: SpellSchool.Arcane,
			knowledgeCost: 2,
			manaCost: 3,
			target: TargetType.Self,
			shield: new Value("2d8")
		);
		return s;
	}

	public static Spell PrismaticBarrier()
	{
		var s = new Spell(
			name: "Prismatic Barrier",
			school: SpellSchool.Arcane,
			knowledgeCost: 9,
			manaCost: 9,
			target: TargetType.Self,
			shield: new Value("4d8")
		);
		return s;
	}

	public static Spell DivineWard()
	{
		var s = new Spell(
			name: "Divine Ward",
			school: SpellSchool.Holy,
			knowledgeCost: 3,
			manaCost: 4,
			target: TargetType.Self,
			shield: new Value("3d6")
		);
		return s;
	}

	public static Spell AegisOfFaith()
	{
		var s = new Spell(
			name: "Aegis of Faith",
			school: SpellSchool.Holy,
			knowledgeCost: 10,
			manaCost: 8,
			target: TargetType.Self,
			shield: new Value("3d12")
		);
		return s;
	}

	public static Spell StoneSkin()
	{
		var s = new Spell(
			name: "Stone Skin",
			school: SpellSchool.Earth,
			knowledgeCost: 2,
			manaCost: 3,
			target: TargetType.Self,
			shield: new Value("2d8")
		);
		return s;
	}

	public static Spell MountainForm()
	{
		var s = new Spell(
			name: "Mountain Form",
			school: SpellSchool.Earth,
			knowledgeCost: 10,
			manaCost: 9,
			target: TargetType.Self,
			shield: new Value("4d10")
		);
		return s;
	}

	public static Spell FrostBarrier()
	{
		var s = new Spell(
			name: "Frost Barrier",
			school: SpellSchool.Ice,
			knowledgeCost: 3,
			manaCost: 3,
			target: TargetType.Self,
			shield: new Value("4d4")
		);
		return s;
	}

	public static Spell GlacialBulwark()
	{
		var s = new Spell(
			name: "Glacial Bulwark",
			school: SpellSchool.Ice,
			knowledgeCost: 10,
			manaCost: 8,
			target: TargetType.Self,
			shield: new Value("3d12")
		);
		return s;
	}
}
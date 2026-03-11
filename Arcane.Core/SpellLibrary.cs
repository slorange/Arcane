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

	public static Spell MagicMissile() => new Spell(
		name: "Magic Missile",
		school: SpellSchool.Arcane,
		knowledgeCost: 0,
		manaCost: 1,
		target: TargetType.Enemy,
		damage: new Value("1d6")
	);

	public static Spell Firebolt() => new Spell(
		name: "Firebolt",
		school: SpellSchool.Fire,
		knowledgeCost: 2,
		manaCost: 1,
		target: TargetType.Enemy,
		damage: new Value("1d8")
	);

	public static Spell IceShard() => new Spell(
		name: "Ice Shard",
		school: SpellSchool.Ice,
		knowledgeCost: 3,
		manaCost: 2,
		target: TargetType.Enemy,
		damage: new Value("2d4"),
		statusEffect: new StatusEffect(StatusEffectType.Freeze, 1, default)
	);

	public static Spell CleaveStrike() => new Spell(
		name: "Cleave Strike",
		school: SpellSchool.Earth,
		knowledgeCost: 3,
		manaCost: 2,
		target: TargetType.Cleave,
		damage: new Value("1d8"),
		splashDamage: new Value("25%")
	);

	public static Spell Firestorm() => new Spell(
		name: "Firestorm",
		school: SpellSchool.Fire,
		knowledgeCost: 5,
		manaCost: 3,
		target: TargetType.AllEnemies,
		damage: new Value("2d6"),
		statusEffect: new StatusEffect(StatusEffectType.Burn, 2, new Value("1d4"))
	);

	public static Spell HealingLight() => new Spell(
		name: "Healing Light",
		school: SpellSchool.Holy,
		knowledgeCost: 3,
		manaCost: 2,
		target: TargetType.Ally,
		heal: new Value("2d6")
	);

	public static Spell LightningBolt() => new Spell(
		name: "Lightning Bolt",
		school: SpellSchool.Lightning,
		knowledgeCost: 3,
		manaCost: 2,
		target: TargetType.Enemy,
		damage: new Value("2d8")
	);

	public static Spell ArcaneBarrage() => new Spell(
		name: "Arcane Barrage",
		school: SpellSchool.Arcane,
		knowledgeCost: 4,
		manaCost: 3,
		target: TargetType.Enemy,
		damage: new Value("3d6")
	);

	public static Spell MeteorShard() => new Spell(
		name: "Meteor Shard",
		school: SpellSchool.Fire,
		knowledgeCost: 4,
		manaCost: 3,
		target: TargetType.Cleave,
		damage: new Value("2d6"),
		splashDamage: new Value("50%")
	);

	public static Spell FrostNova() => new Spell(
		name: "Frost Nova",
		school: SpellSchool.Ice,
		knowledgeCost: 5,
		manaCost: 3,
		target: TargetType.AllEnemies,
		damage: new Value("1d6"),
		statusEffect: new StatusEffect(StatusEffectType.Freeze, 1)
	);

	public static Spell BurningCurse() => new Spell(
		name: "Burning Curse",
		school: SpellSchool.Fire,
		knowledgeCost: 3,
		manaCost: 2,
		target: TargetType.Enemy,
		damage: new Value("1d4"),
		statusEffect: new StatusEffect(StatusEffectType.Burn, 3, new Value("1d6"))
	);

	public static Spell DrainLife() => new Spell(
		name: "Drain Life",
		school: SpellSchool.Blood,
		knowledgeCost: 4,
		manaCost: 2,
		target: TargetType.Enemy,
		damage: new Value("2d6"),
		lifesteal: new Value("50%")
	);

	public static Spell GreaterHeal() => new Spell(
		name: "Greater Heal",
		school: SpellSchool.Holy,
		knowledgeCost: 4,
		manaCost: 3,
		target: TargetType.Self,
		heal: new Value("3d6")
	);

	public static Spell ManaSurge() => new Spell(
		name: "Mana Surge",
		school: SpellSchool.Arcane,
		knowledgeCost: 3,
		manaCost: 0,
		target: TargetType.Self,
		manaGain: new Value("2d4")
	);

	public static Spell LeylineTap() => new Spell(
		name: "Leyline Tap",
		school: SpellSchool.Arcane,
		knowledgeCost: 2,
		manaCost: 0,
		target: TargetType.Self,
		manaGain: new Value("1d6"),
		oncePerBattle: true
	);

	public static Spell ChaosBolt() => new Spell(
		name: "Chaos Bolt",
		school: SpellSchool.Arcane,
		knowledgeCost: 3,
		manaCost: 2,
		target: TargetType.Enemy,
		damage: new Value("1d20")
	);

	public static Spell Thunderstorm() => new Spell(
		name: "Thunderstorm",
		school: SpellSchool.Lightning,
		knowledgeCost: 6,
		manaCost: 4,
		target: TargetType.AllEnemies,
		damage: new Value("3d6")
	);
}
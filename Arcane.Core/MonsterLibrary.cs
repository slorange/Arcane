using Arcane.Core.Cards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcane.Core;

public static class MonsterLibrary
{
	private static readonly List<Monster> _allMonsters = LoadAll();

	public static IReadOnlyList<Monster> AllMonsters() => _allMonsters;

	private static List<Monster> LoadAll()
	{
		var methods = typeof(MonsterLibrary)
			.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
			.Where(m => m.ReturnType == typeof(Monster));

		return methods
			.Select(m => (Monster)m.Invoke(null, null)!)
			.ToList();
	}


	public static void ApplyThemeDefaults(Monster m)
	{
		switch (m.Theme)
		{
			case MonsterTheme.Beasts:
				break;

			case MonsterTheme.Goblins:
				m.Resistances.Add(SpellSchool.Lightning);
				break;

			case MonsterTheme.Ogres:
				m.Resistances.Add(SpellSchool.Ice);
				m.Resistances.Add(SpellSchool.Earth);
				break;

			case MonsterTheme.Undead:
				m.Weaknesses.Add(SpellSchool.Holy);
				m.Weaknesses.Add(SpellSchool.Fire);
				m.Resistances.Add(SpellSchool.Blood);
				m.Resistances.Add(SpellSchool.Ice);
				m.StatusImmunities.Add(StatusEffectType.Weak);
				break;

			case MonsterTheme.Elemental:
				m.Immunities.Add(SpellSchool.Blood);
				m.Immunities.Add(SpellSchool.Holy);
				m.StatusImmunities.Add(StatusEffectType.Blinded);
				break;

			case MonsterTheme.Warlocks:
				m.Weaknesses.Add(SpellSchool.Arcane);
				m.Resistances.Add(SpellSchool.Fire);
				m.Resistances.Add(SpellSchool.Blood);
				break;

			case MonsterTheme.Demons:
				m.Weaknesses.Add(SpellSchool.Ice);
				m.Weaknesses.Add(SpellSchool.Holy);
				m.Immunities.Add(SpellSchool.Fire);
				m.StatusImmunities.Add(StatusEffectType.Freeze);
				m.StatusImmunities.Add(StatusEffectType.Burn);
				break;

			case MonsterTheme.Vampires:
				m.Weaknesses.Add(SpellSchool.Holy);
				m.Resistances.Add(SpellSchool.Blood);
				break;
		}
	}


	// ===== BEASTS =====

	public static Monster Wolf()
	{
		var m = new Monster(
			name: "Wolf",
			health: 18,
			attackDamage: new Value("1d6"),
			threat: 2,
			theme: MonsterTheme.Beasts
		);
		return m;
	}

	public static Monster GiantSpider()
	{
		var m = new Monster(
			name: "Giant Spider",
			health: 20,
			attackDamage: new Value("1d6"),
			threat: 2,
			theme: MonsterTheme.Beasts
		);

		m.Weaknesses.Add(SpellSchool.Fire);

		return m;
	}

	public static Monster Werewolf()
	{
		var m = new Monster(
			name: "Werewolf",
			health: 35,
			attackDamage: new Value("1d8"),
			threat: 4,
			theme: MonsterTheme.Beasts
		);

		m.Weaknesses.Add(SpellSchool.Holy);

		return m;
	}

	public static Monster Bear()
	{
		var m = new Monster(
			name: "Bear",
			health: 40,
			attackDamage: new Value("2d6"),
			threat: 5,
			theme: MonsterTheme.Beasts
		);
		return m;
	}

	public static Monster GiantBoar()
	{
		var m = new Monster(
			name: "Giant Boar",
			health: 32,
			attackDamage: new Value("1d10"),
			threat: 4,
			theme: MonsterTheme.Beasts
		);

		m.Resistances.Add(SpellSchool.Ice);

		return m;
	}

	public static Monster Chimera()
	{
		var m = new Monster(
			name: "Chimera",
			health: 60,
			attackDamage: new Value("2d8"),
			threat: 8,
			theme: MonsterTheme.Beasts
		);
		return m;
	}

	public static Monster Wyvern()
	{
		var m = new Monster(
			name: "Wyvern",
			health: 70,
			attackDamage: new Value("2d10"),
			threat: 9,
			theme: MonsterTheme.Beasts
		);

		m.Resistances.Add(SpellSchool.Lightning);
		m.Weaknesses.Add(SpellSchool.Ice);

		return m;
	}

	public static Monster Unicorn()
	{
		var m = new Monster(
			name: "Unicorn",
			health: 50,
			attackDamage: new Value("2d6"),
			threat: 7,
			theme: MonsterTheme.Beasts
		);

		m.Resistances.Add(SpellSchool.Blood);

		return m;
	}

	public static Monster Dragon()
	{
		var m = new Monster(
			name: "Dragon",
			health: 120,
			attackDamage: new Value("3d10"),
			threat: 12,
			theme: MonsterTheme.Beasts
		);

		m.Resistances.Add(SpellSchool.Lightning);
		m.Resistances.Add(SpellSchool.Fire);
		m.Weaknesses.Add(SpellSchool.Ice);
		m.StatusImmunities.Add(StatusEffectType.Freeze);
		m.StatusImmunities.Add(StatusEffectType.Burn);

		return m;
	}

	// ===== GOBLINS =====

	public static Monster Goblin()
	{
		var m = new Monster(
			name: "Goblin",
			health: 15,
			attackDamage: new Value("1d4"),
			threat: 1,
			theme: MonsterTheme.Goblins
		);
		return m;
	}

	public static Monster GoblinRaider()
	{
		var m = new Monster(
			name: "Goblin Raider",
			health: 18,
			attackDamage: new Value("1d6"),
			threat: 2,
			theme: MonsterTheme.Goblins
		);
		return m;
	}

	public static Monster GoblinSneak()
	{
		var m = new Monster(
			name: "Goblin Sneak",
			health: 14,
			attackDamage: new Value("1d6"),
			threat: 2,
			theme: MonsterTheme.Goblins
		);
		return m;
	}

	public static Monster GoblinShamanka()
	{
		var m = new Monster(
			name: "Goblin Shamanka",
			health: 22,
			attackDamage: new Value("1d8"),
			threat: 3,
			theme: MonsterTheme.Goblins
		);
		return m;
	}

	public static Monster GoblinBrute()
	{
		var m = new Monster(
			name: "Goblin Brute",
			health: 30,
			attackDamage: new Value("1d10"),
			threat: 4,
			theme: MonsterTheme.Goblins
		);
		return m;
	}

	public static Monster GoblinWarcaller()
	{
		var m = new Monster(
			name: "Goblin Warcaller",
			health: 26,
			attackDamage: new Value("1d8"),
			threat: 4,
			theme: MonsterTheme.Goblins
		);
		return m;
	}

	public static Monster GoblinKing()
	{
		var m = new Monster(
			name: "Goblin King",
			health: 60,
			attackDamage: new Value("2d10"),
			threat: 7,
			theme: MonsterTheme.Goblins
		);
		return m;
	}

	// ===== OGRES =====

	public static Monster Ogre()
	{
		var m = new Monster(
			name: "Ogre",
			health: 45,
			attackDamage: new Value("2d6"),
			threat: 5,
			theme: MonsterTheme.Ogres
		);
		return m;
	}

	public static Monster Troll()
	{
		var m = new Monster(
			name: "Troll",
			health: 60,
			attackDamage: new Value("2d8"),
			threat: 6,
			theme: MonsterTheme.Ogres
		);

		m.Weaknesses.Add(SpellSchool.Fire);

		return m;
	}

	public static Monster Cyclops()
	{
		var m = new Monster(
			name: "Cyclops",
			health: 75,
			attackDamage: new Value("2d10"),
			threat: 8,
			theme: MonsterTheme.Ogres
		);
		return m;
	}

	public static Monster HillGiant()
	{
		var m = new Monster(
			name: "Hill Giant",
			health: 90,
			attackDamage: new Value("3d8"),
			threat: 9,
			theme: MonsterTheme.Ogres
		);
		return m;
	}

	// ===== UNDEAD =====

	public static Monster Skeleton()
	{
		var m = new Monster(
			name: "Skeleton",
			health: 20,
			attackDamage: new Value("1d6"),
			threat: 2,
			theme: MonsterTheme.Undead
		);
		return m;
	}

	public static Monster Zombie()
	{
		var m = new Monster(
			name: "Zombie",
			health: 25,
			attackDamage: new Value("1d6"),
			threat: 2,
			theme: MonsterTheme.Undead
		);
		return m;
	}

	public static Monster Ghoul()
	{
		var m = new Monster(
			name: "Ghoul",
			health: 30,
			attackDamage: new Value("1d8"),
			threat: 3,
			theme: MonsterTheme.Undead
		);
		return m;
	}

	public static Monster RestlessSpirit()
	{
		var m = new Monster(
			name: "Restless Spirit",
			health: 28,
			attackDamage: new Value("1d8"),
			threat: 3,
			theme: MonsterTheme.Undead
		);

		m.Immunities.Add(SpellSchool.Ice);
		m.StatusImmunities.Add(StatusEffectType.Freeze);

		return m;
	}

	public static Monster Spectre()
	{
		var m = new Monster(
			name: "Spectre",
			health: 40,
			attackDamage: new Value("2d6"),
			threat: 5,
			theme: MonsterTheme.Undead
		);

		m.Immunities.Add(SpellSchool.Ice);
		m.StatusImmunities.Add(StatusEffectType.Freeze);

		return m;
	}

	public static Monster BoneKnight()
	{
		var m = new Monster(
			name: "Bone Knight",
			health: 55,
			attackDamage: new Value("2d8"),
			threat: 6,
			theme: MonsterTheme.Undead
		);
		return m;
	}

	public static Monster Lich()
	{
		var m = new Monster(
			name: "Lich",
			health: 100,
			attackDamage: new Value("3d8"),
			threat: 10,
			theme: MonsterTheme.Undead
		);
		return m;
	}

	// ===== ELEMENTALS =====

	public static Monster FireElemental()
	{
		var m = new Monster(
			name: "Fire Elemental",
			health: 40,
			attackDamage: new Value("2d6"),
			threat: 5,
			theme: MonsterTheme.Elemental
		);

		m.Immunities.Add(SpellSchool.Fire);
		m.Weaknesses.Add(SpellSchool.Ice);
		m.StatusImmunities.Add(StatusEffectType.Burn);
		m.StatusImmunities.Add(StatusEffectType.Freeze);

		return m;
	}

	public static Monster IceElemental()
	{
		var m = new Monster(
			name: "Ice Elemental",
			health: 40,
			attackDamage: new Value("2d6"),
			threat: 5,
			theme: MonsterTheme.Elemental
		);

		m.Immunities.Add(SpellSchool.Ice);
		m.Weaknesses.Add(SpellSchool.Fire);
		m.StatusImmunities.Add(StatusEffectType.Freeze);

		return m;
	}

	public static Monster LightningElemental()
	{
		var m = new Monster(
			name: "Lightning Elemental",
			health: 35,
			attackDamage: new Value("2d8"),
			threat: 6,
			theme: MonsterTheme.Elemental
		);

		m.Weaknesses.Add(SpellSchool.Earth);
		m.Immunities.Add(SpellSchool.Lightning);
		m.StatusImmunities.Add(StatusEffectType.Shock);

		return m;
	}

	public static Monster StormElemental()
	{
		var m = new Monster(
			name: "Storm Elemental",
			health: 55,
			attackDamage: new Value("2d10"),
			threat: 7,
			theme: MonsterTheme.Elemental
		);

		m.Resistances.Add(SpellSchool.Lightning);
		m.Resistances.Add(SpellSchool.Earth);

		return m;
	}

	public static Monster PrimalElemental()
	{
		var m = new Monster(
			name: "Primal Elemental",
			health: 85,
			attackDamage: new Value("3d8"),
			threat: 9,
			theme: MonsterTheme.Elemental
		);

		m.Resistances.Add(SpellSchool.Fire);
		m.Resistances.Add(SpellSchool.Ice);
		m.Resistances.Add(SpellSchool.Lightning);
		m.StatusImmunities.Add(StatusEffectType.Freeze);
		m.StatusImmunities.Add(StatusEffectType.Burn);
		m.StatusImmunities.Add(StatusEffectType.Shock);

		return m;
	}

	// ===== WARLOCKS =====

	public static Monster CultMage()
	{
		var m = new Monster(
			name: "Cult Mage",
			health: 22,
			attackDamage: new Value("1d8"),
			threat: 3,
			theme: MonsterTheme.Warlocks
		);
		return m;
	}

	public static Monster ApprenticeWarlock()
	{
		var m = new Monster(
			name: "Apprentice Warlock",
			health: 24,
			attackDamage: new Value("1d8"),
			threat: 3,
			theme: MonsterTheme.Warlocks
		);
		return m;
	}

	public static Monster DarkSorcerer()
	{
		var m = new Monster(
			name: "Dark Sorcerer",
			health: 32,
			attackDamage: new Value("2d6"),
			threat: 4,
			theme: MonsterTheme.Warlocks
		);
		return m;
	}

	public static Monster BloodMage()
	{
		var m = new Monster(
			name: "Blood Mage",
			health: 35,
			attackDamage: new Value("2d6"),
			threat: 5,
			theme: MonsterTheme.Warlocks
		);
		return m;
	}

	public static Monster ArchWarlock()
	{
		var m = new Monster(
			name: "Arch Warlock",
			health: 60,
			attackDamage: new Value("2d10"),
			threat: 7,
			theme: MonsterTheme.Warlocks
		);
		return m;
	}

	// ===== DEMONS =====

	public static Monster Imp()
	{
		var m = new Monster(
			name: "Imp",
			health: 16,
			attackDamage: new Value("1d6"),
			threat: 2,
			theme: MonsterTheme.Demons
		);
		return m;
	}

	public static Monster LesserDemon()
	{
		var m = new Monster(
			name: "Lesser Demon",
			health: 28,
			attackDamage: new Value("1d10"),
			threat: 3,
			theme: MonsterTheme.Demons
		);
		return m;
	}

	public static Monster Hellhound()
	{
		var m = new Monster(
			name: "Hellhound",
			health: 36,
			attackDamage: new Value("2d6"),
			threat: 4,
			theme: MonsterTheme.Demons
		);
		return m;
	}

	public static Monster Fiend()
	{
		var m = new Monster(
			name: "Fiend",
			health: 50,
			attackDamage: new Value("2d8"),
			threat: 6,
			theme: MonsterTheme.Demons
		);
		return m;
	}

	public static Monster AbyssalKnight()
	{
		var m = new Monster(
			name: "Abyssal Knight",
			health: 70,
			attackDamage: new Value("2d10"),
			threat: 8,
			theme: MonsterTheme.Demons
		);
		return m;
	}

	public static Monster Balrog()
	{
		var m = new Monster(
			name: "Balrog",
			health: 140,
			attackDamage: new Value("3d12"),
			threat: 12,
			theme: MonsterTheme.Demons
		);

		m.Weaknesses.Add(SpellSchool.Holy);

		return m;
	}

	// ===== VAMPIRES =====

	public static Monster Thrall()
	{
		var m = new Monster(
			name: "Thrall",
			health: 20,
			attackDamage: new Value("1d6"),
			threat: 2,
			theme: MonsterTheme.Vampires
		);
		return m;
	}

	public static Monster LesserVampire()
	{
		var m = new Monster(
			name: "Lesser Vampire",
			health: 35,
			attackDamage: new Value("1d10"),
			threat: 4,
			theme: MonsterTheme.Vampires
		);
		return m;
	}

	public static Monster VampireStalker()
	{
		var m = new Monster(
			name: "Vampire Stalker",
			health: 40,
			attackDamage: new Value("2d6"),
			threat: 5,
			theme: MonsterTheme.Vampires
		);
		return m;
	}

	public static Monster BloodKnight()
	{
		var m = new Monster(
			name: "Blood Knight",
			health: 55,
			attackDamage: new Value("2d8"),
			threat: 6,
			theme: MonsterTheme.Vampires
		);
		return m;
	}

	public static Monster ElderVampire()
	{
		var m = new Monster(
			name: "Elder Vampire",
			health: 85,
			attackDamage: new Value("2d10"),
			threat: 8,
			theme: MonsterTheme.Vampires
		);
		return m;
	}

	public static Monster VampireLord()
	{
		var m = new Monster(
			name: "Vampire Lord",
			health: 120,
			attackDamage: new Value("3d8"),
			threat: 10,
			theme: MonsterTheme.Vampires
		);
		return m;
	}
}
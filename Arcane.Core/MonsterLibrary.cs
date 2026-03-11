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


	// ===== BEASTS =====

	public static Monster Wolf() => new Monster(
		name: "Wolf",
		health: 18,
		attackDamage: new Value("1d6"),
		threat: 2,
		theme: MonsterTheme.Beasts
	);

	public static Monster GiantSpider() => new Monster(
		name: "Giant Spider",
		health: 20,
		attackDamage: new Value("1d6"),
		threat: 2,
		theme: MonsterTheme.Beasts
	);

	public static Monster Werewolf() => new Monster(
		name: "Werewolf",
		health: 35,
		attackDamage: new Value("1d8"),
		threat: 4,
		theme: MonsterTheme.Beasts
	);

	public static Monster Bear() => new Monster(
		name: "Bear",
		health: 40,
		attackDamage: new Value("2d6"),
		threat: 5,
		theme: MonsterTheme.Beasts
	);

	public static Monster GiantBoar() => new Monster(
		name: "Giant Boar",
		health: 32,
		attackDamage: new Value("1d10"),
		threat: 4,
		theme: MonsterTheme.Beasts
	);

	public static Monster Chimera() => new Monster(
		name: "Chimera",
		health: 60,
		attackDamage: new Value("2d8"),
		threat: 8,
		theme: MonsterTheme.Beasts
	);

	public static Monster Wyvern() => new Monster(
		name: "Wyvern",
		health: 70,
		attackDamage: new Value("2d10"),
		threat: 9,
		theme: MonsterTheme.Beasts
	);

	public static Monster Unicorn() => new Monster(
		name: "Unicorn",
		health: 50,
		attackDamage: new Value("2d6"),
		threat: 7,
		theme: MonsterTheme.Beasts
	);

	public static Monster Dragon() => new Monster(
		name: "Dragon",
		health: 120,
		attackDamage: new Value("3d10"),
		threat: 12,
		theme: MonsterTheme.Beasts
	);


	// ===== GOBLINS =====

	public static Monster Goblin() => new Monster(
		name: "Goblin",
		health: 15,
		attackDamage: new Value("1d4"),
		threat: 1,
		theme: MonsterTheme.Goblins
	);

	public static Monster GoblinRaider() => new Monster(
		name: "Goblin Raider",
		health: 18,
		attackDamage: new Value("1d6"),
		threat: 2,
		theme: MonsterTheme.Goblins
	);

	public static Monster GoblinSneak() => new Monster(
		name: "Goblin Sneak",
		health: 14,
		attackDamage: new Value("1d6"),
		threat: 2,
		theme: MonsterTheme.Goblins
	);

	public static Monster GoblinShamanka() => new Monster(
		name: "Goblin Shamanka",
		health: 22,
		attackDamage: new Value("1d8"),
		threat: 3,
		theme: MonsterTheme.Goblins
	);

	public static Monster GoblinBrute() => new Monster(
		name: "Goblin Brute",
		health: 30,
		attackDamage: new Value("1d10"),
		threat: 4,
		theme: MonsterTheme.Goblins
	);

	public static Monster GoblinWarcaller() => new Monster(
		name: "Goblin Warcaller",
		health: 26,
		attackDamage: new Value("1d8"),
		threat: 4,
		theme: MonsterTheme.Goblins
	);

	public static Monster GoblinKing() => new Monster(
		name: "Goblin King",
		health: 60,
		attackDamage: new Value("2d10"),
		threat: 7,
		theme: MonsterTheme.Goblins
	);


	// ===== OGRES =====

	public static Monster Ogre() => new Monster(
		name: "Ogre",
		health: 45,
		attackDamage: new Value("2d6"),
		threat: 5,
		theme: MonsterTheme.Ogres
	);

	public static Monster Troll() => new Monster(
		name: "Troll",
		health: 60,
		attackDamage: new Value("2d8"),
		threat: 6,
		theme: MonsterTheme.Ogres
	);

	public static Monster Cyclops() => new Monster(
		name: "Cyclops",
		health: 75,
		attackDamage: new Value("2d10"),
		threat: 8,
		theme: MonsterTheme.Ogres
	);

	public static Monster HillGiant() => new Monster(
		name: "Hill Giant",
		health: 90,
		attackDamage: new Value("3d8"),
		threat: 9,
		theme: MonsterTheme.Ogres
	);


	// ===== UNDEAD =====

	public static Monster Skeleton() => new Monster(
		name: "Skeleton",
		health: 20,
		attackDamage: new Value("1d6"),
		threat: 2,
		theme: MonsterTheme.Undead
	);

	public static Monster Zombie() => new Monster(
		name: "Zombie",
		health: 25,
		attackDamage: new Value("1d6"),
		threat: 2,
		theme: MonsterTheme.Undead
	);

	public static Monster Ghoul() => new Monster(
		name: "Ghoul",
		health: 30,
		attackDamage: new Value("1d8"),
		threat: 3,
		theme: MonsterTheme.Undead
	);

	public static Monster RestlessSpirit() => new Monster(
		name: "Restless Spirit",
		health: 28,
		attackDamage: new Value("1d8"),
		threat: 3,
		theme: MonsterTheme.Undead
	);

	public static Monster Spectre() => new Monster(
		name: "Spectre",
		health: 40,
		attackDamage: new Value("2d6"),
		threat: 5,
		theme: MonsterTheme.Undead
	);

	public static Monster BoneKnight() => new Monster(
		name: "Bone Knight",
		health: 55,
		attackDamage: new Value("2d8"),
		threat: 6,
		theme: MonsterTheme.Undead
	);

	public static Monster Lich() => new Monster(
		name: "Lich",
		health: 100,
		attackDamage: new Value("3d8"),
		threat: 10,
		theme: MonsterTheme.Undead
	);


	// ===== ELEMENTALS =====

	public static Monster FireElemental() => new Monster(
		name: "Fire Elemental",
		health: 40,
		attackDamage: new Value("2d6"),
		threat: 5,
		theme: MonsterTheme.Elemental
	);

	public static Monster IceElemental() => new Monster(
		name: "Ice Elemental",
		health: 40,
		attackDamage: new Value("2d6"),
		threat: 5,
		theme: MonsterTheme.Elemental
	);

	public static Monster LightningElemental() => new Monster(
		name: "Lightning Elemental",
		health: 35,
		attackDamage: new Value("2d8"),
		threat: 6,
		theme: MonsterTheme.Elemental
	);

	public static Monster StormElemental() => new Monster(
		name: "Storm Elemental",
		health: 55,
		attackDamage: new Value("2d10"),
		threat: 7,
		theme: MonsterTheme.Elemental
	);

	public static Monster PrimalElemental() => new Monster(
		name: "Primal Elemental",
		health: 85,
		attackDamage: new Value("3d8"),
		threat: 9,
		theme: MonsterTheme.Elemental
	);


	// ===== WARLOCKS =====

	public static Monster CultMage() => new Monster(
		name: "Cult Mage",
		health: 22,
		attackDamage: new Value("1d8"),
		threat: 3,
		theme: MonsterTheme.Warlocks
	);

	public static Monster ApprenticeWarlock() => new Monster(
		name: "Apprentice Warlock",
		health: 24,
		attackDamage: new Value("1d8"),
		threat: 3,
		theme: MonsterTheme.Warlocks
	);

	public static Monster DarkSorcerer() => new Monster(
		name: "Dark Sorcerer",
		health: 32,
		attackDamage: new Value("2d6"),
		threat: 4,
		theme: MonsterTheme.Warlocks
	);

	public static Monster BloodMage() => new Monster(
		name: "Blood Mage",
		health: 35,
		attackDamage: new Value("2d6"),
		threat: 5,
		theme: MonsterTheme.Warlocks
	);

	public static Monster ArchWarlock() => new Monster(
		name: "Arch Warlock",
		health: 60,
		attackDamage: new Value("2d10"),
		threat: 7,
		theme: MonsterTheme.Warlocks
	);


	// ===== DEMONS =====

	public static Monster Imp() => new Monster(
		name: "Imp",
		health: 16,
		attackDamage: new Value("1d6"),
		threat: 2,
		theme: MonsterTheme.Demons
	);

	public static Monster LesserDemon() => new Monster(
		name: "Lesser Demon",
		health: 28,
		attackDamage: new Value("1d10"),
		threat: 3,
		theme: MonsterTheme.Demons
	);

	public static Monster Hellhound() => new Monster(
		name: "Hellhound",
		health: 36,
		attackDamage: new Value("2d6"),
		threat: 4,
		theme: MonsterTheme.Demons
	);

	public static Monster Fiend() => new Monster(
		name: "Fiend",
		health: 50,
		attackDamage: new Value("2d8"),
		threat: 6,
		theme: MonsterTheme.Demons
	);

	public static Monster AbyssalKnight() => new Monster(
		name: "Abyssal Knight",
		health: 70,
		attackDamage: new Value("2d10"),
		threat: 8,
		theme: MonsterTheme.Demons
	);

	public static Monster Balrog() => new Monster(
		name: "Balrog",
		health: 140,
		attackDamage: new Value("3d12"),
		threat: 12,
		theme: MonsterTheme.Demons
	);


	// ===== VAMPIRES =====

	public static Monster Thrall() => new Monster(
		name: "Thrall",
		health: 20,
		attackDamage: new Value("1d6"),
		threat: 2,
		theme: MonsterTheme.Vampires
	);

	public static Monster LesserVampire() => new Monster(
		name: "Lesser Vampire",
		health: 35,
		attackDamage: new Value("1d10"),
		threat: 4,
		theme: MonsterTheme.Vampires
	);

	public static Monster VampireStalker() => new Monster(
		name: "Vampire Stalker",
		health: 40,
		attackDamage: new Value("2d6"),
		threat: 5,
		theme: MonsterTheme.Vampires
	);

	public static Monster BloodKnight() => new Monster(
		name: "Blood Knight",
		health: 55,
		attackDamage: new Value("2d8"),
		threat: 6,
		theme: MonsterTheme.Vampires
	);

	public static Monster ElderVampire() => new Monster(
		name: "Elder Vampire",
		health: 85,
		attackDamage: new Value("2d10"),
		threat: 8,
		theme: MonsterTheme.Vampires
	);

	public static Monster VampireLord() => new Monster(
		name: "Vampire Lord",
		health: 120,
		attackDamage: new Value("3d8"),
		threat: 10,
		theme: MonsterTheme.Vampires
	);
}
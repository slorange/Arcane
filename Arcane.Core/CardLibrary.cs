using Arcane.Core.Cards;
using Arcane.Core.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcane.Core;

public static class CardLibrary
{
	private static List<Card>? _allMarket;

	public static List<Card> AllMarketCards()
	{
		if (_allMarket != null) return _allMarket;

		_allMarket = new List<Card>();
		AddCopies(AdvancedTraining, 5);
		AddCopies(AdvancedChanneling, 5);

		return _allMarket;
	}
	private static void AddCopies(Func<Card> factory, int count)
	{
		for (int i = 0; i < count; i++)
			_allMarket.Add(factory());
	}


	private static readonly List<Artifact> _allArtifacts = LoadAllArtifacts();

	public static IReadOnlyList<Artifact> AllArtifacts() => _allArtifacts;

	private static List<Artifact> LoadAllArtifacts()
	{
		var methods = typeof(CardLibrary)
			.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
			.Where(m => m.ReturnType == typeof(Artifact));

		return methods
			.Select(m => (Artifact)m.Invoke(null, null)!)
			.ToList();
	}


	public static Upgrade AdvancedTraining()
	{
		return new Upgrade(
			name: "Advanced Training",
			knowledgeCost: 8,
			description: "Train costs +1 mana and grants +1 training progress",
			apply: player => player.TrainingBonus++
		);
	}

	public static Upgrade AdvancedChanneling()
	{
		return new Upgrade(
			name: "Advanced Channeling",
			knowledgeCost: 8,
			description: "Channel has an upgraded dice",
			apply: player => player.ChannelBonus++
		);
	}

	public static Artifact SoulLeech()
	{
		var card = new Artifact(
			name: "Soul Leech",
			description: "Gain 3 HP on kill",
			apply: null,
			theme: null
		);
		card.OnEnemyKilledAction = (Monster enemy, Spell? spell) =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();
			player.Heal(3);
			game.Events.Add(new GameEventMessage($"{player.Name} absorbs the soul and heals 3 HP!"));
		};
		return card;
	}

	public static Artifact AetherkindledFocus()
	{
		var card = new Artifact(
			name: "Aetherkindled Focus",
			description: "First spell each turn gains +1 die",
			apply: null,
			theme: null
		);

		bool usedThisTurn = false;

		card.OnTurnStartAction = () => // This doesn't work. Turn start happens every turn, not once per phase
		{
			usedThisTurn = false;
		};

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			if (usedThisTurn) return;

			var player = card.Game.CurrentPlayer();
			player.Effects.Add(new PlayerEffect(SpellSchool.None, 1, 0, consumeOnUse: true));

			card.Game.Events.Add(new GameEventMessage("Aetherkindled Focus empowers the spell!"));
			usedThisTurn = true;
		};

		return card;
	}

	public static Artifact WellOfAether()
	{
		var card = new Artifact(
			name: "Well of Aether",
			description: "Gain 1 mana at the start of each turn",
			apply: null,
			theme: null
		);

		card.OnTurnStartAction = () =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();
			player.Resources.AddMana(1);

			game.Events.Add(new GameEventMessage($"{player.Name} gains 1 mana from Well of Aether."));
		};

		return card;
	}

	public static Artifact ConduitOfExcess()
	{
		var card = new Artifact(
			name: "ConduitofExcess",
			description: "Channel grants +1 additional mana",
			apply: null,
			theme: null
		);

		card.OnBattleStartAction = () =>
		{
			var player = card.Game.CurrentPlayer();
			player.ChannelBonus += 1;
		};

		return card;
	}

	public static Artifact RunicReservoir()
	{
		var card = new Artifact(
			name: "Runic Reservoir",
			description: "Gain +1 max mana after each battle",
			apply: null,
			theme: null
		);

		card.OnBattleEndAction = (Spell? killingBlow) =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();
			player.Resources.MaxMana += 1;

			game.Events.Add(new GameEventMessage($"{player.Name}'s maximum mana increases by 1."));
		};

		return card;
	}

	public static Artifact MantleOfGranite()
	{
		var card = new Artifact(
			name: "Mantle of Granite",
			description: "Gain 5 shield at the start of battle",
			apply: null,
			theme: null
		);

		card.OnBattleStartAction = () =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();
			player.AddShield(5);

			game.Events.Add(new GameEventMessage($"{player.Name} gains 5 shield from Mantle of Granite."));
		};

		return card;
	}

	public static Artifact StoneSkin()
	{
		var card = new Artifact(
			name: "Stone Skin",
			description: "Gain 1 shield at the end of each turn",
			apply: null,
			theme: null
		);

		card.OnTurnEndAction = () =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();
			player.AddShield(1);

			game.Events.Add(new GameEventMessage($"{player.Name} gains 1 shield from Stone Skin."));
		};

		return card;
	}

	public static Artifact EmberIdol()
	{
		var card = new Artifact(
			name: "Ember Idol",
			description: "Fire spells apply burn (1d4)",
			apply: null,
			theme: null
		);

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			if (spell.School != SpellSchool.Fire || targets.Count == 0) return;

			card.Game.Events.Add(new GameEventMessage("Ember Idol burns the enemy"));
			foreach (var monster in targets)
			{
				monster.GiveEffect(new StatusEffect(StatusEffectType.Burn, 0, new Dice(1, 4)), card.Game.Events);
			}
		};

		return card;
	}

	public static Artifact StormboundRing()
	{
		var card = new Artifact(
			name: "Stormbound Ring",
			description: "Lightning spells grant +1 die to your next spell",
			apply: null,
			theme: null
		);

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			if (spell.School != SpellSchool.Lightning) return;

			var player = card.Game.CurrentPlayer();
			player.Effects.Add(new PlayerEffect(SpellSchool.None, 1, 0, consumeOnUse: true));

			card.Game.Events.Add(new GameEventMessage("Stormbound Ring empowers your next spell!"));
		};

		return card;
	}

	public static Artifact SanguineRelic()
	{
		var card = new Artifact(
			name: "Sanguine Relic",
			description: "Lose 1 HP when casting, gain +1 die",
			apply: null,
			theme: null
		);

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();

			player.TakeDamage(1);
			player.Effects.Add(new PlayerEffect(SpellSchool.None, 1, 0, consumeOnUse: true));

			game.Events.Add(new GameEventMessage($"{player.Name} sacrifices blood for power!"));
		};

		return card;
	}

	public static Artifact RunemarkLens()
	{
		var card = new Artifact(
			name: "Runemark Lens",
			description: "Arcane spells apply Mark",
			apply: null,
			theme: null
		);

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			if (spell.School != SpellSchool.Arcane || targets.Count == 0) return;
			foreach (var monster in targets)
			{
				monster.GiveEffect(new StatusEffect(StatusEffectType.Marked, 1), card.Game.Events);
			}
		};

		return card;
	}

	public static Artifact StoneheartTotem()
	{
		var card = new Artifact(
			name: "Stoneheart Totem",
			description: "Earth spells gain +1 die",
			apply: null,
			theme: null
		);

		card.OnBattleStartAction = () =>
		{
			var player = card.Game.CurrentPlayer();
			player.Effects.Add(new PlayerEffect(SpellSchool.Earth, 1, null));
		};

		return card;
	}

	public static Artifact CinderboundGrimoire()
	{
		var card = new Artifact(
			name: "Cinderbound Grimoire",
			description: "Applying Burn also applies Weak",
			apply: null,
			theme: null
		);

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			if (spell.StatusEffect?.Type != StatusEffectType.Burn || targets.Count == 0) return;
			foreach (var monster in targets)
			{
				monster.GiveEffect(new StatusEffect(StatusEffectType.Weak, 1), card.Game.Events);
			}
		};

		return card;
	}

	public static Artifact CoinOfTheGuttersnipe()
	{
		var card = new Artifact(
			name: "Coin Of The Guttersnipe",
			description: "Gain 1 mana when you kill an enemy",
			apply: null,
			theme: MonsterTheme.Goblins
		);

		card.OnEnemyKilledAction = (Monster enemy, Spell? spell) =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();
			player.Resources.AddMana(1);

			game.Events.Add(new GameEventMessage($"{player.Name} snatches a coin and gains 1 mana."));
		};

		return card;
	}

	public static Artifact ReliquaryOfAsh()
	{
		var card = new Artifact(
			name: "Reliquary of Ash",
			description: "Gain 2 shield on kill",
			apply: null,
			theme: MonsterTheme.Undead
		);

		card.OnEnemyKilledAction = (Monster enemy, Spell? spell) =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();

			player.AddShield(2);

			game.Events.Add(new GameEventMessage($"{player.Name} draws protection from the ashes and gains 2 shield."));
		};

		return card;
	}

	public static Artifact WildheartCharm()
	{
		var card = new Artifact(
			name: "Wildheart Charm",
			description: "Heal to max at the start of battle",
			apply: null,
			theme: MonsterTheme.Beasts
		);

		card.OnBattleStartAction = () =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();

			player.FullHeal();

			game.Events.Add(new GameEventMessage($"{player.Name} has fully recovered."));
		};

		return card;
	}

	public static Artifact HellfireBrand()
	{
		var card = new Artifact(
			name: "Hellfire Brand",
			description: "Fire spells gain +1 die",
			apply: null,
			theme: MonsterTheme.Demons
		);

		card.OnBattleStartAction = () =>
		{
			var player = card.Game.CurrentPlayer();

			player.Effects.Add(new PlayerEffect(SpellSchool.Fire, 1, null));
		};

		return card;
	}

	public static Artifact CrimsonBrooch()
	{
		var card = new Artifact(
			name: "Crimson Brooch",
			description: "Heal 1 whenever you cast a spell",
			apply: null,
			theme: MonsterTheme.Vampires
		);

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			var game = card.Game;
			var player = game.CurrentPlayer();

			player.Heal(1);

			game.Events.Add(new GameEventMessage($"{player.Name} draws vitality from magic and heals 1 HP."));
		};

		return card;
	}

	public static Artifact HexbindersRing()
	{
		var card = new Artifact(
			name: "Hexbinder's Ring",
			description: "Spells apply Weak",
			apply: null,
			theme: MonsterTheme.Warlocks
		);

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			if (targets.Count == 0) return;

			foreach (var monster in targets)
			{
				monster.GiveEffect(new StatusEffect(StatusEffectType.Weak, 1), card.Game.Events);
			}
		};

		return card;
	}

	public static Artifact StormRelic()
	{
		var card = new Artifact(
			name: "Storm Relic",
			description: "Lightning spells grant +1 die to your next spell",
			apply: null,
			theme: MonsterTheme.Elemental
		);

		card.OnSpellCastAction = (Spell spell, List<Monster> targets) =>
		{
			if (spell.School != SpellSchool.Lightning) return;

			var player = card.Game.CurrentPlayer();

			player.Effects.Add(new PlayerEffect(SpellSchool.None, 1, 0, consumeOnUse: true));

			card.Game.Events.Add(new GameEventMessage("The Storm Relic crackles with energy, empowering your next spell!"));
		};

		return card;
	}
}
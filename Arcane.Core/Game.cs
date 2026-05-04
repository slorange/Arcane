using Arcane.Core.Cards;
using Arcane.Core.Commands;
using Arcane.Core.Events;
using Newtonsoft.Json;
using System.Text.Json;
using System.Xml;

namespace Arcane.Core;

public class Game
{
	static Random rng = new Random();

	public State State = new();
	public IReadOnlyList<Player> GetPlayers() => State.Players;
	public Player CurrentPlayer() => State.Players[0];
	public Market GetMarket() => State.Market;
	public IReadOnlyList<Monster> GetMonsters() => State.Monsters;
	public PhaseInfo GetPhaseInfo() => State.GetPhaseInfo();

	public List<GameEvent> Events = new List<GameEvent>();

	private readonly List<IGameEventListener> _listeners = new();

	public Game()
	{
		var player = new Player("Player", this);
		State.Players.Add(player);

		State.StartGame(this);

		StartPrepRound();
	}

	public IReadOnlyList<GameEvent> Process(GameCommand command)
	{
		Events = new List<GameEvent>();

		if (command is SaveCommand)
		{
			Save("save.json");
			Events.Add(new GameEventMessage("Game saved."));
			return Events;
		}

		if (command is LoadCommand)
		{
			Load("save.json");
			Events.Add(new GameEventMessage("Game loaded."));
			return Events;
		}

		if (command is ExecuteAction exec)
		{
			var player = FindPlayer(exec.PlayerName);
			if (player == null) return Events;

			var actions = GetAvailableActions(player);

			var action = actions.FirstOrDefault(a => string.Equals(a.Name, exec.ActionName, StringComparison.OrdinalIgnoreCase));

			if (action == null)
			{
				Events.Add(new ErrorOccurred($"Action {exec.ActionName} not available."));
				return Events;
			}

			if (action is SpellCastAction && !string.IsNullOrEmpty(exec.Parameters))
			{
				var monsters = State.Monsters.Where(m => m.IsAlive).ToList();
				var target = monsters.FirstOrDefault(m => m.Name.Equals(exec.Parameters, StringComparison.OrdinalIgnoreCase));
				if (target == null)
				{
					Events.Add(new ErrorOccurred($"Target {exec.Parameters} not available."));
					return Events;
				}
			}

			foreach (var l in _listeners) l.OnTurnStart();

			action.Execute(exec.Parameters);

			foreach (var l in _listeners) l.OnTurnEnd();

			var result = State.AdvanceAfterPlayerAction();

			if (result.EnteredBattle) StartBattle();

			if (result.TriggerMonsterAttack) ResolveMonsterAttack();

			if (result.EnteredArtifactChoice) StartArtifactChoice();

			if (result.EnteredPrep) StartPrepRound();
		}

		return Events;
	}

	private void StartArtifactChoice()
	{
		var theme = State.EncounterHistory.LastOrDefault();
		State.ArtifactDeck.Refresh(theme);
	}

	private void ResolveMonsterAttack()
	{
		var player = State.Players.First();

		foreach (var monster in State.Monsters.Where(m => m.IsAlive))
		{
			if (monster.HasEffect(StatusEffectType.Freeze))
			{
				Events.Add(new GameEventMessage($"{monster.Name} is frozen and cannot act!"));
			}
			else
			{
				if (monster.HasEffect(StatusEffectType.Blinded) && rng.NextDouble() < 0.5)
				{
					Events.Add(new GameEventMessage($"{monster.Name} misses due to blindness!"));
				}
				else
				{

					var attack = monster.AttackDamage;

					int weakStacks = monster.Effects.Count(e => e.Type == StatusEffectType.Weak);
					if (weakStacks > 0 && attack.Type == ValueKind.Dice)
					{
						var dice = attack.Dice;
						dice = dice.Modify(-weakStacks);
						attack = new Value(dice);
						Events.Add(new GameEventMessage($"{monster.Name} is weakened!"));
					}

					int damage = attack.Resolve(Events, $"{monster.Name} attack");

					player.TakeDamage(damage);

					Events.Add(new PlayerTookDamage(player.Name, damage, player.Health, player.Shield));
				}
			}

			foreach (var effect in monster.Effects.Where(e => e.Type == StatusEffectType.Burn))
			{
				var damage = effect.BurnDice.Value.Roll(Events, $"{monster.Name} Burn");
				monster.TakeDamage(damage, null);
			}

			monster.TickEffects(Events);
			//TODO if (!player.IsAlive())...
		}
	}

	private Player? FindPlayer(string name)
	{
		var player = State.Players.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

		if (player == null) Events.Add(new ErrorOccurred($"Player {name} not found."));

		return player;
	}

	private void StartPrepRound()
	{
		Events.Add(new RoundStarted(State.Round));

		foreach (var player in State.Players.Where(p => p.IsAlive))
		{
			player.Resources.FullMana();
			player.Heal(5);
			foreach (var implement in player.Implements.Where(i => i.IsActive))
			{
				implement.Deactivate();
			}
		}

	}
	private void StartBattle()
	{
		State.Monsters.Clear();

		var monsters = Generate(State.Round);

		State.Monsters.AddRange(monsters);

		foreach (var monster in State.Monsters)
		{
			Events.Add(new MonsterSpawned(monster.Name, monster.Health));
		}

		foreach (var player in State.Players.Where(p => p.IsAlive))
		{
			player.ResetShield();
			player.Resources.FullMana();
			player.ResetBuffs();
			//events.Add(new PlayerGainedMana(player.Name, 3));
			foreach (var spell in player.Spells)
				spell.UsedThisBattle = false;
		}

		foreach (var l in _listeners)
			l.OnBattleStart();
	}

	public List<Monster> Generate(int round)
	{
		var monsters = new List<Monster>();

		// difficulty scaling
		int threatBudget = round * 2;

		// pick a theme
		var theme = PickTheme();

		// get monster pool
		var pool = MonsterLibrary.AllMonsters().Where(m => m.Theme == theme);

		while (threatBudget > 0)
		{
			var options = pool.Where(m => m.Threat <= threatBudget).ToList();

			if (options.Count == 0) break;

			var template = options[rng.Next(options.Count)];

			var monster = template.Clone(this);

			monsters.Add(monster);
			threatBudget -= template.Threat;
		}

		if (monsters.Count > 0)
		{
			State.EncounterHistory.Add(theme);
			return monsters;
		}
		// It's possible we pick a theme that doesn't have any weak options for earlier rounds, so we have to try again
		return Generate(round);
	}

	private MonsterTheme PickTheme()
	{
		var themes = Enum.GetValues<MonsterTheme>();

		var recent = State.EncounterHistory.TakeLast(2).ToHashSet();

		var validThemes = themes.Where(t => !recent.Contains(t)).ToList();

		return validThemes[rng.Next(validThemes.Count)];
	}

	public List<PlayerAction> GetAvailableActions(Player player)
	{
		var actions = new List<PlayerAction>
		{
			// Intrinsic actions
			new ChannelAction(this),
			new TrainAction(player.TrainingBonus, this),
			new LearnAction(this),
			new EndTurnAction(this),
			new RestAction(this)
		};

		// Market buy actions
		foreach (var card in GetMarket().Current)
			actions.Add(new BuyCardAction(card, this));

		// Spell cast actions
		foreach (var spell in player.Spells)
			actions.Add(new SpellCastAction(spell, this));

		// Artifact choose actions
		foreach (var artifact in State.ArtifactDeck.Current)
			actions.Add(new ArtifactSelectAction(this, artifact));

		// Implement activate actions
		foreach (var implement in player.Implements)
			actions.Add(new ActivateImplementAction(implement, this));

		return actions.Where(a => a.CanExecute()).ToList();
	}

	public void Save(string file)
	{
		var settings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.All,
			Formatting = Newtonsoft.Json.Formatting.Indented
		};

		var json = JsonConvert.SerializeObject(State, settings);
		File.WriteAllText("save.json", json);
	}

	// This doesn't work. Issues with private and get only fields
	public void Load(string file)
	{
		var settings = new JsonSerializerSettings
		{
			TypeNameHandling = TypeNameHandling.All,
			//ObjectCreationHandling = ObjectCreationHandling.Replace
		};

		var json = File.ReadAllText("save.json");
		State = JsonConvert.DeserializeObject<State>(json, settings);
	}

	public void RegisterListener(IGameEventListener listener)
	{
		_listeners.Add(listener);
	}

	public void RaiseEnemyKilled(Monster enemy, Spell? spell)
	{
		foreach (var l in _listeners)
			l.OnEnemyKilled(enemy, spell);
	}

	public void RaiseSpellCast(Spell spell, List<Monster> targets)
	{
		foreach (var l in _listeners)
			l.OnSpellCast(spell, targets);
	}

	public void RaiseDamageDealt(Spell spell, int amount, List<Monster> targets)
	{
		foreach (var l in _listeners)
			l.OnDamageDealt(targets, amount, spell);
	}
}
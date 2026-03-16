using Arcane.Core.Cards;
using Arcane.Core.Commands;
using Arcane.Core.Events;

namespace Arcane.Core;

public class Game
{
	static Random rng = new Random();

	private readonly State _state = new();
	public IReadOnlyList<Player> GetPlayers() => _state.Players;
	public Market GetMarket() => _state.Market;
	public IReadOnlyList<Monster> GetMonsters() => _state.Monsters;
	public PhaseInfo GetPhaseInfo() => _state.GetPhaseInfo();

	public Game()
	{
		var player = new Player("Player");
		_state.Players.Add(player);

		_state.StartGame();

		StartPrepRound(new List<GameEvent>());
	}

	public IReadOnlyList<GameEvent> Process(GameCommand command)
	{
		var events = new List<GameEvent>();

		if (command is ExecuteAction exec)
		{
			var player = FindPlayer(exec.PlayerName, events);
			if (player == null) return events;

			var actions = GetAvailableActions(player);

			var action = actions.FirstOrDefault(a => string.Equals(a.Name, exec.ActionName, StringComparison.OrdinalIgnoreCase));

			if (action == null)
			{
				events.Add(new ErrorOccurred($"Action {exec.ActionName} not available."));
				return events;
			}

			if (action is SpellCastAction && !string.IsNullOrEmpty(exec.Parameters))
			{
				var monsters = _state.Monsters.Where(m => m.IsAlive).ToList();
				var target = monsters.FirstOrDefault(m => m.Name.Equals(exec.Parameters, StringComparison.OrdinalIgnoreCase));
				if (target == null)
				{
					events.Add(new ErrorOccurred($"Target {exec.Parameters} not available."));
					return events;
				}
			}

			action.Execute(_state, player, events, exec.Parameters);

			var result = _state.AdvanceAfterPlayerAction();

			if (result.EnteredBattle) StartBattle(events);

			if (result.TriggerMonsterAttack) ResolveMonsterAttack(events);

			if (result.EnteredPrep) StartPrepRound(events);
		}

		return events;
	}

	private void ResolveMonsterAttack(List<GameEvent> events)
	{
		var player = _state.Players.First();

		foreach (var monster in _state.Monsters.Where(m => m.IsAlive))
		{
			if (monster.HasEffect(StatusEffectType.Freeze))
			{
				events.Add(new GameEventMessage($"{monster.Name} is frozen and cannot act!"));
			}
			else
			{
				if (monster.HasEffect(StatusEffectType.Blinded) && rng.NextDouble() < 0.5)
				{
					events.Add(new GameEventMessage($"{monster.Name} misses due to blindness!"));
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
						events.Add(new GameEventMessage($"{monster.Name} is weakened!"));
					}

					int damage = attack.Resolve(events, $"{monster.Name} attack");

					player.TakeDamage(damage);

					events.Add(new PlayerTookDamage(player.Name, damage, player.Health, player.Shield));
				}
			}

			foreach (var effect in monster.Effects.Where(e => e.Type == StatusEffectType.Burn))
			{
				var damage = effect.BurnDice.Value.Roll(events, $"{monster.Name} Burn");
				monster.TakeDamage(damage);
			}

			monster.TickEffects(events);
			//TODO if (!player.IsAlive())...
		}
	}

	private Player? FindPlayer(string name, List<GameEvent> events)
	{
		var player = _state.Players.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

		if (player == null) events.Add(new ErrorOccurred($"Player {name} not found."));

		return player;
	}

	private void StartPrepRound(List<GameEvent> events)
	{
		events.Add(new RoundStarted(_state.Round));

		foreach (var player in _state.Players.Where(p => p.IsAlive))
		{
			player.Resources.FullMana();
			//events.Add(new PlayerGainedMana(player.Name, 3));

			player.Heal(5);
		}

	}
	private void StartBattle(List<GameEvent> events)
	{
		_state.Monsters.Clear();

		var monsters = Generate(_state.Round);

		_state.Monsters.AddRange(monsters);

		foreach (var monster in _state.Monsters)
		{
			events.Add(new MonsterSpawned(monster.Name, monster.Health));
		}

		foreach (var player in _state.Players.Where(p => p.IsAlive))
		{
			player.ResetShield();
			player.Resources.FullMana();
			//events.Add(new PlayerGainedMana(player.Name, 3));
			foreach (var spell in player.Spells)
				spell.UsedThisBattle = false;
		}
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

			var monster = template.Clone();

			monsters.Add(monster);
			threatBudget -= template.Threat;
		}

		if (monsters.Count > 0)
		{
			_state.EncounterHistory.Add(theme);
			return monsters;
		}
		// It's possible we pick a theme that doesn't have any weak options for earlier rounds, so we have to try again
		return Generate(round);
	}

	private MonsterTheme PickTheme()
	{
		var themes = Enum.GetValues<MonsterTheme>();

		var recent = _state.EncounterHistory.TakeLast(2).ToHashSet();

		var validThemes = themes.Where(t => !recent.Contains(t)).ToList();

		return validThemes[rng.Next(validThemes.Count)];
	}

	public List<PlayerAction> GetAvailableActions(Player player)
	{
		var actions = new List<PlayerAction>
		{
			// Intrinsic actions
			new ChannelAction(),
			new TrainAction(player.AdvancedTraining),
			new LearnAction(),
			new EndTurnAction(),
			new RestAction()
		};

		// Market buy actions
		foreach (var card in GetMarket().Current)
			actions.Add(new BuyCardAction(card));

		// Spell cast actions
		foreach (var spell in player.Spells)
			actions.Add(new SpellCastAction(spell));

		return actions.Where(a => a.CanExecute(_state, player)).ToList();
	}
}
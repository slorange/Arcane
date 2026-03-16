using Arcane.Core;
using Arcane.Core.Cards;
using Arcane.Core.Commands;
using Arcane.Core.Events;

namespace Arcane.Cmd;

internal class Program
{
	Game game;
	List<PlayerAction> _currentActions = new();

	static void Main(string[] args)
	{
		new Program();
	}

	Program()
	{
		Analyzer.AnalyzeSpells();

		game = new Game();
		DisplayGameState();
		DisplayAvailableActions();

		while (true)
		{
			Console.Write("> ");
			var input = Console.ReadLine();
			if (input == null) continue;

			var command = ParseInput(input);
			if (command == null)
			{
				Console.WriteLine("Invalid command.");
				continue;
			}

			var events = game.Process(command);
			Render(events);

			if (events.Any(e => e is GameEnded))
				break;
		}
	}

	GameCommand? ParseInput(string input)
	{
		if (string.IsNullOrWhiteSpace(input)) return null;

		input = input.Trim();

		// Allow "Train" shorthand for "Train LvX"
		if (input.Equals("Train", StringComparison.OrdinalIgnoreCase))
		{
			var action1 = _currentActions.FirstOrDefault(a => a.Name.StartsWith("Train"));
			if(action1 != null) return new ExecuteAction("Player", action1.Name, null);
		}

		var action = _currentActions
			.OrderByDescending(a => a.Name.Length) // longest first
			.FirstOrDefault(a => input.StartsWith(a.Name, StringComparison.OrdinalIgnoreCase));

		if (action == null)
		{
			Console.WriteLine($"Action {input} not available.");
			return null;
		}

		var parametersText = input.Substring(action.Name.Length).Trim();

		return new ExecuteAction("Player", action.Name, parametersText);
	}

	void Render(IEnumerable<GameEvent> events)
	{
		Console.ForegroundColor = ConsoleColor.Blue;
		foreach (var e in events)
		{
			switch (e)
			{
				case PlayerJoined pj:
					Console.WriteLine($"{pj.PlayerName} joined the game.");
					break;

				case GameStarted:
					Console.WriteLine("Game started.");
					break;

				case RoundStarted r:
					Console.WriteLine($"--- Round {r.RoundNumber} ---");
					break;

				case PlayerGainedMana m:
					Console.WriteLine($"{m.PlayerName} gains {m.Amount} mana.");
					break;

				case PlayerTookDamage d:
					Console.WriteLine($"{d.PlayerName} takes {d.Amount} damage (HP: {d.RemainingHealth} Shield: {d.RemainingShield})");
					break;

				case GameEnded:
					Console.WriteLine("Game over.");
					break;

				case ErrorOccurred err:
					Console.WriteLine($"Error: {err.Message}");
					break;

				case PlayerGainedKnowledge k:
					Console.WriteLine($"{k.PlayerName} gains {k.Amount} knowledge.");
					break;

				case SpellPurchased s:
					Console.WriteLine($"{s.PlayerName} bought spell '{s.SpellName}'.");
					break;

				case MonsterSpawned m:
					Console.WriteLine($"A wild {m.Name} appears! (HP: {m.Health})");
					break;

				case MonsterTookDamage m:
					Console.WriteLine($"{m.Name} takes {m.Damage} damage (HP: {m.RemainingHealth})");
					break;

				case MonsterDefeated m:
					Console.WriteLine($"{m.Name} has been defeated!");
					break;

				case GameEventMessage msg:
					Console.WriteLine(msg.Message);
					break;
			}
		}

		Console.ForegroundColor = ConsoleColor.White;
		DisplayGameState();
		DisplayAvailableActions();
	}

	void DisplayGameState()
	{
		Console.WriteLine("\n--- Game State ---");

		// Phase
		Console.ForegroundColor = ConsoleColor.Magenta;
		var phaseInfo = game.GetPhaseInfo();

		if (phaseInfo.Phase == Phase.Prep)
		{
			Console.WriteLine($"Round {phaseInfo.RoundNumber} - Prep Phase");
			Console.WriteLine($"Prep Rounds Remaining: {phaseInfo.PrepRoundsRemaining}");
			Console.WriteLine($"Actions Remaining: {phaseInfo.PrepActionsRemaining}");
		}
		else
		{
			Console.WriteLine(
				$"Round {phaseInfo.RoundNumber} - Battle Phase " +
				$"(Actions this cycle: {phaseInfo.BattleActionsThisCycle}/3)");
		}

		// Players
		Console.ForegroundColor = ConsoleColor.Green;
		foreach (var player in game.GetPlayers())
		{
			if (phaseInfo.Phase == Phase.Prep)
				Console.WriteLine($"{player.Name} | HP: {player.Health} | Mana: {player.Resources.CurrentMana} / {player.Resources.MaxMana} | Knowledge: {player.Resources.Knowledge}");
			else
				Console.WriteLine($"{player.Name} | HP: {player.Health} | Mana: {player.Resources.CurrentMana} / {player.Resources.MaxMana}");

			if (player.Spells.Any())
			{
				Console.WriteLine("  Spells:");
				foreach (var spell in player.Spells)
				{
					Console.WriteLine($"    {spell.GetPlayerDisplay()}");
				}
			}

			if (player.Effects.Any())
			{
				Console.WriteLine("  Buffs:");

				foreach (var effect in player.Effects)
				{
					var school = effect.School == SpellSchool.None
						? "spells"
						: effect.School.ToString().ToLower();

					var duration = effect.Duration == null
						? ""
						: $" ({effect.Duration})";

					if (effect.ConsumeOnUse)
						duration = " (next cast)";

					Console.WriteLine($"    {school} +{effect.Modifier} die{duration}");
				}
			}
		}

		if (phaseInfo.Phase == Phase.Prep)
		{
			// Market
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine();
			var market = game.GetMarket().Current;
			if (market.Count > 0)
			{
				Console.WriteLine("Market:");
				foreach (var spell in market)
				{
					Console.WriteLine(spell.GetMarketDisplay());
				}
			}
			else
			{
				Console.WriteLine("Market is empty");
			}
		}
		else
		{
			// Monsters
			Console.ForegroundColor = ConsoleColor.Red;
			var monsters = game.GetMonsters();

			if (monsters.Any())
			{
				Console.WriteLine();
				Console.WriteLine("Enemies:");

				foreach (var monster in monsters)
				{
					if (monster.IsAlive)
					{
						var effectText = monster.Effects.Any()
							? $" | {string.Join(", ", monster.Effects.Select(e =>
								e.Type == StatusEffectType.Burn
									? $"{e.Type}({e.BurnDice.Value})"
									: $"{e.Type}({e.Duration})"
							))}"
							: ""; 
						Console.WriteLine($"  {monster.Name} | HP: {monster.Health}{effectText}");
					}
					else
						Console.WriteLine($"  {monster.Name} (DEFEATED)");
				}
			}
		}

		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("-------------------\n");
	}

	void DisplayAvailableActions()
	{
		Console.ForegroundColor = ConsoleColor.Cyan;
		var player = game.GetPlayers().First();

		_currentActions  = game.GetAvailableActions(player);

		Console.WriteLine("Available actions:");
		foreach (var action in _currentActions)
		{
			Console.WriteLine($"  {action.Name}");
		}

		Console.WriteLine();
		Console.ForegroundColor = ConsoleColor.White;
	}
}
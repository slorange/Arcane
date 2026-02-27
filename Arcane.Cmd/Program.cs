using Arcane.Core;
using Arcane.Core.Commands;
using Arcane.Core.Events;

namespace Arcane.Cmd;

internal class Program
{
	static void Main(string[] args)
	{
		var game = new Game();

		Console.WriteLine("Arcane Command Interface");
		Console.WriteLine("Type: start, round, mana <name>, dmg <name> <amount>, end");

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

	static GameCommand? ParseInput(string input)
	{
		var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		if (parts.Length == 0)
			return null;

		return parts[0].ToLower() switch
		{
			"start" => new StartGame(new List<string> { "Alice", "Bob" }),
			"round" => new StartNextRound(),
			"mana" when parts.Length >= 2 => new GainMana(parts[1]),
			"dmg" when parts.Length >= 3 && int.TryParse(parts[2], out var amt)
				=> new DamagePlayer(parts[1], amt),
			"end" => new EndGame(),
			_ => null
		};
	}

	static void Render(IEnumerable<GameEvent> events)
	{
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
					Console.WriteLine($"{d.PlayerName} takes {d.Amount} damage (HP: {d.RemainingHealth})");
					break;

				case GameEnded:
					Console.WriteLine("Game over.");
					break;

				case ErrorOccurred err:
					Console.WriteLine($"Error: {err.Message}");
					break;
			}
		}
	}
}
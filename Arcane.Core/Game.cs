using Arcane.Core.Commands;
using Arcane.Core.Events;

namespace Arcane.Core;

public class Game
{
	private readonly State _state = new();

	public IReadOnlyList<GameEvent> Process(GameCommand command)
	{
		var events = new List<GameEvent>();

		switch (command)
		{
			case StartGame start:
			{
				if (_state.GameStarted)
				{
					events.Add(new ErrorOccurred("Game already started."));
					return events;
				}

				foreach (var name in start.PlayerNames)
				{
					var player = new Player(name);
					_state.Players.Add(player);
					events.Add(new PlayerJoined(name));
				}

				_state.StartGame();
				events.Add(new GameStarted(start.PlayerNames));
				events.Add(new RoundStarted(_state.Round));
				break;
			}

			case StartNextRound:
				if (!_state.GameStarted)
				{
					events.Add(new ErrorOccurred("Game not started."));
					return events;
				}

				_state.NextRound();
				events.Add(new RoundStarted(_state.Round));
				break;

			case GainMana gain:
			{
				var player = _state.Players.FirstOrDefault(p => string.Equals(p.Name, gain.PlayerName, StringComparison.OrdinalIgnoreCase));
				if (player == null)
				{
					events.Add(new ErrorOccurred($"Player {gain.PlayerName} not found."));
					return events;
				}

				player.Resources.AddMana(2);
				events.Add(new PlayerGainedMana(player.Name, 2));
				break;
			}

			case DamagePlayer dmg:
				var target = _state.Players.FirstOrDefault(p => string.Equals(p.Name, dmg.PlayerName, StringComparison.OrdinalIgnoreCase));
				if (target == null)
				{
					events.Add(new ErrorOccurred($"Player {dmg.PlayerName} not found."));
					return events;
				}

				target.TakeDamage(dmg.Amount);
				events.Add(new PlayerTookDamage(target.Name, dmg.Amount, target.Health));

				if (_state.Players.All(p => !p.IsAlive))
				{
					events.Add(new GameEnded());
				}
				break;

			case EndGame:
				events.Add(new GameEnded());
				break;
		}

		return events;
	}
}
namespace Arcane.Core.Events;

public abstract record GameEvent;

public record GameStarted(List<string> PlayerNames) : GameEvent;

public record RoundStarted(int RoundNumber) : GameEvent;

public record PlayerJoined(string PlayerName) : GameEvent;

public record PlayerGainedMana(string PlayerName, int Amount) : GameEvent;

public record PlayerTookDamage(string PlayerName, int Amount, int RemainingHealth) : GameEvent;

public record GameEnded() : GameEvent;

public record ErrorOccurred(string Message) : GameEvent;
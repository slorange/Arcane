namespace Arcane.Core.Commands;

public abstract record GameCommand;

public record StartGame(List<string> PlayerNames) : GameCommand;

public record StartNextRound() : GameCommand;

public record GainMana(string PlayerName) : GameCommand;

public record DamagePlayer(string PlayerName, int Amount) : GameCommand;

public record EndGame() : GameCommand;
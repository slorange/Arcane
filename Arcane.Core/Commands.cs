namespace Arcane.Core.Commands;

public abstract record GameCommand;

public record ExecuteAction(string PlayerName, string ActionName, string? Target) : GameCommand;
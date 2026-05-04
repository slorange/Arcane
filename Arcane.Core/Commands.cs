namespace Arcane.Core.Commands;

public abstract record GameCommand;
public record ExecuteAction(string PlayerName, string ActionName, string Parameters) : GameCommand;
public record SaveCommand() : GameCommand;
public record LoadCommand() : GameCommand;
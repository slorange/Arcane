namespace Arcane.Core;

public class State
{
	public List<Player> Players { get; } = new();
	public int Round { get; private set; } = 1;
	public bool GameStarted { get; private set; }

	public void StartGame()
	{
		GameStarted = true;
		Round = 1;
	}

	public void NextRound()
	{
		Round++;
	}
}
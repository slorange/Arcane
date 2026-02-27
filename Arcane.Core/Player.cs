namespace Arcane.Core;

public class Player
{
	public string Name { get; }
	public int Health { get; private set; }
	public Resources Resources { get; } = new();

	public Player(string name, int startingHealth = 20)
	{
		Name = name;
		Health = startingHealth;
	}

	public void TakeDamage(int amount)
	{
		Health -= amount;
		if (Health < 0)
			Health = 0;
	}

	public bool IsAlive => Health > 0;
}
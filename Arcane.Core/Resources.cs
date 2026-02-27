namespace Arcane.Core;

public class Resources
{
	public int Mana { get; private set; }
	public int Knowledge { get; private set; }

	public void AddMana(int amount) => Mana += amount;

	public bool SpendMana(int amount)
	{
		if (Mana < amount) return false;
		Mana -= amount;
		return true;
	}
}
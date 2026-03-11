using Arcane.Core.Cards;
using System.Numerics;

namespace Arcane.Core;

public class Player
{
	public string Name { get; }
	public int Health { get; private set; }
	public Resources Resources { get; } = new();
	public List<Spell> Spells { get; } = new();

	public int MaxHealth = 20;
	public Player(string name)
	{
		Name = name;
		Health = MaxHealth;
		Spells.Add(SpellLibrary.MagicMissile());
	}

	public void TakeDamage(int amount)
	{
		Health -= amount;
		if (Health < 0) Health = 0;
	}

	public void Heal(int amount)
	{
		Health += amount;
		if (Health > MaxHealth) Health = MaxHealth;
	}

	public void FullHeal()
	{
		Health = MaxHealth;
	}

	public bool IsAlive => Health > 0;
	public List<Card> Actions { get; } = new();
}


namespace Arcane.Core;

public class Resources
{
	public int CurrentMana { get; private set; }
	public int MaxMana { get; private set; } = 3;
	public int TrainingProgress { get; private set; }

	public int Knowledge { get; private set; } = 10;

	public void SetMana(int amount) => CurrentMana = amount;
	public void FullMana() => CurrentMana = MaxMana;
	public bool HasMana(int amount) => CurrentMana >= amount;

	public void AddMana(int amount)
	{
		CurrentMana += amount;
		if (CurrentMana > MaxMana) CurrentMana = MaxMana;
	}

	public bool SpendMana(int amount)
	{
		if (CurrentMana < amount) return false;
		CurrentMana -= amount;
		return true;
	}

	public void AddKnowledge(int amount) => Knowledge += amount;

	public bool SpendKnowledge(int amount)
	{
		if (Knowledge < amount) return false;
		Knowledge -= amount;
		return true;
	}

	public bool Train(int manaCost, int progressGain)
	{
		if (CurrentMana < manaCost) return false;

		CurrentMana -= manaCost;
		TrainingProgress += progressGain;

		if (TrainingProgress >= MaxMana)
		{
			TrainingProgress -= MaxMana; // important change
			MaxMana++;
			return true;
		}

		return false;
	}
}
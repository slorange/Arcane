namespace Arcane.Core.Cards;

public abstract class Card
{
	public Guid Id { get; } = Guid.NewGuid();
	public string Name { get; }
	public int KnowledgeCost { get; }

	protected Card(string name, int knowledgeCost)
	{
		Name = name;
		KnowledgeCost = knowledgeCost;
	}
	public abstract string GetMarketDisplay();
}

public class Passive : Card
{
	public string Description { get; }
	public Action<Player> Apply { get; }

	public Passive(string name, int knowledgeCost, string description, Action<Player> apply)
		: base(name, knowledgeCost)
	{
		Description = description;
		Apply = apply;
	}

	public override string GetMarketDisplay()
	{
		return $"{Name, -26} — {KnowledgeCost} Knowledge - {Description}";
	}
}
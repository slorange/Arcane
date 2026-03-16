using Arcane.Core.Cards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcane.Core;

public static class CardLibrary
{
	private static List<Card>? _all;

	public static List<Card> AllCards()
	{
		if (_all != null) return _all;

		_all = new List<Card>();
		AddCopies(AdvancedTraining, 5);

		return _all;
	}
	private static void AddCopies(Func<Card> factory, int count)
	{
		for (int i = 0; i < count; i++)
			_all.Add(factory());
	}

	public static Card AdvancedTraining()
	{
		return new Passive(
			name: "Advanced Training",
			knowledgeCost: 8,
			description: "Train costs +1 mana and grants +1 training progress",
			apply: player => player.AdvancedTraining++
		);
	}
}
using Arcane.Core.Cards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcane.Core;

public class Market
{
	private static Random rng = new Random();

	private readonly List<Spell> _pool;

	public List<Spell> Current { get; } = new();

	public int ShopSize { get; } = 6;

	public Market(List<Spell> spells)
	{
		_pool = [.. spells];
		Refresh();
	}

	public void Refresh()
	{
		Current.Clear();

		var shuffled = _pool.OrderBy(x => rng.Next()).ToList();

		for (int i = 0; i < Math.Min(ShopSize, shuffled.Count); i++)
			Current.Add(shuffled[i]);
	}

	public void Purchase(Spell spell)
	{
		Current.Remove(spell);
		_pool.Remove(spell);
	}
}

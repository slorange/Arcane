using Arcane.Core.Cards;
using System;
using System.Collections.Generic;
using System.Text;

namespace Arcane.Core;

public class Market
{
	private static Random rng = new Random();

	private List<Card> _pool;
	public List<Card> Current { get; } = new();

	public int ShopSize { get; } = 6;

	public Market(List<Card> cards)
	{
		_pool = [.. cards];
		Refresh();
	}

	public void Refresh()
	{
		Current.Clear();

		var shuffled = _pool.OrderBy(x => rng.Next()).ToList();

		for (int i = 0; i < Math.Min(ShopSize, shuffled.Count); i++)
			Current.Add(shuffled[i]);
	}

	public void Purchase(Card card)
	{
		Current.Remove(card);
		_pool.Remove(card);
	}
}

public class ArtifactDeck
{
	private static Random rng = new Random();

	private List<Artifact> _pool;
	public List<Artifact> Current { get; } = new();

	public int ChoiceSize { get; } = 3;

	public ArtifactDeck()
	{
		_pool = [.. CardLibrary.AllArtifacts()]; // must return NEW instances
	}

	public void Refresh(MonsterTheme theme)
	{
		Current.Clear();

		// Split pools
		var themed = _pool.Where(a => a.Theme == theme).ToList();
		var general = _pool.Where(a => a.Theme == null).ToList();

		// Shuffle
		var shuffledGeneral = general.OrderBy(x => rng.Next()).ToList();
		var shuffledThemed = themed.OrderBy(x => rng.Next()).ToList();

		// 2 general
		Current.AddRange(shuffledGeneral.Take(2));

		// 1 themed (if possible)
		if (shuffledThemed.Any())
			Current.Add(shuffledThemed.First());
		else if (shuffledGeneral.Count > 2)
			Current.Add(shuffledGeneral[2]);

		// Safety: ensure max 3
		while (Current.Count > ChoiceSize)
			Current.RemoveAt(Current.Count - 1);
	}

	public void Choose(Artifact card)
	{
		Current.Remove(card);
		_pool.Remove(card);
	}
}
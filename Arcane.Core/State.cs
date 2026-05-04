using Arcane.Core.Cards;

namespace Arcane.Core;

public enum Phase
{
	Prep,
	Battle,
	ArtifactChoice
}

public record PhaseInfo(
	Phase Phase,
	int RoundNumber,
	int PrepRoundsRemaining,
	int PrepActionsRemaining,
	int BattleActionsThisCycle
);

public record AdvanceResult(
	bool TriggerMonsterAttack,
	bool EnteredBattle,
	bool EnteredPrep, 
	bool EnteredArtifactChoice,
	bool RoundAdvanced
);

public class State
{
	public Game Game;
	public List<Player> Players { get; private set; } = new();
	public int Round { get; private set; } = 1;
	public bool GameStarted { get; private set; }
	public Market Market { get; private set; }
	public List<Monster> Monsters { get; private set; } = new();
	public Phase CurrentPhase { get; private set; } = Phase.Prep;
	public int PrepRoundsRemaining { get; private set; }
	public int PrepActionsRemaining { get; private set; }
	public int TurnNumber { get; private set; } = 1;
	public int BattleActionsThisCycle { get; private set; } = 0;
	public List<MonsterTheme> EncounterHistory { get; private set; } = new();
	public PhaseInfo GetPhaseInfo() => new PhaseInfo(CurrentPhase, Round, PrepRoundsRemaining, PrepActionsRemaining, BattleActionsThisCycle);
	public ArtifactDeck ArtifactDeck { get; private set; }

	public void StartGame(Game game)
	{
		Game = game;
		var pool = new List<Card>();
		pool.AddRange(SpellLibrary.AllSpells());
		pool.AddRange(CardLibrary.AllMarketCards());
		foreach(var card in pool) card.Game = Game;
		Market = new Market(pool);
		ArtifactDeck = new ArtifactDeck();

		GameStarted = true;
		Round = 1;
		StartPrep();
	}

	public void NextRound()
	{
		Round++;
	}

	public void StartPrep()
	{
		CurrentPhase = Phase.Prep;
		PrepRoundsRemaining = 3;
		PrepActionsRemaining = 5;
		Market.Refresh();
	}

	public void EndPrepRound()
	{
		PrepActionsRemaining = 0;
	}

	public AdvanceResult AdvanceAfterPlayerAction()
	{
		bool triggerAttack = false;
		bool enteredBattle = false;
		bool enteredPrep = false;
		bool roundAdvanced = false;
		bool enteredArtifactChoice = false;


		if (CurrentPhase == Phase.Prep)
		{
			if (PrepActionsRemaining > 0)
				PrepActionsRemaining--;

			if (PrepActionsRemaining == 0)
			{
				if (PrepRoundsRemaining > 0)
					PrepRoundsRemaining--;

				if (PrepRoundsRemaining > 0)
				{
					PrepActionsRemaining = 5;
					enteredPrep = true;
				}
				else
				{
					CurrentPhase = Phase.Battle;
					enteredBattle = true;
					BattleActionsThisCycle = 0;
				}
			}
		}
		else if (CurrentPhase == Phase.Battle)
		{
			BattleActionsThisCycle++;

			if (BattleActionsThisCycle >= 3)
			{
				BattleActionsThisCycle = 0;
				triggerAttack = true;
			}

			// Battle ends when monsters dead
			if (!Monsters.Any(m => m.IsAlive))
			{
				CurrentPhase = Phase.ArtifactChoice;
				enteredArtifactChoice = true;
			}
		}
		else if (CurrentPhase == Phase.ArtifactChoice)
		{
			StartPrep();
			Round++;
			enteredPrep = true;
			roundAdvanced = true;
		}

		return new AdvanceResult(triggerAttack, enteredBattle, enteredPrep, enteredArtifactChoice, roundAdvanced);
	}
}
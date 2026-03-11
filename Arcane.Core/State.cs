using Arcane.Core.Cards;

namespace Arcane.Core;

public enum Phase
{
	Prep,
	Battle
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
	bool RoundAdvanced
);

public class State
{
	public List<Player> Players { get; } = new();
	public int Round { get; private set; } = 1;
	public bool GameStarted { get; private set; }
	public Market Market { get; private set; } = null!;
	public List<Monster> Monsters { get; } = new();
	public Phase CurrentPhase { get; private set; } = Phase.Prep;
	public int PrepRoundsRemaining { get; private set; }
	public int PrepActionsRemaining { get; private set; }
	public int TurnNumber { get; private set; } = 1;
	public int BattleActionsThisCycle { get; private set; } = 0;
	public List<MonsterTheme> EncounterHistory { get; } = new();
	public PhaseInfo GetPhaseInfo() => new PhaseInfo(CurrentPhase, Round, PrepRoundsRemaining, PrepActionsRemaining, BattleActionsThisCycle);

	public void StartGame()
	{
		Market = new Market(SpellLibrary.AllSpells());
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
				StartPrep();
				Round++;
				enteredPrep = true;
				roundAdvanced = true;
			}
		}

		return new AdvanceResult(triggerAttack, enteredBattle, enteredPrep, roundAdvanced);
	}
}
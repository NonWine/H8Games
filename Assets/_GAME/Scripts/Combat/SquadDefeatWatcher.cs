using System;

public class SquadDefeatWatcher
{
    private bool squadDefeatedRaised;

    public event Action SquadDefeated;

    public void ResetForNewEncounter()
    {
        squadDefeatedRaised = false;
    }

    public void TryRaiseDefeat(bool hasActiveEncounter, bool hasLivingAllies)
    {
        if (squadDefeatedRaised)
            return;

        if (!hasActiveEncounter)
            return;

        if (hasLivingAllies)
            return;

        squadDefeatedRaised = true;
        SquadDefeated?.Invoke();
    }
}
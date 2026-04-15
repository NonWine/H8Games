using System;

public class SquadEncounterController
{
    public event Action<EnemyGroupFacade> CombatStartedBattle;
    public event Action<EnemyGroupFacade> CombatClearedZone;

    public EnemyGroupFacade CurrentTargetGroup { get; private set; }

    public bool HasActiveEncounter => CurrentTargetGroup != null && CurrentTargetGroup.State == EnemyGroupState.Activated;

    public void TryBeginEncounter(EnemyGroupFacade enemyGroup)
    {
        if (ReferenceEquals(CurrentTargetGroup, enemyGroup) && HasActiveEncounter)
            return;

        CurrentTargetGroup = enemyGroup;
        CurrentTargetGroup.Cleared += HandleEncounterCleared;
        CombatStartedBattle?.Invoke(enemyGroup);
    }
    

    private void HandleEncounterCleared(EnemyGroupFacade clearedGroup)
    {
        
        clearedGroup.Cleared -= HandleEncounterCleared;
        CombatClearedZone?.Invoke(clearedGroup);
        CurrentTargetGroup = null;
    }
}

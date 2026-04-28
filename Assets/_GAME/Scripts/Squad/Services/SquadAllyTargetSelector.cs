using System.Collections.Generic;
using UnityEngine;

public class SquadAllyTargetSelector : IAllyTargetProvider
{
    private readonly SquadFormationRegistry soldierRegistry;

    public SquadAllyTargetSelector(SquadFormationRegistry soldierRegistry)
    {
        this.soldierRegistry = soldierRegistry;
    }

    public ICombatTarget GetBestLivingAllyTarget(Vector3 worldPosition, TargetingData targetingData)
    {
        soldierRegistry.PruneInvalid();
        return CombatTargetSelectionUtility.SelectBestTarget(
            soldierRegistry.Soldiers,
            worldPosition,
            targetingData);
    }
}

using UnityEngine;

public class AllyCombatTargetProvider : ICombatTargetProvider
{
    private readonly Transform ownerTransform;
    private readonly TargetingData targetingData;
    private readonly SquadFormationRegistry soldierRegistry;

    public AllyCombatTargetProvider(
        Transform ownerTransform,
        TargetingData targetingData,
        SquadFormationRegistry soldierRegistry)
    {
        this.ownerTransform = ownerTransform;
        this.targetingData = targetingData;
        this.soldierRegistry = soldierRegistry;
    }

    public ICombatTarget GetTarget()
    {
        soldierRegistry.PruneInvalid();
        return CombatTargetSelectionUtility.SelectBestTarget(
            soldierRegistry.Soldiers,
            ownerTransform.position,
            targetingData);
    }
}

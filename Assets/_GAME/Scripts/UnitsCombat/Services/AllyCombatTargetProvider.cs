using UnityEngine;

public class AllyCombatTargetProvider : ICombatTargetProvider
{
    private readonly Transform ownerTransform;
    private readonly TargetingData targetingData;
    private readonly IAllyTargetProvider allyTargetProvider;

    public AllyCombatTargetProvider(
        Transform ownerTransform,
        TargetingData targetingData,
        IAllyTargetProvider allyTargetProvider)
    {
        this.ownerTransform = ownerTransform;
        this.allyTargetProvider = allyTargetProvider;
        this.targetingData = targetingData;
    }

    public ICombatTarget GetTarget()
    {
        return allyTargetProvider.GetBestLivingAllyTarget(ownerTransform.position, targetingData);
    }
}

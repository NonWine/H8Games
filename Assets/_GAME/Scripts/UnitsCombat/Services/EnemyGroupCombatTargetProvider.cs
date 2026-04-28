using UnityEngine;

public class EnemyGroupCombatTargetProvider : ICombatTargetProvider
{
    private readonly Transform ownerTransform;
    private readonly TargetingData targetingData;
    private readonly IEnemyGroupProvider enemyGroupProvider;

    public EnemyGroupCombatTargetProvider(
        Transform ownerTransform,
        TargetingData targetingData,
        IEnemyGroupProvider enemyGroupProvider)
    {
        this.ownerTransform = ownerTransform;
        this.targetingData = targetingData;
        this.enemyGroupProvider = enemyGroupProvider;
    }

    public ICombatTarget GetTarget()
    {
        EnemyGroupViewController currentGroup = enemyGroupProvider.CurrentTargetGroup;
        return currentGroup != null
            ? currentGroup.GetBestLivingEnemyTarget(ownerTransform.position, targetingData)
            : null;
    }
}

public class CombatTargetProvider : ICombatTargetProvider
{
    public ICombatTarget GetTarget()
    {
        return null;
    }
}

using UnityEngine;

public class EnemyGroupCombatTargetProvider : ICombatTargetProvider
{
    private readonly Transform ownerTransform;
    private readonly float reservationPenalty;
    private readonly float maxEngageRange;
    private readonly IEnemyGroupProvider enemyGroupProvider;

    public EnemyGroupCombatTargetProvider(
        Transform ownerTransform,
        float reservationPenalty,
        float maxEngageRange,
        IEnemyGroupProvider enemyGroupProvider)
    {
        this.ownerTransform = ownerTransform;
        this.reservationPenalty = reservationPenalty;
        this.maxEngageRange = maxEngageRange;
        this.enemyGroupProvider = enemyGroupProvider;
    }

    public ICombatTarget GetTarget()
    {
        EnemyGroupViewController currentGroup = enemyGroupProvider.CurrentTargetGroup;
        return currentGroup != null
            ? currentGroup.GetBestLivingEnemyTarget(ownerTransform.position, reservationPenalty, maxEngageRange)
            : null;
    }
}

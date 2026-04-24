using UnityEngine;

public class EnemyGroupCombatTargetProvider : ICombatTargetProvider
{
    private readonly Transform ownerTransform;
    private readonly float reservationPenalty;
    private readonly IEnemyGroupProvider enemyGroupProvider;

    public EnemyGroupCombatTargetProvider(
        Transform ownerTransform,
        float reservationPenalty,
        IEnemyGroupProvider enemyGroupProvider)
    {
        this.ownerTransform = ownerTransform;
        this.reservationPenalty = reservationPenalty;
        this.enemyGroupProvider = enemyGroupProvider;
    }

    public ICombatTarget GetTarget()
    {
        EnemyGroupViewController currentGroup = enemyGroupProvider.CurrentTargetGroup;
        return currentGroup != null
            ? currentGroup.GetBestLivingEnemyTarget(ownerTransform.position, reservationPenalty)
            : null;
    }
}

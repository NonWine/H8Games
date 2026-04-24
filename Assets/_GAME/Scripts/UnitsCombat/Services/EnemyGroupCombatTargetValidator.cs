public class EnemyGroupCombatTargetValidator : ICombatTargetValidator
{
    private readonly IEnemyGroupProvider enemyGroupProvider;

    public EnemyGroupCombatTargetValidator(IEnemyGroupProvider enemyGroupProvider)
    {
        this.enemyGroupProvider = enemyGroupProvider;
    }

    public bool IsValid(ICombatTarget target)
    {
        EnemyGroupViewController currentGroup = enemyGroupProvider.CurrentTargetGroup;
        return currentGroup != null && currentGroup.ContainsEnemy(target);
    }
}

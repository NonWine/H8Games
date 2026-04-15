public interface IEnemyGroupProvider
{
    public EnemyGroupViewController CurrentTargetGroup { get; set; }
    public bool HasActiveEncounter { get; }
}
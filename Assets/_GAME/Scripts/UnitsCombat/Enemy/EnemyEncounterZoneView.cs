using UnityEngine;

//View for future improvements
public sealed class EnemyEncounterZoneView : MonoBehaviour
{
    [SerializeField] private EnemyGroupViewController enemyGroup;

    public EnemyGroupViewController EnemyGroup => enemyGroup;
    
}

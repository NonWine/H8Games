using UnityEngine;

public class EnemyEncounterZoneView : MonoBehaviour
{
    [SerializeField] private EnemyGroupViewController enemyGroup;

    public EnemyGroupViewController EnemyGroup => enemyGroup;
    
}

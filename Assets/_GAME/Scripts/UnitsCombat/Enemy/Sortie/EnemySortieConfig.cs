using UnityEngine;

[CreateAssetMenu(menuName = "Configs/Enemy Sortie Config", fileName = "EnemySortieConfig")]
public class EnemySortieConfig : ScriptableObject
{
    [Header("Decision")]
    [Tooltip("Chance that an enemy steps out of position at least once during a fight. Rolled once per fight.")]
    [Range(0f, 1f)]
    [SerializeField] private float chance = 0.9f;
    [Tooltip("Seconds after a fight starts before the first sortie, picked at random between X and Y.")]
    [SerializeField] private Vector2 firstSortieDelayRange = new(0.2f, 0.8f);
    [Tooltip("Seconds between sorties within the same fight, picked at random between X and Y.")]
    [SerializeField] private Vector2 repeatSortieDelayRange = new(1.5f, 3.5f);
    [Tooltip("Upper limit of sorties per fight.")]
    [Min(0)]
    [SerializeField] private int maxSortiesPerFight = 10;

    [Tooltip("Maximum enemies that can perform a sortie simultaneously.")]
    [Min(1)]
    [SerializeField] private int maxConcurrentSorties = 2;

    [Header("Step")]
    [Tooltip("Metres covered by a single sortie, picked at random between X and Y.")]
    [SerializeField] private Vector2 stepDistanceRange = new(2.5f, 4.5f);
    [Tooltip("A sortie shorter than this is skipped instead of shuffling on the spot.")]
    [Min(0.05f)]
    [SerializeField] private float minStepDistance = 0.4f;
    [Tooltip("The enemy never ends a sortie farther than this from its post.")]
    [Min(0f)]
    [SerializeField] private float leashRadius = 9f;
    [Tooltip("The enemy never walks closer than this to its target.")]
    [Min(0f)]
    [SerializeField] private float minDistanceToTarget = 3.8f;
    [Tooltip("Minimum distance between enemies when choosing a sortie destination to prevent bunching up.")]
    [Min(0.5f)]
    [SerializeField] private float minAlliedSeparation = 2.2f;
    [Tooltip("Random yaw applied to the heading, so enemies do not walk in a straight line at the target.")]
    [Range(0f, 90f)]
    [SerializeField] private float headingJitterAngle = 50f;

    [Header("Movement")]
    [Tooltip("Multiplier on UnitStats.MoveSpeed while stepping out.")]
    [Min(0.1f)]
    [SerializeField] private float sortieSpeedMultiplier = 1.2f;
    [Tooltip("Multiplier on UnitStats.MoveSpeed while walking back to the post.")]
    [Min(0.1f)]
    [SerializeField] private float returnSpeedMultiplier = 1f;
    [Tooltip("A sortie is cut short after this many seconds, in case the path is blocked.")]
    [Min(0.1f)]
    [SerializeField] private float maxSortieDuration = 3f;
    [Tooltip("The walk back is abandoned after this many seconds, in case the path is blocked.")]
    [Min(0.1f)]
    [SerializeField] private float maxReturnDuration = 6f;
    [Tooltip("Remaining path distance at which a destination counts as reached.")]
    [Min(0.01f)]
    [SerializeField] private float arriveThreshold = 0.15f;
    [Tooltip("An enemy farther than this from its post walks back after the fight.")]
    [Min(0.01f)]
    [SerializeField] private float postReachThreshold = 0.3f;
    [Tooltip("Degrees per second while turning on the way back and when settling into the post facing.")]
    [Min(30f)]
    [SerializeField] private float returnRotationSpeed = 360f;

    public float Chance => chance;
    public Vector2 FirstSortieDelayRange => firstSortieDelayRange;
    public Vector2 RepeatSortieDelayRange => repeatSortieDelayRange;
    public int MaxSortiesPerFight => maxSortiesPerFight;
    public int MaxConcurrentSorties => Mathf.Max(1, maxConcurrentSorties);

    public Vector2 StepDistanceRange => stepDistanceRange;
    public float MinStepDistance => minStepDistance;
    public float LeashRadius => leashRadius;
    public float MinDistanceToTarget => minDistanceToTarget;
    public float MinAlliedSeparation => minAlliedSeparation;
    public float HeadingJitterAngle => headingJitterAngle;

    public float SortieSpeedMultiplier => sortieSpeedMultiplier;
    public float ReturnSpeedMultiplier => returnSpeedMultiplier;
    public float MaxSortieDuration => maxSortieDuration;
    public float MaxReturnDuration => maxReturnDuration;
    public float ArriveThreshold => arriveThreshold;
    public float PostReachThreshold => postReachThreshold;
    public float ReturnRotationSpeed => returnRotationSpeed;
}

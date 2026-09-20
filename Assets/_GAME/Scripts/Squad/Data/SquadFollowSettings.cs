using UnityEngine;

[CreateAssetMenu(fileName = "SquadFollowSettings", menuName = "Gameplay/Squad/Squad Follow Settings")]
public class SquadFollowSettings : ScriptableObject
{
    [Header("Squad Root")]
    [Min(0.1f)]
    [SerializeField] private float rootMoveSpeed = 4.5f;
    [Min(0.1f)]
    [SerializeField] private float rootFollowSmoothness = 12f;
    [Min(0.1f)]
    [SerializeField] private float rootAcceleration = 6f;

    [Header("Formation")]
    [Min(1)]
    [SerializeField] private int maxSquadSize = 8;
    [Min(1)]
    [SerializeField] private int columns = 4;
    [Min(0.1f)]
    [SerializeField] private float spacingX = 1.2f;
    [Min(0.1f)]
    [SerializeField] private float spacingZ = 1.35f;

    [Header("Soldier Follow")]
    [Min(0.1f)]
    [SerializeField] private float soldierMoveSpeed = 5.5f;
    [Min(30f)]
    [SerializeField] private float soldierRotationSpeed = 540f;
    [Min(0.01f)]
    [SerializeField] private float slotReachThreshold = 0.12f;

    [Header("Moving Formation Variation")]
    [Min(0f)]
    [SerializeField] private float movingSlotOffsetRadius = 0.35f;
    [SerializeField] private Vector2 soldierMoveSpeedMultiplierRange = new(0.92f, 1.08f);
    [SerializeField] private Vector2 soldierRotationSpeedMultiplierRange = new(0.9f, 1.1f);
    [Min(0.01f)]
    [SerializeField] private float soldierFollowSmoothTime = 0.14f;
    [Range(0f, 1f)]
    [SerializeField] private float movingFacingToSlotWeight = 0.65f;
    [Min(0f)]
    [SerializeField] private float movingFacingYawJitter = 14f;
    [Min(0f)]
    [SerializeField] private float movingSwayAmplitude = 0.16f;
    [SerializeField] private Vector2 movingSwayFrequencyRange = new(0.3f, 0.65f);

    [Header("Soldier NavMesh")]
    [SerializeField] private Vector2 destinationRefreshIntervalRange = new(0.08f, 0.16f);
    [SerializeField] private Vector2Int avoidancePriorityRange = new(30, 70);
    [Min(0.05f)]
    [SerializeField] private float soldierAvoidanceRadius = 0.42f;

    [Header("Combat Positioning")]
    [Min(0.5f)]
    [SerializeField] private float engageRange = 5f;

    public float RootMoveSpeed => rootMoveSpeed;
    public float RootFollowSmoothness => rootFollowSmoothness;
    public float RootAcceleration => rootAcceleration;
    public int MaxSquadSize => Mathf.Max(1, maxSquadSize);
    public int Columns => Mathf.Max(1, columns);
    public float SpacingX => spacingX;
    public float SpacingZ => spacingZ;
    public float SoldierMoveSpeed => soldierMoveSpeed;
    public float SoldierRotationSpeed => soldierRotationSpeed;
    public float SlotReachThreshold => slotReachThreshold;
    public float MovingSlotOffsetRadius => movingSlotOffsetRadius;
    public float SoldierFollowSmoothTime => soldierFollowSmoothTime;
    public float MovingFacingToSlotWeight => movingFacingToSlotWeight;
    public float MovingFacingYawJitter => movingFacingYawJitter;
    public float MovingSwayAmplitude => movingSwayAmplitude;
    public float MovingSwayFrequencyMin => Mathf.Min(movingSwayFrequencyRange.x, movingSwayFrequencyRange.y);
    public float MovingSwayFrequencyMax => Mathf.Max(movingSwayFrequencyRange.x, movingSwayFrequencyRange.y);
    public float SoldierAvoidanceRadius => soldierAvoidanceRadius;
    public float EngageRange => engageRange;
    public float SoldierMoveSpeedMultiplierMin => Mathf.Min(soldierMoveSpeedMultiplierRange.x, soldierMoveSpeedMultiplierRange.y);
    public float SoldierMoveSpeedMultiplierMax => Mathf.Max(soldierMoveSpeedMultiplierRange.x, soldierMoveSpeedMultiplierRange.y);
    public float SoldierRotationSpeedMultiplierMin => Mathf.Min(soldierRotationSpeedMultiplierRange.x, soldierRotationSpeedMultiplierRange.y);
    public float SoldierRotationSpeedMultiplierMax => Mathf.Max(soldierRotationSpeedMultiplierRange.x, soldierRotationSpeedMultiplierRange.y);
    public float DestinationRefreshIntervalMin => Mathf.Min(destinationRefreshIntervalRange.x, destinationRefreshIntervalRange.y);
    public float DestinationRefreshIntervalMax => Mathf.Max(destinationRefreshIntervalRange.x, destinationRefreshIntervalRange.y);
    public int AvoidancePriorityMin => Mathf.Clamp(Mathf.Min(avoidancePriorityRange.x, avoidancePriorityRange.y), 0, 99);
    public int AvoidancePriorityMax => Mathf.Clamp(Mathf.Max(avoidancePriorityRange.x, avoidancePriorityRange.y), 0, 99);
}

using UnityEngine;

[CreateAssetMenu(fileName = "SquadFollowSettings", menuName = "Gameplay/Squad/Squad Follow Settings")]
public class SquadFollowSettings : ScriptableObject
{
    [Header("Squad Root")]
    [Min(0.1f)]
    [SerializeField] private float rootMoveSpeed = 4.5f;
    [Min(0.1f)]
    [SerializeField] private float rootFollowSmoothness = 12f;

    [Header("Formation")]
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
    [SerializeField] private float movingSlotOffsetRadius = 0.14f;
    [SerializeField] private Vector2 soldierMoveSpeedMultiplierRange = new(0.92f, 1.08f);
    [SerializeField] private Vector2 soldierRotationSpeedMultiplierRange = new(0.9f, 1.1f);
    [Min(0.01f)]
    [SerializeField] private float soldierFollowSmoothTime = 0.14f;
    [Range(0f, 1f)]
    [SerializeField] private float movingFacingToSlotWeight = 0.65f;
    [Min(0f)]
    [SerializeField] private float movingFacingYawJitter = 7f;

    public float RootMoveSpeed => rootMoveSpeed;
    public float RootFollowSmoothness => rootFollowSmoothness;
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
    public float SoldierMoveSpeedMultiplierMin => Mathf.Min(soldierMoveSpeedMultiplierRange.x, soldierMoveSpeedMultiplierRange.y);
    public float SoldierMoveSpeedMultiplierMax => Mathf.Max(soldierMoveSpeedMultiplierRange.x, soldierMoveSpeedMultiplierRange.y);
    public float SoldierRotationSpeedMultiplierMin => Mathf.Min(soldierRotationSpeedMultiplierRange.x, soldierRotationSpeedMultiplierRange.y);
    public float SoldierRotationSpeedMultiplierMax => Mathf.Max(soldierRotationSpeedMultiplierRange.x, soldierRotationSpeedMultiplierRange.y);
}

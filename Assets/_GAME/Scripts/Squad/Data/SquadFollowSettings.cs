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

    public float RootMoveSpeed => rootMoveSpeed;
    public float RootFollowSmoothness => rootFollowSmoothness;
    public int Columns => Mathf.Max(1, columns);
    public float SpacingX => spacingX;
    public float SpacingZ => spacingZ;
    public float SoldierMoveSpeed => soldierMoveSpeed;
    public float SoldierRotationSpeed => soldierRotationSpeed;
    public float SlotReachThreshold => slotReachThreshold;
}

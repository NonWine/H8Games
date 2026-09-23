using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "BarracksRevealConfig", menuName = "Config/BarracksRevealConfig")]
public class BarracksRevealConfig : ScriptableObject
{
    [Header("Anticipation (previous level)")]
    [SerializeField, Min(0.01f)] private float anticipationDuration = 0.18f;
    [SerializeField] private Vector3 anticipationSquash = new Vector3(1.06f, 0.88f, 1.06f);
    [SerializeField, Min(0f)] private float trembleStrength = 0.05f;
    [SerializeField, Min(1)] private int trembleVibrato = 25;
    [SerializeField] private SfxId anticipationSfx = SfxId.None;

    [Header("Building (new level)")]
    [SerializeField, Range(0f, 1f)] private float buildingStartScale = 0.85f;
    [SerializeField, Min(1f)] private float buildingOvershoot = 1.12f;
    [SerializeField, Min(0.01f)] private float buildingGrowDuration = 0.18f;
    [SerializeField, Min(0.01f)] private float buildingSettleDuration = 0.14f;
    [SerializeField, Min(0)] private int buildingDustCount = 14;
    [SerializeField, Min(0f)] private float buildingDustRadius = 2.2f;
    [SerializeField] private SfxId buildingSfx = SfxId.BarracksBuild;

    [Header("Props")]
    [SerializeField] private BarracksPropMotion crates = new BarracksPropMotion();
    [SerializeField] private BarracksPropMotion barrels = new BarracksPropMotion
    {
        StartTime = 0.26f, Stagger = 0.09f, Duration = 0.24f, StartHeight = 0.5f,
        MaxTilt = 8f, WobbleAngle = 12f, WobbleCycles = 2.5f, WobbleDuration = 0.35f,
        DustCount = 4, DustRadius = 0.3f, LandPitch = 1.1f,
    };
    [SerializeField] private BarracksPropMotion equipment = new BarracksPropMotion
    {
        StartTime = 0.42f, Stagger = 0.05f, Duration = 0.28f, StartHeight = -0.45f,
        MoveEase = Ease.OutBack, StartScale = 0.9f, MaxTilt = 0f,
        LandSquash = new Vector3(1.05f, 0.94f, 1.05f), LandSettleDuration = 0.14f,
        DustCount = 10, DustRadius = 0.9f, LandPitch = 0.75f,
    };

    [Header("Dust")]
    [SerializeField] private Vector2 dustSpeed = new Vector2(0.6f, 1.4f);
    [SerializeField, Min(0f)] private float dustUpwardSpeed = 0.35f;
    [SerializeField, Min(0f)] private float dustHeight = 0.1f;

    [Header("Completion")]
    [SerializeField] private SfxId completionSfx = SfxId.None;
    [SerializeField] private int tiltSeed = 7;

    public float AnticipationDuration => anticipationDuration;
    public Vector3 AnticipationSquash => anticipationSquash;
    public float TrembleStrength => trembleStrength;
    public int TrembleVibrato => trembleVibrato;
    public SfxId AnticipationSfx => anticipationSfx;
    public float BuildingStartScale => buildingStartScale;
    public float BuildingOvershoot => buildingOvershoot;
    public float BuildingGrowDuration => buildingGrowDuration;
    public float BuildingSettleDuration => buildingSettleDuration;
    public int BuildingDustCount => buildingDustCount;
    public float BuildingDustRadius => buildingDustRadius;
    public SfxId BuildingSfx => buildingSfx;
    public BarracksPropMotion Crates => crates;
    public BarracksPropMotion Barrels => barrels;
    public BarracksPropMotion Equipment => equipment;
    public Vector2 DustSpeed => dustSpeed;
    public float DustUpwardSpeed => dustUpwardSpeed;
    public float DustHeight => dustHeight;
    public SfxId CompletionSfx => completionSfx;
    public int TiltSeed => tiltSeed;
}

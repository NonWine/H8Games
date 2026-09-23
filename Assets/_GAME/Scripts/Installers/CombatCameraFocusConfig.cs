using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "CombatCameraFocusConfig", menuName = "Configs/Combat Camera Focus Config")]
public class CombatCameraFocusConfig : ScriptableObject
{
    [field: SerializeField] public bool Enabled { get; private set; } = true;
    [field: SerializeField, HideInInspector] public float OffsetScale { get; private set; } = 0.62f;
    [field: SerializeField, HideInInspector] public float HeightScale { get; private set; } = 0.9f;
    [field: SerializeField, Range(10f, 120f)] public float DefaultFieldOfView { get; private set; } = 60f;
    [field: SerializeField, HideInInspector] public float FieldOfView { get; private set; } = 50f;
    [field: SerializeField, Min(0.01f)] public float PositionSmoothTime { get; private set; } = 0.45f;
    [field: SerializeField, Min(0.01f)] public float LookAtSmoothTime { get; private set; } = 0.55f;
    [field: SerializeField, HideInInspector] public float CombatAnchorBlend { get; private set; } = 0.65f;
    [field: SerializeField, HideInInspector] public float TargetScreenWeight { get; private set; } = 0.85f;
    [field: SerializeField, HideInInspector] public float MaxTargetLookAhead { get; private set; } = 5.5f;
    [field: SerializeField, HideInInspector] public float EnterDuration { get; private set; } = 0.95f;
    [field: SerializeField, HideInInspector] public float ExitDuration { get; private set; } = 1.1f;
    [field: SerializeField, Range(0f, 1f)] public float TargetInfluence { get; private set; } = 0.25f;
    [field: SerializeField, Min(0f)] public float MaxAttentionOffset { get; private set; } = 2.5f;
    [field: SerializeField, Min(0.01f)] public float FocusInDuration { get; private set; } = 0.2f;
    [field: SerializeField, Min(0f)] public float FocusHoldDuration { get; private set; } = 0.35f;
    [field: SerializeField, Min(0.01f)] public float FocusOutDuration { get; private set; } = 0.4f;
    [field: SerializeField, Min(0.01f)] public float TargetSwitchSmoothTime { get; private set; } = 0.15f;
    [field: SerializeField, Min(0.1f)] public float AttackerMatchRadius { get; private set; } = 1.5f;
    [field: SerializeField] public bool UseFocusZoom { get; private set; } = true;
    [field: SerializeField, Range(0f, 0.2f)] public float FocusZoomPercent { get; private set; } = 0.05f;
    [field: SerializeField, Range(0f, 10f)] public float SwitchPunchDegrees { get; private set; } = 1f;
    [field: SerializeField, Min(0.01f)] public float SwitchPunchDuration { get; private set; } = 0.22f;
    [field: SerializeField] public Color SwitchFlashColor { get; private set; } = new Color(0.22f, 0.85f, 1f);
    [field: SerializeField, Min(0.01f)] public float SwitchFlashDuration { get; private set; } = 0.18f;
    [field: SerializeField, HideInInspector] public Ease EnterEase { get; private set; } = Ease.OutCubic;
    [field: SerializeField, HideInInspector] public Ease ExitEase { get; private set; } = Ease.InOutSine;

    public void UpdateRuntime(
        float fieldOfView,
        float enterDuration,
        float exitDuration)
    {
        FieldOfView = Mathf.Clamp(fieldOfView, 10f, 120f);
        EnterDuration = Mathf.Max(enterDuration, 0.01f);
        ExitDuration = Mathf.Max(exitDuration, 0.01f);
    }
}

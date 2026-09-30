using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "BarracksRevealConfig", menuName = "Config/BarracksRevealConfig")]
public class BarracksRevealConfig : ScriptableObject
{
    [SerializeField] private BarracksRevealPreset preset = BarracksRevealPreset.Balanced;

    [Tooltip("Only used when Preset is set to Custom. Right-click the asset header to bake the " +
             "selected preset into these fields and switch to Custom.")]
    [SerializeField] private BarracksRevealSettings custom = new BarracksRevealSettings();

    [SerializeField] private int tiltSeed = 7;

    [System.NonSerialized] private BarracksRevealSettings resolved;
    [System.NonSerialized] private BarracksRevealPreset resolvedFor = (BarracksRevealPreset)(-1);

    // Resolved once per preset change rather than per access: the view reads these
    // getters from inside per-frame tween callbacks.
    public BarracksRevealSettings Active
    {
        get
        {
            if (resolved == null || resolvedFor != preset)
            {
                resolved = preset == BarracksRevealPreset.Custom ? custom : BarracksRevealPresets.Create(preset);
                resolvedFor = preset;
            }

            return resolved;
        }
    }

    public BarracksRevealPreset Preset => preset;

    public float AnticipationDuration => Active.AnticipationDuration;
    public Vector3 AnticipationSquash => Active.AnticipationSquash;
    public float TrembleStrength => Active.TrembleStrength;
    public int TrembleVibrato => Active.TrembleVibrato;
    public SfxId AnticipationSfx => Active.AnticipationSfx;

    public float BuildingJumpUpDuration => Active.BuildingJumpUpDuration;
    public float BuildingJumpHeight => Active.BuildingJumpHeight;
    public Ease BuildingJumpUpEase => Active.BuildingJumpUpEase;
    public Vector3 BuildingLaunchStretch => Active.BuildingLaunchStretch;
    public SfxId BuildingLaunchSfx => Active.BuildingLaunchSfx;

    public float BuildingApexDuration => Active.BuildingApexDuration;
    public float BuildingApexOvershoot => Active.BuildingApexOvershoot;
    public float BuildingApexSpin => Active.BuildingApexSpin;
    public float BuildingApexSpinCycles => Active.BuildingApexSpinCycles;
    public Vector3 BuildingApexSpinAxis => Active.BuildingApexSpinAxis;

    public float BuildingFallDuration => Active.BuildingFallDuration;
    public Ease BuildingFallEase => Active.BuildingFallEase;
    public Vector3 BuildingLandSquash => Active.BuildingLandSquash;
    public float BuildingLandSettleDuration => Active.BuildingLandSettleDuration;
    public int BuildingDustCount => Active.BuildingDustCount;
    public float BuildingDustRadius => Active.BuildingDustRadius;
    public SfxId BuildingLandSfx => Active.BuildingLandSfx;

    public BarracksPropMotion Crates => Active.Crates;
    public BarracksPropMotion Barrels => Active.Barrels;
    public BarracksPropMotion Equipment => Active.Equipment;

    public Vector2 DustSpeed => Active.DustSpeed;
    public float DustUpwardSpeed => Active.DustUpwardSpeed;
    public float DustHeight => Active.DustHeight;

    public SfxId CompletionSfx => Active.CompletionSfx;
    public int TiltSeed => tiltSeed;

    private void OnValidate()
    {
        resolved = null;
    }

#if UNITY_EDITOR
    [ContextMenu("Bake Preset Into Custom")]
    private void BakePresetIntoCustom()
    {
        custom = BarracksRevealPresets.Create(preset).Clone();
        preset = BarracksRevealPreset.Custom;
        resolved = null;

        UnityEditor.EditorUtility.SetDirty(this);
        UnityEditor.AssetDatabase.SaveAssetIfDirty(this);
    }
#endif
}

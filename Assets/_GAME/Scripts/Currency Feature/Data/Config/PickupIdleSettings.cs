using System;
using UnityEngine;

[Serializable]
public class PickupIdleSettings
{
    [SerializeField] private bool enabled = true;
    [SerializeField, Min(0f)] private float settleDuration = 0.25f;
    [SerializeField, Min(0f)] private float maxLinearSpeed = 0.08f;
    [SerializeField, Min(0f)] private float maxAngularSpeed = 0.3f;
    [SerializeField, Range(0f, 1f)] private float minSupportNormal = 0.5f;
    [SerializeField] private LayerMask supportMask = 1;
    [SerializeField] private Vector3 faceDirection = new Vector3(0f, 0.8f, -0.6f);

    [Header("Animation Preset")]
    [SerializeField] private PickupIdlePreset preset = PickupIdlePreset.Balanced;
    [Tooltip("Only used when Preset is set to Custom. Use the config's context menu to bake the " +
             "selected preset into these fields and switch to Custom.")]
    [SerializeField] private PickupIdleAnimationSettings custom = new PickupIdleAnimationSettings();

    [NonSerialized] private PickupIdleAnimationSettings resolved;
    [NonSerialized] private PickupIdlePreset resolvedFor = (PickupIdlePreset)(-1);

    private PickupIdleAnimationSettings Active
    {
        get
        {
            if (resolved == null || resolvedFor != preset)
            {
                resolved = preset == PickupIdlePreset.Custom ? custom : PickupIdlePresets.Create(preset);
                resolvedFor = preset;
            }

            return resolved;
        }
    }

    public bool Enabled => enabled;
    public Vector2 Interval => Active.Interval;
    public float Duration => Active.Duration;
    public float SettleDuration => settleDuration;
    public float MaxLinearSpeed => maxLinearSpeed;
    public float MaxAngularSpeed => maxAngularSpeed;
    public float MinSupportNormal => minSupportNormal;
    public LayerMask SupportMask => supportMask;
    public float HopHeight => Active.HopHeight;
    public float TiltDegrees => Active.TiltDegrees;
    public Vector3 FaceDirection => faceDirection;
    public float ShineTime => Active.ShineTime;
    public AnimationCurve Height => Active.Height;
    public AnimationCurve Tilt => Active.Tilt;

    public void InvalidateCache()
    {
        resolved = null;
    }

    public void BakePresetIntoCustom()
    {
        custom = PickupIdlePresets.Create(preset).Clone();
        preset = PickupIdlePreset.Custom;
        resolved = null;
    }
}

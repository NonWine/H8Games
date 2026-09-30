using System;
using UnityEngine;

[Serializable]
public class PickupSpendSettings
{
    [SerializeField] private PickupSpendPreset preset = PickupSpendPreset.Balanced;
    [Tooltip("Only used when Preset is set to Custom. Use the config's context menu to bake the " +
             "selected preset into these fields and switch to Custom.")]
    [SerializeField] private PickupSpendAnimationSettings custom = new PickupSpendAnimationSettings();

    [NonSerialized] private PickupSpendAnimationSettings resolved;
    [NonSerialized] private PickupSpendPreset resolvedFor = (PickupSpendPreset)(-1);

    private PickupSpendAnimationSettings Active
    {
        get
        {
            if (resolved == null || resolvedFor != preset)
            {
                resolved = preset == PickupSpendPreset.Custom ? custom : PickupSpendPresets.Create(preset);
                resolvedFor = preset;
            }

            return resolved;
        }
    }

    public Vector2 ArcMultiplier => Active.ArcMultiplier;
    public float LateralOffset => Active.LateralOffset;
    public Vector2 SpinMultiplier => Active.SpinMultiplier;
    public float InitialTilt => Active.InitialTilt;
    public AnimationCurve Scale => Active.Scale;

    public void InvalidateCache()
    {
        resolved = null;
    }

    public void BakePresetIntoCustom()
    {
        custom = PickupSpendPresets.Create(preset).Clone();
        preset = PickupSpendPreset.Custom;
        resolved = null;
    }
}

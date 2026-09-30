using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class BarracksUpgradeZoneAnimationSettings
{
    [Header("Panel Show / Hide")]
    [Min(0.01f)] public float ShowDuration = 0.18f;
    [Min(1f)] public float ShowOvershootScale = 1.2f;
    public Ease ShowEase = Ease.OutBack;
    [Min(0.01f)] public float HideDuration = 0.2f;
    public Ease HideEase = Ease.Linear;

    [Header("Fill")]
    [Min(0.01f)] public float FillDuration = 0.2f;
    public Ease FillEase = Ease.OutBack;

    [Header("Toss Pitch")]
    [Min(0.1f)] public float BaseTossPitch = 1f;
    [Min(0f)] public float TossPitchStep = 0.03f;
    [Min(0.1f)] public float MaxTossPitch = 1.6f;

    [Header("Arrival Punch")]
    [Min(0f)] public float LabelPunch = 0.18f;
    [Min(0.01f)] public float LabelPunchDuration = 0.12f;
    [Min(0)] public int LabelPunchVibrato = 1;
    [Range(0f, 1f)] public float LabelPunchElasticity = 0.75f;
    [Min(0f)] public float ArrivalAccentInterval = 0.09f;
    [Min(1)] public int ArrivalParticleCount = 1;
    [Min(1f)] public float FinalArrivalMultiplier = 1.6f;

    [Header("Completion Shake")]
    [Min(0f)] public float CompletionShakeStrength = 0.4f;
    [Min(0.01f)] public float CompletionShakeDuration = 0.3f;
    [Min(1)] public int CompletionShakeVibrato = 10;

    public BarracksUpgradeZoneAnimationSettings Clone()
    {
        return JsonUtility.FromJson<BarracksUpgradeZoneAnimationSettings>(JsonUtility.ToJson(this));
    }
}

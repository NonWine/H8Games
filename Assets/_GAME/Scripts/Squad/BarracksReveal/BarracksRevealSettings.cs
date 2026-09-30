using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class BarracksRevealSettings
{
    [Header("Anticipation (previous level)")]
    [Min(0.01f)] public float AnticipationDuration = 0.15f;
    public Vector3 AnticipationSquash = new Vector3(1.06f, 0.88f, 1.06f);
    [Min(0f)] public float TrembleStrength = 0.05f;
    [Min(1)] public int TrembleVibrato = 25;
    public SfxId AnticipationSfx = SfxId.None;

    [Header("Building - Launch")]
    [Min(0.01f)] public float BuildingJumpUpDuration = 0.16f;
    [Min(0f)] public float BuildingJumpHeight = 0.9f;
    public Ease BuildingJumpUpEase = Ease.OutQuad;
    public Vector3 BuildingLaunchStretch = new Vector3(0.9f, 1.2f, 0.9f);
    public SfxId BuildingLaunchSfx = SfxId.None;

    [Header("Building - Apex (air time, props reveal here)")]
    [Min(0.01f)] public float BuildingApexDuration = 0.32f;
    [Min(1f)] public float BuildingApexOvershoot = 1.18f;
    [Range(0f, 45f)] public float BuildingApexSpin = 12f;
    [Min(0f)] public float BuildingApexSpinCycles = 1.5f;
    public Vector3 BuildingApexSpinAxis = Vector3.up;

    [Header("Building - Landing")]
    [Min(0.01f)] public float BuildingFallDuration = 0.14f;
    public Ease BuildingFallEase = Ease.InQuad;
    public Vector3 BuildingLandSquash = new Vector3(1.22f, 0.78f, 1.22f);
    [Min(0.01f)] public float BuildingLandSettleDuration = 0.16f;
    [Min(0)] public int BuildingDustCount = 14;
    [Min(0f)] public float BuildingDustRadius = 2.2f;
    public SfxId BuildingLandSfx = SfxId.BarracksBuild;

    [Header("Props")]
    public BarracksPropMotion Crates = new BarracksPropMotion
    {
        AirHeight = 0.5f, RevealStartTime = 0f, RevealStagger = 0.06f, RevealDuration = 0.16f,
        HopHeight = 0.12f, HopDuration = 0.18f, HopStretch = 1.15f,
        HopSpinAngle = 14f, HopSpinCycles = 1.5f, HopSpinAxis = Vector3.up,
        LandStagger = 0.05f, FallDuration = 0.14f, FallEase = Ease.InQuad,
        LandSquash = new Vector3(1.15f, 0.82f, 1.15f), LandSettleDuration = 0.14f,
        DustCount = 4, DustRadius = 0.35f, LandPitch = 1f,
    };
    public BarracksPropMotion Barrels = new BarracksPropMotion
    {
        AirHeight = 0.45f, RevealStartTime = 0.08f, RevealStagger = 0.07f, RevealDuration = 0.18f,
        HopHeight = 0.12f, HopDuration = 0.2f, HopStretch = 1.15f,
        HopSpinAngle = 10f, HopSpinCycles = 1.5f, HopSpinAxis = Vector3.forward,
        LandStagger = 0.05f, FallDuration = 0.14f, FallEase = Ease.InQuad,
        LandSquash = new Vector3(1.12f, 0.85f, 1.12f), LandSettleDuration = 0.14f,
        DustCount = 4, DustRadius = 0.3f, LandPitch = 1.1f,
    };
    public BarracksPropMotion Equipment = new BarracksPropMotion
    {
        AirHeight = 0.35f, RevealStartTime = 0.16f, RevealStagger = 0.05f, RevealDuration = 0.2f,
        HopHeight = 0.08f, HopDuration = 0.22f, HopStretch = 1.08f,
        HopSpinAngle = 6f, HopSpinCycles = 1.2f, HopSpinAxis = Vector3.up,
        LandStagger = 0.04f, FallDuration = 0.16f, FallEase = Ease.InQuad,
        LandSquash = new Vector3(1.06f, 0.93f, 1.06f), LandSettleDuration = 0.16f,
        DustCount = 10, DustRadius = 0.9f, LandPitch = 0.75f,
    };

    [Header("Dust")]
    public Vector2 DustSpeed = new Vector2(0.6f, 1.4f);
    [Min(0f)] public float DustUpwardSpeed = 0.35f;
    [Min(0f)] public float DustHeight = 0.1f;

    [Header("Completion")]
    public SfxId CompletionSfx = SfxId.None;

    public BarracksRevealSettings Clone()
    {
        return JsonUtility.FromJson<BarracksRevealSettings>(JsonUtility.ToJson(this));
    }
}

using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class BarracksPropMotion
{
    [Header("Timing")]
    [Min(0f)] public float StartTime = 0.12f;
    [Min(0f)] public float Stagger = 0.07f;
    [Min(0.01f)] public float Duration = 0.22f;

    [Header("Entry")]
    public float StartHeight = 0.6f;
    public Ease MoveEase = Ease.InQuad;
    [Range(0f, 1f)] public float StartScale = 0.6f;
    [Range(0f, 45f)] public float MaxTilt = 12f;

    [Header("Landing")]
    public Vector3 LandSquash = new Vector3(1.12f, 0.85f, 1.12f);
    [Min(0.01f)] public float LandSettleDuration = 0.1f;
    [Range(0f, 30f)] public float WobbleAngle = 0f;
    [Min(0f)] public float WobbleCycles = 2.5f;
    [Min(0.01f)] public float WobbleDuration = 0.35f;

    [Header("Dust")]
    [Min(0)] public int DustCount = 4;
    [Min(0f)] public float DustRadius = 0.35f;

    [Header("Audio")]
    public SfxId LandSfx = SfxId.BarracksPropLand;
    [Range(0.1f, 3f)] public float LandPitch = 1f;
}

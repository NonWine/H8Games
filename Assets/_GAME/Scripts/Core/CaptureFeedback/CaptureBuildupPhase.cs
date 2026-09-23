using System;
using UnityEngine;

[Serializable]
public struct CaptureBuildupPhase
{
    [Range(0f, 1f)] public float Progress;

    [Header("Pulse")]
    [Min(0.05f)] public float PulseInterval;
    [Range(0f, 1f)] public float PulseStrength;
    [Range(0f, 1f)] public float Tension;
    [Range(0.1f, 3f)] public float TickPitch;

    [Header("Shake")]
    [Min(0f)] public float ShakePosition;
    [Min(0f)] public float ShakeRotation;
    [Min(0f)] public float ShakeFrequency;

    public bool HasShake => ShakePosition > 0f || ShakeRotation > 0f;

    public static CaptureBuildupPhase Lerp(CaptureBuildupPhase from, CaptureBuildupPhase to, float t)
    {
        return new CaptureBuildupPhase
        {
            Progress = Mathf.Lerp(from.Progress, to.Progress, t),
            PulseInterval = Mathf.Lerp(from.PulseInterval, to.PulseInterval, t),
            PulseStrength = Mathf.Lerp(from.PulseStrength, to.PulseStrength, t),
            Tension = Mathf.Lerp(from.Tension, to.Tension, t),
            TickPitch = Mathf.Lerp(from.TickPitch, to.TickPitch, t),
            ShakePosition = Mathf.Lerp(from.ShakePosition, to.ShakePosition, t),
            ShakeRotation = Mathf.Lerp(from.ShakeRotation, to.ShakeRotation, t),
            ShakeFrequency = Mathf.Lerp(from.ShakeFrequency, to.ShakeFrequency, t),
        };
    }
}

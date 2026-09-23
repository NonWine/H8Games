using System;
using UnityEngine;

// One tunable shake "shape". Serialized inline inside CameraShakeConfig so every
// shake in the game is authored from a single asset instead of from code.
[Serializable]
public class CameraShakeSettings
{
    [Tooltip("How long the shake lives, in seconds.")]
    [Min(0f)] public float Duration = 0.22f;

    [Tooltip("Peak positional offset, in world units, at envelope value 1.")]
    [Min(0f)] public float PositionAmplitude = 0.16f;

    [Tooltip("Peak rotational offset, in degrees, at envelope value 1. Reads stronger " +
             "than positional shake on a distant follow camera, so keep it small.")]
    [Min(0f)] public float RotationAmplitude = 0.8f;

    [Tooltip("Noise oscillations per second. Low values feel like a heavy rumble, " +
             "high values like a sharp hit.")]
    [Min(0f)] public float Frequency = 24f;

    [Tooltip("One-directional push away from the damage source, on top of the noise. " +
             "Set to 0 for a pure omnidirectional shake.")]
    [Min(0f)] public float DirectionalPunch = 0.10f;

    [Tooltip("How much of the noise is allowed along the camera's forward axis. " +
             "Full depth shake reads as a zoom wobble, so this is damped by default.")]
    [Range(0f, 1f)] public float DepthInfluence = 0.25f;

    [Tooltip("Amplitude over the shake's lifetime. X is normalized time 0..1, Y scales " +
             "both position and rotation. A fast attack with a long tail feels punchiest.")]
    public AnimationCurve Envelope = new AnimationCurve(
        new Keyframe(0f, 1f, 0f, -2.5f),
        new Keyframe(1f, 0f, -0.5f, 0f));
}

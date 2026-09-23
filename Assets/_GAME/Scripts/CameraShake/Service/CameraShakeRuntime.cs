using System.Collections.Generic;
using UnityEngine;

// The shake math: owns the live shakes and folds them into a single offset per
// frame. Works entirely in camera-local space and touches no Cinemachine type
// and no MonoBehaviour, so the feel can be unit tested without a scene.
public class CameraShakeRuntime
{
    // Pitch and yaw wobble read as camera drift rather than impact, so they get a
    // fraction of the authored rotation while roll takes the full amount.
    private const float TiltRatio = 0.35f;

    // Perlin noise returns 0 at whole coordinates, which would make every shake
    // start from a dead frame; a random offset keeps each axis off the lattice.
    private const float NoiseSeedRange = 1000f;

    // Perlin only wanders about a third of the way to its extremes over the few
    // frames a hit shake lives, so raw output makes every authored amplitude feel
    // roughly three times weaker than the number says. The gain stretches a
    // typical excursion to full scale and the clamp keeps the rare spike from
    // overshooting the authored ceiling.
    private const float NoiseGain = 2.5f;

    private struct ActiveShake
    {
        public CameraShakeSettings Settings;
        public Vector3 LocalDirection;
        public float Scale;
        public float Elapsed;
        public float SeedX;
        public float SeedY;
        public float SeedZ;
        public float SeedRoll;
    }

    private readonly List<ActiveShake> shakes = new List<ActiveShake>();

    public int ActiveCount => shakes.Count;

    // localDirection is expressed in camera space; Vector3.zero means no
    // directional punch, only noise.
    public void Add(CameraShakeSettings settings, float scale, Vector3 localDirection, int maxConcurrent)
    {
        if (settings.Duration <= 0f || scale <= 0f)
        {
            return;
        }

        int limit = Mathf.Max(1, maxConcurrent);
        while (shakes.Count >= limit)
        {
            RemoveWeakest();
        }

        shakes.Add(new ActiveShake
        {
            Settings = settings,
            LocalDirection = localDirection,
            Scale = scale,
            Elapsed = 0f,
            SeedX = Random.value * NoiseSeedRange,
            SeedY = Random.value * NoiseSeedRange,
            SeedZ = Random.value * NoiseSeedRange,
            SeedRoll = Random.value * NoiseSeedRange
        });
    }

    public void Advance(float deltaTime, out Vector3 localPositionOffset, out Quaternion localRotationOffset)
    {
        localPositionOffset = Vector3.zero;
        localRotationOffset = Quaternion.identity;

        if (shakes.Count == 0)
        {
            return;
        }

        float roll = 0f;
        Vector3 tilt = Vector3.zero;

        for (int i = shakes.Count - 1; i >= 0; i--)
        {
            ActiveShake shake = shakes[i];
            shake.Elapsed += Mathf.Max(0f, deltaTime);

            float normalizedTime = shake.Elapsed / shake.Settings.Duration;
            if (normalizedTime >= 1f)
            {
                shakes.RemoveAt(i);
                continue;
            }

            shakes[i] = shake;

            float amplitude = EvaluateEnvelope(shake.Settings.Envelope, normalizedTime) * shake.Scale;
            if (amplitude <= 0f)
            {
                continue;
            }

            float noiseTime = shake.Elapsed * shake.Settings.Frequency;
            float x = Noise(shake.SeedX, noiseTime);
            float y = Noise(shake.SeedY, noiseTime);
            float z = Noise(shake.SeedZ, noiseTime) * shake.Settings.DepthInfluence;

            localPositionOffset += new Vector3(x, y, z) * (shake.Settings.PositionAmplitude * amplitude);
            localPositionOffset += shake.LocalDirection * (shake.Settings.DirectionalPunch * amplitude);

            float rotation = shake.Settings.RotationAmplitude * amplitude;
            roll += Noise(shake.SeedRoll, noiseTime) * rotation;
            tilt += new Vector3(y, x, 0f) * (rotation * TiltRatio);
        }

        localRotationOffset = Quaternion.Euler(tilt.x, tilt.y, roll);
    }

    public void Clear()
    {
        shakes.Clear();
    }

    // Evicts on remaining energy rather than on remaining time: a nearly spent
    // heavy shake still contributes more than a fresh chip hit, so lifetime alone
    // is the wrong thing to drop.
    private void RemoveWeakest()
    {
        int weakestIndex = 0;
        float weakestEnergy = float.MaxValue;

        for (int i = 0; i < shakes.Count; i++)
        {
            ActiveShake shake = shakes[i];
            float remaining = 1f - Mathf.Clamp01(shake.Elapsed / shake.Settings.Duration);
            float energy = shake.Scale * shake.Settings.PositionAmplitude * remaining;

            if (energy < weakestEnergy)
            {
                weakestEnergy = energy;
                weakestIndex = i;
            }
        }

        shakes.RemoveAt(weakestIndex);
    }

    private static float EvaluateEnvelope(AnimationCurve curve, float normalizedTime)
    {
        if (curve.length == 0)
        {
            return 1f - normalizedTime;
        }

        return Mathf.Max(0f, curve.Evaluate(normalizedTime));
    }

    private static float Noise(float seed, float time)
    {
        return Mathf.Clamp((Mathf.PerlinNoise(seed, time) * 2f - 1f) * NoiseGain, -1f, 1f);
    }
}

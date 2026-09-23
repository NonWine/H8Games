using UnityEngine;

public class PickupIdleHandler
{
    private readonly Transform visualRoot;
    private readonly PickupPhysicsHandler physics;
    private readonly ParticleSystem shine;
    private readonly Vector3 authoredPosition;
    private readonly Quaternion authoredRotation;
    private readonly System.Random random = new System.Random(System.Guid.NewGuid().GetHashCode());
    private PickupIdleSettings settings;
    private Quaternion hopRotation;
    private float settledTime;
    private float delay;
    private float elapsed;
    private bool enabled;
    private bool playing;
    private bool shinePlayed;

    public PickupIdleHandler(Transform visualRoot, PickupPhysicsHandler physics, ParticleSystem shine)
    {
        this.visualRoot = visualRoot;
        this.physics = physics;
        this.shine = shine;
        authoredPosition = visualRoot.localPosition;
        authoredRotation = visualRoot.localRotation;
    }

    public void Begin(PickupIdleSettings settings)
    {
        Stop();
        this.settings = settings;
        enabled = settings.Enabled;
        Schedule();
    }

    public void Stop()
    {
        enabled = false;
        settledTime = 0f;
        playing = false;
        RestorePose();
        if (shine != null)
            shine.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void Tick(float deltaTime)
    {
        if (!enabled)
            return;

        if (!physics.IsSettled(settings))
        {
            settledTime = 0f;
            if (playing)
            {
                RestorePose();
                if (shine != null)
                    shine.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                Schedule();
            }
            return;
        }

        settledTime += deltaTime;
        if (settledTime < settings.SettleDuration)
            return;

        if (!playing)
        {
            delay -= deltaTime;
            if (delay > 0f)
                return;
            BeginHop();
        }

        elapsed += deltaTime;
        float progress = settings.Duration > 0f ? Mathf.Clamp01(elapsed / settings.Duration) : 1f;
        visualRoot.localPosition = authoredPosition + visualRoot.parent.InverseTransformVector(Vector3.up * (settings.HopHeight * settings.Height.Evaluate(progress)));
        visualRoot.localRotation = Quaternion.SlerpUnclamped(authoredRotation, hopRotation, settings.Tilt.Evaluate(progress));
        if (!shinePlayed && progress >= settings.ShineTime)
        {
            shinePlayed = true;
            if (shine != null)
                shine.Play(true);
        }

        if (progress >= 1f)
        {
            RestorePose();
            Schedule();
        }
    }

    private void BeginHop()
    {
        playing = true;
        elapsed = 0f;
        shinePlayed = false;
        Quaternion worldRotation = visualRoot.parent.rotation * authoredRotation;
        Quaternion target = Quaternion.FromToRotation(worldRotation * Vector3.forward, settings.FaceDirection.normalized) * worldRotation;
        hopRotation = Quaternion.Inverse(visualRoot.parent.rotation) * Quaternion.RotateTowards(worldRotation, target, settings.TiltDegrees);
    }

    private void Schedule()
    {
        playing = false;
        delay = Mathf.Lerp(settings.Interval.x, settings.Interval.y, (float)random.NextDouble());
    }

    private void RestorePose()
    {
        if (visualRoot == null)
            return;
        visualRoot.localPosition = authoredPosition;
        visualRoot.localRotation = authoredRotation;
    }
}

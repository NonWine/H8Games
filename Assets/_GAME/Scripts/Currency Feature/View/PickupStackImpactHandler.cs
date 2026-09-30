using UnityEngine;

public class PickupStackImpactHandler
{
    private readonly Transform visualRoot;
    private readonly Vector3   authoredScale;
    private readonly System.Random random = new System.Random(System.Guid.NewGuid().GetHashCode());

    private PickupStackImpactSettings settings;
    private Vector3 tiltAxis;
    private int     thicknessAxis;
    private float   strength;
    private float   dipStrength;
    private float   delay;
    private float   elapsed;
    private bool    playing;

    public PickupStackImpactHandler(Transform visualRoot)
    {
        this.visualRoot = visualRoot;
        authoredScale   = visualRoot.localScale;
    }

    public void Play(PickupStackImpactSettings settings, float strength, float dipStrength, float delay)
    {
        if (settings == null || !settings.Enabled || strength <= 0f)
            return;

        this.settings    = settings;
        this.strength    = strength;
        this.dipStrength = dipStrength;
        this.delay       = delay;

        elapsed = 0f;
        playing = true;

        var localUp = visualRoot.InverseTransformDirection(Vector3.up).normalized;

        thicknessAxis = DominantAxis(localUp);
        tiltAxis      = SampleAxisPerpendicularTo(localUp);
    }

    public void Stop()
    {
        if (!playing)
            return;

        playing  = false;
        settings = null;

        if (visualRoot != null)
            visualRoot.localScale = authoredScale;
    }

    public void Tick(float deltaTime)
    {
        if (!playing)
            return;

        if (delay > 0f)
        {
            delay -= deltaTime;
            return;
        }

        elapsed += deltaTime;

        var progress = Mathf.Clamp01(elapsed / settings.Duration);

        Apply(settings.Response.Evaluate(progress));

        if (progress >= 1f)
            Stop();
    }

    private void Apply(float impulse)
    {
        var parent = visualRoot.parent;
        var dip    = settings.SlotCompression * dipStrength * impulse;

        if (parent != null && !Mathf.Approximately(dip, 0f))
            visualRoot.localPosition += parent.InverseTransformVector(Vector3.down * dip);

        var local    = strength * impulse;
        var compress = Mathf.Max(0f, local);
        var rebound  = Mathf.Max(0f, -local) * settings.Overshoot;

        if (settings.TiltDegrees > 0f)
            visualRoot.localRotation *= Quaternion.AngleAxis(settings.TiltDegrees * local, tiltAxis);

        var squash = Vector3.one * (1f + settings.FaceExpand * compress + rebound);

        squash[thicknessAxis] = 1f - settings.Flatten * compress + rebound * 2f;

        visualRoot.localScale = Vector3.Scale(authoredScale, squash);
    }

    private static int DominantAxis(Vector3 direction)
    {
        var x = Mathf.Abs(direction.x);
        var y = Mathf.Abs(direction.y);
        var z = Mathf.Abs(direction.z);

        if (x >= y && x >= z)
            return 0;

        return y >= z ? 1 : 2;
    }

    private Vector3 SampleAxisPerpendicularTo(Vector3 up)
    {
        var reference = Mathf.Abs(Vector3.Dot(up, Vector3.right)) > 0.9f ? Vector3.forward : Vector3.right;
        var first     = Vector3.Cross(up, reference).normalized;
        var second    = Vector3.Cross(up, first).normalized;
        var angle     = (float)random.NextDouble() * Mathf.PI * 2f;

        return first * Mathf.Cos(angle) + second * Mathf.Sin(angle);
    }
}

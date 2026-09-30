using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class PickupCarryStackMotion : IPickupCarryStackSpring, ITickable
{
    private const float MaxIntegrationStep  = 1f / 120f;
    private const int   MaxIntegrationSteps = 8;

    private struct Link
    {
        public Vector3 Offset;
        public Vector3 Velocity;
        public int     Stamp;
    }

    private readonly IPickupCarrySink           sink;
    private readonly IPickupCarryAnchorProvider anchorProvider;
    private readonly PickupServiceConfig        config;

    private readonly Dictionary<PickupItemController, Link> links = new();
    private readonly List<PickupItemController>             stale = new();

    private const float AccelerationSmoothing = 0.2f;

    private Vector3 lastAnchorPosition;
    private Vector3 smoothedVelocity;
    private Vector3 smoothedAcceleration;
    private Vector3 lean;
    private int     stamp;
    private bool    hasAnchorSample;

    public PickupCarryStackMotion(
        IPickupCarrySink           sink,
        IPickupCarryAnchorProvider anchorProvider,
        PickupServiceConfig        config)
    {
        this.sink           = sink;
        this.anchorProvider = anchorProvider;
        this.config         = config;
    }

    public void AddImpulse(Vector3 worldImpulse, int slotIndex)
    {
        var carried = sink.Carried;

        if (slotIndex < 0 || slotIndex >= carried.Count)
            return;

        var horizontal = Vector3.ProjectOnPlane(worldImpulse, Vector3.up);
        var topShare   = config.CarrySway.TopShare;

        for (var i = 0; i <= slotIndex; i++)
        {
            if (!links.TryGetValue(carried[i], out var link))
                continue;

            link.Velocity     += horizontal * SlotInfluence(i, carried.Count, topShare);
            links[carried[i]]  = link;
        }
    }

    public void Tick()
    {
        var settings  = config.CarrySway;
        var deltaTime = Time.deltaTime;

        if (!settings.Enabled || deltaTime <= 0f)
            return;

        if (!anchorProvider.TryGetAnchor(out var anchor))
        {
            hasAnchorSample = false;
            return;
        }

        stamp++;

        SampleLean(settings, anchor.position, deltaTime);
        SimulateChain(settings, deltaTime);
        PruneReleasedLinks();
    }

    private void SampleLean(PickupCarrySwaySettings settings, Vector3 anchorPosition, float deltaTime)
    {
        if (!hasAnchorSample)
        {
            lastAnchorPosition = anchorPosition;
            hasAnchorSample    = true;
        }

        var velocity         = Vector3.ProjectOnPlane((anchorPosition - lastAnchorPosition) / deltaTime, Vector3.up);
        var previousVelocity = smoothedVelocity;

        lastAnchorPosition = anchorPosition;
        smoothedVelocity   = Vector3.Lerp(smoothedVelocity, velocity, settings.VelocitySmoothing);

        smoothedAcceleration = Vector3.Lerp(
            smoothedAcceleration, (smoothedVelocity - previousVelocity) / deltaTime, AccelerationSmoothing);

        lean = Vector3.ClampMagnitude(
            -(smoothedVelocity * settings.LeanPerSpeed + smoothedAcceleration * settings.LeanPerAcceleration),
            settings.MaxLeanRatio);
    }

    private void SimulateChain(PickupCarrySwaySettings settings, float deltaTime)
    {
        var carried = sink.Carried;

        if (carried.Count == 0)
            return;

        var influenceSum = 0f;

        for (var i = 0; i < carried.Count; i++)
            influenceSum += SlotInfluence(i, carried.Count, settings.TopShare);

        var leaningHeight = settings.TopShare * carried.Count * config.CarrySlotSpacing;
        var leanPerSlot   = influenceSum > 0.0001f ? lean * (leaningHeight / influenceSum) : Vector3.zero;
        var maxLink       = settings.LinkClearance * config.CarrySlotSpacing;
        var steps         = Mathf.Clamp(Mathf.CeilToInt(deltaTime / MaxIntegrationStep), 1, MaxIntegrationSteps);
        var stepTime      = deltaTime / steps;

        for (var step = 0; step < steps; step++)
            IntegrateStep(settings, carried, stepTime, leanPerSlot, maxLink);

        ApplyToStack(settings, carried);
    }

    private void IntegrateStep(
        PickupCarrySwaySettings settings,
        IReadOnlyList<PickupItemController> carried,
        float stepTime,
        Vector3 leanPerSlot,
        float maxLink)
    {
        var below = Vector3.zero;

        for (var i = 0; i < carried.Count; i++)
        {
            var item = carried[i];

            if (!links.TryGetValue(item, out var link))
                link = new Link { Offset = below, Velocity = Vector3.zero };

            var target    = leanPerSlot * SlotInfluence(i, carried.Count, settings.TopShare);
            var stretch   = link.Offset - below;
            var returning = target.sqrMagnitude < stretch.sqrMagnitude;
            var stiffness = returning ? settings.ReturnStiffness : settings.Stiffness;
            var damping   = returning ? settings.ReturnDamping : settings.Damping;

            link.Velocity += ((target - stretch) * stiffness - link.Velocity * damping) * stepTime;
            link.Offset   += link.Velocity * stepTime;
            link.Offset    = below + Vector3.ClampMagnitude(link.Offset - below, maxLink);
            link.Stamp     = stamp;

            links[item] = link;
            below       = link.Offset;
        }
    }

    private void ApplyToStack(PickupCarrySwaySettings settings, IReadOnlyList<PickupItemController> carried)
    {
        var below = Vector3.zero;

        for (var i = 0; i < carried.Count; i++)
        {
            var item = carried[i];

            if (!links.TryGetValue(item, out var link))
                continue;

            var bend     = link.Offset - below;
            var tiltAxis = Vector3.Cross(Vector3.up, bend);

            if (tiltAxis.sqrMagnitude > 0.0001f)
                tiltAxis.Normalize();

            item.SetCarrySway(
                link.Offset,
                Mathf.Min(bend.magnitude * settings.TiltPerUnitBend, settings.MaxTiltDegrees),
                tiltAxis,
                settings.VisualSmoothing);

            below = link.Offset;
        }
    }

    private static float SlotInfluence(int index, int count, float topShare)
    {
        if (count <= 1)
            return 1f;

        var position = index / (float)(count - 1);
        var start    = 1f - topShare;

        if (position <= start)
            return 0f;

        var t = (position - start) / Mathf.Max(0.0001f, topShare);

        return t * t * (3f - 2f * t);
    }

    private void PruneReleasedLinks()
    {
        foreach (var pair in links)
        {
            if (pair.Value.Stamp != stamp)
                stale.Add(pair.Key);
        }

        for (var i = 0; i < stale.Count; i++)
            links.Remove(stale[i]);

        stale.Clear();
    }
}

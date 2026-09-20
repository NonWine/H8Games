using UnityEngine;
using UnityEngine.AI;

// The baked NavMesh is the single source of truth for where the hero may walk.
// The level has no walk-blocking colliders, so a move is validated against the
// mesh instead of being resolved by physics.
public class HeroNavMeshMover : IHeroMover
{
    private const float MinDirectionSqrMagnitude = 0.0001f;
    private const float MinSlideSqrMagnitude = 0.000001f;

    // Covers the hero drifting slightly off the mesh - a teleport onto a seam, an
    // avoidance nudge - without being wide enough to yank him across a gap onto an
    // unrelated walkable island.
    private const float GroundingSampleRadius = 1.5f;

    // A teleport target is authored (a level StartPoint), so it can sit further
    // from the mesh than a drifting hero ever would.
    private const float TeleportSampleRadius = 5f;

    // Resuming a slide exactly on the border re-reports the same crossing, so the
    // second probe starts a hair back inside the walkable area.
    private const float BorderPullback = 0.01f;

    private readonly PlayerView heroView;

    public HeroNavMeshMover(PlayerView heroView)
    {
        this.heroView = heroView;
    }

    public void Move(Vector3 direction, float speed, float deltaTime)
    {
        if (direction.sqrMagnitude <= MinDirectionSqrMagnitude)
            return;

        Transform heroTransform = heroView.transform;

        // NavMesh.Raycast only answers correctly from an origin already on the mesh.
        // Re-grounding first means a hero who ended up off it walks back on, instead
        // of locking up against an origin the NavMesh cannot resolve.
        if (!TryGetPointOnNavMesh(heroTransform.position, GroundingSampleRadius, out Vector3 origin))
            return;

        Vector3 motion = direction.normalized * Mathf.Max(0f, speed) * deltaTime;
        heroTransform.position = ConstrainToNavMesh(origin, origin + motion);
    }

    public void Teleport(Vector3 position)
    {
        heroView.transform.position = TryGetPointOnNavMesh(position, TeleportSampleRadius, out Vector3 onMesh)
            ? onMesh
            : position;
    }

    public void FaceDirection(Vector3 direction)
    {
        Vector3 flatDirection = direction;
        flatDirection.y = 0f;

        if (flatDirection.sqrMagnitude <= MinDirectionSqrMagnitude)
            return;

        heroView.transform.forward = flatDirection.normalized;
    }

    // Walking into a border cancels the blocked component only: the rest of the
    // input is projected along that border, so the hero slides past a wall instead
    // of sticking to it, which is what a joystick makes players expect.
    private static Vector3 ConstrainToNavMesh(Vector3 origin, Vector3 target)
    {
        if (!NavMesh.Raycast(origin, target, out NavMeshHit borderHit, NavMesh.AllAreas))
            return target;

        Vector3 blockedAt = borderHit.position;
        Vector3 slide = Vector3.ProjectOnPlane(target - blockedAt, borderHit.normal);

        if (slide.sqrMagnitude <= MinSlideSqrMagnitude)
            return blockedAt;

        Vector3 slideOrigin = Vector3.MoveTowards(blockedAt, origin, BorderPullback);
        Vector3 slideTarget = slideOrigin + slide;

        return NavMesh.Raycast(slideOrigin, slideTarget, out NavMeshHit slideHit, NavMesh.AllAreas)
            ? slideHit.position
            : slideTarget;
    }

    private static bool TryGetPointOnNavMesh(Vector3 position, float sampleRadius, out Vector3 result)
    {
        if (NavMesh.SamplePosition(position, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = position;
        return false;
    }
}

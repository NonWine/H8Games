using UnityEngine;
using UnityEngine.AI;

public class SoldierNavMeshFormationMover : ISoldierFormationMover
{
    private const float MinPlanarSqrMagnitude = 0.0001f;
    private const float NavMeshSampleDistance = 1.5f;
    private const float DestinationRefreshDistance = 0.08f;
    private const float AgentStoppingDistance = 0.03f;
    private const float AgentAccelerationMultiplier = 6f;
    private const float AgentAngularSpeed = 720f;

    private readonly BaseCombatAgentView combatView;
    private readonly SquadFollowSettings settings;
    private readonly Vector2 movingLocalOffset;
    private readonly float moveSpeedMultiplier;
    private readonly float rotationSpeedMultiplier;
    private readonly float movingYawOffset;
    private readonly float destinationRefreshInterval;
    private readonly int avoidancePriority;

    private Vector3 lastDestination;
    private float nextDestinationRefreshTime;

    public SoldierNavMeshFormationMover(BaseCombatAgentView combatView, SquadFollowSettings settings, int seed)
    {
        this.combatView = combatView;
        this.settings = settings;

        moveSpeedMultiplier = Mathf.Lerp(
            settings.SoldierMoveSpeedMultiplierMin,
            settings.SoldierMoveSpeedMultiplierMax,
            Hash01(seed * 17 + 3));

        rotationSpeedMultiplier = Mathf.Lerp(
            settings.SoldierRotationSpeedMultiplierMin,
            settings.SoldierRotationSpeedMultiplierMax,
            Hash01(seed * 31 + 7));

        float offsetX = Mathf.Lerp(
            -settings.MovingSlotOffsetRadius,
            settings.MovingSlotOffsetRadius,
            Hash01(seed * 47 + 11));

        float offsetZ = Mathf.Lerp(
            -settings.MovingSlotOffsetRadius,
            settings.MovingSlotOffsetRadius,
            Hash01(seed * 59 + 13));

        movingLocalOffset = new Vector2(offsetX, offsetZ);
        movingYawOffset = Mathf.Lerp(
            -settings.MovingFacingYawJitter,
            settings.MovingFacingYawJitter,
            Hash01(seed * 71 + 17));

        destinationRefreshInterval = Mathf.Lerp(
            settings.DestinationRefreshIntervalMin,
            settings.DestinationRefreshIntervalMax,
            Hash01(seed * 83 + 19));

        avoidancePriority = Mathf.RoundToInt(Mathf.Lerp(
            settings.AvoidancePriorityMin,
            settings.AvoidancePriorityMax,
            Hash01(seed * 97 + 23)));
    }

    public void Reset()
    {
        lastDestination = Vector3.positiveInfinity;
        nextDestinationRefreshTime = 0f;
        ConfigureAgent();
    }

    public void Stop(bool clearPath = true)
    {
        NavMeshAgent agent = Agent;

        if (!IsAgentReady(agent))
        {
            return;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (clearPath)
        {
            agent.ResetPath();
        }
    }

    public void SyncToCurrentPosition()
    {
        NavMeshAgent agent = Agent;

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        if (NavMesh.SamplePosition(combatView.Transform.position, out NavMeshHit hit, NavMeshSampleDistance, agent.areaMask))
        {
            combatView.Transform.position = hit.position;
            agent.Warp(hit.position);
        }

        Reset();
        Stop();
    }

    public SoldierFormationState MoveToSlot(Transform squadRoot, Vector3 slotCenter, bool squadRootIsMoving, float deltaTime)
    {
        NavMeshAgent agent = Agent;

        if (!IsAgentReady(agent))
        {
            return SoldierFormationState.WaitingInFormation;
        }

        ConfigureAgent();

        Vector3 desiredPosition = squadRootIsMoving
            ? slotCenter + GetMovingWorldOffset(squadRoot)
            : slotCenter;

        desiredPosition.y = combatView.Transform.position.y;

        Vector3 delta = desiredPosition - combatView.Transform.position;
        delta.y = 0f;

        if (delta.sqrMagnitude <= settings.SlotReachThreshold * settings.SlotReachThreshold)
        {
            Stop();
            RotateTowards(GetLookDirection(squadRoot, delta, squadRootIsMoving), deltaTime);

            return SoldierFormationState.WaitingInFormation;
        }

        TrySetDestination(agent, desiredPosition);
        RotateTowards(GetLookDirection(squadRoot, delta, squadRootIsMoving), deltaTime);

        return SoldierFormationState.MovingToSlot;
    }

    public bool IsAt(Vector3 worldPosition, float threshold)
    {
        Vector3 delta = worldPosition - combatView.Transform.position;
        delta.y = 0f;

        return delta.sqrMagnitude <= threshold * threshold;
    }

    private NavMeshAgent Agent => combatView.NavMeshAgent;

    private void ConfigureAgent()
    {
        NavMeshAgent agent = Agent;

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        agent.speed = Mathf.Max(0.01f, settings.SoldierMoveSpeed * moveSpeedMultiplier);
        agent.acceleration = Mathf.Max(0.01f, settings.SoldierMoveSpeed * AgentAccelerationMultiplier);
        agent.angularSpeed = AgentAngularSpeed;
        agent.stoppingDistance = AgentStoppingDistance;
        agent.autoBraking = true;
        agent.autoRepath = true;
        agent.updatePosition = true;
        agent.updateRotation = false;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = Mathf.Clamp(avoidancePriority, 0, 99);
    }

    private bool IsAgentReady(NavMeshAgent agent)
    {
        if (!agent.enabled)
        {
            return false;
        }

        if (agent.isOnNavMesh)
        {
            return true;
        }

        return NavMesh.SamplePosition(combatView.Transform.position, out NavMeshHit hit, NavMeshSampleDistance, agent.areaMask)
               && agent.Warp(hit.position);
    }

    private void TrySetDestination(NavMeshAgent agent, Vector3 destination)
    {
        bool shouldRefreshByTime = Time.time >= nextDestinationRefreshTime;
        bool shouldRefreshByDistance = !IsFinite(lastDestination)
            || (lastDestination - destination).sqrMagnitude >= DestinationRefreshDistance * DestinationRefreshDistance;

        if (!shouldRefreshByTime && !shouldRefreshByDistance && agent.hasPath)
        {
            return;
        }

        Vector3 resolvedDestination = destination;

        if (NavMesh.SamplePosition(destination, out NavMeshHit hit, NavMeshSampleDistance, agent.areaMask))
        {
            resolvedDestination = hit.position;
        }

        agent.isStopped = false;
        agent.SetDestination(resolvedDestination);
        lastDestination = resolvedDestination;
        nextDestinationRefreshTime = Time.time + destinationRefreshInterval;
    }

    private Vector3 GetMovingWorldOffset(Transform squadRoot)
    {
        return squadRoot.right * movingLocalOffset.x
               + squadRoot.forward * movingLocalOffset.y;
    }

    private Vector3 GetLookDirection(Transform squadRoot, Vector3 toSlot, bool squadRootIsMoving)
    {
        Vector3 rootForward = squadRoot.forward;
        rootForward.y = 0f;

        if (rootForward.sqrMagnitude <= MinPlanarSqrMagnitude)
        {
            rootForward = combatView.Transform.forward;
            rootForward.y = 0f;
        }

        if (rootForward.sqrMagnitude <= MinPlanarSqrMagnitude)
        {
            rootForward = Vector3.forward;
        }

        rootForward.Normalize();

        Vector3 slotDirection = toSlot.sqrMagnitude > MinPlanarSqrMagnitude ? toSlot.normalized : rootForward;
        Vector3 direction = squadRootIsMoving
            ? Vector3.Lerp(rootForward, slotDirection, settings.MovingFacingToSlotWeight).normalized
            : slotDirection;

        return Quaternion.AngleAxis(movingYawOffset, Vector3.up) * direction;
    }

    private void RotateTowards(Vector3 direction, float deltaTime)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= MinPlanarSqrMagnitude)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        combatView.Transform.rotation = Quaternion.RotateTowards(
            combatView.Transform.rotation,
            targetRotation,
            settings.SoldierRotationSpeed * rotationSpeedMultiplier * deltaTime);
    }

    private static bool IsFinite(Vector3 value)
    {
        return !float.IsNaN(value.x)
               && !float.IsNaN(value.y)
               && !float.IsNaN(value.z)
               && !float.IsInfinity(value.x)
               && !float.IsInfinity(value.y)
               && !float.IsInfinity(value.z);
    }

    private static float Hash01(int value)
    {
        unchecked
        {
            uint x = (uint)(Mathf.Abs(value) + 1);
            x ^= 2747636419u;
            x *= 2654435769u;
            x ^= x >> 16;
            x *= 2654435769u;
            x ^= x >> 16;
            x *= 2654435769u;
            return (x & 0x00FFFFFFu) / 16777215f;
        }
    }
}

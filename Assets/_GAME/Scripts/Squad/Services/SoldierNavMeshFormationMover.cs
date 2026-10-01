using UnityEngine;
using UnityEngine.AI;

public class SoldierNavMeshFormationMover : ISoldierFormationMover
{
    private const float MinPlanarSqrMagnitude = 0.0001f;
    private const float NavMeshSampleDistance = 5f;
    private const float DestinationRefreshDistance = 0.08f;
    private const float AgentStoppingDistance = 0.03f;
    private const float AgentAccelerationMultiplier = 6f;
    private const float AgentAngularSpeed = 720f;
    private const float FullCircleRadians = Mathf.PI * 2f;

    // If a soldier makes less than this distance of progress toward its slot
    // over StuckTimeout seconds while the squad root is stationary, warp it
    // there. Covers NavMesh holes, obstacle geometry the agent can't path
    // around, and any other edge case that leaves a soldier stranded.
    private const float StuckTimeout = 3f;
    private const float StuckProgressThreshold = 0.15f;

    private readonly BaseCombatAgentView combatView;
    private readonly SquadFollowSettings settings;
    private readonly Vector2 movingLocalOffset;
    private readonly float moveSpeedMultiplier;
    private readonly float rotationSpeedMultiplier;
    private readonly float movingYawOffset;
    private readonly float destinationRefreshInterval;
    private readonly float swayFrequency;
    private readonly float swayPhase;
    private readonly int avoidancePriority;

    private Vector3 lastDestination;
    private float nextDestinationRefreshTime;

    // Stuck detection: tracks how long the soldier has been trying to reach
    // a stationary slot without meaningful progress.
    private float stuckTimer;
    private float stuckCheckDistance;

    public SoldierNavMeshFormationMover(BaseCombatAgentView combatView, SquadFollowSettings settings, int seed)
    {
        this.combatView = combatView;
        this.settings = settings;

        moveSpeedMultiplier = Mathf.Lerp(
            settings.SoldierMoveSpeedMultiplierMin,
            settings.SoldierMoveSpeedMultiplierMax,
            DeterministicHashUtility.Hash01(seed * 17 + 3));

        rotationSpeedMultiplier = Mathf.Lerp(
            settings.SoldierRotationSpeedMultiplierMin,
            settings.SoldierRotationSpeedMultiplierMax,
            DeterministicHashUtility.Hash01(seed * 31 + 7));

        float offsetX = Mathf.Lerp(
            -settings.MovingSlotOffsetRadius,
            settings.MovingSlotOffsetRadius,
            DeterministicHashUtility.Hash01(seed * 47 + 11));

        float offsetZ = Mathf.Lerp(
            -settings.MovingSlotOffsetRadius,
            settings.MovingSlotOffsetRadius,
            DeterministicHashUtility.Hash01(seed * 59 + 13));

        movingLocalOffset = new Vector2(offsetX, offsetZ);
        movingYawOffset = Mathf.Lerp(
            -settings.MovingFacingYawJitter,
            settings.MovingFacingYawJitter,
            DeterministicHashUtility.Hash01(seed * 71 + 17));

        destinationRefreshInterval = Mathf.Lerp(
            settings.DestinationRefreshIntervalMin,
            settings.DestinationRefreshIntervalMax,
            DeterministicHashUtility.Hash01(seed * 83 + 19));

        swayFrequency = Mathf.Lerp(
            settings.MovingSwayFrequencyMin,
            settings.MovingSwayFrequencyMax,
            DeterministicHashUtility.Hash01(seed * 103 + 29));

        swayPhase = DeterministicHashUtility.Hash01(seed * 113 + 31) * FullCircleRadians;

        avoidancePriority = Mathf.RoundToInt(Mathf.Lerp(
            settings.AvoidancePriorityMin,
            settings.AvoidancePriorityMax,
            DeterministicHashUtility.Hash01(seed * 97 + 23)));
    }

    public void Reset()
    {
        lastDestination = Vector3.positiveInfinity;
        nextDestinationRefreshTime = 0f;
        stuckTimer = 0f;
        stuckCheckDistance = float.MaxValue;
        ConfigureAgent(true);
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

    public SoldierFormationState MoveToSlot(Transform squadRoot, Vector3 slotCenter, bool squadRootIsMoving, float deltaTime)
    {
        NavMeshAgent agent = Agent;

        if (!IsAgentReady(agent))
        {
            return SoldierFormationState.WaitingInFormation;
        }

        Vector3 desiredPosition = squadRootIsMoving
            ? slotCenter + GetMovingWorldOffset(squadRoot)
            : slotCenter;

        desiredPosition.y = combatView.Transform.position.y;

        Vector3 delta = desiredPosition - combatView.Transform.position;
        delta.y = 0f;

        ConfigureAgent(ShouldAvoidOthers(delta, squadRootIsMoving));

        if (delta.sqrMagnitude <= settings.SlotReachThreshold * settings.SlotReachThreshold)
        {
            Stop();
            RotateTowards(GetLookDirection(squadRoot, delta, squadRootIsMoving), deltaTime);
            stuckTimer = 0f;

            return SoldierFormationState.WaitingInFormation;
        }

        // Stuck detection: when the squad root is parked and the soldier is not
        // making meaningful progress toward the slot, warp it there so it is
        // never permanently stranded by NavMesh holes or obstacle colliders.
        if (!squadRootIsMoving)
        {
            float currentDistance = delta.magnitude;

            if (currentDistance < stuckCheckDistance - StuckProgressThreshold)
            {
                // Made real progress — reset the timer.
                stuckTimer = 0f;
                stuckCheckDistance = currentDistance;
            }
            else
            {
                stuckTimer += deltaTime;

                if (stuckTimer >= StuckTimeout)
                {
                    agent.Warp(desiredPosition);
                    Stop();
                    stuckTimer = 0f;
                    stuckCheckDistance = float.MaxValue;

                    return SoldierFormationState.WaitingInFormation;
                }
            }
        }
        else
        {
            stuckTimer = 0f;
            stuckCheckDistance = float.MaxValue;
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

    // Avoidance is what makes a marching squad read as a crowd, and it is also
    // what deadlocks the last metre: two soldiers settling onto neighbouring
    // slots push each other off both of them and neither ever arrives. Once the
    // squad root has stopped and the slot is within reach, the slots themselves
    // guarantee the spacing, so avoidance has nothing left to solve.
    //
    // Disable entirely when the root is stationary: soldiers returning from combat
    // are scattered far from their slots and all converge at once — avoidance at
    // full quality during that convergence is the main cause of "some of them
    // can't go back".
    private bool ShouldAvoidOthers(Vector3 toSlot, bool squadRootIsMoving)
    {
        return squadRootIsMoving;
    }

    private void ConfigureAgent(bool avoidOthers)
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
        agent.obstacleAvoidanceType = avoidOthers
            ? ObstacleAvoidanceType.HighQualityObstacleAvoidance
            : ObstacleAvoidanceType.NoObstacleAvoidance;
        agent.avoidancePriority = Mathf.Clamp(avoidancePriority, 0, 99);

        // Drives local avoidance only (the baked NavMesh uses the agent type's
        // radius), so it can be widened to match the visual footprint and keep
        // soldiers from clipping into each other when they bunch up.
        agent.radius = settings.SoldierAvoidanceRadius;
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
        float sway = Mathf.Sin(Time.time * swayFrequency * FullCircleRadians + swayPhase)
                     * settings.MovingSwayAmplitude;

        return squadRoot.right * (movingLocalOffset.x + sway)
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
}

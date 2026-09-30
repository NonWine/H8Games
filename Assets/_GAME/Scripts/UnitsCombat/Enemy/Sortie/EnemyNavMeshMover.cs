using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshMover
{
    private const float NavMeshSampleDistance = 1.5f;
    private const float AccelerationMultiplier = 6f;
    private const float MinSpeed = 0.01f;

    private readonly BaseCombatAgentView combatView;
    private readonly UnitStats unitStats;

    public EnemyNavMeshMover(BaseCombatAgentView combatView, UnitStats unitStats)
    {
        this.combatView = combatView;
        this.unitStats = unitStats;
    }

    public Vector3 Heading => IsAgentReady(Agent) ? Agent.desiredVelocity : Vector3.zero;

    private NavMeshAgent Agent => combatView.NavMeshAgent;

    public bool TryMoveTo(Vector3 destination, float speedMultiplier)
    {
        NavMeshAgent agent = Agent;

        if (!IsAgentReady(agent)
            || !NavMesh.SamplePosition(destination, out NavMeshHit hit, NavMeshSampleDistance, agent.areaMask))
        {
            return false;
        }

        float speed = Mathf.Max(MinSpeed, unitStats.MoveSpeed * speedMultiplier);
        agent.speed = speed;
        agent.acceleration = speed * AccelerationMultiplier;
        agent.updateRotation = false;
        agent.isStopped = false;

        return agent.SetDestination(hit.position);
    }

    public bool HasArrived(float threshold)
    {
        NavMeshAgent agent = Agent;

        if (!IsAgentReady(agent))
        {
            return true;
        }

        return !agent.pathPending && (!agent.hasPath || agent.remainingDistance <= threshold);
    }

    public void Stop()
    {
        NavMeshAgent agent = Agent;

        if (!IsAgentReady(agent))
        {
            return;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
    }

    private static bool IsAgentReady(NavMeshAgent agent)
    {
        return agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh;
    }
}

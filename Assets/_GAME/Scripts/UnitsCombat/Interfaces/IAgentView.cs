using UnityEngine;
using UnityEngine.AI;

public interface IAgentView
{
    Transform Transform { get; }
    void PlayHitFeedback();
    NavMeshAgent NavMeshAgent { get; }
}

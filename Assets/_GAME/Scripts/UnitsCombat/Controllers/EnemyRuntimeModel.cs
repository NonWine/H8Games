using UnityEngine;

public class EnemyRuntimeModel : AgentRuntimeModel
{
    public EnemyRuntimeModel(BaseCombatAgentView view, UnitStats unitStats, ITargetTrackerHandler targetTracker)
        : base(view, unitStats, targetTracker)
    {
    }

    public Vector3 PostPosition { get; private set; }
    public Quaternion PostRotation { get; private set; }

    public void CapturePost()
    {
        PostPosition = Transform.position;
        PostRotation = Transform.rotation;
    }

    public bool IsAtPost(float threshold)
    {
        Vector3 delta = PostPosition - Transform.position;
        delta.y = 0f;

        return delta.sqrMagnitude <= threshold * threshold;
    }
}

using UnityEngine;

public interface IAgentView
{
    Transform Transform { get; }
    bool IsActive { get; }

    void PlayHitFeedback();
}

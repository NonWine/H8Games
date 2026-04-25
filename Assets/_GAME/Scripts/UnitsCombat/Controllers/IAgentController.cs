using UnityEngine;

public interface IAgentController
{
    Transform transform { get; }
    void SetIdentity(string identity);
}

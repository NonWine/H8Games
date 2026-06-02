using UnityEngine;
using UnityEngine.AI;

public class BaseCombatAgentView : BaseCombatUnitView
{
    [field: SerializeField] public UnitConfig unitConfig { get; private set; }
    [field: SerializeField] public UnitRagdollView RagdollView { get; private set; }

    private void Reset()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
    }
}

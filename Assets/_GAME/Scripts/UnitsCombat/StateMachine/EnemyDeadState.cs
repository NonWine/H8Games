using Cysharp.Threading.Tasks;
using UnityEngine;

public class EnemyDeadState : EnemyStateBase
{
    public EnemyDeadState(
        EnemyRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController)
        : base(model, modules, agentAnimationController)
    {
    }

    public override void Enter()
    {
        var baseView = model.View as BaseCombatAgentView;
        if (baseView != null && baseView.NavMeshAgent != null)
        {
            baseView.NavMeshAgent.enabled = false;
        }

        var ragdoll = baseView?.RagdollView;

        var centerPosition = model.Transform.position + Vector3.up * 1f;
        var damageData = UnitDamageData.FromHitData(model.LastHitData, centerPosition);
        ragdoll?.EnableRagdoll(damageData, true);

        modules.Death.HandleDeathAsync().Forget();
    }

    public override void Exit()
    {
    }
}

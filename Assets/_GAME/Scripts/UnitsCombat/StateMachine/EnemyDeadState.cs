using Cysharp.Threading.Tasks;

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
        var ragdoll = (model.View as BaseCombatAgentView)?.RagdollView;

        var damageData = UnitDamageData.FromHitData(model.LastHitData, model.Transform.position);
        ragdoll.EnableRagdoll(damageData);

        modules.Death.HandleDeathAsync().Forget();
    }

    public override void Exit()
    {
    }
}

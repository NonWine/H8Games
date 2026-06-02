using Cysharp.Threading.Tasks;

public class SoldierDeadState : SoldierStateBase
{
    private readonly IAgentDespawnRequester despawnRequester;
    private readonly ISoldierFormationMover formationMover;

    public SoldierDeadState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        IAgentDespawnRequester despawnRequester,
        ISoldierFormationMover formationMover)
        : base(model, modules, agentAnimationController)
    {
        this.despawnRequester = despawnRequester;
        this.formationMover = formationMover;
    }

    public override void Enter()
    {
        formationMover.Stop();

        var view = (BaseCombatAgentView)model.View;
        view.NavMeshAgent.enabled = false;

        var damageData = UnitDamageData.FromHitData(model.LastHitData, model.Transform.position);
        view.RagdollView.EnableRagdoll(damageData);

        HandleDeathSequenceAsync(view).Forget();
    }

    public override void Exit()
    {
    }

    private async UniTaskVoid HandleDeathSequenceAsync(BaseCombatAgentView view)
    {
        await modules.Death.HandleDeathAsync();
        await view.PlayDeathSinkAsync();
        despawnRequester.RequestDespawn();
    }
}

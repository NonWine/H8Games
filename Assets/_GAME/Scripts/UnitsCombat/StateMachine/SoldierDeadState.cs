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
        agentAnimationController.SetAnimationState(UnitState.Dead);
        HandleDeathSequenceAsync().Forget();
    }

    public override void Exit()
    {
    }

    private async UniTaskVoid HandleDeathSequenceAsync()
    {
        await modules.Death.HandleDeathAsync();
        despawnRequester.RequestDespawn();
    }
}

using Cysharp.Threading.Tasks;

public class SoldierDeadState : SoldierStateBase
{
    private readonly IAgentDespawnRequester despawnRequester;

    public SoldierDeadState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController,
        IAgentDespawnRequester despawnRequester)
        : base(model, modules, agentAnimationController)
    {
        this.despawnRequester = despawnRequester;
    }

    public override void Enter()
    {
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

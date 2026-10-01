using UnityEngine;
using Zenject;

public class SoldierCombatAgentController : BaseCombatAgentController<SoldierRuntimeModel>
{
    private readonly SoldierStateMachine stateMachine;
    private readonly AgentAnimationController agentAnimationController;
    private readonly SignalBus signalBus;

    protected override CombatSide Side => CombatSide.Ally;

    public SoldierCombatAgentController(
        SoldierRuntimeModel runtimeModel,
        CombatUnitModules modules,
        SoldierStateMachine stateMachine,
        ITargetTrackerHandler targetTrackerHandler,
        ITargetReservationHandler targetReservationHandler,
        SignalBus signalBus,
        AgentAnimationController agentAnimationController)
        : base(runtimeModel, modules, targetTrackerHandler, targetReservationHandler, signalBus)
    {
        this.stateMachine = stateMachine;
        this.agentAnimationController = agentAnimationController;
        this.signalBus = signalBus;
    }

    public override void Initialize()
    {
        base.Initialize();
        signalBus.Subscribe<LevelCompletedSignal>(OnLevelCompleted);
    }

    public override void Dispose()
    {
        base.Dispose();
        signalBus.TryUnsubscribe<LevelCompletedSignal>(OnLevelCompleted);
    }

    private async void OnLevelCompleted()
    {
        if (!IsAlive) return;
        
        // 70% chance to play the victory animation
        if (UnityEngine.Random.value > 0.7f) return;

        // Random delay so they don't all sync up robotically
        int randomDelay = UnityEngine.Random.Range(100, 1500);
        await Cysharp.Threading.Tasks.UniTask.Delay(randomDelay);

        if (IsAlive)
        {
            agentAnimationController.SetVictoryTrigger();
        }
    }

    protected override void ResetView()
    {
        var view = (BaseCombatAgentView)runtimeModel.View;
        view.RagdollView.ResetStateImmediate();
        view.NavMeshAgent.enabled = true;
    }

    protected override void TeardownView()
    {
        runtimeModel.View.NavMeshAgent.enabled = false;
    }

    protected override void TickBehaviour() => stateMachine.Tick();

    public void AssignSquad(SquadRootView squadRootView) => runtimeModel.AssignSquad(squadRootView);
    public void AssignSlot(FormationSlot slot) => runtimeModel.AssignSlot(slot);
    public void ClearSquad(SquadRootView owner) => runtimeModel.ClearSquad(owner);
    
    protected override void ChangeToIdleState() => stateMachine.ChangeState<SoldierIdleState>();
    protected override void ChangeToDeadState() => stateMachine.ChangeState<SoldierDeadState>();
}

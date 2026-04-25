using Cysharp.Threading.Tasks;

public class SoldierDeadState : SoldierStateBase
{
    public SoldierDeadState(
        SoldierRuntimeModel model,
        CombatUnitModules modules,
        AgentAnimationController agentAnimationController)
        : base(model, modules, agentAnimationController)
    {
    }

    public override void Enter()
    {
        agentAnimationController.SetAnimationState(UnitState.Dead);
        modules.Death.HandleDeathAsync().Forget();
    }

    public override void Exit()
    {
    }
}

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
        agentAnimationController.SetAnimationState(UnitState.Dead);
        modules.Death.HandleDeathAsync().Forget();
    }

    public override void Exit()
    {
    }
}

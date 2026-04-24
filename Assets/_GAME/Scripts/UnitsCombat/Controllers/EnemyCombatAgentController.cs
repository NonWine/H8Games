using UnityEngine;

public sealed class EnemyCombatAgentController : BaseCombatAgentController
{
    private readonly IAllyTargetProvider allyTargetProvider;
    private readonly CurrencyService currencyService;

    public EnemyCombatAgentController(
        BaseCombatAgentView baseCombatAgentView,
        Transform agentTransform,
        GameObject agentGameObject,
        ModulesFactoryCollection modulesFactoryCollection,
        IAllyTargetProvider allyTargetProvider,
        CurrencyService currencyService)
        : base(baseCombatAgentView, agentTransform, agentGameObject, modulesFactoryCollection)
    {
        this.allyTargetProvider = allyTargetProvider;
        this.currencyService = currencyService;
    }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime);

        if (!IsAlive || allyTargetProvider == null)
            return;

        if (State != UnitState.Attack)
            return;

        var tracker = modules.TargetTracker;

        if (!tracker.IsCurrentTargetValid())
        {
            TryAcquireTarget();
        }
        else if (tracker.ShouldRetarget())
        {
            TryAcquireTarget();
        }

        if (tracker.CurrentTarget == null)
            return;

        tracker.RotateTowardsCurrentTarget(Transform);
    }

    public void ResetRunTimeState()
    {
        if (!IsAlive)
        {
            State = UnitState.Dead;
            return;
        }

        State = UnitState.Idle;
        modules.ResetModules();
    }

    public void Activate()
    {
        State = UnitState.Attack;
    }

    protected override void OnDied()
    {
        currencyService?.Add(unitStats.DeathReward);
        base.OnDied();
    }

    private void TryAcquireTarget()
    {
        ICombatTarget target = allyTargetProvider.GetBestLivingAllyTarget(agentTransform.position, unitStats.ReservationPenalty);
        var tracker = modules.TargetTracker;

        if (target == null)
        {
            tracker.SetCurrentTarget(null, agentTransform);
            tracker.ResetTargetingTimers();
            return;
        }

        tracker.SetCurrentTarget(target, agentTransform);
        tracker.MarkRetargetWindow();
    }
}

using UnityEngine;

public class EnemyCombatAgentController : BaseCombatAgentController
{
    private readonly IAllyTargetProvider allyTargetProvider;
    private readonly CurrencyService currencyService;

    public bool IsActive { get; private set; }

    public EnemyCombatAgentController(
        BaseCombatAgentView baseCombatAgentView,
        ModulesFactoryCollection modulesFactoryCollection,
        IAllyTargetProvider allyTargetProvider,
        CurrencyService currencyService)
        : base(baseCombatAgentView, modulesFactoryCollection)
    {
        this.allyTargetProvider = allyTargetProvider;
        this.currencyService = currencyService;
    }

    public override void Tick()
    {
        base.Tick();

        if (!IsAlive)
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

        tracker.RotateTowardsCurrentTarget(baseCombatAgentView.transform);
    }

    public void ResetRunTimeState()
    {
        IsActive = false;

        if (!IsAlive)
        {
            State = UnitState.Dead;
            return;
        }

        modules.ResetModules();

        State = UnitState.Idle;
    }

    public void Activate()
    {
        if (!IsAlive)
            return;

        IsActive = true;
        modules.Attack.RandomizeAttackAnimationSpeed();
        State = UnitState.Attack;
    }

    protected override void OnDied()
    {
        IsActive = false;
        currencyService.Add(unitStats.DeathReward);
        base.OnDied();
    }

    private void TryAcquireTarget()
    {
        ICombatTarget target = allyTargetProvider.GetBestLivingAllyTarget(baseCombatAgentView.transform.position, unitStats.ReservationPenalty);
        var tracker = modules.TargetTracker;

        if (target == null)
        {
            tracker.SetCurrentTarget(null);
            return;
        }

        tracker.SetCurrentTarget(target);
        tracker.MarkRetargetWindow();
    }
}

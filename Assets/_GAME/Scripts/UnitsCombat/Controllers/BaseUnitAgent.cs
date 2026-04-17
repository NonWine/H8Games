using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(BaseCombatUnitView))]
public abstract class BaseTargetingCombatAgent : MonoBehaviour , ICombatTarget
{
    [field: SerializeField] public BaseCombatUnitView CombatView { get; private set; }
    [field:SerializeField]  public UnitState State { get; protected set; } = UnitState.Idle;
    
    [SerializeField] private UnitModuleType unitModuleType;
    [SerializeField] private UnitConfig unitConfig;
    protected UnitStats unitStats;
    protected CombatUnitModules modules;
    
    public bool IsAlive => modules.Health.IsAlive;
    
    public string UnitId { get; private set; }

    [Inject]
    public void Construct(ModulesFactoryCollection modulesFactory)
    { 
       unitStats = unitConfig.CreateRuntimeStats();
       var unitModuleFactory = modulesFactory.Create(unitModuleType);
       modules = unitModuleFactory.Create(new CombatUnitModulesArgs(CombatView, unitStats));
       modules.Health.Died += OnDied;
       ApplyAttackAnimationSpeed();
       CombatView.AttackAnimationEvents.AttackTriggered += HandleAttackAnimationTriggered;
    }

    protected virtual void Update()
    {
        modules.Animation.Apply(State);
    }

    private void OnDisable()
    {
        modules.TargetTracker.ReleaseCurrentTarget(this);
        modules.Reservation.ClearReservations();
    }

    private void OnDestroy()
    {
        if (modules != null)
            modules.Health.Died -= OnDied;

        if (CombatView != null && CombatView.AttackAnimationEvents != null)
            CombatView.AttackAnimationEvents.AttackTriggered -= HandleAttackAnimationTriggered;
    }

    public void GetDamage(float damage, Vector3 sourceWorldPosition)
    {
        modules.Health.ApplyDamage(damage);
        CombatView?.SetEmissionHitFlash();
    }

    public void SetIdentity(string unitId)
    {
        UnitId = unitId;
    }

    protected virtual void OnDied()
    {
        State = UnitState.Dead;
        modules.Death.HandleDeathAsync();
    }

    protected void ApplyAttackAnimationSpeed()
    {
        modules.Animation.SetAttackAnimationSpeed(
            modules.Attack.GetAttackAnimationSpeed(CombatView.AttackAnimationCycleDuration));
    }

    private void HandleAttackAnimationTriggered()
    {
        if (!IsAlive || State != UnitState.Attack || !modules.TargetTracker.IsCurrentTargetValid())
            return;

        ICombatTarget currentTarget = modules.TargetTracker.CurrentTarget;
        Vector3 attackOrigin = CombatView.AttackPoint.position;

        if (!modules.ProjectileSpawner.Spawn(
                CombatView.AttackPoint,
                currentTarget.transform,
                unitStats.ProjectileSpeed,
                () => modules.Attack.ApplyDamage(currentTarget, attackOrigin)))
        {
            modules.Attack.ApplyDamage(currentTarget, attackOrigin);
        }
    }
}

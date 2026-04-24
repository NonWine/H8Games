using System;
using UnityEngine;
using Zenject;

public class BaseCombatAgentController : ITickable, IInitializable, IDisposable, ICombatTarget
{
    protected readonly BaseCombatAgentView baseCombatAgentView;
    protected readonly UnitStats unitStats;
    protected readonly CombatUnitModules modules;
    public BaseCombatUnitView CombatView => baseCombatAgentView;
    public Transform transform => baseCombatAgentView.transform;
    public UnitState State { get; protected set; } = UnitState.Idle;

    public string UnitId { get; private set; }
    public bool IsAlive => modules.Health.IsAlive;
    public ITargetReservation Reservation => modules.Reservation;
    public event Action Died;
    public event Action<HitData> HitReceived;


    public BaseCombatAgentController(BaseCombatAgentView baseCombatAgentView, ModulesFactoryCollection modulesFactoryCollection)
    {
        this.baseCombatAgentView = baseCombatAgentView;
        unitStats = baseCombatAgentView.unitConfig.CreateRuntimeStats();

        var unitModuleFactory = modulesFactoryCollection.Create(baseCombatAgentView.unitConfig.unitModuleType);
        if (unitModuleFactory == null)
        {
            throw new InvalidOperationException($"No combat unit module factory registered for {baseCombatAgentView.unitConfig.unitModuleType}.");
        }

        modules = unitModuleFactory.Create(new CombatUnitModulesArgs(baseCombatAgentView, unitStats));
    }

    public virtual void Tick()
    {
        modules.Tick(State, Time.deltaTime);
    }

    public void Initialize()
    {
        modules.Health.Died += OnDied;
    }

    public virtual void GetDamage(float damage, Vector3 sourceWorldPosition)
    {
        HitData hitData = new HitData
        {
            damage = damage,
            sourceWorldPosition = sourceWorldPosition
        };

        HitReceived?.Invoke(hitData);
        modules.Health.ApplyDamage(hitData.damage);
        baseCombatAgentView.SetEmissionHitFlash();
    }

    public void SetIdentity(string unitId)
    {
        UnitId = unitId;
    }

    protected virtual void OnDied()
    {
        State = UnitState.Dead;
        modules.DisposeModules();
        Died?.Invoke();
        modules.Death.HandleDeathAsync();
    }

    public void Dispose()
    {
        modules.Health.Died -= OnDied;
        modules.DisposeModules();
    }
}

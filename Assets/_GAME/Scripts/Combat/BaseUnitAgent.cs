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
    [SerializeField] protected UnitStats stats = new();
    protected CombatUnitModules modules;
    
    public bool IsAlive => modules.Health.IsAlive;
    
    public string UnitId { get; private set; }

    [Inject]
    public void Construct(ModulesFactoryCollection modulesFactory)
    {

       var unitModuleFactory = modulesFactory.Create(unitModuleType);
       modules = unitModuleFactory.Create(new CombatUnitModulesArgs(CombatView, stats));
       modules.Health.Died += OnDied;
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

    protected virtual async void OnDied()
    {
        State = UnitState.Dead;
        await modules.Death.HandleDeathAsync();
    }
}
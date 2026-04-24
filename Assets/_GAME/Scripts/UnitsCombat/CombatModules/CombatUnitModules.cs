using System.Collections.Generic;

public class CombatUnitModules
{
    private readonly List<ICombatTickModule> tickModules;
    private readonly List<IResetModule> resetModules;
    private readonly List<IDisposeModule> disposeModules;
    public UnitAttackAgentHandler Attack { get; }
    public UnitHealthHandler Health { get; }
    public TargetReservation Reservation { get; }
    public ITargetTrackerModule TargetTracker { get; }
    public CombatAnimationPresenter Animation { get; }
    public ProjectileVisualSpawner ProjectileSpawner { get; }
    public UnitDeathHandler Death { get; }
    
    public CombatUnitModules(
        UnitAttackAgentHandler attack,
        UnitHealthHandler health,
        TargetReservation reservation,
        CombatAnimationPresenter animation,
        ProjectileVisualSpawner projectileSpawner,
        UnitDeathHandler death)
    {
        Attack = attack;
        Health = health;
        Reservation = reservation;
        Animation = animation;
        ProjectileSpawner = projectileSpawner;
        Death = death;
        tickModules = new List<ICombatTickModule>
        {
            animation
        };
        resetModules = new List<IResetModule>
        {
            reservation,
            animation
        };
        disposeModules = new List<IDisposeModule>
        {
            reservation,
        };
    }



    public void Tick(UnitState state, float deltaTime)
    {
        for (int i = 0; i < tickModules.Count; i++)
        {
            tickModules[i]?.Tick(state, deltaTime);
        }
    }

    public void ResetModules()
    {
        for (int i = 0; i < resetModules.Count; i++)
        {
            resetModules[i]?.Reset();
        }
    }

    public void DisposeModules()
    {
        for (int i = 0; i < disposeModules.Count; i++)
        {
            disposeModules[i]?.Dispose();
        }
    }
}

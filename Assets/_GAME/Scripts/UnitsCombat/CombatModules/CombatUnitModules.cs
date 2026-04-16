public class CombatUnitModules
{
    public CombatUnitModules(
        UnitAttackAgentHandler attack,
        UnitHealthHandler health,
        TargetReservation reservation,
        CombatTargetTracker targetTracker,
        CombatAnimationPresenter animation,
        ProjectileVisualSpawner projectileSpawner,
        UnitDeathHandler death)
    {
        Attack = attack;
        Health = health;
        Reservation = reservation;
        TargetTracker = targetTracker;
        Animation = animation;
        ProjectileSpawner = projectileSpawner;
        Death = death;
    }

    public UnitAttackAgentHandler Attack { get; }
    public UnitHealthHandler Health { get; }
    public TargetReservation Reservation { get; }
    public CombatTargetTracker TargetTracker { get; }
    public CombatAnimationPresenter Animation { get; }
    public ProjectileVisualSpawner ProjectileSpawner { get; }
    public UnitDeathHandler Death { get; }
}
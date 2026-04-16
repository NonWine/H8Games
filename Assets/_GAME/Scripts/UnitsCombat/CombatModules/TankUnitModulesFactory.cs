// Example of Tank Module for future
public class TankUnitModulesFactory : UnitModulesFactory
{
    public override UnitModuleType ModuleType => UnitModuleType.Tank;

    public override CombatUnitModules Create(CombatUnitModulesArgs args)
    {
        AttackRuntimeModel attackRuntime = new AttackRuntimeModel(args.Stats);
        UnitAttackAgentHandler attack = new UnitAttackAgentHandler(attackRuntime);
        UnitHealthHandler health = new UnitHealthHandler(args.Stats.MaxHealth);
        TargetReservation reservation = new TargetReservation();
        CombatTargetTracker targetTracker = new CombatTargetTracker(args.Stats.RetargetInterval, args.Stats.TargetLockDuration);

        CombatAnimationPresenter animation = new CombatAnimationPresenter(args.ViewRefs.Animator);
        ProjectileVisualSpawner projectileSpawner = new ProjectileVisualSpawner(args.ViewRefs.ProjectilePrefab);
        UnitDeathHandler death = new UnitDeathHandler(args.ViewRefs.gameObject);

        return new CombatUnitModules(
            attack,
            health,
            reservation,
            targetTracker,
            animation,
            projectileSpawner,
            death);
    }
}
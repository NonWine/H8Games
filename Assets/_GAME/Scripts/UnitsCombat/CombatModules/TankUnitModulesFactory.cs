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
        ProjectileVisualSpawner projectileSpawner = new ProjectileVisualSpawner(args.ViewRefs.ProjectilePrefab);

        CombatAnimationPresenter animation = new CombatAnimationPresenter(args.ViewRefs);
        UnitDeathHandler death = new UnitDeathHandler(args.ViewRefs.gameObject);

        return new CombatUnitModules(
            attack,
            health,
            reservation,
            animation,
            projectileSpawner,
            death);
    }
}

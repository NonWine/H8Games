// Example of Tank Module for future
public class TankUnitModulesFactory : UnitModulesFactory
{
    public override UnitModuleType ModuleType => UnitModuleType.Tank;

    public override CombatUnitModules Create(CombatUnitModulesArgs args)
    {
        AttackRuntimeModel attackRuntime = new AttackRuntimeModel(args.Stats);
        ProjectileVisualSpawner projectileSpawner = new ProjectileVisualSpawner(args.ViewRefs.ProjectilePrefab);
        UnitAttackAgentHandler attack = new UnitAttackAgentHandler(attackRuntime, projectileSpawner);
        UnitHealthHandler health = new UnitHealthHandler(args.Stats.MaxHealth, args.AliveState);
        UnitDeathModule death = new UnitDeathModule(args.ViewRefs.gameObject);

        return new CombatUnitModules(
            attack,
            health,
            projectileSpawner,
            death);
    }
}

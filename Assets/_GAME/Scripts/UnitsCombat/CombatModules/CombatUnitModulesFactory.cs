public class CombatUnitModulesFactory : UnitModulesFactory
{
    public override UnitModuleType ModuleType => UnitModuleType.Combat;

    public override CombatUnitModules Create(CombatUnitModulesArgs args)
    {
        AttackRuntimeModel attackRuntime = new AttackRuntimeModel(args.Stats);
        UnitAttackAgentHandler attack = new UnitAttackAgentHandler(attackRuntime);
        UnitHealthHandler health = new UnitHealthHandler(args.Stats.MaxHealth);
        ProjectileVisualSpawner projectileSpawner = new ProjectileVisualSpawner(args.ViewRefs.ProjectilePrefab);
        CombatAnimationPresenter animation = new CombatAnimationPresenter(args.ViewRefs);
        UnitDeathHandler death = new UnitDeathHandler(args.ViewRefs.gameObject);

        return new CombatUnitModules(
            attack,
            health,
            animation,
            projectileSpawner,
            death);
    }
}

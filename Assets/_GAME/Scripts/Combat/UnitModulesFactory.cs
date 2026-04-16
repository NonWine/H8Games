public abstract class UnitModulesFactory : IUnitModulesFactory
{
    public abstract UnitModuleType ModuleType { get; }
    public abstract CombatUnitModules Create(CombatUnitModulesArgs args);
}
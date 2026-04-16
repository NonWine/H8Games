using System.Collections.Generic;
using System.Linq;

public class ModulesFactoryCollection 
{
    private readonly Dictionary<UnitModuleType, IUnitModulesFactory> combatUnitModulesFactories;

    public ModulesFactoryCollection(List<IUnitModulesFactory> definitions)
    {
        combatUnitModulesFactories = new Dictionary<UnitModuleType, IUnitModulesFactory>();

        foreach (var definition in definitions.Where(x => x != null))
        {
            combatUnitModulesFactories.Add(definition.ModuleType, definition);
        }
    }

    public IUnitModulesFactory Create(UnitModuleType moduleType)
    {
        return combatUnitModulesFactories.GetValueOrDefault(moduleType);
    }
}
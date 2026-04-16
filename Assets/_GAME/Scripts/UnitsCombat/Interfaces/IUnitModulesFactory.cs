using UnityEngine;
using Zenject;

public interface IUnitModulesFactory
{
    public UnitModuleType ModuleType { get; }
    CombatUnitModules Create(CombatUnitModulesArgs args);
}
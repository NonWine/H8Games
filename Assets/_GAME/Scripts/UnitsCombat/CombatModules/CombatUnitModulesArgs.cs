using UnityEngine;

public struct CombatUnitModulesArgs
{
    public BaseCombatUnitView ViewRefs { get; }
    public UnitStats Stats { get; }
    public CombatUnitModulesArgs(BaseCombatUnitView viewRefs, UnitStats stats)
    {

        ViewRefs = viewRefs;
        Stats = stats;
    }
}
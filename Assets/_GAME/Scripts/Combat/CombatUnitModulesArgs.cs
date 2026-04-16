using UnityEngine;

public struct CombatUnitModulesArgs
{
    public CombatUnitModulesArgs(BaseCombatUnitView viewRefs, UnitStats stats)
    {

        ViewRefs = viewRefs;
        Stats = stats;
    }

    public BaseCombatUnitView ViewRefs { get; }
    public UnitStats Stats { get; }

}
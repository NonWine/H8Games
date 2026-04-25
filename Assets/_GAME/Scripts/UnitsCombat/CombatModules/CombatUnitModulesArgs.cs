using UnityEngine;

public struct CombatUnitModulesArgs
{
    public BaseCombatUnitView ViewRefs { get; }
    public UnitStats Stats { get; }
    public IAliveState AliveState { get; }
    public CombatUnitModulesArgs(BaseCombatUnitView viewRefs, UnitStats stats, IAliveState aliveState)
    {
        ViewRefs = viewRefs;
        Stats = stats;
        AliveState = aliveState;
    }
}

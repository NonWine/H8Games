using UnityEngine;

public class BaseCombatAgentView : BaseCombatUnitView
{
    [field: SerializeField] public UnitConfig unitConfig { get; private set; }
}

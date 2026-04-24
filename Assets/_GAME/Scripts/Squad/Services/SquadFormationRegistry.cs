using System.Collections.Generic;

public class SquadFormationRegistry : BaseUnitRegistry<SoldierCombatAgentController>
{
    public SquadFormationRegistry() : base(soldier => soldier != null && soldier.IsAlive)
    {
    }

    public IReadOnlyList<SoldierCombatAgentController> Soldiers => Items;

    public bool HasLivingAllies => Count > 0;
}

using System.Collections.Generic;

public class SquadSoldierRegistry : BaseUnitRegistry<SoldierCombatAgent>
{
    public SquadSoldierRegistry() : base(soldier => soldier != null && soldier.IsAlive) { }

    public IReadOnlyList<SoldierCombatAgent> Soldiers => Items;

    public bool HasLivingAllies => Count > 0;
    
}
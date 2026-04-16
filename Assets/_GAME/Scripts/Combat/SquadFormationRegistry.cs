using System.Collections.Generic;

public class SquadFormationRegistry : BaseUnitRegistry<SoldierCombatAgent>
{
    public SquadFormationRegistry() : base(soldier => soldier != null && soldier.IsAlive) { }

    public IReadOnlyList<SoldierCombatAgent> Soldiers => Items;

    public bool HasLivingAllies => Count > 0;
}
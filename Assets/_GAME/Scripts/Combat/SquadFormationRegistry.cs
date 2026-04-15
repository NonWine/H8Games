using System.Collections.Generic;

public class SquadFormationRegistry : BaseUnitRegistry<SoldierFollower>
{
    public SquadFormationRegistry() : base(soldier => soldier != null) { }

    public IReadOnlyList<SoldierFollower> Soldiers => Items;
    
}
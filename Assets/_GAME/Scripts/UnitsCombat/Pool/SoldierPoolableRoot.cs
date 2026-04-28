
public class SoldierPoolableRoot : CombatUnitPoolableRoot<SoldierCombatAgentController>, IAgentDespawnRequester
{
    public void RequestDespawn() => Dispose();
}

public interface ISoldierCombatRegistryProvider
{
    bool RegisterSoldier(SoldierCombatAgentController soldier);
    void UnregisterSoldier(SoldierCombatAgentController soldier);
}

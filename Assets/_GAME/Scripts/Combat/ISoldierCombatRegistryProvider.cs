public interface ISoldierCombatRegistryProvider
{
    bool RegisterSoldier(SoldierCombatAgent soldier);
    void UnregisterSoldier(SoldierCombatAgent soldier);
}


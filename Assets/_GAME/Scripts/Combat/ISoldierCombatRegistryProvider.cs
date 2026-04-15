public interface ISoldierCombatRegistryProvider
{
    void RegisterSoldier(SoldierCombatAgent soldier);
    void UnregisterSoldier(SoldierCombatAgent soldier);
}


using UnityEngine;

public class SquadFormationFacade : ISoldierCombatRegistryProvider, ISquadSlotPositionProvider
{
    private readonly SquadFormationController formationController;

    public bool HasAlly => formationController.HasAlly;

    public SquadFormationFacade(SquadFormationController formationController)
    {
        this.formationController = formationController;
    }

    public bool HasFreeSlot => formationController.HasFreeSlot;

    public bool RegisterSoldier(SoldierCombatAgentController soldier)
    {
        return formationController.RegisterSoldier(soldier);
    }

    public void UnregisterSoldier(SoldierCombatAgentController soldier)
    {
        formationController.UnregisterSoldier(soldier);
    }

    public void RebuildFormation()
    {
        formationController.RebuildFormation();
    }

    public Vector3 GetSlotWorldPosition(FormationSlot slot)
    {
        return formationController.GetSlotWorldPosition(slot);
    }

    public Vector3 GetSlotWorldPosition(int slotIndex)
    {
        return formationController.GetSlotWorldPosition(slotIndex);
    }

    public void ClearSoldiers()
    {
        formationController.ClearFormation();
    }
}

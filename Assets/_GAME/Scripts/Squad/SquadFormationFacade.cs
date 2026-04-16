using System.Collections.Generic;
using UnityEngine;

public class SquadFormationFacade : ISoldierCombatRegistryProvider, ISquadSlotPositionProvider
{
    private readonly SquadFormationController formationController;

    public SquadFormationFacade(SquadFormationController formationController)
    {
        this.formationController = formationController;
    }
    
    public bool HasFreeSlot => formationController.HasFreeSlot;

    public bool RegisterSoldier(SoldierCombatAgent soldier)
    {
        return formationController.RegisterSoldier(soldier);
    }

    public void UnregisterSoldier(SoldierCombatAgent soldier)
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

    public void ClearSoldiers() => formationController.ClearFormation();
}

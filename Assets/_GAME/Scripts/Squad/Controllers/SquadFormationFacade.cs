using System;
using System.Collections.Generic;
using UnityEngine;

public class SquadFormationFacade : ISoldierCombatRegistryProvider, ISquadSlotPositionProvider, ISquadFormationLayoutSource
{
    private readonly SquadFormationController formationController;

    public bool HasAlly => formationController.HasAlly;

    public event Action FormationChanged
    {
        add => formationController.FormationChanged += value;
        remove => formationController.FormationChanged -= value;
    }

    public IReadOnlyList<FormationSlot> Slots => formationController.Slots;

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

using System.Collections.Generic;
using UnityEngine;

public class SquadFormationFacade : ISoldierFollowerRegistratorProvider, ISquadSlotPositionProvider
{
    private readonly SquadFormationController formationController;

    public SquadFormationFacade(SquadFormationController formationController)
    {
        this.formationController = formationController;
    }

    public int Capacity => formationController.Capacity;
    public int SoldierCount => formationController.SoldierCount;
    public bool HasFreeSlot => formationController.HasFreeSlot;
    public IReadOnlyList<FormationSlot> Slots => formationController.Slots;
    public bool IsFormationSettled => formationController.IsFormationSettled;

    public bool RegisterSoldier(SoldierFollower soldier)
    {
        return formationController.RegisterSoldier(soldier);
    }

    public void UnregisterSoldier(SoldierFollower soldier)
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
}

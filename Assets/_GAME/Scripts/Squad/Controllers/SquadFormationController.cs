using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SquadFormationController
{
    private readonly SquadRootView squadRootView;
    private readonly SquadFormationLayoutService squadFormationLayoutService;
    private readonly SquadFormationRegistry registry;
    private readonly List<FormationSlot> slots = new();
    private SignalBus  signalBus;
    private int capacity;
    
    public bool HasFreeSlot => registry.Count < capacity;
    

    public SquadFormationController(
        SquadRootView squadRootView, 
        SquadFormationLayoutService squadFormationLayoutService,
        SquadFormationRegistry registry, 
        SignalBus signalBus)
    {
        this.signalBus  = signalBus;
        this.squadRootView = squadRootView;
        this.squadFormationLayoutService = squadFormationLayoutService;
        this.registry = registry;
        capacity = Mathf.Max(0, squadRootView.InitialCapacity);
        RebuildFormation();
    }

    public bool RegisterSoldier(SoldierCombatAgent soldier)
    {
        if (soldier == null || !HasFreeSlot)
            return false;
        
        if (!registry.Register(soldier))
            return false;

        soldier.SoldierFollower.AssignSquad(squadRootView);
        RebuildFormation();
        return true;
    }

    public void UnregisterSoldier(SoldierCombatAgent soldier)
    {
        registry.Unregister(soldier);
        soldier.SoldierFollower.ClearSquad(squadRootView);
        RebuildFormation();
        if (!registry.HasLivingAllies)
        {
            signalBus.Fire<SquadDefeatedSignal>();
        }
    }

    public void IncreaseCapacity(int amount)
    {
        if (amount <= 0)
            return;

        capacity += amount;
        RebuildFormation();
    }

    public void ClearFormation()
    {
        registry.Clear();
        squadRootView.transform.position = squadRootView.HomePosition;
        squadRootView.transform.rotation = Quaternion.identity;
    }

    public void RebuildFormation()
    {
        registry.PruneInvalid();
        slots.Clear();

        List<Vector3> offsets = squadFormationLayoutService.CalculateLocalOffsets(capacity);
        for (int i = 0; i < offsets.Count; i++)
        {
            slots.Add(new FormationSlot(i, offsets[i]));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            SoldierFollower soldier = i < registry.Soldiers.Count ? registry.Soldiers[i].SoldierFollower : null;
            FormationSlot slot = slots[i];
            slot.AssignedSoldier = soldier;

            if (soldier != null)
            {
                soldier.AssignSquad(squadRootView);
                soldier.AssignSlot(slot);
            }
        }
    }

    public Vector3 GetSlotWorldPosition(FormationSlot slot)
    {
        return squadRootView.transform.TransformPoint(slot.LocalOffset);
    }

    public Vector3 GetSlotWorldPosition(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return squadRootView.transform.position;

        return GetSlotWorldPosition(slots[slotIndex]);
    }
}

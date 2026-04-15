using System.Collections.Generic;
using UnityEngine;

public class SquadFormationController
{
    private readonly SquadRootView owner;
    private readonly Transform squadRootTransform;
    private readonly FormationLayoutService formationLayoutService;
    private readonly SquadFormationRegistry registry;
    private readonly SquadFollowSettings settings;
    private readonly List<FormationSlot> slots = new();
    private int capacity;

    public int Capacity => capacity;
    public int SoldierCount => registry.Count;
    public bool HasFreeSlot => registry.Count < capacity;
    public IReadOnlyList<FormationSlot> Slots => slots;

    public bool IsFormationSettled
    {
        get
        {
            float threshold = settings != null ? settings.SlotReachThreshold * 1.5f : 0.2f;
            IReadOnlyList<SoldierFollower> soldiers = registry.Soldiers;

            for (int i = 0; i < soldiers.Count; i++)
            {
                SoldierFollower soldier = soldiers[i];
                if (soldier == null || !soldier.gameObject.activeInHierarchy)
                    continue;

                if (!soldier.IsInAssignedSlot(threshold))
                    return false;
            }

            return true;
        }
    }

    public SquadFormationController(
        SquadRootView owner,
        Transform squadRootTransform,
        FormationLayoutService formationLayoutService,
        SquadFormationRegistry registry,
        SquadFollowSettings settings,
        int initialCapacity)
    {
        this.owner = owner;
        this.squadRootTransform = squadRootTransform;
        this.formationLayoutService = formationLayoutService;
        this.registry = registry;
        this.settings = settings;
        capacity = Mathf.Max(0, initialCapacity);

        RebuildFormation();
    }

    public bool RegisterSoldier(SoldierFollower soldier)
    {
        if (soldier == null)
            return false;

        registry.PruneInvalid();

        if (registry.Contains(soldier) || !HasFreeSlot)
            return false;

        if (!registry.Register(soldier))
            return false;

        soldier.AssignSquad(owner);
        RebuildFormation();
        return true;
    }

    public void UnregisterSoldier(SoldierFollower soldier)
    {
        if (soldier == null)
            return;

        registry.Unregister(soldier);
        soldier.ClearSquad(owner);
        RebuildFormation();
    }

    public void IncreaseCapacity(int amount)
    {
        if (amount <= 0)
            return;

        capacity += amount;
        RebuildFormation();
    }

    public void RebuildFormation()
    {
        registry.PruneInvalid();
        slots.Clear();

        List<Vector3> offsets = formationLayoutService.CalculateLocalOffsets(capacity);
        for (int i = 0; i < offsets.Count; i++)
        {
            slots.Add(new FormationSlot(i, offsets[i]));
        }

        for (int i = 0; i < slots.Count; i++)
        {
            SoldierFollower soldier = i < registry.Soldiers.Count ? registry.Soldiers[i] : null;
            FormationSlot slot = slots[i];
            slot.AssignedSoldier = soldier;

            if (soldier != null)
            {
                soldier.AssignSquad(owner);
                soldier.AssignSlot(slot);
            }
        }
    }

    public Vector3 GetSlotWorldPosition(FormationSlot slot)
    {
        return squadRootTransform.TransformPoint(slot.LocalOffset);
    }

    public Vector3 GetSlotWorldPosition(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count)
            return squadRootTransform.position;

        return GetSlotWorldPosition(slots[slotIndex]);
    }
}

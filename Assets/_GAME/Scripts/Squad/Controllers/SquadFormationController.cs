using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SquadFormationController
{
    private readonly SquadRootView squadRootView;
    private readonly SquadFollowSettings settings;
    private readonly SquadFormationLayoutService squadFormationLayoutService;
    private readonly SquadFormationRegistry registry;

    // Lazy on purpose: SoldierFactory builds the soldier pools, whose prefabs resolve
    // ISquadSlotPositionProvider (this controller) during injection. Resolving the
    // despawner eagerly would close that loop into a circular dependency.
    private readonly LazyInject<ISoldierDespawner> soldierDespawner;

    private readonly List<FormationSlot> slots = new();
    private readonly Dictionary<SoldierCombatAgentController, Action> diedHandlers = new();
    private readonly SignalBus signalBus;
    private int capacity;

    public int Capacity => capacity;
    public bool HasFreeSlot => registry.Count < capacity;
    public bool HasAlly => registry.HasLivingAllies;

    public SquadFormationController(
        SquadRootView squadRootView,
        SquadFollowSettings settings,
        SquadFormationLayoutService squadFormationLayoutService,
        SquadFormationRegistry registry,
        LazyInject<ISoldierDespawner> soldierDespawner,
        SignalBus signalBus)
    {
        this.signalBus = signalBus;
        this.squadRootView = squadRootView;
        this.settings = settings;
        this.squadFormationLayoutService = squadFormationLayoutService;
        this.registry = registry;
        this.soldierDespawner = soldierDespawner;
        capacity = settings.MaxSquadSize;
        RebuildFormation();
    }

    public bool RegisterSoldier(SoldierCombatAgentController soldier)
    {
        if (soldier == null || !HasFreeSlot)
        {
            return false;
        }

        if (!registry.Register(soldier))
        {
            return false;
        }

        SubscribeDied(soldier);
        soldier.AssignSquad(squadRootView);
        RebuildFormation();
        return true;
    }

    public void UnregisterSoldier(SoldierCombatAgentController soldier)
    {
        if (soldier == null)
        {
            return;
        }

        UnsubscribeDied(soldier);
        registry.Unregister(soldier);
        soldier.ClearSquad(squadRootView);
        RebuildFormation();

        if (!registry.HasLivingAllies)
        {
            signalBus.Fire<SquadDefeatedSignal>();
        }
    }

    public void IncreaseCapacity(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        int grownCapacity = Mathf.Min(capacity + amount, settings.MaxSquadSize);

        if (grownCapacity == capacity)
        {
            return;
        }

        capacity = grownCapacity;
        RebuildFormation();
    }

    public void ClearFormation()
    {
        // Dropping soldiers from the registry alone left them alive in the scene while
        // the barracks refilled the squad, so the cap silently grew by a full squad
        // after every defeat. Hand the bodies back to the pool as well.
        for (int i = registry.Soldiers.Count - 1; i >= 0; i--)
        {
            SoldierCombatAgentController soldier = registry.Soldiers[i];

            if (soldier == null)
            {
                continue;
            }

            UnsubscribeDied(soldier);
            soldier.ClearSquad(squadRootView);
            soldierDespawner.Value.Release(soldier);
        }

        diedHandlers.Clear();
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
            SoldierCombatAgentController soldier = i < registry.Soldiers.Count ? registry.Soldiers[i] : null;
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
        {
            return squadRootView.transform.position;
        }

        return GetSlotWorldPosition(slots[slotIndex]);
    }

    // The squad owns the death subscription so a slot is released exactly once, and can
    // be released again when ClearFormation despawns a soldier that is still alive.
    private void SubscribeDied(SoldierCombatAgentController soldier)
    {
        UnsubscribeDied(soldier);

        Action diedHandler = () => UnregisterSoldier(soldier);
        diedHandlers[soldier] = diedHandler;
        soldier.Died += diedHandler;
    }

    private void UnsubscribeDied(SoldierCombatAgentController soldier)
    {
        if (!diedHandlers.TryGetValue(soldier, out Action diedHandler))
        {
            return;
        }

        soldier.Died -= diedHandler;
        diedHandlers.Remove(soldier);
    }
}

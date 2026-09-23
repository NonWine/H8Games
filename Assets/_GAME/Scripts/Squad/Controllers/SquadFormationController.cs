using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class SquadFormationController : ISquadFormationLayoutSource
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
    private readonly List<SoldierCombatAgentController> unassignedSoldiers = new();
    private readonly Dictionary<SoldierCombatAgentController, Action> diedHandlers = new();
    private readonly SignalBus signalBus;
    private int capacity;

    public event Action FormationChanged;

    public IReadOnlyList<FormationSlot> Slots => slots;
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
        BuildSlots();
        AssignSoldiersToClosestSlots();
        FormationChanged?.Invoke();
    }

    private void BuildSlots()
    {
        slots.Clear();

        List<Vector3> offsets = squadFormationLayoutService.CalculateLocalOffsets(capacity);

        for (int i = 0; i < offsets.Count; i++)
        {
            slots.Add(new FormationSlot(i, offsets[i]));
        }
    }

    // Slots used to be handed out in registry order, so one death re-indexed every
    // soldier behind the gap and the whole squad swapped places at once. Eight
    // soldiers walking through each other is exactly the case local avoidance
    // cannot resolve, and whoever lost the standoff never reached its slot.
    // Filling each slot with the closest soldier keeps the formation just as
    // compact while leaving almost everybody where they already stand.
    private void AssignSoldiersToClosestSlots()
    {
        unassignedSoldiers.Clear();
        unassignedSoldiers.AddRange(registry.Soldiers);

        for (int i = 0; i < slots.Count; i++)
        {
            FormationSlot slot = slots[i];
            SoldierCombatAgentController soldier = TakeClosestSoldier(GetSlotWorldPosition(slot));
            slot.AssignedSoldier = soldier;

            if (soldier == null)
            {
                continue;
            }

            soldier.AssignSquad(squadRootView);
            soldier.AssignSlot(slot);
        }
    }

    private SoldierCombatAgentController TakeClosestSoldier(Vector3 slotWorldPosition)
    {
        int closestIndex = -1;
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < unassignedSoldiers.Count; i++)
        {
            Vector3 delta = unassignedSoldiers[i].Position - slotWorldPosition;
            delta.y = 0f;

            if (delta.sqrMagnitude >= closestSqrDistance)
            {
                continue;
            }

            closestSqrDistance = delta.sqrMagnitude;
            closestIndex = i;
        }

        if (closestIndex < 0)
        {
            return null;
        }

        SoldierCombatAgentController closest = unassignedSoldiers[closestIndex];
        unassignedSoldiers.RemoveAt(closestIndex);

        return closest;
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

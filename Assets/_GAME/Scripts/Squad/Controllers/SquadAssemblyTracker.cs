using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

// Owns the "is the squad fully formed and settled" question. Registration alone is
// not enough: the last soldier still has to walk from the spawn point to its slot,
// so this polls positions against the same SlotReachThreshold the movement states
// use, rather than reacting to registration/signal timing that fires too early.
public class SquadAssemblyTracker : ITickable, IInitializable, IDisposable, ISquadAssemblyReader
{
    private readonly SquadFormationController formationController;
    private readonly SquadFollowSettings settings;

    private bool isAssembled;

    public event Action AssemblyChanged;

    public bool IsAssembled => isAssembled;

    public SquadAssemblyTracker(SquadFormationController formationController, SquadFollowSettings settings)
    {
        this.formationController = formationController;
        this.settings = settings;
    }

    public void Initialize()
    {
        formationController.FormationChanged += Refresh;
        Refresh();
    }

    public void Dispose()
    {
        formationController.FormationChanged -= Refresh;
    }

    public void Tick()
    {
        Refresh();
    }

    private void Refresh()
    {
        bool assembled = formationController.IsFull && AllSoldiersSettled();

        if (assembled == isAssembled)
        {
            return;
        }

        isAssembled = assembled;
        AssemblyChanged?.Invoke();
    }

    private bool AllSoldiersSettled()
    {
        IReadOnlyList<FormationSlot> slots = formationController.Slots;
        float reachThreshold = settings.SlotReachThreshold;

        for (int i = 0; i < slots.Count; i++)
        {
            SoldierCombatAgentController soldier = slots[i].AssignedSoldier;

            if (soldier == null)
            {
                return false;
            }

            Vector3 delta = formationController.GetSlotWorldPosition(slots[i]) - soldier.Position;
            delta.y = 0f;

            if (delta.sqrMagnitude > reachThreshold * reachThreshold)
            {
                return false;
            }
        }

        return true;
    }
}

using UnityEngine;

public class SoldierRuntimeModel : AgentRuntimeModel
{
    public SoldierRuntimeModel(BaseCombatAgentView view, UnitStats unitStats, ITargetTrackerHandler targetTracker)
        : base(view, unitStats, targetTracker)
    {
    }

    public SquadRootView SquadRootView { get; private set; }
    public FormationSlot AssignedSlot { get; private set; }

    public bool HasFormationAssignment => SquadRootView != null && AssignedSlot != null;

    public Vector3 GetAssignedSlotCenter(ISquadSlotPositionProvider squadSlotPositionProvider)
    {
        Vector3 slotCenter = squadSlotPositionProvider.GetSlotWorldPosition(AssignedSlot);
        slotCenter.y = Transform.position.y;
        return slotCenter;
    }

    public void AssignSquad(SquadRootView squadRootView)
    {
        SquadRootView = squadRootView;
    }

    public void AssignSlot(FormationSlot slot)
    {
        AssignedSlot = slot;
    }

    public void ClearSquad(SquadRootView owner)
    {
        if (SquadRootView != owner)
        {
            return;
        }

        AssignedSlot = null;
        SquadRootView = null;
    }
}

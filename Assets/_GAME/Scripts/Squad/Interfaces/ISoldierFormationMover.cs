using UnityEngine;

public interface ISoldierFormationMover
{
    void Reset();
    void Stop(bool clearPath = true);
    SoldierFormationState MoveToSlot(Transform squadRoot, Vector3 slotCenter, bool squadRootIsMoving, float deltaTime);
    SoldierFormationState MoveTowardsPosition(Vector3 destination, float stoppingDistance, float deltaTime);
    bool IsAt(Vector3 worldPosition, float threshold);
}

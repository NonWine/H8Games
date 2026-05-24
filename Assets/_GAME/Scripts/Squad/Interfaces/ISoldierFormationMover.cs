using UnityEngine;

public interface ISoldierFormationMover
{
    void Reset();
    void Stop(bool clearPath = true);
    void TeleportTo(Vector3 position, Quaternion rotation);
    SoldierFormationState MoveToSlot(Transform squadRoot, Vector3 slotCenter, bool squadRootIsMoving, float deltaTime);
    bool IsAt(Vector3 worldPosition, float threshold);
}

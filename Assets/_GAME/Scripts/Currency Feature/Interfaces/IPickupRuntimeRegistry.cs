using System.Collections.Generic;

public interface IPickupRuntimeRegistry
{
    IReadOnlyList<PickupItemController> WorldItems { get; }

    void AddWorld(PickupItemController controller);
    void AddAnimating(PickupItemController controller);
    void RemoveAnimating(PickupItemController controller);
    void RemoveWorldAt(int index);
    void PromoteToAnimating(int worldIndex);
    void TickAnimations(float deltaTime);
    void Despawn(PickupItemController controller);
    void DespawnAnimated(PickupItemController controller);
    void Clear();
}

using UnityEngine;

public interface IPickupCarrySink
{
    bool TryAttach(PickupItemController controller, out Transform anchor, out Vector3 localPos, out Quaternion localRot);
    void Detach(PickupItemController controller);
}

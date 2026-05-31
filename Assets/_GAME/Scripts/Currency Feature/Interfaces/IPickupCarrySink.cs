using System;
using UnityEngine;

public interface IPickupCarrySink
{
    event Action<PickupItemController> Evicted;

    bool TryAttach(PickupItemController controller, out Transform anchor, out Vector3 localPos, out Quaternion localRot);
    void Detach(PickupItemController controller);
    bool TryDetachNewest(out PickupItemController controller);
    void Clear();
}

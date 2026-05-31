using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IPickupPool
{
    UniTask<PickupItemView> RentAsync(GameObject prefab, CancellationToken ct);
    void Return(PickupItemView view);
}

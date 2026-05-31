using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public interface IPickupService
{
    event Action<PickupCollectedEvent> Collected;
    event Action<PickupSpawnedEvent>   Spawned;

    UniTask SpawnAsync(PickupSpawnRequest request, CancellationToken ct = default);
    void    Return(PickupItemController controller);
    void    SpendCarried(int count, Transform target);
    void    Clear();
}

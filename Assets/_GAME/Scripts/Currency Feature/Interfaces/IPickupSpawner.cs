using System;
using System.Threading;
using Cysharp.Threading.Tasks;

public interface IPickupSpawner
{
    event Action<PickupSpawnedEvent> Spawned;

    UniTask SpawnAsync(PickupSpawnRequest request, CancellationToken ct = default);
}

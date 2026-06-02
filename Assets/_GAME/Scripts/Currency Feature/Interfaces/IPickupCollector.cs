using System;

public interface IPickupCollector
{
    event Action<PickupCollectedEvent> Collected;

    void Tick();
}

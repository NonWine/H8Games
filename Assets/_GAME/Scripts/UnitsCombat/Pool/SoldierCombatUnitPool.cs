using Zenject;

public class SoldierCombatUnitPool : MonoMemoryPool<AgentSpawnParams, SoldierPoolableRoot>
{
    protected override void Reinitialize(AgentSpawnParams spawnParams, SoldierPoolableRoot item)
    {
        item.OnSpawned(spawnParams, this);
    }

    protected override void OnDespawned(SoldierPoolableRoot item)
    {
        item.OnDespawned();
        base.OnDespawned(item);
    }
}

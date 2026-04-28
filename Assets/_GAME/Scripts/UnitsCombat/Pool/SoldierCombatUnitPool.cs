using Zenject;

public class SoldierCombatUnitPool : MonoMemoryPool<AgentSpawnParams, SoldierPoolableRoot>
{
}

public class SoldierPoolableRoot : CombatUnitPoolableRoot<SoldierCombatAgentController>, IAgentDespawnRequester
{
    public void RequestDespawn() => Dispose();
}

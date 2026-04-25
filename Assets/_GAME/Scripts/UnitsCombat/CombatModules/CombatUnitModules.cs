using System.Collections.Generic;

public class CombatUnitModules
{
    private readonly List<ICombatTickModule> tickModules;
    private readonly List<IResetModule> resetModules;
    private readonly List<IDisposeModule> disposeModules;
    
    public UnitAttackAgentHandler Attack { get; }
    public UnitHealthHandler Health { get; }
    public ProjectileVisualSpawner ProjectileSpawner { get; }
    public UnitDeathModule Death { get; }
    
    public CombatUnitModules(
        UnitAttackAgentHandler attack,
        UnitHealthHandler health,
        ProjectileVisualSpawner projectileSpawner,
        UnitDeathModule death)
    {
        Attack = attack;
        Health = health;
        ProjectileSpawner = projectileSpawner;
        Death = death;
    }



    public void Tick(float deltaTime)
    {
        for (int i = 0; i < tickModules.Count; i++)
        {
            tickModules[i]?.Tick(deltaTime);
        }
    }

    public void ResetModules()
    {
        for (int i = 0; i < resetModules.Count; i++)
        {
            resetModules[i]?.Reset();
        }
    }

    public void DisposeModules()
    {
        for (int i = 0; i < disposeModules.Count; i++)
        {
            disposeModules[i]?.Dispose();
        }
    }
}

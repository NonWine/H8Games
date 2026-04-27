using System.Collections.Generic;

public class CombatUnitModules
{
    private readonly List<ICombatTickModule> tickModules = new List<ICombatTickModule>();
    private readonly List<IResetModule> resetModules = new List<IResetModule>();
    private readonly List<IDisposeModule> disposeModules = new List<IDisposeModule>();

    public IAttackModule Attack { get; }
    public IHealthModule Health { get; }
    public IDeathModule Death { get; }

    public CombatUnitModules(IAttackModule attack, IHealthModule health, IDeathModule death)
    {
        Attack = attack;
        Health = health;
        Death = death;

        TryRegister(attack);
        TryRegister(health);
        TryRegister(death);
    }

    public void Tick(float deltaTime)
    {
        for (int i = 0; i < tickModules.Count; i++)
        {
            tickModules[i].Tick(deltaTime);
        }
    }

    public void ResetModules()
    {
        for (int i = 0; i < resetModules.Count; i++)
        {
            resetModules[i].Reset();
        }
    }

    public void DisposeModules()
    {
        for (int i = 0; i < disposeModules.Count; i++)
        {
            disposeModules[i].Dispose();
        }
    }

    private void TryRegister(object module)
    {
        if (module is ICombatTickModule tick) tickModules.Add(tick);
        if (module is IResetModule reset) resetModules.Add(reset);
        if (module is IDisposeModule dispose) disposeModules.Add(dispose);
    }
}

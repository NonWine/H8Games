using System;
using UnityEngine;
using Zenject;

public class SquadCombatRegisterProvider : ISoldierCombatRegistryProvider
{
    private readonly SquadSoldierRegistry soldierRegistry;
    private readonly SignalBus signalBus;

    [Inject]
    public SquadCombatRegisterProvider(SquadSoldierRegistry soldierRegistry, SignalBus signalBus)
    {
        this.soldierRegistry = soldierRegistry;
        this.signalBus = signalBus;
    }


    public void RegisterSoldier(SoldierCombatAgent soldier)
    {
        soldierRegistry.Register(soldier);
    }

    public void UnregisterSoldier(SoldierCombatAgent soldier)
    {
        soldierRegistry.Unregister(soldier);
        if (!soldierRegistry.HasLivingAllies)
        {
            signalBus.Fire<SquadDefeatedSignal>();
        }
    }
}

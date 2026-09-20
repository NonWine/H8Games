using System.Collections.Generic;
using UnityEngine;

public class AllyCombatTargetProvider : ICombatTargetProvider
{
    private readonly Transform ownerTransform;
    private readonly TargetingData targetingData;
    private readonly SquadFormationRegistry soldierRegistry;
    private readonly HeroCombatAgentController hero;
    private readonly List<ITargetSelectionCandidate> candidatesBuffer = new();

    public AllyCombatTargetProvider(
        Transform ownerTransform,
        TargetingData targetingData,
        SquadFormationRegistry soldierRegistry,
        HeroCombatAgentController hero)
    {
        this.ownerTransform = ownerTransform;
        this.targetingData = targetingData;
        this.soldierRegistry = soldierRegistry;
        this.hero = hero;
    }

    public ICombatTarget GetTarget()
    {
        soldierRegistry.PruneInvalid();

        candidatesBuffer.Clear();
        candidatesBuffer.AddRange(soldierRegistry.Soldiers);

        if (hero.IsAlive)
        {
            candidatesBuffer.Add(hero);
        }

        return CombatTargetSelectionUtility.SelectBestTarget(
            candidatesBuffer,
            ownerTransform.position,
            targetingData);
    }
}

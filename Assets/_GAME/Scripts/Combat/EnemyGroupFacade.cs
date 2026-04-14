using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class EnemyGroupFacade : MonoBehaviour
{
    [SerializeField] private Transform engagePoint;
    [SerializeField] private bool encounterAvailable = true;
    [SerializeField] private List<StaticEnemyAgent> enemies = new();

    public event Action<EnemyGroupFacade> Cleared;

    public EnemyGroupState State { get; private set; } = EnemyGroupState.Idle;
    public bool IsCleared => State == EnemyGroupState.Cleared;
    public bool HasAliveMembers => HasLivingEnemies();
    public bool IsAvailableForEncounter => encounterAvailable && !IsCleared && HasAliveMembers;
    public Transform EngagePoint => engagePoint != null ? engagePoint : transform;
    public Vector3 EngagePointPosition => EngagePoint.position;

    private void Awake()
    {
        RefreshEnemies();
    }

    private void OnDestroy()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null)
                enemies[i].Died -= HandleEnemyDied;
        }
    }

    public void Activate(SquadCombatCoordinator coordinator)
    {
        if (!IsAvailableForEncounter)
            return;

        State = EnemyGroupState.Activated;

        for (int i = 0; i < enemies.Count; i++)
        {
            StaticEnemyAgent enemy = enemies[i];
            if (enemy == null || !enemy.IsAlive)
                continue;

            enemy.Activate(coordinator);
        }

        TryMarkCleared();
    }

    public ICombatTarget GetClosestLivingEnemy(Vector3 worldPosition)
    {
        StaticEnemyAgent closest = null;
        float closestSqrDistance = float.MaxValue;

        for (int i = 0; i < enemies.Count; i++)
        {
            StaticEnemyAgent enemy = enemies[i];
            if (enemy == null || !enemy.IsAlive)
                continue;

            Vector3 delta = enemy.transform.position - worldPosition;
            delta.y = 0f;
            float sqrDistance = delta.sqrMagnitude;
            if (sqrDistance >= closestSqrDistance)
                continue;

            closest = enemy;
            closestSqrDistance = sqrDistance;
        }

        return closest;
    }

    public bool HasLivingEnemies()
    {
        for (int i = 0; i < enemies.Count; i++)
        {
            if (enemies[i] != null && enemies[i].IsAlive)
                return true;
        }

        return false;
    }

    public void ResetRuntimeState()
    {
        RefreshEnemies();
        State = EnemyGroupState.Idle;

        for (int i = 0; i < enemies.Count; i++)
        {
            StaticEnemyAgent enemy = enemies[i];
            if (enemy == null)
                continue;

            enemy.ResetRuntimeState();
        }

        if (!HasLivingEnemies())
            State = EnemyGroupState.Cleared;
    }

    private void RefreshEnemies()
    {

        for (int i = 0; i < enemies.Count; i++)
        {
            StaticEnemyAgent enemy = enemies[i];
            if (enemy == null)
                continue;

            enemy.SetGroup(this);
            enemy.Died -= HandleEnemyDied;
            enemy.Died += HandleEnemyDied;
        }
    }

    private void HandleEnemyDied(StaticEnemyAgent enemy)
    {
        TryMarkCleared();
    }

    private void TryMarkCleared()
    {
        if (State == EnemyGroupState.Cleared || HasLivingEnemies())
            return;

        State = EnemyGroupState.Cleared;
        Cleared?.Invoke(this);
    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

public class EnemyGroupViewController : MonoBehaviour
{
    [SerializeField] private Transform engagePoint;
    [SerializeField] private List<BaseCombatAgentView> enemyViews = new();

    private readonly List<EnemyCombatAgentController> enemyControllers = new();

    public event Action<EnemyGroupViewController> Cleared;

    public EnemyGroupState State { get; private set; } = EnemyGroupState.Idle;
    public bool HasAliveMembers => HasLivingEnemies();
    public Transform EngagePoint => engagePoint != null ? engagePoint : transform;
    public Vector3 EngagePointPosition => EngagePoint.position;
    public IReadOnlyList<EnemyCombatAgentController> Enemies
    {
        get
        {
            CacheControllers();
            return enemyControllers;
        }
    }

    private void Start()
    {
        CacheControllers();
    }

    private void OnValidate()
    {
        enemyViews = GetComponentsInChildren<BaseCombatAgentView>().ToList();
    }

    public void Activate()
    {
        CacheControllers();
        State = EnemyGroupState.Activated;
    }

    public bool ContainsEnemy(ICombatTarget target)
    {
        if (target == null)
        {
            return false;
        }

        CacheControllers();

        for (int i = 0; i < enemyControllers.Count; i++)
        {
            EnemyCombatAgentController enemy = enemyControllers[i];
            if (enemy == null)
            {
                continue;
            }

            if (ReferenceEquals(enemy, target))
            {
                return enemy.IsAlive;
            }
        }

        return false;
    }

    public void ResetRuntimeState()
    {
        State = EnemyGroupState.Idle;
        RefreshEnemies();
    }

    private bool HasLivingEnemies()
    {
        CacheControllers();

        for (int i = 0; i < enemyControllers.Count; i++)
        {
            if (enemyControllers[i] != null && enemyControllers[i].IsAlive)
            {
                return true;
            }
        }

        return false;
    }

    private void CacheControllers()
    {
        if (enemyViews == null || enemyViews.Count == 0 || enemyControllers.Count == enemyViews.Count)
        {
            return;
        }

        enemyControllers.Clear();

        for (int i = 0; i < enemyViews.Count; i++)
        {
            BaseCombatAgentView enemyView = enemyViews[i];
            if (enemyView == null)
            {
                continue;
            }

            GameObjectContext context = enemyView.GetComponent<GameObjectContext>();
            if (context == null || context.Container == null)
            {
                continue;
            }

            try
            {
                EnemyCombatAgentController controller = context.Container.Resolve<EnemyCombatAgentController>();
                if (controller is not EnemyCombatAgentController enemyController)
                {
                    Debug.LogError($"[EnemyGroupViewController] Controller on '{enemyView.name}' is not an enemy controller.");
                    continue;
                }

                enemyController.Died += HandleEnemyDied;
                enemyControllers.Add(enemyController);
            }
            catch (Exception exception)
            {
                Debug.LogError($"[EnemyGroupViewController] Could not resolve controller for '{enemyView.name}': {exception.Message}");
            }
        }
    }

    private void RefreshEnemies()
    {
        CacheControllers();

        for (int i = 0; i < enemyControllers.Count; i++)
        {
            EnemyCombatAgentController enemy = enemyControllers[i];
            if (enemy == null)
            {
                continue;
            }

            enemy.Died -= HandleEnemyDied;
            enemy.Died += HandleEnemyDied;
            enemy.ResetRunTimeState();
        }
    }

    private void HandleEnemyDied()
    {
        TryMarkCleared();
    }

    private void TryMarkCleared()
    {
        if (State == EnemyGroupState.Cleared || HasAliveMembers)
        {
            return;
        }

        State = EnemyGroupState.Cleared;
        Cleared?.Invoke(this);
    }
}

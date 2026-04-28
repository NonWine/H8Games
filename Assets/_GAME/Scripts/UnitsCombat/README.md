# AgentSystem — Combat Unit Framework

Reusable combat agent architecture for Unity + Zenject projects.
Designed for fast delivery of new unit types (soldiers, enemies, tanks, air units)
without duplicating behavior logic.

---

## Core Concepts

```
IAgentView                    ← Unity view contract (transform, hit feedback)
AgentRuntimeModel             ← per-unit runtime state (health, target, alive)
CombatUnitModules             ← composable behavior bag (attack, health, death)
BaseCombatAgentController     ← pure C# orchestrator (tick, damage, lifecycle)
StateMachine / State          ← per-unit behavior modes
CombatUnitPoolableRoot        ← Zenject pool bridge (spawn/despawn only)
```

---

## Adding a New Unit Type

### 1. Create a View
```csharp
public class TankView : BaseCombatUnitView
{
    [field: SerializeField] public Transform CannonPoint { get; private set; }
}
```

### 2. Create a Runtime Model
```csharp
public class TankRuntimeModel : AgentRuntimeModel
{
    public TankRuntimeModel(IAgentView view, UnitStats stats, ITargetTrackerHandler tracker)
        : base(view, stats, tracker) { }
}
```

### 3. Create an Installer — override only what differs
```csharp
public class TankCombatAgentInstaller : CombatAgentInstaller
{
    protected override void BindModules()
    {
        base.BindModules();
        Container.Rebind<IAttackModule>().To<TankAttackModule>().AsSingle();
        Container.Rebind<IDeathModule>().To<ExplosionDeathModule>().AsSingle();
    }

    protected override void InstallFeatureBindings()
    {
        BindTargeting();
        BindRuntime();
        BindStateMachine();
        BindController();
    }
    // ...
}
```

### 4. Bind the Pool (in a parent installer)
```csharp
Container.BindMemoryPool<CombatUnitPoolableRoot, CombatUnitPool>()
    .WithInitialSize(10)
    .FromSubContainerResolve()
    .ByNewPrefabInstaller<TankCombatAgentInstaller>(tankPrefab)
    .UnderTransformGroup("Tanks [Pool]");
```

### 5. Spawn / Despawn
```csharp
// Spawn
CombatUnitPoolableRoot unit = pool.Spawn(
    new AgentSpawnParams(spawnPos, spawnRot, "tank_01"));

// Despawn (from anywhere that has a reference)
unit.Despawn();
```

---

## Module System

Modules implement one of these interfaces:

| Interface           | Purpose                          |
|---------------------|----------------------------------|
| `IAttackModule`     | Attack logic, projectile spawn   |
| `IHealthModule`     | Health, damage, death event      |
| `IDeathModule`      | Death sequence (async)           |
| `ICombatTickModule` | Per-frame update                 |
| `IResetModule`      | State reset on pool reuse        |
| `IDisposeModule`    | Cleanup on pool destroy          |

`CombatUnitModules` auto-registers each module into the correct lifecycle list
via `TryRegister()` — no manual wiring needed.

**Custom module example:**
```csharp
public class ShieldModule : IHealthModule, IResetModule, ICombatTickModule
{
    // implements all three — auto-registered into all three lists
}
```

---

## Pool Architecture

```
MemoryPool (parent container)
    └── Spawn() → CombatUnitPoolableRoot.OnSpawned()
                      → agentController.ResetState()   resets health, states
                      → SetPosition / SetIdentity
                      → gameObject.SetActive(true)

    └── Despawn() → CombatUnitPoolableRoot.OnDespawned()
                      → gameObject.SetActive(false)
                      (subcontainer stays alive, modules stay initialized)
```

The subcontainer is created **once per pool slot** and reused across spawns.
`ResetState()` restores the agent to a clean initial state without reinstantiating anything.

---

## Dependencies

- **Zenject** — DI, ITickable, IInitializable, IDisposable, MemoryPool
- **UniTask** — async death sequences
- **DOTween** — hit flash feedback (view layer only)
- **Unity 2021.3+**

---

## Package Boundary

Files safe to extract into a standalone UPM package:

```
Interfaces/     IAgentView, IDamageable, ICombatTarget, IAliveState,
                IAttackModule, IHealthModule, IDeathModule,
                ICombatTargetProvider, ICombatTargetValidator,
                ITargetTrackerHandler, ICombatTickModule,
                IResetModule, IDisposeModule, IAgentController

Controllers/    BaseCombatAgentController, AgentRuntimeModel, AgentStateBase

StateMachine/   State<T>, StateMachine<T>

Modules/        CombatUnitModules, UnitAttackAgentHandler,
                UnitHealthHandler, UnitDeathModule, AttackRuntimeModel

Services/       CombatTargetTracker, CombatTargetScoringUtility,
                DefaultCombatTargetValidator, UnitRotatorService,
                AgentAnimationController

Pool/           CombatUnitPoolableRoot, CombatUnitPool, AgentSpawnParams

Data/           HitData, UnitState
```

Game-specific code (soldiers, enemies, squad system, installers) stays in the project.

# Unity project architecture rules

## Project intent
This repository is a showcase-quality Unity pet project.
The code must look production-ready, reviewer-friendly, scalable, and maintainable.

The project is an MVP / vertical slice, not a full game framework.
Architecture must support fast delivery while still looking like strong production-style Unity code.

Main rule:
- finish playable gameplay first;
- keep architecture clean enough for review by a technical lead;
- avoid both spaghetti prototype code and unnecessary enterprise overengineering.

## Core stack
- Unity C#
- Zenject
- UniTask
- DOTween
- Odin Inspector

## Architectural defaults
- Follow SOLID where it improves clarity and maintainability
- Prefer composition over inheritance
- Keep MonoBehaviours thin
- Keep business and gameplay logic outside views
- Views should render, play feedback, expose Unity references, and forward events
- Prefer plain C# services, presenters, controllers, use cases, and state classes for logic
- Installers are for composition only
- Avoid service locator usage outside composition root
- Prefer explicit dependencies
- Prefer small focused classes
- Prefer MVP-first architecture over speculative frameworks
- Do not create abstractions before there is a real reason
- Prefer feature modules before full isolated entity architecture
- For medium gameplay features, prefer `Installer + View + Presenter/Controller + focused Services/Reactions + Data/Settings`
- Use inheritance mainly for shared feature-family setup such as base installers or shared reaction bases

## MVP architecture rule
Always implement the smallest clean version that supports the current vertical slice.

Before adding a new abstraction, system, manager, factory, signal, pool, subcontainer, or state machine, check:
- Does this directly support the current playable feature?
- Does this remove real coupling or only make the code look more abstract?
- Will this make the feature easier to extend soon?
- Can the same result be achieved with a simpler focused class?

If the feature is small, keep it small.
If the feature is medium, use the lightweight feature module pattern described below.
If the feature is complex, use the complex entity pattern described below.

## Layering rules
- Domain and gameplay rules must not depend on Unity view details unless clearly justified
- Animation belongs to presentation/view layer unless there is a strong reason otherwise
- Systems such as inventory, resource logic, progression, quests, and economy should remain testable without scene objects
- Do not place business rules directly into view classes
- UI views must not own gameplay decisions
- VFX and audio modules must not own gameplay decisions
- Gameplay systems should depend on narrow contracts, not full concrete views/controllers

## Zenject rules
- Prefer constructor injection in non-MonoBehaviour classes
- Use factories for dynamic object creation
- Evaluate pools for frequently spawned objects, temporary effects, and drops
- Use subcontainers for complex entities with multiple internal dependencies
- Use signals only when decoupling is actually needed
- Avoid using DiContainer directly outside installers and dedicated factories
- Do not inject DiContainer into ordinary gameplay logic
- Bind by interfaces only when the interface gives real value
- Do not bind everything as `AsSingle()` by default; think about lifetime
- Keep installer methods grouped by responsibility

Good installer style:

```csharp
public override void InstallBindings()
{
    BindViews();
    BindData();
    BindSignals();
    BindStateMachine();
    BindModules();
    BindFacade();
}
```

Bad installer style:

```csharp
public override void InstallBindings()
{
    // Huge unreadable list of unrelated bindings.
}
```

## Reference pattern: medium feature module architecture
Use the current `Resources` module as the main architectural reference for medium-size gameplay features.

The Resources pattern demonstrates:
- one installer per prefab or feature root;
- optional base installer with template methods for a feature family;
- one presenter or top-level controller that orchestrates the flow;
- focused services, reactions, animators, and helpers for each responsibility;
- clean view that exposes references and forwards events;
- serializable settings plus ScriptableObject installers for tuning;
- lightweight interfaces and result structs instead of heavy global abstractions;
- optional debug controllers that stay isolated from normal runtime flow.

Use this pattern for:
- resource nodes;
- destructible props;
- simple building parts;
- world interactables with presentation flow;
- respawn/drop/hit-feedback features;
- any feature that needs several classes but does not need a facade, subcontainer, or full state machine.

Expected flow:

```text
View raises event
    -> Presenter/controller receives it
        -> Gameplay service calculates result
            -> Presentation/reaction plays feedback
            -> Drop/respawn/reset services finish the flow
```

Folder shape usually looks like:

```text
Assets/_Project/Scripts/FeatureName/
  Controllers/
  Data/
  Installers/
  Interfaces/
  Services/
    Flow/
  View/
```

Do not introduce a full state machine, facade, factory, or subcontainer until the feature has real behavior modes, runtime creation needs, or isolated internal dependencies.

## Reference pattern: complex entity module architecture
Use the existing Car Controller module as the architectural reference for complex gameplay entities.

The Car Controller pattern demonstrates:
- state machine;
- factory;
- subcontainer / GameObjectContext;
- installer that assembles the entity;
- clean view;
- internal modules;
- facade-style public API.

Use this pattern for entities that are more complex than a simple interactable object.

Good candidates:
- Player
- Enemy
- Worker
- Vehicle
- Boss
- Expedition unit
- Complex building with internal behavior
- Any entity with states, config, runtime creation, internal modules, and isolated dependencies

Do not force this pattern onto small objects.

## Complex entity core idea
A complex entity should be built as an isolated module with:
- prefab-level `GameObjectContext` when runtime entity isolation is needed;
- dedicated installer;
- public facade or narrow public interface;
- clean view;
- state machine when behavior has real distinct modes;
- internal focused modules;
- local signals/events when decoupling is useful;
- config/data classes;
- factory for runtime creation.

External systems must not know how the entity works inside.
They should interact only through the facade or narrow interfaces.

Expected flow:

```text
Factory creates entity
    -> prefab has GameObjectContext
        -> Installer binds View, Config, Signals, Modules, StateMachine
            -> Facade exposes public API
            -> View stays clean
            -> States control behavior
            -> Modules handle focused technical/gameplay parts
```

## Complex entity folder structure
Use this as a reference, not as a mandatory structure for every feature:

```text
Assets/_Project/Scripts/EntityName/
  Core/
    EntityFacade.cs
    EntityInstaller.cs
    EntityFactory.cs
    EntityBrain.cs

  View/
    EntityView.cs
    EntityVisuals.cs

  StateMachine/
    EntityStateMachine.cs
    IState.cs
    IdleState.cs
    ActiveState.cs
    ExitState.cs

  Input/
    IEntityInput.cs
    EntityInputHandler.cs

  Physics/
    EntityMovementModule.cs
    EntityRotationModule.cs

  Modules/
    Audio/
    VFX/
    Gameplay/

  Data/
    EntityConfig.cs
    EntityRuntimeData.cs

  Signals/
    EntitySignals.cs
```

For small features, use fewer folders and fewer classes.
Do not create empty architecture folders without real code.

## Facade rule
The facade is the public contract of a complex entity.

External systems should interact with the entity through the facade or narrow interfaces.

Example:

```csharp
public class EnemyFacade : MonoBehaviour, ICombatTarget
{
    public Transform Transform => transform;
    public bool IsAlive => health.IsAlive;

    private Health health;
    private DamageReceiver damageReceiver;

    [Inject]
    public void Construct(Health health, DamageReceiver damageReceiver)
    {
        this.health = health;
        this.damageReceiver = damageReceiver;
    }

    public void ApplyDamage(DamageData damage)
    {
        damageReceiver.ApplyDamage(damage);
    }
}
```

The facade should:
- expose only high-level operations;
- hide internal modules;
- avoid gameplay implementation details;
- own identity if needed;
- be resolved from the entity subcontainer when using subcontainers.

Do not let external systems directly use internal states, modules, views, or controllers.

## View rule
Views must be clean MonoBehaviours.

A view may contain:
- serialized Unity references;
- `[field: SerializeField]` auto-properties for read-only exposed references;
- `Transform`, `Rigidbody`, `Animator`, `Collider`, `WheelCollider`, UI refs, VFX refs;
- simple access to prefab-assigned config/data assets when the view is the natural holder of that reference;
- simple Unity events;
- interaction forwarding;
- visual-only helper methods;
- injected read-only state for inspector/debug display when it does not move gameplay logic into the view;
- empty typed subclasses when a prefab/category marker is enough.

A view must not contain:
- business logic;
- state transitions;
- economy rules;
- quest progression;
- AI decisions;
- direct service calls;
- object creation logic.

Good style:

```csharp
public class EntityView : MonoBehaviour, IInteractable
{
    [field: SerializeField] public Transform ExitPoint { get; private set; }
    [field: SerializeField] public Rigidbody Rigidbody { get; private set; }

    public event Action<GameObject> Interacted;

    public void Interact(GameObject interactor)
    {
        Interacted?.Invoke(interactor);
    }
}
```

MonoBehaviour is an adapter.
Real logic lives in injected C# classes where possible.

## Presenter, controller, service, and reaction rule
Use naming by responsibility, not by habit.

- `Presenter` or top-level feature `Controller` orchestrates the whole feature flow, subscriptions, async sequencing, reset flow, and collaboration between services
- `Service` owns one gameplay or technical responsibility such as damage, respawn, drop spawn, or occupancy
- `Reaction`, `Animator`, or specialized presentation controller owns one authored feedback sequence
- `Installer` binds the graph and nothing else
- `View` exposes refs and forwards events

Prefer one obvious orchestrator over several managers calling each other.
For medium features, this is usually enough and is preferred over introducing a state machine.

## Feature family installer rule
When several prefabs in one family share most bindings, use a base installer with narrow extension points.

Good examples:
- `EnvironmentResourceInstaller` binds shared view and core services
- `TreeInstaller` overrides only tree-specific bindings
- specialized settings installers bind only local tuning data

Rules:
- keep common bindings in the base installer;
- expose only a few protected template methods such as `InstallFeatureBindings()` or `InstallDropSpawner()`;
- do not push gameplay logic into installers;
- use child installers for grouped config only when it actually improves reuse.

## Factory rule
Runtime-created complex entities should be created through a factory.

Factories may use `DiContainer`.
Normal gameplay classes must not use `DiContainer.Resolve()` or direct prefab instantiation.

Allowed:

```csharp
public class EntityFactory : IFactory<EntitySpawnData, EntityFacade>
{
    private readonly DiContainer container;
    private readonly EntityDefinition definition;

    public EntityFactory(DiContainer container, EntityDefinition definition)
    {
        this.container = container;
        this.definition = definition;
    }

    public EntityFacade Create(EntitySpawnData spawnData)
    {
        GameObject instance = container.InstantiatePrefab(definition.Prefab, spawnData.Position, spawnData.Rotation, null);
        GameObjectContext context = instance.GetComponent<GameObjectContext>();
        EntityFacade facade = context.Container.Resolve<EntityFacade>();

        facade.Initialize(spawnData.Id);
        return facade;
    }
}
```

Not allowed in gameplay logic:

```csharp
container.Resolve<SomeService>();
Object.Instantiate(prefab);
FindObjectOfType<SomeManager>();
```

Factories and installers are composition-root code.
Gameplay logic is not.

## Subcontainer rule
Use a prefab-level subcontainer for complex entities.

The entity prefab should own:
- its own view references;
- its own states;
- its own modules;
- its own local services;
- its own facade.

Parent scene systems should receive only the facade or narrow interfaces.
This prevents scene-level systems from depending on internal implementation details.

Do not use subcontainers for tiny objects that have no internal dependency graph.

## State machine rule
Use a state machine when the entity has real behavior modes.

Good examples:
- empty vehicle;
- entering vehicle;
- driving vehicle;
- exiting vehicle;
- enemy idle/chase/attack/death;
- worker idle/move/gather/return/deposit;
- player idle/move/interact/attack.

Basic state contract:

```csharp
public interface IState
{
    void Enter();
    void Exit();
    void Tick();
}
```

Each state should:
- own only state-specific logic;
- subscribe to events/signals in `Enter`;
- unsubscribe in `Exit`;
- request transitions explicitly;
- not directly manipulate unrelated systems.

The state machine owns current state and transition flow.
Do not use a state machine for one-step behavior.

## Module controller rule
For groups of similar modules, use module controllers.

Good examples:
- physics modules;
- VFX modules;
- sound modules;
- AI modules;
- ability modules.

Pattern:

```csharp
public interface IEntityModule
{
    void Initialize();
    void Tick();
    void Dispose();
}

public class EntityModuleController<T> : IInitializable, ITickable, IDisposable
    where T : IEntityModule
{
    private readonly List<T> modules;

    public EntityModuleController(List<T> modules)
    {
        this.modules = modules;
    }

    public void Initialize()
    {
        foreach (T module in modules)
        {
            module.Initialize();
        }
    }

    public void Tick()
    {
        foreach (T module in modules)
        {
            module.Tick();
        }
    }

    public void Dispose()
    {
        foreach (T module in modules)
        {
            module.Dispose();
        }
    }
}
```

Use this when several modules share the same lifecycle.
Do not create module controllers for only one tiny class unless it is clearly expected to grow soon.

## Internal modules rule
Internal modules should be small and focused.

Good examples based on Car-style architecture:
- `CarMotorModule`
- `CarSteeringModule`
- `CarFrictionModule`
- `CarStabilizerModule`
- `CarEngineSoundModule`
- `CarTireSoundModule`
- `CarLightsVfxModule`
- `CarTireEffectsVfxModule`

Each module should have one reason to change.

Bad examples:
- `EnemyController` that handles movement, damage, animation, AI, sound, VFX, loot, and UI
- `WorkerManager` that knows every building and every worker detail
- `PlayerController` with input, movement, interaction, combat, inventory, animation, and UI in one class

## Signals and events rules
Use signals/events carefully.

Good use cases:
- UI reacting to inventory changes;
- quest system reacting to building rebuilt;
- VFX/SFX reacting to gameplay events;
- local entity decoupling inside a complex entity;
- analytics/debug hooks;
- optional reactions.

Avoid using signals as the main gameplay flow controller.

Good local entity signal examples:

```csharp
public readonly struct EntityInteractionSignal
{
    public readonly GameObject Interactor;

    public EntityInteractionSignal(GameObject interactor)
    {
        Interactor = interactor;
    }
}

public readonly struct ChangeEntityStateSignal
{
    public readonly Type TargetStateType;

    public ChangeEntityStateSignal(Type targetStateType)
    {
        TargetStateType = targetStateType;
    }
}
```

Rules:
- declare local entity signals in the entity installer;
- subscribe in `Initialize` or `Enter`;
- unsubscribe in `Dispose` or `Exit`;
- do not use signals to hide unclear gameplay flow;
- do not turn the whole game into an invisible signal chain.

Direct method call is better when ownership is obvious.

## Config and ScriptableObject rules
Use ScriptableObjects for design-time config.

Good examples:
- building config;
- resource source config;
- worker config;
- enemy config;
- vehicle config;
- upgrade config;
- expedition config.

Rules:
- ScriptableObjects should not store mutable runtime state unless explicitly intended;
- runtime state should live in runtime classes;
- configs should be easy to tune from Inspector;
- create runtime copies from templates when instance-specific mutation is needed;
- not every tunable needs its own ScriptableObject asset; nested `[Serializable]` settings classes are valid for local feature tuning;
- ScriptableObject installers may bind either the raw settings instance or a runtime copy, whichever better fits the feature;
- config/settings classes should stay dumb and dependency-free.

Good style:

```csharp
public override void InstallBindings()
{
    EntityRuntimeData runtimeData = template != null
        ? new EntityRuntimeData(template)
        : new EntityRuntimeData();

    Container.BindInstance(runtimeData);
}
```

Reason:
- ScriptableObject assets stay immutable during play;
- runtime state does not accidentally dirty project assets;
- each entity instance can have independent values.

## Input rules
Separate input reading from entity behavior.

Input interfaces should expose only values/actions:

```csharp
public interface IEntityInput
{
    float Move { get; }
    float Rotate { get; }
    bool IsActionPressed { get; }
}

public interface IEntityInputHandler : IEntityInput
{
    void Enable();
    void Disable();
}
```

States decide when input is enabled or disabled.
Input class should not decide high-level gameplay state.

## Physics rules
Physics logic should be split into focused modules.

Good examples:
- motor;
- steering;
- friction;
- stabilizer;
- gravity;
- collision handling.

Use `IFixedTickable` for physics update orchestration when needed.
Do not mix physics, input, VFX, sound, and state logic in one MonoBehaviour.

## Audio and VFX rules
Audio and VFX are entity modules, not gameplay owners.

They may read:
- input state;
- physics state;
- config;
- view references;
- state change events.

They should not:
- change quest progression;
- change inventory;
- trigger core gameplay decisions;
- spawn gameplay entities;
- own gameplay state transitions.

## Occupancy and ownership rule
If an entity can be occupied, controlled, selected, or used by another entity, isolate this into a dedicated handler.

Good examples:
- `CarOccupancyHandler`
- `TurretOccupancyHandler`
- `WorkerAssignmentHandler`
- `InteractableOwnershipHandler`

This handler owns:
- current user/owner;
- enter/exit logic;
- attach/detach behavior;
- validation like "already occupied".

Do not put occupancy logic into View or directly into StateMachine unless it is truly state-specific.

## DOTween rules
- Prefer Sequence for authored gameplay feedback
- Link or kill tweens safely according to object lifetime
- Avoid orphan tweens
- Expose timings, amplitudes, punch strengths, offsets, and durations as serialized settings or config
- Avoid hidden looping tweens unless intentionally designed
- Keep DOTween code in view, VFX, feedback, or presentation classes
- Gameplay logic must not depend on tween completion unless the sequence is explicitly part of gameplay flow

## UniTask rules
- Use UniTask for async gameplay flows, UI transitions, delays, and scene loading
- Always use cancellation tokens for async operations tied to MonoBehaviour lifetime
- Avoid unmanaged fire-and-forget tasks
- Avoid `async void` except Unity event handlers when unavoidable
- Handle cancellation intentionally

Good style:

```csharp
private async UniTask PlayAsync(CancellationToken cancellationToken)
{
    await UniTask.Delay(TimeSpan.FromSeconds(delay), cancellationToken: cancellationToken);
}
```

## Odin Inspector rules
Use Odin to improve editor workflow.

Good use cases:
- debug buttons;
- read-only runtime state;
- grouped settings;
- validation warnings;
- ScriptableObject configs.

Do not use Odin to hide bad runtime architecture.

## Code style requirements

### Naming
- Use PascalCase for classes, interfaces, methods, properties, delegates, and events
- All interfaces must start with `I`
- Use camelCase for non-static fields and method parameters
- Use PascalCase for static fields
- Omit namespaces by default to match the current project style unless the surrounding module already uses them
- Treat abbreviations as normal words:
  - `XmlHttpRequest`
  - `url`
  - `FindPostById`
- Method names must be verbs
- Boolean fields, properties, and methods should answer a question and use names like `Is`, `Has`, `Can`
- Use meaningful full names, do not shorten names unnecessarily
- In compound container names, use plural first word:
  - `ObjectsPool`
  - `NodesList`

### Files and class layout
- Each new class should normally be placed in its own file
- Exception: a very small helper class tightly coupled to the main class may stay in the same file
- Prefer feature folders that keep `Installers`, `Controllers`, `Services`, `Data`, `Interfaces`, and `View` together for the same feature
- Member order inside a class:
  1. constants
  2. fields
  3. properties
  4. constructors or initialization methods
  5. methods
- Inside each category use this order:
  1. public
  2. protected
  3. private
- Static members go before instance members inside the same visibility group
- A paired private field and public property may stay close together when it improves readability

### Fields and properties
- Prefer private serialized fields for Inspector-only exposure:
  - `[SerializeField] private float moveSpeed;`
- In views, `[field: SerializeField] public Transform ExitPoint { get; private set; }` is preferred when the reference should be inspector-assigned but read-only in code
- In plain settings/data classes, public tune fields are acceptable when the class is a simple Inspector DTO
- Each variable declaration must be on its own line
- Always write explicit access modifiers
- Prefer readonly fields where possible
- Do not use leading underscore for fields unless the existing local project convention already uses it

### Formatting
- Use Allman style braces
- Every opening brace must be on a new line
- Prefer braces for conditionals
- A short guard-clause `return` may stay without braces if it improves readability and remains consistent
- Keep exactly one empty line between methods
- Use internal blank lines only to separate logical sections inside a method
- If a method needs too many visual sections, refactor it into smaller methods

### Switch statements
- Always include a functional `default` branch
- At minimum, log or otherwise handle unexpected state explicitly

### Attributes
- Attributes for fields and properties stay on the same line
- Attributes for classes and methods go on the line above

### Comments and documentation
- Write comments in English only
- Place comments above the code they describe, never inline at the end of a line
- Public classes, public methods, and public enums should use XML documentation comments when they are part of a shared public API or actually need explanation
- Do not add XML docs to every local gameplay class by default
- Write comments only when they add information that is not obvious from good naming
- Do not write comments that repeat the method name in prose

### Constants and literals
- Avoid magic constants
- Numeric literals in code should normally be limited to:
  - `0`
  - `1`
  - `-1`
  - `2`
  - `0.5f` when calculating center or average
- In other cases, extract a named constant or config value
- Prefer serialized config/settings over hardcoded tuning numbers

### Null checks
- Do not write unnecessary defensive null checks
- Do not add null checks for constructor-injected dependencies unless null is a valid documented state
- Do not add null checks for required serialized references just to hide broken prefab setup
- Required serialized references should fail clearly during validation, install, or play mode instead of silently doing nothing
- Null checks are allowed only when null is a real expected case:
  - optional dependency;
  - external API result;
  - user-provided data;
  - lookup that can legitimately fail;
  - destroyed Unity object boundary;
  - feature explicitly designed to work without that reference
- Prefer explicit validation over scattered defensive checks

### Sealed classes
- Do not mark classes as `sealed`
- Do not generate `sealed` by default for services, controllers, presenters, states, modules, factories, installers, views, facades, helpers, or data classes
- Use normal classes unless the user explicitly asks for `sealed`

### Code hygiene
- Remove dead code
- Do not keep large commented-out code blocks
- Do not leave temporary hacks without an explicit reason
- Keep methods focused and short
- Prefer readonly fields where possible
- Minimize allocations in gameplay hot paths
- Match existing folder and naming conventions
- Avoid broad unrelated refactors just to solve a local task
- Avoid clever code when simple code is easier to review

## Testing rules
- Non-trivial pure logic should have unit tests
- DI wiring and MonoBehaviour-driven feature behavior should have integration tests where useful
- Startup-critical scenes/features should have smoke coverage when justified
- Prefer EditMode tests for pure C# logic
- Prefer PlayMode tests only when Unity scene/prefab integration matters
- Do not waste time testing simple view forwarding unless it protects important behavior

## Forbidden patterns
- No god classes
- No hidden singleton dependencies
- No business logic dumped into animation classes
- No business logic dumped into view classes
- No broad unrelated refactors just to solve a local task
- No architecture shortcuts that reduce long-term readability
- No unnecessary null checks
- No `sealed` classes
- No service locator in gameplay logic
- No direct `Object.Instantiate` in normal gameplay classes when a factory should own creation
- No `FindObjectOfType` / scene search as normal dependency access
- No signal chains that hide the core gameplay flow
- No premature generic frameworks for one use case
- No forcing Car-style entity architecture onto medium features that only need the Resources pattern

## Simple object vs feature module vs complex entity decision
Before implementing a new entity, classify it.

Simple Object:
- one view;
- one interaction;
- no internal state machine;
- no runtime spawning complexity;
- no internal module graph.

Use simple MonoBehaviour + small service/controller if needed.

Feature Module:
- one feature root or prefab root;
- several collaborating services/controllers;
- local data/settings;
- local async presentation flow or reset flow;
- optional base installer for a feature family;
- no real need for facade, subcontainer, or full state machine.

Use the Resources-style architecture:
- installer as feature composition root;
- one presenter/controller as orchestrator;
- focused services/reactions/helpers underneath.

Complex Entity:
- has multiple behavior states;
- has internal modules;
- has config;
- may be spawned dynamically;
- needs isolated dependencies;
- external systems should see only facade/interface.

Use Car-style architecture.

## Expected answer style
When proposing code changes:
1. Briefly explain the design choice
2. List files to create or modify
3. Provide implementation
4. Mention tests
5. Mention risks or follow-ups

When editing existing code:
1. Preserve existing architecture unless there is a clear reason to change it
2. Keep changes local to the requested feature
3. Explain any architectural deviation
4. Do not silently introduce new frameworks or global systems

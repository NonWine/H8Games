using Unity.AI.Navigation;
using UnityEngine;

// Fixes a load-order race, and the execution order attribute is the whole fix.
//
// Zenject's SceneContext runs at -9999 and instantiates the hero and every unit
// from inside InstallBindings. NavMeshSurface only registers its baked data from
// its own OnEnable at -102, so every NavMeshAgent that Zenject spawned came up
// before a NavMesh existed and logged "Failed to create agent because there is
// no valid NavMesh" - an agent that fails to register never steers again.
//
// Running below SceneContext and calling AddData() early closes the window.
// AddData() is idempotent (it early-outs on a valid instance), so the surface's
// own OnEnable stays correct and nothing here has to be undone.
[DefaultExecutionOrder(-10000)]
public class NavMeshSurfaceBootstrap : MonoBehaviour
{
    [Tooltip("Every surface whose data must exist before the container spawns " +
             "anything that walks on it.")]
    [SerializeField] private NavMeshSurface[] surfaces;

    private void Awake()
    {
        for (int i = 0; i < surfaces.Length; i++)
        {
            NavMeshSurface surface = surfaces[i];
            if (surface == null)
            {
                continue;
            }

            surface.AddData();
        }
    }
}

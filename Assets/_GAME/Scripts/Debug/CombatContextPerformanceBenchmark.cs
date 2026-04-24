using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
using Zenject;

public class CombatContextPerformanceBenchmark : MonoBehaviour
{
    [Header("Zenject")]
    [SerializeField] private SceneContext sceneContext;

    [Header("Prefabs")]
    [SerializeField] private GameObject prefabA;
    [SerializeField] private GameObject prefabB;

    [Header("Test")]
    [SerializeField, Min(1)] private int instanceCount = 100;
    [SerializeField, Min(0.1f)] private float warmupSeconds = 2f;
    [SerializeField, Min(0.5f)] private float measureSeconds = 5f;
    [SerializeField] private Vector3 spawnSpacing = new Vector3(2f, 0f, 2f);
    [SerializeField] private Vector3 startOffset = Vector3.zero;
    [SerializeField] private bool runOnStart = true;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();
    private ProfilerRecorder gcAllocRecorder;
    private bool running;
    private int previousVSyncCount;
    private int previousTargetFrameRate;

    private void Start()
    {
        if (sceneContext == null)
            sceneContext = FindObjectOfType<SceneContext>();

        if (runOnStart)
            StartCoroutine(RunBenchmark());
    }

    [ContextMenu("Run Benchmark")]
    public void RunFromContextMenu()
    {
        if (!running)
            StartCoroutine(RunBenchmark());
    }

    private IEnumerator RunBenchmark()
    {
        running = true;

        if (!EnsureSceneContext())
        {
            running = false;
            yield break;
        }

        CacheAndDisableFrameCap();

        if (prefabA == null || prefabB == null)
        {
            Debug.LogError("[CombatContextPerformanceBenchmark] Assign both prefabA and prefabB.");
            RestoreFrameCap();
            running = false;
            yield break;
        }

        Debug.Log(
            $"[CombatContextPerformanceBenchmark] Frame cap disabled for benchmark. " +
            $"Previous vSyncCount={previousVSyncCount}, previous targetFrameRate={previousTargetFrameRate}");

        yield return RunCase("A", prefabA);
        yield return RunCase("B", prefabB);

        RestoreFrameCap();
        running = false;
    }

    private IEnumerator RunCase(string label, GameObject prefab)
    {
        ClearSpawned();
        yield return null;

        float spawnStart = Time.realtimeSinceStartup;
        if (!SpawnBatch(prefab))
        {
            RestoreFrameCap();
            running = false;
            yield break;
        }
        float spawnMs = (Time.realtimeSinceStartup - spawnStart) * 1000f;

        yield return new WaitForSecondsRealtime(warmupSeconds);

        gcAllocRecorder = StartGcRecorder();

        int frames = 0;
        float sumDelta = 0f;
        float minDelta = float.MaxValue;
        float maxDelta = 0f;

        float start = Time.realtimeSinceStartup;
        float end = start + measureSeconds;

        while (Time.realtimeSinceStartup < end)
        {
            float dt = Time.unscaledDeltaTime;
            if (dt > 0f)
            {
                sumDelta += dt;
                frames++;
                if (dt < minDelta) minDelta = dt;
                if (dt > maxDelta) maxDelta = dt;
            }

            yield return null;
        }

        float elapsed = Mathf.Max(0.0001f, Time.realtimeSinceStartup - start);
        float avgDelta = frames > 0 ? sumDelta / frames : 0f;
        float avgFps = avgDelta > 0f ? 1f / avgDelta : 0f;
        float minFps = maxDelta > 0f ? 1f / maxDelta : 0f;
        float maxFps = minDelta > 0f ? 1f / minDelta : 0f;
        long gcBytes = gcAllocRecorder.Valid ? gcAllocRecorder.LastValue : -1;

        gcAllocRecorder.Dispose();

        Debug.Log(
            $"[CombatContextPerformanceBenchmark] {label}: " +
            $"spawn={spawnMs:0.00}ms, " +
            $"avgFps={avgFps:0.00}, " +
            $"avgFrameMs={(avgDelta * 1000f):0.00}, " +
            $"minFps={minFps:0.00}, " +
            $"maxFps={maxFps:0.00}, " +
            $"gcAllocBytes={gcBytes}, " +
            $"frames={frames}, " +
            $"elapsed={elapsed:0.00}s");
    }

    private bool SpawnBatch(GameObject prefab)
    {
        if (sceneContext == null || sceneContext.Container == null)
        {
            Debug.LogError("[CombatContextPerformanceBenchmark] Missing SceneContext or container.");
            return false;
        }

        int side = Mathf.CeilToInt(Mathf.Sqrt(instanceCount));
        Vector3 origin = transform.position + startOffset;

        for (int i = 0; i < instanceCount; i++)
        {
            int x = i % side;
            int z = i / side;
            Vector3 pos = origin + new Vector3(x * spawnSpacing.x, 0f, z * spawnSpacing.z);

            GameObject instance = sceneContext.Container.InstantiatePrefab(prefab, pos, Quaternion.identity, transform);
            spawnedObjects.Add(instance);
        }

        return true;
    }

    private void CacheAndDisableFrameCap()
    {
        previousVSyncCount = QualitySettings.vSyncCount;
        previousTargetFrameRate = Application.targetFrameRate;

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = -1;
    }

    private void RestoreFrameCap()
    {
        QualitySettings.vSyncCount = previousVSyncCount;
        Application.targetFrameRate = previousTargetFrameRate;
    }

    private bool EnsureSceneContext()
    {
        if (sceneContext != null && sceneContext.Container != null)
            return true;

        if (sceneContext == null)
            sceneContext = FindObjectOfType<SceneContext>();

        return sceneContext != null && sceneContext.Container != null;
    }

    private void ClearSpawned()
    {
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null)
                Destroy(spawnedObjects[i]);
        }

        spawnedObjects.Clear();
    }

    private static ProfilerRecorder StartGcRecorder()
    {
        return ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
    }

    private void OnDestroy()
    {
        if (gcAllocRecorder.Valid)
            gcAllocRecorder.Dispose();

        ClearSpawned();
    }
}

using UnityEngine;
using Zenject;

public class FloatingTextService : IFloatingTextService, IInitializable
{
    private readonly FloatingTextPool pool;
    private readonly FloatingTextConfig config;
    private readonly Transform cameraTransform;

    private float lastShowTime = float.NegativeInfinity;

    public FloatingTextService(FloatingTextPool pool, FloatingTextConfig config, Transform cameraTransform)
    {
        this.pool = pool;
        this.config = config;
        this.cameraTransform = cameraTransform;
    }

    public void Initialize()
    {
        pool.Prepare(config.PoolSize);
    }

    public void Show(string text, Color color, Vector3 worldPosition)
    {
        if (!config.Enabled)
        {
            return;
        }

        float now = Time.unscaledTime;
        if (now - lastShowTime < config.MinIntervalSeconds)
        {
            return;
        }

        if (!pool.TryTake(out FloatingTextView view))
        {
            return;
        }

        lastShowTime = now;
        view.Play(text, color, ResolveSpawnPosition(worldPosition), cameraTransform.rotation, config);
    }

    private Vector3 ResolveSpawnPosition(Vector3 worldPosition)
    {
        float spread = config.HorizontalSpread;

        return worldPosition + new Vector3(
            Random.Range(-spread, spread),
            config.SpawnHeight,
            Random.Range(-spread, spread));
    }
}

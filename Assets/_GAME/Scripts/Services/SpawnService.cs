using System;
using System.Collections.Generic;

public sealed class SpawnService<T> where T : class
{
    private readonly Func<T> spawnFactory;
    private readonly Func<T, bool> alivePredicate;
    private readonly List<T> alive = new();
    private readonly Func<float> spawnIntervalProvider;
    private readonly Func<int> maxAliveProvider;
    private float timer;

    public SpawnService(Func<T> spawnFactory, Func<T, bool> alivePredicate, Func<float> spawnIntervalProvider,
        Func<int> maxAliveProvider)
    {
        this.spawnFactory = spawnFactory;
        this.alivePredicate = alivePredicate;
        this.spawnIntervalProvider = spawnIntervalProvider;
        this.maxAliveProvider = maxAliveProvider;
    }

    public int AliveCount
    {
        get
        {
            Prune();
            return alive.Count;
        }
    }

    public bool Tick(float deltaTime, out T spawned)
    {
        spawned = null;
        timer += deltaTime;
        Prune();

        if (timer < spawnIntervalProvider())
            return false;

        if (alive.Count >= maxAliveProvider())
            return false;

        timer = 0f;
        spawned = spawnFactory();
        if (spawned == null)
            return false;

        alive.Add(spawned);
        return true;
    }

    public void ResetTimer()
    {
        timer = 0f;
    }

    private void Prune()
    {
        alive.RemoveAll(item => item == null || !alivePredicate(item));
    }
}

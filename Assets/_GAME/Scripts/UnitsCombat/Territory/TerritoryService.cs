using System;
using UnityEngine;
using Zenject;

public class TerritoryService : IInitializable, ITickable, IDisposable
{
    private readonly ITerritoryView      view;
    private readonly TerritoryConfig     config;
    private readonly TerritoryUnitTracker tracker;

    private float scanTimer;

    [Inject]
    public TerritoryService(ITerritoryView view, TerritoryConfig config, LevelManager levelManager)
    {
        this.view    = view;
        this.config  = config;
        tracker      = new TerritoryUnitTracker(config, levelManager);
    }

    public void Initialize()
    {
        scanTimer = config.UpdateInterval;
    }

    public void Tick()
    {
        float dt = Time.deltaTime;

        scanTimer += dt;
        bool scanChanged = false;
        if (scanTimer >= config.UpdateInterval)
        {
            scanTimer   = 0f;
            scanChanged = tracker.Scan(out bool levelChanged);
            if (levelChanged)
            {
                view.Clear();
                return;
            }
        }

        bool positionsChanged = tracker.UpdatePositions(dt);
        if (positionsChanged || scanChanged)
            view.Refresh(tracker.SmoothedPositions, config);
    }

    public void Dispose()
    {
        view.Clear();
        tracker.Reset();
    }
}

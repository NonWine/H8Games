using System.Collections.Generic;
using UnityEngine;

// Every ally sweeps for its own target, so the level's enemies are gathered once
// per frame here rather than once per unit per sweep: the whole squad plus the
// hero asking in the same frame share a single pass over the groups.
// A missing level or a half-built group yields an empty list instead of throwing -
// sensing must never be the thing that breaks a combat frame.
public class LevelEnemyCandidateSource : IEnemyCandidateSource
{
    private readonly LevelManager levelManager;
    private readonly List<ITargetSelectionCandidate> candidates = new();

    private int lastRebuiltFrame = -1;

    public IReadOnlyList<ITargetSelectionCandidate> Candidates
    {
        get
        {
            RebuildIfStale();
            return candidates;
        }
    }

    public LevelEnemyCandidateSource(LevelManager levelManager)
    {
        this.levelManager = levelManager;
    }

    private void RebuildIfStale()
    {
        if (lastRebuiltFrame == Time.frameCount)
        {
            return;
        }

        lastRebuiltFrame = Time.frameCount;
        candidates.Clear();

        LevelRuntime level = levelManager.CurrentLevel;
        if (level == null)
        {
            return;
        }

        IReadOnlyList<EnemyGroupViewController> groups = level.Groups;

        for (int i = 0; i < groups.Count; i++)
        {
            EnemyGroupViewController group = groups[i];
            if (group == null || group.State == EnemyGroupState.Cleared)
            {
                continue;
            }

            candidates.AddRange(group.Enemies);
        }
    }
}

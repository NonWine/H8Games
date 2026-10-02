using System.Collections.Generic;
using UnityEngine;

public static class EnemySortieCoordinator
{
    private static readonly List<Vector3> ActiveSortieDestinations = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Reset()
    {
        ActiveSortieDestinations.Clear();
    }

    public static bool CanSortie(int maxConcurrent)
    {
        return ActiveSortieDestinations.Count < maxConcurrent;
    }

    public static void RegisterSortie(Vector3 destination)
    {
        ActiveSortieDestinations.Add(destination);
    }

    public static void UnregisterSortie(Vector3 destination)
    {
        for (int i = 0; i < ActiveSortieDestinations.Count; i++)
        {
            if (Vector3.SqrMagnitude(ActiveSortieDestinations[i] - destination) < 0.25f)
            {
                ActiveSortieDestinations.RemoveAt(i);
                return;
            }
        }

        if (ActiveSortieDestinations.Count > 0)
        {
            ActiveSortieDestinations.RemoveAt(ActiveSortieDestinations.Count - 1);
        }
    }

    public static bool IsPositionClear(Vector3 candidate, float minSeparation)
    {
        float minSepSq = minSeparation * minSeparation;
        for (int i = 0; i < ActiveSortieDestinations.Count; i++)
        {
            Vector3 diff = ActiveSortieDestinations[i] - candidate;
            diff.y = 0f;
            if (diff.sqrMagnitude < minSepSq)
            {
                return false;
            }
        }

        return true;
    }
}

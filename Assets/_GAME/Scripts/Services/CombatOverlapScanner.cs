using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class CombatOverlapScanner
{
    private readonly Collider[] buffer;

    public CombatOverlapScanner(int maxColliders = 32)
    {
        buffer = new Collider[Mathf.Max(1, maxColliders)];
    }

    public List<T> GetFilteredObjects<T>(Vector3 position, float radius, LayerMask layerMask, Func<T, bool> filter = null)
    {
        int count = Physics.OverlapSphereNonAlloc(position, radius, buffer, layerMask);
        var results = new List<T>(count);

        for (int i = 0; i < count; i++)
        {
            Collider collider = buffer[i];
            if (collider == null || !collider.TryGetComponent(out T component))
                continue;

            if (filter == null || filter(component))
                results.Add(component);
        }

        return results;
    }
}

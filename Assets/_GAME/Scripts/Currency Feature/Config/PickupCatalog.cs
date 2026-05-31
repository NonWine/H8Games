using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PickupCatalog", menuName = "Configs/Pickup Catalog")]
public sealed class PickupCatalog : ScriptableObject
{
    [Serializable]
    private sealed class Entry
    {
        [SerializeField] private string             pickupId;
        [SerializeField] private GameObject         prefab;
        [SerializeField] private PickupVisualConfig overrideVisuals;

        public string             PickupId        => pickupId;
        public GameObject         Prefab          => prefab;
        public PickupVisualConfig OverrideVisuals => overrideVisuals;
    }

    [SerializeField] private List<Entry> entries = new();

    public bool TryGet(string pickupId, out GameObject prefab, out PickupVisualConfig overrideVisuals)
    {
        for (var i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];

            if (entry.PickupId != pickupId)
                continue;

            prefab          = entry.Prefab;
            overrideVisuals = entry.OverrideVisuals;

            return prefab != null;
        }

        prefab          = null;
        overrideVisuals = null;

        return false;
    }

    public void ForEachEntry(Action<string, GameObject, PickupVisualConfig> callback)
    {
        for (var i = 0; i < entries.Count; i++)
            callback(entries[i].PickupId, entries[i].Prefab, entries[i].OverrideVisuals);
    }
}

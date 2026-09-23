using System.Collections.Generic;
using UnityEngine;

// Draws one ground marker per formation slot. The markers are pinned to the
// squad's home pose and never follow the marching root - they mark where the
// formation belongs, not where it currently is. The layout is written once per
// formation change; nothing here runs per frame.
public class SquadSlotMarkersView : MonoBehaviour
{
    [SerializeField] private Transform markersRoot;
    [SerializeField] private GameObject markerPrefab;
    [SerializeField] private float groundOffsetY = 0.02f;

    private readonly List<GameObject> markers = new();

    private Transform MarkersParent => markersRoot != null ? markersRoot : transform;

    public void AnchorTo(Vector3 worldPosition, Quaternion worldRotation)
    {
        MarkersParent.SetPositionAndRotation(worldPosition, worldRotation);
    }

    public void ApplyLayout(IReadOnlyList<FormationSlot> slots)
    {
        if (markerPrefab == null || slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject marker = GetOrCreateMarker(i);
            marker.transform.localPosition = slots[i].LocalOffset + Vector3.up * groundOffsetY;
            marker.SetActive(true);
        }

        for (int i = slots.Count; i < markers.Count; i++)
        {
            markers[i].SetActive(false);
        }
    }

    private GameObject GetOrCreateMarker(int index)
    {
        if (index < markers.Count)
        {
            return markers[index];
        }

        GameObject marker = Instantiate(markerPrefab, MarkersParent);
        marker.name = $"{markerPrefab.name}({index})";
        marker.transform.localRotation = Quaternion.identity;
        markers.Add(marker);

        return marker;
    }
}

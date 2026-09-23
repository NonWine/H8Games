using UnityEngine;

// Read-only answer to a single question: "where did this level's fight end?".
// Kept separate from ITerritoryView so the capture zone can place itself on that
// spot without pulling in unit tracking, meshes or any of the territory visuals.
public interface ITerritoryCaptureFocusProvider
{
    bool TryGetCaptureFocus(out Vector3 worldPosition);
}

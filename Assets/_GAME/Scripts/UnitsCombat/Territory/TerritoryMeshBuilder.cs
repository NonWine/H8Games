using System.Collections.Generic;
using UnityEngine;

public class TerritoryMeshBuilder
{
    private readonly List<Vector2> sourcePoints      = new();
    private readonly List<Vector2> hull2D            = new();
    private readonly List<Vector2> boundary2D        = new();
    private readonly List<Vector2> alignedBoundary2D = new();
    private readonly List<Vector3> vertices          = new();
    private readonly List<int>     triangles         = new();

    // ── public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Computes the target boundary polygon in local 2D space, resampled to
    /// exactly <paramref name="sampleCount"/> evenly-spaced points. The polygon
    /// is aligned to start from the eastmost vertex so successive calls produce
    /// consistent point correspondence for interpolation.
    /// </summary>
    public bool TryComputeBoundary(
        IReadOnlyList<Vector3> unitPositions,
        TerritoryConfig        config,
        Transform              origin,
        int                    sampleCount,
        List<Vector2>          outBoundary,
        out Vector2            outCentroid)
    {
        outCentroid = Vector2.zero;
        outBoundary.Clear();

        if (unitPositions == null || unitPositions.Count == 0)
            return false;

        sourcePoints.Clear();
        Vector2 centroid = Vector2.zero;

        for (int i = 0; i < unitPositions.Count; i++)
        {
            Vector3 local = origin.InverseTransformPoint(unitPositions[i]);
            Vector2 p     = new Vector2(local.x, local.z);
            sourcePoints.Add(p);
            centroid += p;
        }

        centroid   /= sourcePoints.Count;
        outCentroid = centroid;

        if (sourcePoints.Count < 3)
        {
            BuildCircleBoundary(outBoundary, centroid, config.MinRadius, sampleCount);
            return true;
        }

        hull2D.Clear();
        BuildConvexHull(sourcePoints, hull2D);

        if (hull2D.Count < 3)
        {
            BuildCircleBoundary(outBoundary, centroid, config.MinRadius, sampleCount);
            return true;
        }

        boundary2D.Clear();
        BuildExpandedBoundary(hull2D, config.Padding, config.CornerSegments, boundary2D);
        EnforceMinRadius(boundary2D, centroid, config.MinRadius);

        // Rotate the polygon to start from the eastmost vertex — ensures
        // point[i]_old corresponds to point[i]_new when interpolating.
        int eastIdx = FindEastmostIndex(boundary2D, centroid);
        alignedBoundary2D.Clear();
        for (int i = 0; i < boundary2D.Count; i++)
            alignedBoundary2D.Add(boundary2D[(eastIdx + i) % boundary2D.Count]);

        ResamplePolygon2D(alignedBoundary2D, outBoundary, sampleCount);
        return true;
    }

    /// <summary>
    /// Builds the fill mesh, border ring mesh, and optional border point list
    /// from a pre-computed (and optionally smoothed) local 2D boundary.
    /// </summary>
    public void BuildFromBoundary(
        Mesh            fillMesh,
        Mesh            borderMesh,
        List<Vector2>   boundary,
        Vector2         centroid,
        TerritoryConfig config,
        Transform       origin,
        List<Vector3>   outBorderPoints)
    {
        fillMesh?.Clear();
        borderMesh?.Clear();
        outBorderPoints?.Clear();

        if (boundary == null || boundary.Count == 0)
            return;

        float meshY   = config.GroundY + config.VerticalOffset;
        float borderY = meshY + config.BorderYOffset;

        if (fillMesh != null)
            BuildFillMesh(fillMesh, boundary, centroid, meshY);

        if (borderMesh != null)
            BuildBorderRing(borderMesh, boundary, centroid, borderY, config.BorderWidth);

        if (outBorderPoints != null)
        {
            for (int i = 0; i < boundary.Count; i++)
            {
                Vector3 local = new Vector3(boundary[i].x, meshY, boundary[i].y);
                outBorderPoints.Add(origin.TransformPoint(local));
            }
        }
    }

    // Flat XZ quad-ring — zero z-fighting because Y is explicitly controlled.
    private void BuildBorderRing(
        Mesh mesh, List<Vector2> boundary, Vector2 centroid, float y, float width)
    {
        int n = boundary.Count;
        vertices.Clear();
        triangles.Clear();

        for (int i = 0; i < n; i++)
        {
            Vector2 outDir = boundary[i] - centroid;
            float   len    = outDir.magnitude;
            if (len > 0.0001f) outDir /= len; else outDir = Vector2.right;

            Vector2 inner = boundary[i];
            Vector2 outer = boundary[i] + outDir * width;

            vertices.Add(new Vector3(inner.x, y, inner.y));
            vertices.Add(new Vector3(outer.x, y, outer.y));
        }

        for (int i = 0; i < n; i++)
        {
            int i0 = i * 2,           i1 = i * 2 + 1;
            int i2 = ((i+1) % n) * 2, i3 = ((i+1) % n) * 2 + 1;
            triangles.Add(i0); triangles.Add(i2); triangles.Add(i1);
            triangles.Add(i1); triangles.Add(i2); triangles.Add(i3);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // ── convex hull ───────────────────────────────────────────────────────────

    private void BuildConvexHull(List<Vector2> points, List<Vector2> result)
    {
        var sorted = new List<Vector2>(points);
        sorted.Sort((a, b) =>
        {
            int cmp = a.x.CompareTo(b.x);
            return cmp != 0 ? cmp : a.y.CompareTo(b.y);
        });

        int n = sorted.Count;

        var lower = new List<Vector2>(n);
        for (int i = 0; i < n; i++)
        {
            while (lower.Count >= 2 &&
                   Cross(lower[lower.Count - 2], lower[lower.Count - 1], sorted[i]) <= HullEpsilon)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(sorted[i]);
        }

        var upper = new List<Vector2>(n);
        for (int i = n - 1; i >= 0; i--)
        {
            while (upper.Count >= 2 &&
                   Cross(upper[upper.Count - 2], upper[upper.Count - 1], sorted[i]) <= HullEpsilon)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(sorted[i]);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);

        result.AddRange(lower);
        result.AddRange(upper);
    }

    private const float HullEpsilon = 0.01f;

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
        => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

    // ── expanded boundary ─────────────────────────────────────────────────────

    private void BuildExpandedBoundary(
        List<Vector2> hull, float padding, int cornerSegments, List<Vector2> result)
    {
        int n = hull.Count;
        cornerSegments = Mathf.Max(1, cornerSegments);

        for (int i = 0; i < n; i++)
        {
            Vector2 prev = hull[(i - 1 + n) % n];
            Vector2 curr = hull[i];
            Vector2 next = hull[(i + 1) % n];

            Vector2 edgeIn  = (curr - prev).normalized;
            Vector2 edgeOut = (next - curr).normalized;

            Vector2 normalIn  = new Vector2(edgeIn.y,  -edgeIn.x);
            Vector2 normalOut = new Vector2(edgeOut.y, -edgeOut.x);

            float angleStart = Mathf.Atan2(normalIn.y,  normalIn.x);
            float angleEnd   = Mathf.Atan2(normalOut.y, normalOut.x);

            float angleDiff = angleEnd - angleStart;
            if (angleDiff < 0f) angleDiff += 2f * Mathf.PI;
            if (angleDiff > 2f * Mathf.PI - 0.001f) angleDiff = 0f;

            for (int s = 0; s <= cornerSegments; s++)
            {
                float t     = (float)s / cornerSegments;
                float angle = angleStart + angleDiff * t;
                result.Add(curr + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * padding);
            }
        }
    }

    private static void EnforceMinRadius(List<Vector2> boundary, Vector2 centroid, float minRadius)
    {
        float minDist = float.MaxValue;
        for (int i = 0; i < boundary.Count; i++)
        {
            float d = Vector2.Distance(centroid, boundary[i]);
            if (d < minDist) minDist = d;
        }

        if (minDist < minRadius && minDist > 0.0001f)
        {
            float scale = minRadius / minDist;
            for (int i = 0; i < boundary.Count; i++)
                boundary[i] = centroid + (boundary[i] - centroid) * scale;
        }
    }

    // ── fill mesh ─────────────────────────────────────────────────────────────

    private void BuildFillMesh(Mesh mesh, List<Vector2> boundary, Vector2 centroid, float y)
    {
        int n = boundary.Count;
        vertices.Clear();
        triangles.Clear();

        vertices.Add(new Vector3(centroid.x, y, centroid.y));
        for (int i = 0; i < n; i++)
            vertices.Add(new Vector3(boundary[i].x, y, boundary[i].y));

        for (int i = 0; i < n; i++)
        {
            triangles.Add(0);
            triangles.Add((i + 1) % n + 1);
            triangles.Add(i + 1);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    // ── helpers ───────────────────────────────────────────────────────────────

    private static void BuildCircleBoundary(List<Vector2> result, Vector2 center, float radius, int count)
    {
        result.Clear();
        for (int i = 0; i < count; i++)
        {
            float angle = 2f * Mathf.PI * i / count;
            result.Add(center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius);
        }
    }

    // Returns index of the polygon vertex closest to the east direction (angle ≈ 0)
    // from the centroid. Used to align successive boundaries for interpolation.
    private static int FindEastmostIndex(List<Vector2> polygon, Vector2 centroid)
    {
        int   bestIdx      = 0;
        float minAbsAngle  = float.MaxValue;
        for (int i = 0; i < polygon.Count; i++)
        {
            float a = Mathf.Abs(Mathf.Atan2(polygon[i].y - centroid.y, polygon[i].x - centroid.x));
            if (a < minAbsAngle) { minAbsAngle = a; bestIdx = i; }
        }
        return bestIdx;
    }

    // Resample closed polygon to exactly `count` evenly-spaced points by arc length.
    private static void ResamplePolygon2D(List<Vector2> source, List<Vector2> result, int count)
    {
        result.Clear();
        if (source.Count == 0 || count <= 0) return;
        if (source.Count == 1) { for (int i = 0; i < count; i++) result.Add(source[0]); return; }

        float perimeter = 0f;
        for (int i = 0; i < source.Count; i++)
            perimeter += Vector2.Distance(source[i], source[(i + 1) % source.Count]);

        if (perimeter < 0.0001f) { for (int i = 0; i < count; i++) result.Add(source[0]); return; }

        float step  = perimeter / count;
        float accum = 0f;
        int   src   = 0;

        for (int i = 0; i < count; i++)
        {
            float target = i * step;
            while (src < source.Count - 1)
            {
                float len = Vector2.Distance(source[src], source[(src + 1) % source.Count]);
                if (accum + len >= target) break;
                accum += len;
                src++;
            }
            int   nxt = (src + 1) % source.Count;
            float seg = Vector2.Distance(source[src], source[nxt]);
            float t   = seg > 0.0001f ? (target - accum) / seg : 0f;
            result.Add(Vector2.Lerp(source[src], source[nxt], t));
        }
    }
}

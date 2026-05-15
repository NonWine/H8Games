using System.Collections.Generic;
using UnityEngine;

public static class TerritoryMeshBuilder
{
    private static readonly List<Vector2> sourcePoints = new();
    private static readonly List<Vector2> hull2D       = new();
    private static readonly List<Vector2> boundary2D   = new();
    private static readonly List<Vector3> vertices     = new();
    private static readonly List<int>     triangles    = new();

    public static bool TryBuild(
        Mesh                   mesh,
        Transform              origin,
        IReadOnlyList<Vector3> unitPositions,
        TerritoryConfig        config,
        List<Vector3>          outBorderPoints = null)
    {
        mesh.Clear();
        outBorderPoints?.Clear();

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

        centroid /= sourcePoints.Count;
        float meshY = config.GroundY + config.VerticalOffset;

        if (sourcePoints.Count < 3)
        {
            BuildCircleMesh(mesh, centroid, config.MinRadius, config.CircleSegments, meshY);
            FillBorderCircle(outBorderPoints, centroid, config.MinRadius, config.CircleSegments, meshY, origin);
            return true;
        }

        hull2D.Clear();
        BuildConvexHull(sourcePoints, hull2D);

        if (hull2D.Count < 3)
        {
            BuildCircleMesh(mesh, centroid, config.MinRadius, config.CircleSegments, meshY);
            FillBorderCircle(outBorderPoints, centroid, config.MinRadius, config.CircleSegments, meshY, origin);
            return true;
        }

        boundary2D.Clear();
        BuildExpandedBoundary(hull2D, config.Padding, config.CornerSegments, boundary2D);
        EnforceMinRadius(boundary2D, centroid, config.MinRadius);
        BuildFillMesh(mesh, boundary2D, centroid, meshY);

        if (outBorderPoints != null)
        {
            for (int i = 0; i < boundary2D.Count; i++)
            {
                Vector3 localPt = new Vector3(boundary2D[i].x, meshY, boundary2D[i].y);
                outBorderPoints.Add(origin.TransformPoint(localPt));
            }
        }

        return true;
    }

    private static void BuildConvexHull(List<Vector2> points, List<Vector2> result)
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
                   Cross(lower[lower.Count - 2], lower[lower.Count - 1], sorted[i]) <= 0f)
                lower.RemoveAt(lower.Count - 1);
            lower.Add(sorted[i]);
        }

        var upper = new List<Vector2>(n);
        for (int i = n - 1; i >= 0; i--)
        {
            while (upper.Count >= 2 &&
                   Cross(upper[upper.Count - 2], upper[upper.Count - 1], sorted[i]) <= 0f)
                upper.RemoveAt(upper.Count - 1);
            upper.Add(sorted[i]);
        }

        lower.RemoveAt(lower.Count - 1);
        upper.RemoveAt(upper.Count - 1);

        result.AddRange(lower);
        result.AddRange(upper);
    }

    private static float Cross(Vector2 o, Vector2 a, Vector2 b)
        => (a.x - o.x) * (b.y - o.y) - (a.y - o.y) * (b.x - o.x);

    private static void BuildExpandedBoundary(
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

    private static void BuildFillMesh(Mesh mesh, List<Vector2> boundary, Vector2 centroid, float y)
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

    private static void BuildCircleMesh(Mesh mesh, Vector2 center, float radius, int segments, float y)
    {
        vertices.Clear();
        triangles.Clear();

        vertices.Add(new Vector3(center.x, y, center.y));

        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            vertices.Add(new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                y,
                center.y + Mathf.Sin(angle) * radius));
        }

        for (int i = 0; i < segments; i++)
        {
            triangles.Add(0);
            triangles.Add((i + 1) % segments + 1);
            triangles.Add(i + 1);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    private static void FillBorderCircle(
        List<Vector3> outBorder, Vector2 center, float radius,
        int segments, float y, Transform origin)
    {
        if (outBorder == null) return;
        for (int i = 0; i < segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            Vector3 local = new Vector3(
                center.x + Mathf.Cos(angle) * radius,
                y,
                center.y + Mathf.Sin(angle) * radius);
            outBorder.Add(origin.TransformPoint(local));
        }
    }
}

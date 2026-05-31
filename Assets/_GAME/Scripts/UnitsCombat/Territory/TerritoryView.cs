using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerritoryView : MonoBehaviour, ITerritoryView
{
    [SerializeField] private MeshFilter           meshFilter;
    [SerializeField] private MeshRenderer         meshRenderer;
    [SerializeField] private TerritoryZoneAnimator zoneAnimator;

    private TerritoryMeshBuilder meshBuilder;
    private Mesh                 runtimeMesh;

    // Border ring — child GO created at runtime, flat XZ mesh, no z-fighting.
    private MeshFilter            borderMeshFilter;
    private MeshRenderer          borderMeshRenderer;
    private Mesh                  runtimeBorderMesh;
    private MaterialPropertyBlock borderMpb;
    private int                   borderColorId;
    private Color                 borderBaseColor;
    private float                 borderAlpha;
    private Tween                 borderFadeTween;

    private bool isVisible;

    // ── boundary smoothing ────────────────────────────────────────────────────
    // Mesh is built from `smoothedBoundary2D` which exponentially chases
    // `targetBoundary2D` every tick. Both always hold CircleSegments points,
    // so topology changes (3→2 enemies) produce a smooth morph, not a jump.

    private readonly List<Vector2> targetBoundary2D         = new();
    private readonly List<Vector2> smoothedBoundary2D       = new();
    private readonly List<Vector2> collapseSourceBoundary2D = new();
    private readonly List<Vector2> collapseWorkBoundary2D   = new();
    private Vector2 targetCentroid2D;
    private Vector2 smoothedCentroid2D;
    private bool    boundaryInitialized;

    // Border points for the particle system.
    private readonly List<Vector3> rawBorderPoints      = new();
    private readonly List<Vector3> resampledBorderPoints = new();

    private float collapseT;
    private Tween collapseTween;

    [Inject]
    public void Construct(TerritoryMeshBuilder builder, TerritoryConfig config)
    {
        meshBuilder = builder;
        zoneAnimator.Initialize(config);
        SetupBorderRenderer(config);
    }

    // ── ITerritoryView ────────────────────────────────────────────────────────

    public void Refresh(IReadOnlyList<Vector3> unitPositions, TerritoryConfig config, float dt)
    {
        if (unitPositions == null || unitPositions.Count == 0)
        {
            CollapseAndFade(config);
            return;
        }

        bool computed = meshBuilder.TryComputeBoundary(
            unitPositions, config, transform,
            config.CircleSegments, targetBoundary2D, out targetCentroid2D);

        if (!computed)
        {
            CollapseAndFade(config);
            return;
        }

        EnsureMeshes();

        // First call — init smoothed = target instantly (no lerp on spawn).
        if (!boundaryInitialized || smoothedBoundary2D.Count != targetBoundary2D.Count)
        {
            smoothedBoundary2D.Clear();
            smoothedBoundary2D.AddRange(targetBoundary2D);
            smoothedCentroid2D  = targetCentroid2D;
            boundaryInitialized = true;
        }

        // Exponentially smooth boundary toward target.
        float expandSpeed = config.ExpandDuration > 0f ? 4f / config.ExpandDuration : 16f;
        float shrinkSpeed = config.ShrinkDuration > 0f ? 4f / config.ShrinkDuration : 10f;

        float smoothedR = AverageRadius(smoothedBoundary2D, smoothedCentroid2D);
        float targetR   = AverageRadius(targetBoundary2D,   targetCentroid2D);
        float speed     = targetR >= smoothedR ? expandSpeed : shrinkSpeed;
        float lerp      = dt > 0f ? 1f - Mathf.Exp(-speed * dt) : 1f;

        bool changed = false;
        for (int i = 0; i < smoothedBoundary2D.Count; i++)
        {
            Vector2 next = Vector2.Lerp(smoothedBoundary2D[i], targetBoundary2D[i], lerp);
            if ((next - smoothedBoundary2D[i]).sqrMagnitude > 1e-8f)
            {
                smoothedBoundary2D[i] = next;
                changed = true;
            }
        }
        smoothedCentroid2D = Vector2.Lerp(smoothedCentroid2D, targetCentroid2D, lerp);

        // Skip rebuild if fully converged and already visible.
        if (!changed && isVisible)
            return;

        RebuildMeshes(smoothedBoundary2D, smoothedCentroid2D, config);

        if (!isVisible)
            FadeIn(config);
    }

    public void Clear()
    {
        collapseTween?.Kill();
        borderFadeTween?.Kill();

        zoneAnimator.Hide();
        meshRenderer.enabled = false;

        if (borderMeshRenderer != null)
            borderMeshRenderer.enabled = false;

        isVisible           = false;
        borderAlpha         = 0f;
        boundaryInitialized = false;

        smoothedBoundary2D.Clear();
        collapseSourceBoundary2D.Clear();
        collapseWorkBoundary2D.Clear();
        rawBorderPoints.Clear();
        resampledBorderPoints.Clear();
    }

    public void SetAnimating(bool active)   => zoneAnimator.SetAnimating(active);
    public void SetCombatAlert(bool active) => zoneAnimator.SetCombatAlert(active);

    // ── border setup ──────────────────────────────────────────────────────────

    private void SetupBorderRenderer(TerritoryConfig config)
    {
        // Child GO so we can have a second MeshRenderer alongside the fill.
        // Identity local transform → mesh vertices (parent local space) are correct.
        var go = new GameObject("TerritoryBorder");
        go.transform.SetParent(transform, false);

        borderMeshFilter   = go.AddComponent<MeshFilter>();
        borderMeshRenderer = go.AddComponent<MeshRenderer>();
        borderMpb          = new MaterialPropertyBlock();

        if (config.BorderMaterial != null)
        {
            borderMeshRenderer.sharedMaterial = config.BorderMaterial;

            int urp    = Shader.PropertyToID("_BaseColor");
            int legacy = Shader.PropertyToID("_Color");
            borderColorId   = config.BorderMaterial.HasProperty(urp) ? urp : legacy;
            borderBaseColor = config.BorderMaterial.HasProperty(borderColorId)
                ? config.BorderMaterial.GetColor(borderColorId)
                : Color.white;
        }
        else
        {
            borderColorId   = Shader.PropertyToID("_BaseColor");
            borderBaseColor = Color.white;
        }

        borderBaseColor.a          = 0f;
        borderMeshRenderer.enabled = false;
    }

    // ── fade in / out ─────────────────────────────────────────────────────────

    private void FadeIn(TerritoryConfig config)
    {
        isVisible            = true;
        meshRenderer.enabled = true;

        if (borderMeshRenderer != null)
            borderMeshRenderer.enabled = true;

        collapseTween?.Kill();
        borderFadeTween?.Kill();

        zoneAnimator.FadeIn(config.FillAlpha, config.FadeInDuration);

        borderFadeTween = DOTween
            .To(() => borderAlpha, a => { borderAlpha = a; ApplyBorderAlpha(); },
                1f, config.FadeInDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void CollapseAndFade(TerritoryConfig config)
    {
        if (!isVisible)
            return;

        if (!boundaryInitialized || smoothedBoundary2D.Count == 0)
            return;

        collapseTween?.Kill();
        borderFadeTween?.Kill();

        collapseSourceBoundary2D.Clear();
        collapseSourceBoundary2D.AddRange(smoothedBoundary2D);
        collapseWorkBoundary2D.Clear();
        collapseWorkBoundary2D.AddRange(smoothedBoundary2D);
        collapseT = 0f;

        collapseTween = DOTween
            .To(() => collapseT, t =>
                {
                    collapseT = t;
                    for (int i = 0; i < collapseWorkBoundary2D.Count; i++)
                        collapseWorkBoundary2D[i] = Vector2.Lerp(
                            collapseSourceBoundary2D[i], smoothedCentroid2D, t);

                    RebuildMeshes(collapseWorkBoundary2D, smoothedCentroid2D, config);
                },
                1f, config.FadeOutDuration)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject);

        borderFadeTween = DOTween
            .To(() => borderAlpha, a => { borderAlpha = a; ApplyBorderAlpha(); },
                0f, config.FadeOutDuration)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject);

        zoneAnimator.FadeOut(config.FadeOutDuration, () =>
        {
            meshRenderer.enabled = false;

            if (borderMeshRenderer != null)
                borderMeshRenderer.enabled = false;

            isVisible           = false;
            boundaryInitialized = false;
        });
    }

    // ── mesh helpers ──────────────────────────────────────────────────────────

    private void RebuildMeshes(List<Vector2> boundary, Vector2 centroid, TerritoryConfig config)
    {
        meshBuilder.BuildFromBoundary(
            runtimeMesh, runtimeBorderMesh,
            boundary, centroid,
            config, transform, rawBorderPoints);

        ResampleBorderPoints(rawBorderPoints, resampledBorderPoints, config.BorderResampleCount);
        zoneAnimator.UpdateBorderPoints(resampledBorderPoints);
    }

    private void ApplyBorderAlpha()
    {
        if (borderMeshRenderer == null)
            return;

        Color c = borderBaseColor;
        c.a = borderAlpha;
        borderMpb.SetColor(borderColorId, c);
        borderMeshRenderer.SetPropertyBlock(borderMpb);
    }

    private void EnsureMeshes()
    {
        if (runtimeMesh == null)
        {
            runtimeMesh = new Mesh { name = "TerritoryFillMesh" };
            runtimeMesh.MarkDynamic();
            meshFilter.mesh = runtimeMesh;
        }

        if (runtimeBorderMesh == null && borderMeshFilter != null)
        {
            runtimeBorderMesh = new Mesh { name = "TerritoryBorderMesh" };
            runtimeBorderMesh.MarkDynamic();
            borderMeshFilter.mesh = runtimeBorderMesh;
        }
    }

    private static float AverageRadius(List<Vector2> boundary, Vector2 centroid)
    {
        if (boundary.Count == 0) return 0f;
        float sum = 0f;
        for (int i = 0; i < boundary.Count; i++)
            sum += (boundary[i] - centroid).magnitude;
        return sum / boundary.Count;
    }

    private static void ResampleBorderPoints(List<Vector3> source, List<Vector3> result, int count)
    {
        result.Clear();
        if (source.Count == 0 || count <= 0) return;
        if (source.Count == 1) { for (int i = 0; i < count; i++) result.Add(source[0]); return; }

        float perimeter = 0f;
        for (int i = 0; i < source.Count; i++)
            perimeter += Vector3.Distance(source[i], source[(i + 1) % source.Count]);

        if (perimeter < 0.0001f) { for (int i = 0; i < count; i++) result.Add(source[0]); return; }

        float step  = perimeter / count;
        float accum = 0f;
        int   src   = 0;

        for (int i = 0; i < count; i++)
        {
            float target = i * step;
            while (src < source.Count - 1)
            {
                float len = Vector3.Distance(source[src], source[(src + 1) % source.Count]);
                if (accum + len >= target) break;
                accum += len;
                src++;
            }
            int   nxt = (src + 1) % source.Count;
            float seg = Vector3.Distance(source[src], source[nxt]);
            float t   = seg > 0.0001f ? (target - accum) / seg : 0f;
            result.Add(Vector3.Lerp(source[src], source[nxt], t));
        }
    }

    // ── lifecycle ─────────────────────────────────────────────────────────────

    private void OnDestroy()
    {
        collapseTween?.Kill();
        borderFadeTween?.Kill();

        if (runtimeMesh != null)       Destroy(runtimeMesh);
        if (runtimeBorderMesh != null) Destroy(runtimeBorderMesh);
    }

    private void Reset()
    {
        meshFilter   = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        zoneAnimator = GetComponent<TerritoryZoneAnimator>();
    }
}

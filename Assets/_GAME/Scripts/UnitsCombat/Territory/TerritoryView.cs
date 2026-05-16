using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TerritoryView : MonoBehaviour, ITerritoryView
{
    [SerializeField] private MeshFilter meshFilter;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private LineRenderer borderLine;
    [SerializeField] private TerritoryZoneAnimator zoneAnimator;

    private TerritoryMeshBuilder meshBuilder;

    private Mesh          runtimeMesh;
    private List<Vector3> borderPoints = new();
    private Vector3[]     borderArray  = Array.Empty<Vector3>();

    private Tween   borderTween;
    private float   borderAlpha;
    private bool    isVisible;

    private readonly List<Vector3> collapseSourcePositions = new();
    private readonly List<Vector3> collapseWorkPositions   = new();
    private Vector3                collapseCentroid;
    private float                  collapseT;
    private Tween                  collapseTween;

    [Inject]
    public void Construct(TerritoryMeshBuilder builder, TerritoryConfig config)
    {
        meshBuilder = builder;
        zoneAnimator.Initialize(config);
    }

    public void Refresh(IReadOnlyList<Vector3> unitPositions, TerritoryConfig config)
    {
        if (unitPositions == null || unitPositions.Count == 0)
        {
            CollapseAndFade(config);
            return;
        }

        StoreCollapseSnapshot(unitPositions);

        EnsureMesh();

        bool built = meshBuilder.TryBuild(runtimeMesh, transform, unitPositions, config, borderPoints);

        if (!built)
        {
            CollapseAndFade(config);
            return;
        }

        UpdateBorderPositions(config);

        if (!isVisible)
            FadeIn(config);
    }

    public void Clear()
    {
        KillTweens();
        zoneAnimator.Hide();
        ApplyBorderAlpha(0f);
        meshRenderer.enabled = false;
        if (borderLine != null)
            borderLine.enabled = false;

        isVisible = false;
        collapseSourcePositions.Clear();
        collapseWorkPositions.Clear();
    }

    public void SetAnimating(bool active)
    {
        zoneAnimator.SetAnimating(active);
    }

    public void SetCombatAlert(bool active)
    {
        zoneAnimator.SetCombatAlert(active);
    }

    private void FadeIn(TerritoryConfig config)
    {
        isVisible            = true;
        meshRenderer.enabled = true;
        borderLine.enabled   = true;

        KillTweens();

        zoneAnimator.FadeIn(config.FillAlpha, config.FadeInDuration);

        borderTween = DOTween
            .To(() => borderAlpha, a => { borderAlpha = a; ApplyBorderAlpha(a); }, TargetBorderAlpha(), config.FadeInDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    private void CollapseAndFade(TerritoryConfig config)
    {
        if (!isVisible)
            return;

        if (collapseSourcePositions.Count == 0)
            return;

        KillTweens();

        collapseT = 0f;
        collapseWorkPositions.Clear();
        for (int i = 0; i < collapseSourcePositions.Count; i++)
            collapseWorkPositions.Add(collapseSourcePositions[i]);

        collapseTween = DOTween
            .To(() => collapseT, t =>
                {
                    collapseT = t;
                    for (int i = 0; i < collapseWorkPositions.Count; i++)
                        collapseWorkPositions[i] = Vector3.Lerp(collapseSourcePositions[i], collapseCentroid, t);
                    meshBuilder.TryBuild(runtimeMesh, transform, collapseWorkPositions, config, null);
                },
                1f, config.FadeOutDuration)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject);

        zoneAnimator.FadeOut(config.FadeOutDuration, () =>
        {
            meshRenderer.enabled = false;
            isVisible            = false;
        });

        borderTween = DOTween
            .To(() => borderAlpha, a => { borderAlpha = a; ApplyBorderAlpha(a); }, 0f, config.FadeOutDuration)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                if (borderLine != null)
                    borderLine.enabled = false;
            });
    }

    private void StoreCollapseSnapshot(IReadOnlyList<Vector3> positions)
    {
        collapseSourcePositions.Clear();
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < positions.Count; i++)
        {
            collapseSourcePositions.Add(positions[i]);
            sum += positions[i];
        }
        collapseCentroid = sum / positions.Count;
    }

    private void ApplyBorderAlpha(float a)
    {
        Color s = borderLine.startColor; s.a = a; borderLine.startColor = s;
        Color e = borderLine.endColor;   e.a = a; borderLine.endColor   = e;
    }

    private float TargetBorderAlpha()
        => borderLine != null ? borderLine.startColor.a : 1f;

    private void UpdateBorderPositions(TerritoryConfig config)
    {
        if (borderPoints.Count == 0)
            return;

        if (borderArray.Length != borderPoints.Count)
            borderArray = new Vector3[borderPoints.Count];

        for (int i = 0; i < borderPoints.Count; i++)
            borderArray[i] = borderPoints[i];

        borderLine.useWorldSpace = true;
        borderLine.loop          = true;
        borderLine.startWidth    = config.BorderWidth;
        borderLine.endWidth      = config.BorderWidth;
        borderLine.positionCount = borderArray.Length;
        borderLine.SetPositions(borderArray);

        if (config.BorderMaterial != null)
            borderLine.sharedMaterial = config.BorderMaterial;

        zoneAnimator.UpdateBorderPoints(borderPoints);
    }

    private void EnsureMesh()
    {
        if (runtimeMesh != null)
            return;

        runtimeMesh = new Mesh { name = "TerritoryMesh" };
        runtimeMesh.MarkDynamic();
        meshFilter.mesh = runtimeMesh;
    }

    private void KillTweens()
    {
        borderTween?.Kill();
        collapseTween?.Kill();
    }

    private void OnDestroy()
    {
        KillTweens();
        if (runtimeMesh != null)
            Destroy(runtimeMesh);
    }

    private void Reset()
    {
        meshFilter   = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        borderLine   = GetComponent<LineRenderer>();
        zoneAnimator = GetComponent<TerritoryZoneAnimator>();
    }
}

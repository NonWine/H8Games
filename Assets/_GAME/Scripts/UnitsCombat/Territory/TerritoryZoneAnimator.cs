using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(ParticleSystem))]
public class TerritoryZoneAnimator : MonoBehaviour
{
    private TerritoryConfig config;
    private MeshRenderer meshRenderer;
    private ParticleSystem borderParticles;
    private MaterialPropertyBlock mpb;
    private int colorPropertyId;

    private float fillAlpha;
    private Tween fillTween;
    private bool isActive;
    private bool isAnimating;
    private bool combatAlert;

    private readonly List<Vector3> borderPoints = new();
    private Vector3 borderCentroid;
    private float emitAccum;

    public void Initialize(TerritoryConfig config)
    {
        this.config = config;
    }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        borderParticles = GetComponent<ParticleSystem>();
        mpb = new MaterialPropertyBlock();

        int urp = Shader.PropertyToID("_BaseColor");
        int legacy = Shader.PropertyToID("_Color");
        colorPropertyId = meshRenderer.sharedMaterial != null && meshRenderer.sharedMaterial.HasProperty(urp)
            ? urp
            : legacy;

        SetupParticleSystem();
    }

    private void Update()
    {
        if (!isActive)
            return;

        if (isAnimating)
        {
            ApplyPulseColor();
            TickBorderParticles();
        }
        else
        {
            ApplyStaticColor();
        }
    }

    public void FadeIn(float targetAlpha, float duration)
    {
        isActive = true;
        KillFillTween();

        fillTween = DOTween
            .To(() => fillAlpha, a => fillAlpha = a, targetAlpha, duration)
            .SetEase(Ease.OutQuad)
            .SetLink(gameObject);
    }

    public void FadeOut(float duration, Action onComplete = null)
    {
        KillFillTween();

        fillTween = DOTween
            .To(() => fillAlpha, a => fillAlpha = a, 0f, duration)
            .SetEase(Ease.InQuad)
            .SetLink(gameObject)
            .OnComplete(() =>
            {
                isActive = false;
                onComplete?.Invoke();
            });
    }

    public void Hide()
    {
        KillFillTween();
        fillAlpha   = 0f;
        isActive    = false;
        isAnimating = false;
        combatAlert = false;

        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorPropertyId, Color.clear);
        meshRenderer.SetPropertyBlock(mpb);
    }

    public void SetAnimating(bool active)
    {
        isAnimating = active;

        if (!active)
        {
            combatAlert = false;
            emitAccum   = 0f;
        }
    }

    public void UpdateBorderPoints(IReadOnlyList<Vector3> points)
    {
        borderPoints.Clear();

        Vector3 sum = Vector3.zero;
        for (int i = 0; i < points.Count; i++)
        {
            borderPoints.Add(points[i]);
            sum += points[i];
        }

        borderCentroid = points.Count > 0 ? sum / points.Count : Vector3.zero;
    }

    public void SetCombatAlert(bool active)
    {
        if (combatAlert == active)
            return;

        combatAlert = active;

        if (active)
            EmitCombatBurst();
    }

    private void ApplyStaticColor()
    {
        Color c = config.StaticFillColor;
        c.a = fillAlpha;
        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorPropertyId, c);
        meshRenderer.SetPropertyBlock(mpb);
    }

    private void ApplyPulseColor()
    {
        float freq = combatAlert ? config.CombatPulseFrequency : config.PulseFrequency;
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * freq * Mathf.PI * 2f);

        Color pulseColor = Color.Lerp(config.PulseColorA, config.PulseColorB, t);

        if (combatAlert)
            pulseColor *= config.CombatBrightnessMult;

        pulseColor.a = fillAlpha;

        meshRenderer.GetPropertyBlock(mpb);
        mpb.SetColor(colorPropertyId, pulseColor);
        meshRenderer.SetPropertyBlock(mpb);
    }

    private void TickBorderParticles()
    {
        if (borderPoints.Count == 0)
            return;

        emitAccum += Time.deltaTime * config.BorderParticlesPerSecond;
        int toEmit = Mathf.FloorToInt(emitAccum);

        if (toEmit <= 0)
            return;

        emitAccum -= toEmit;
        EmitBorderParticles(toEmit);
    }

    private void EmitBorderParticles(int count)
    {
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, borderPoints.Count);
            Vector3 pos = borderPoints[idx];
            pos.x += Random.Range(-0.15f, 0.15f);
            pos.z += Random.Range(-0.15f, 0.15f);

            emitParams.position = pos;
            emitParams.velocity = Vector3.up * config.BorderParticleSpeed;
            emitParams.startLifetime = config.BorderParticleLifetime;
            emitParams.startSize = config.BorderParticleSize;
            borderParticles.Emit(emitParams, 1);
        }
    }

    private void EmitCombatBurst()
    {
        if (borderPoints.Count == 0)
            return;

        int perPoint = Mathf.Max(1, config.CombatBurstCount / borderPoints.Count);
        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams();

        for (int i = 0; i < borderPoints.Count; i++)
        {
            Vector3 outDir = (borderPoints[i] - borderCentroid).normalized;

            for (int j = 0; j < perPoint; j++)
            {
                emitParams.position = borderPoints[i];
                emitParams.velocity = outDir * config.BorderParticleSpeed * 2f
                    + Vector3.up * config.BorderParticleSpeed;
                emitParams.startLifetime = config.BorderParticleLifetime * 1.5f;
                emitParams.startSize = config.BorderParticleSize * 1.5f;
                borderParticles.Emit(emitParams, 1);
            }
        }
    }

    private void SetupParticleSystem()
    {
        ParticleSystem.MainModule main = borderParticles.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = false;
        main.playOnAwake = false;
        main.gravityModifier = 0f;
        main.startSpeed = 0f;
        main.startLifetime = 1f;
        main.startSize = 0.1f;
        main.maxParticles = 500;

        ParticleSystem.EmissionModule emission = borderParticles.emission;
        emission.enabled = false;

        ParticleSystem.ShapeModule shape = borderParticles.shape;
        shape.enabled = false;

        ParticleSystem.ColorOverLifetimeModule colorOverLifetime = borderParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.13f, 0f), 0f),
                new GradientColorKey(new Color(1f, 0.53f, 0f), 0.4f),
                new GradientColorKey(new Color(1f, 0.53f, 0f), 1f)
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.3f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

        ParticleSystem.VelocityOverLifetimeModule velocityOverLifetime = borderParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(0f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(1f);
        velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(0f);
    }

    private void KillFillTween()
    {
        fillTween?.Kill();
    }

    private void OnDestroy()
    {
        KillFillTween();
    }
}

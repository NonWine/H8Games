using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class BaseCombatUnitView : MonoBehaviour, IAgentView
{
    [field: SerializeField] public SkinnedMeshRenderer[] renderers { get; private set; }
    [SerializeField] private float timeToSetColor = 0.2f;
    [SerializeField, Min(0f)] private float hitFlashMinInterval;
    [SerializeField] private bool useUnscaledHitFlashTime;
    [SerializeField] private Color emissionHitColor = Color.white;
    [SerializeField, Range(0f, 100f)] private float hitGlow = 5f;
    [SerializeField, Range(0f, 1f)] private float hitBlend = 0.5f;

    [field: SerializeField] public SimpleProjectileView ProjectilePrefab { get; private set; }
    [field: SerializeField] public Transform AttackPoint { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public UnitAttackAnimationEventRelay AttackAnimationEvents { get; private set; }
    [field: SerializeField] public NavMeshAgent  NavMeshAgent { get; protected set; }
    public Transform Transform => transform;

    private Material[] hitFlashMaterials = Array.Empty<Material>();
    private float lastHitFlashTime = float.NegativeInfinity;

    private void Awake()
    {
        CacheHitFlashMaterials();
    }

    private void OnDestroy()
    {
        foreach (var material in hitFlashMaterials)
        {
            DOTween.Kill(material);
        }
    }

    public void PlayHitFeedback() => SetEmissionHitFlash();

    public void SetEmissionHitFlash()
    {
        float now = useUnscaledHitFlashTime ? Time.unscaledTime : Time.time;
        if (now - lastHitFlashTime < hitFlashMinInterval)
        {
            return;
        }

        lastHitFlashTime = now;

        if (hitFlashMaterials.Length != renderers.Length)
            CacheHitFlashMaterials();

        for (int i = 0; i < hitFlashMaterials.Length; i++)
        {
            Material material = hitFlashMaterials[i];
            DOTween.Kill(material);

            material.SetColor("_HitColor", emissionHitColor);
            material.SetFloat("_HitGlow", hitGlow);
            material.SetFloat("_HitBlend", hitBlend);

            DOTween.To(
                () => material.GetFloat("_HitBlend"),
                x => material.SetFloat("_HitBlend", x),
                0f,
                timeToSetColor
            ).SetTarget(material).SetUpdate(useUnscaledHitFlashTime);
        }
    }

    public void PlayTargetSwitchFeedback(Color color, float duration)
    {
        if (hitFlashMaterials.Length != renderers.Length)
            CacheHitFlashMaterials();

        for (int i = 0; i < hitFlashMaterials.Length; i++)
        {
            Material material = hitFlashMaterials[i];
            DOTween.Kill(material);

            material.SetColor("_HitColor", color);
            material.SetFloat("_HitGlow", hitGlow);
            material.SetFloat("_HitBlend", hitBlend);

            DOTween.To(
                () => material.GetFloat("_HitBlend"),
                x => material.SetFloat("_HitBlend", x),
                0f,
                duration
            ).SetTarget(material).SetUpdate(true);
        }
    }

    private void CacheHitFlashMaterials()
    {
        hitFlashMaterials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            hitFlashMaterials[i] = renderers[i].material;
        }
    }
}

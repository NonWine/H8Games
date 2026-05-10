using System;
using DG.Tweening;
using UnityEngine;

public class BaseCombatUnitView : MonoBehaviour, IAgentView
{
    [field: SerializeField] public SkinnedMeshRenderer[] renderers { get; private set; }
    [SerializeField] private float timeToSetColor = 0.2f;
    [SerializeField, ColorUsage(true, true)] protected Color emissionHitColor;
    [field: SerializeField] public SimpleProjectileView ProjectilePrefab { get; private set; }
    [field: SerializeField] public Transform AttackPoint { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }
    [field: SerializeField] public UnitAttackAnimationEventRelay AttackAnimationEvents { get; private set; }

    public Transform Transform => transform;
    public bool IsActive => gameObject.activeInHierarchy;

    private Material[] hitFlashMaterials = Array.Empty<Material>();
    private Color[] baseEmissionColors = Array.Empty<Color>();

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
        if (hitFlashMaterials.Length != renderers.Length)
        {
            CacheHitFlashMaterials();
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Material material = hitFlashMaterials[i];
            Color baseEmissionColor = baseEmissionColors[i];
            DOTween.Kill(material);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", baseEmissionColor);

            DOTween.Sequence()
                .SetTarget(material)
                .Append(material.DOColor(emissionHitColor, "_EmissionColor", timeToSetColor))
                .Append(material.DOColor(baseEmissionColor, "_EmissionColor", 0.10f));
        }
    }

    private void CacheHitFlashMaterials()
    {
        hitFlashMaterials = new Material[renderers.Length];
        baseEmissionColors = new Color[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            SkinnedMeshRenderer renderer = renderers[i];
            Material material = renderer.material;
            hitFlashMaterials[i] = material;

            material.EnableKeyword("_EMISSION");
            baseEmissionColors[i] = material.HasProperty("_EmissionColor")
                ? material.GetColor("_EmissionColor")
                : Color.black;
        }
    }
}

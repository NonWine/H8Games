using System;
using DG.Tweening;
using UnityEngine;

public class BaseCombatUnitView : MonoBehaviour
{
    [field: SerializeField] public SkinnedMeshRenderer[] renderers { get; private set; }
    [SerializeField] private float timeToSetColor = 0.2f;
    [SerializeField, ColorUsage(true, true)] protected Color emissionHitColor;

    public void SetEmissionHitFlash()
    {
        foreach (var renderer in renderers)
        {
            if (renderer == null)
            {
                continue;
            }

            var material = renderer.material;
            material.EnableKeyword("_EMISSION");

            var originalColor = material.GetColor("_EmissionColor");

            DOTween.Sequence()
                .Append(material.DOColor(emissionHitColor, "_EmissionColor", timeToSetColor))
                .Append(material.DOColor(originalColor, "_EmissionColor", 0.10f));
        }
    }


}
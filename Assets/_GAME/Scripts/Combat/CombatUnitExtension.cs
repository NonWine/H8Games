using DG.Tweening;
using UnityEngine;

public static class CombatUnitExtension
{
    public static void SetEmissionColor(Color32 color, SkinnedMeshRenderer[] skinnedMeshRenderer, float time)
    {
        foreach (var meshRenderer in skinnedMeshRenderer)
        {
            meshRenderer.material.DOColor(color,"_EmissionColor", time);
        }
    }
}
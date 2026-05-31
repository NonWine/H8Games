using DG.Tweening;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Zenject;

[RequireComponent(typeof(Volume))]
public class TerritoryDangerCameraFX : MonoBehaviour
{
    private TerritoryConfig config;
    private Volume volume;
    private Vignette vignette;
    private ChromaticAberration chromaticAberration;
    private CinemachineImpulseSource impulseSource;
    private Tween weightTween;
    private float microShakeTimer;

    [Inject]
    public void Construct(TerritoryConfig config)
    {
        this.config = config;
    }

    private void Awake()
    {
        volume = GetComponent<Volume>();
        volume.isGlobal = true;
        volume.weight = 0f;

        VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
        volume.profile = profile;

        vignette = profile.Add<Vignette>(true);
        vignette.active = true;
        vignette.color.Override(new Color(0.8f, 0f, 0f));
        vignette.intensity.Override(config.DangerVignetteIntensity);
        vignette.smoothness.Override(0.5f);

        chromaticAberration = profile.Add<ChromaticAberration>(true);
        chromaticAberration.active = true;
        chromaticAberration.intensity.Override(config.DangerChromaticAberration);

        impulseSource = GetComponent<CinemachineImpulseSource>();
    }

    public void SetDangerState(bool active)
    {
        weightTween?.Kill();

        float target = active ? 1f : 0f;
        weightTween = DOTween
            .To(() => volume.weight, w => volume.weight = w, target, config.DangerFXDuration)
            .SetLink(gameObject);
    }

    private void Update()
    {
        if (volume.weight < 0.01f || impulseSource == null)
            return;

        microShakeTimer -= Time.deltaTime;
        if (microShakeTimer > 0f)
            return;

        microShakeTimer = 1f / config.CameraShakeFrequency;

        Vector3 impulseDir = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            0f
        ).normalized * config.CameraShakeAmplitude;

        impulseSource.GenerateImpulseWithVelocity(impulseDir);
    }

    private void OnDestroy()
    {
        weightTween?.Kill();

        if (volume != null && volume.profile != null)
            Destroy(volume.profile);
    }
}

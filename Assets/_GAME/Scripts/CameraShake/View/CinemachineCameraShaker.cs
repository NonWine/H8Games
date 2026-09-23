using Unity.Cinemachine;
using UnityEngine;
using Zenject;

// The only Cinemachine-aware piece of the shake feature: a camera extension that
// adds the runtime's offset onto the finished camera state. Sitting on the
// CinemachineCamera means no scene reparenting and no shared impulse channel to
// tune, so hero damage shake stays independent of the impulse source that
// TerritoryDangerCameraFX already drives.
[AddComponentMenu("Cinemachine/Procedural/Extensions/Cinemachine Camera Shaker")]
[DisallowMultipleComponent]
public class CinemachineCameraShaker : CinemachineExtension, ICameraShakeService
{
    private const float MinimumDirectionSqrMagnitude = 0.0001f;

    private readonly CameraShakeRuntime runtime = new CameraShakeRuntime();

    private CameraShakeConfig config;
    private Vector3 localPositionOffset;
    private Quaternion localRotationOffset = Quaternion.identity;
    private int lastAdvancedFrame = -1;

    [Inject]
    public void Construct(CameraShakeConfig config)
    {
        this.config = config;
    }

    public void Shake(CameraShakeSettings settings, float scale, Vector3 worldDirection)
    {
        if (!config.Enabled)
        {
            return;
        }

        runtime.Add(settings, scale, ToCameraLocal(worldDirection), config.MaxConcurrentShakes);
    }

    public void StopAll()
    {
        runtime.Clear();
        localPositionOffset = Vector3.zero;
        localRotationOffset = Quaternion.identity;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        // Cinemachine also runs the pipeline outside play mode to drive the scene
        // view preview, where Zenject has never injected anything, so the config
        // really is absent here rather than merely unassigned.
        if (stage != CinemachineCore.Stage.Finalize || config == null)
        {
            return;
        }

        AdvanceOncePerFrame();

        float master = Mathf.Max(0f, config.MasterAmplitude);

        // PositionCorrection is world space while the runtime works camera-local,
        // so the offset is rotated by the state's own orientation rather than by
        // this transform, which Cinemachine has not written yet at this stage.
        state.PositionCorrection += state.RawOrientation * (localPositionOffset * master);
        state.OrientationCorrection *= Quaternion.SlerpUnclamped(Quaternion.identity, localRotationOffset, master);
    }

    // Cinemachine can run the pipeline more than once per frame (blends, a second
    // brain, editor previews). Advancing the timers on every call would decay the
    // same shake two or three times as fast, so the offset is computed once per
    // frame and reused for the remaining calls.
    private void AdvanceOncePerFrame()
    {
        if (lastAdvancedFrame == Time.frameCount)
        {
            return;
        }

        lastAdvancedFrame = Time.frameCount;

        float delta = config.UseUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        runtime.Advance(delta, out localPositionOffset, out localRotationOffset);
    }

    private Vector3 ToCameraLocal(Vector3 worldDirection)
    {
        if (worldDirection.sqrMagnitude < MinimumDirectionSqrMagnitude)
        {
            return Vector3.zero;
        }

        return Quaternion.Inverse(transform.rotation) * worldDirection.normalized;
    }
}

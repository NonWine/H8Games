using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Zenject;

public class BarracksUpgradeRevealView : MonoBehaviour
{
    [SerializeField] private BarracksRevealConfig config;
    [SerializeField] private Transform building;
    [SerializeField] private Transform[] crates;
    [SerializeField] private Transform[] barrels;
    [SerializeField] private Transform[] equipment;
    [SerializeField] private ParticleSystem dust;
    [SerializeField] private ParticleSystem[] upgradeStartEffects;

    private readonly struct LocalPose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;

        public LocalPose(Transform transform)
        {
            Position = transform.localPosition;
            Rotation = transform.localRotation;
            Scale = transform.localScale;
        }

        public void ApplyTo(Transform transform)
        {
            transform.localPosition = Position;
            transform.localRotation = Rotation;
            transform.localScale = Scale;
        }
    }

    private readonly Dictionary<Transform, LocalPose> authoredPoses = new Dictionary<Transform, LocalPose>();

    private IAudioService audioService;
    private ICameraShakeService cameraShake;
    private CameraShakeConfig shakeConfig;
    private System.Random random;
    private Sequence sequence;
    private GameObject previousModel;
    private GameObject nextModel;
    private Action pendingCompletion;

    [Inject]
    public void Construct(IAudioService audioService, ICameraShakeService cameraShake, CameraShakeConfig shakeConfig)
    {
        this.audioService = audioService;
        this.cameraShake = cameraShake;
        this.shakeConfig = shakeConfig;
    }

    private void Awake()
    {
        CachePose(building);
        CachePoses(crates);
        CachePoses(barrels);
        CachePoses(equipment);
    }

    private void OnDisable()
    {
        Finish();
    }

    public void Play(GameObject previous, GameObject next, Action onCompleted = null)
    {
        Finish();

        pendingCompletion = onCompleted;
        previousModel = previous;
        nextModel = next;
        CachePose(previous.transform);
        random = new System.Random(config.TiltSeed);

        float launchTime = config.AnticipationDuration;
        float apexTime = launchTime + config.BuildingJumpUpDuration;
        float fallTime = apexTime + config.BuildingApexDuration;
        float landTime = fallTime + config.BuildingFallDuration;

        sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Insert(0f, CreateAnticipation(previous.transform));
        sequence.InsertCallback(launchTime, SwitchModels);

        InsertBuildingLaunch(launchTime, apexTime);
        InsertBuildingApex(apexTime, fallTime);
        InsertBuildingFall(fallTime, landTime);
        InsertBuildingLand(landTime);

        InsertProps(crates, config.Crates, apexTime, landTime);
        InsertProps(barrels, config.Barrels, apexTime, landTime);
        InsertProps(equipment, config.Equipment, apexTime, landTime);

        sequence.OnComplete(OnSequenceCompleted);

        PlayUpgradeStartEffects();
        PlaySfx(config.AnticipationSfx, 1f);
    }

    private Tween CreateAnticipation(Transform model)
    {
        LocalPose pose = authoredPoses[model];

        return DOVirtual.Float(0f, 1f, config.AnticipationDuration, t =>
        {
            float squash = DOVirtual.EasedValue(0f, 1f, t, Ease.OutQuad);
            model.localScale = Vector3.Scale(pose.Scale, Vector3.Lerp(Vector3.one, config.AnticipationSquash, squash));

            float phase = t * config.TrembleVibrato * Mathf.PI;
            Vector3 tremble = new Vector3(Mathf.Sin(phase), 0f, Mathf.Cos(phase * 1.3f)) * config.TrembleStrength;
            model.localPosition = pose.Position + tremble;
        });
    }

    private void SwitchModels()
    {
        RestorePose(previousModel.transform);
        previousModel.SetActive(false);
        nextModel.SetActive(true);

        HideProps(crates);
        HideProps(barrels);
        HideProps(equipment);

        if (building != null)
        {
            // Set the pose explicitly so the very first rendered frame already
            // matches what the launch tween's t=0 sample would produce - avoids
            // a one-frame flash of a leftover scale from the previous play.
            LocalPose restPose = authoredPoses[building];
            building.localPosition = restPose.Position;
            building.localRotation = restPose.Rotation;
            building.localScale = Vector3.Scale(restPose.Scale, config.BuildingLaunchStretch);
        }

        PlaySfx(config.BuildingLaunchSfx, 1f);
    }

    private void InsertBuildingLaunch(float startTime, float endTime)
    {
        if (building == null)
            return;

        LocalPose rest = authoredPoses[building];
        float duration = endTime - startTime;

        sequence.Insert(startTime, DOVirtual.Float(0f, 1f, duration, t =>
        {
            float move = DOVirtual.EasedValue(0f, 1f, t, config.BuildingJumpUpEase);
            building.localPosition = rest.Position + Vector3.up * (config.BuildingJumpHeight * move);
            building.localScale = Vector3.Scale(rest.Scale, Vector3.Lerp(config.BuildingLaunchStretch, Vector3.one, move));
        }));
    }

    private void InsertBuildingApex(float startTime, float endTime)
    {
        if (building == null)
            return;

        LocalPose rest = authoredPoses[building];
        float duration = endTime - startTime;
        Vector3 spinAxis = NormalizedOrUp(config.BuildingApexSpinAxis);

        sequence.Insert(startTime, DOVirtual.Float(0f, 1f, duration, t =>
        {
            // 0 -> 1 -> 0 bump centered on the apex window: scale balloons to the
            // overshoot and settles back to 1 right as the fall begins.
            float bump = Mathf.Sin(t * Mathf.PI);
            building.localScale = rest.Scale * Mathf.Lerp(1f, config.BuildingApexOvershoot, bump);

            float phase = t * config.BuildingApexSpinCycles * 2f * Mathf.PI;
            float angle = config.BuildingApexSpin * Mathf.Sin(phase) * (1f - t);
            building.localRotation = Quaternion.AngleAxis(angle, spinAxis) * rest.Rotation;
        }));
    }

    private void InsertBuildingFall(float startTime, float endTime)
    {
        if (building == null)
            return;

        LocalPose rest = authoredPoses[building];
        float duration = endTime - startTime;

        sequence.Insert(startTime, DOVirtual.Float(1f, 0f, duration, t =>
            building.localPosition = rest.Position + Vector3.up * (config.BuildingJumpHeight * t))
            .SetEase(config.BuildingFallEase));
    }

    private void InsertBuildingLand(float landTime)
    {
        if (building == null)
            return;

        LocalPose rest = authoredPoses[building];

        sequence.InsertCallback(landTime, () =>
        {
            building.localPosition = rest.Position;
            building.localRotation = rest.Rotation;
            EmitDust(building.position, config.BuildingDustCount, config.BuildingDustRadius);
            PlaySfx(config.BuildingLandSfx, 1f);
            cameraShake.Shake(shakeConfig.BarracksUpgrade, 1f, Vector3.zero);
        });

        sequence.Insert(landTime, DOVirtual.Float(0f, 1f, config.BuildingLandSettleDuration, t =>
            building.localScale = Vector3.Scale(rest.Scale, Vector3.Lerp(config.BuildingLandSquash, Vector3.one, t)))
            .SetEase(Ease.OutQuad));
    }

    private void InsertProps(Transform[] props, BarracksPropMotion motion, float apexTime, float buildingLandTime)
    {
        if (props == null)
            return;

        for (int i = 0; i < props.Length; i++)
        {
            Transform prop = props[i];
            if (prop == null)
                continue;

            float revealStart = apexTime + motion.RevealStartTime + motion.RevealStagger * i;
            float hopEnd = revealStart + motion.RevealDuration + motion.HopDuration;

            // Landing is staged after the building lands, but never before this
            // prop's own spawn + hop has actually finished playing.
            float landStart = Mathf.Max(buildingLandTime + motion.LandStagger * i, hopEnd);

            InsertProp(prop, motion, revealStart, landStart);
        }
    }

    private void InsertProp(Transform prop, BarracksPropMotion motion, float revealStart, float landStart)
    {
        LocalPose pose = authoredPoses[prop];
        Vector3 airPos = pose.Position + Vector3.up * motion.AirHeight;
        Vector3 spinAxis = NormalizedOrUp(motion.HopSpinAxis);
        float hopStart = revealStart + motion.RevealDuration;
        float spinDirection = NextRange(0f, 1f) < 0.5f ? -1f : 1f;

        // Spawn: pops into existence already airborne, at a fixed point in the air.
        sequence.Insert(revealStart, DOVirtual.Float(0f, 1f, motion.RevealDuration, t =>
        {
            prop.localPosition = airPos;
            prop.localScale = pose.Scale * DOVirtual.EasedValue(0f, 1f, t, Ease.OutBack);
        }));

        // Air hop: a little extra bounce back to the same air height, with a squash & stretch
        // pulse and a small springy rotation that peaks right at the highest point of the hop
        // (bump == 1) and settles flat again by the time it comes back down (bump == 0).
        sequence.Insert(hopStart, DOVirtual.Float(0f, 1f, motion.HopDuration, t =>
        {
            float bump = Mathf.Sin(t * Mathf.PI);
            prop.localPosition = airPos + Vector3.up * (motion.HopHeight * bump);
            prop.localScale = pose.Scale * Mathf.Lerp(1f, motion.HopStretch, bump);

            float spinPhase = t * motion.HopSpinCycles * 2f * Mathf.PI;
            float angle = motion.HopSpinAngle * spinDirection * Mathf.Sin(spinPhase) * bump;
            prop.localRotation = Quaternion.AngleAxis(angle, spinAxis) * pose.Rotation;
        }));

        // Fall & land: drops from the air spawn height to the authored resting position,
        // squashing on impact.
        sequence.Insert(landStart, DOVirtual.Float(0f, 1f, motion.FallDuration, t =>
            prop.localPosition = Vector3.Lerp(airPos, pose.Position, t))
            .SetEase(motion.FallEase));

        float landImpactTime = landStart + motion.FallDuration;

        sequence.InsertCallback(landImpactTime, () =>
        {
            prop.localPosition = pose.Position;
            prop.localRotation = pose.Rotation;
            EmitDust(prop.position, motion.DustCount, motion.DustRadius);
            PlaySfx(motion.LandSfx, motion.LandPitch);
        });

        sequence.Insert(landImpactTime, DOVirtual.Float(0f, 1f, motion.LandSettleDuration, t =>
            prop.localScale = Vector3.Scale(pose.Scale, Vector3.Lerp(motion.LandSquash, Vector3.one, t)))
            .SetEase(Ease.OutQuad));
    }

    private void OnSequenceCompleted()
    {
        sequence = null;
        PlaySfx(config.CompletionSfx, 1f);
        Finish();
    }

    private void Finish()
    {
        if (sequence != null)
        {
            sequence.Kill();
            sequence = null;
        }

        foreach (KeyValuePair<Transform, LocalPose> entry in authoredPoses)
        {
            if (entry.Key != null)
                entry.Value.ApplyTo(entry.Key);
        }

        if (nextModel != null)
        {
            if (previousModel != null && previousModel != nextModel)
                previousModel.SetActive(false);

            nextModel.SetActive(true);
        }

        previousModel = null;
        nextModel = null;

        RaisePendingCompletion();
    }

    private void RaisePendingCompletion()
    {
        Action completion = pendingCompletion;
        pendingCompletion = null;
        completion?.Invoke();
    }

    private void EmitDust(Vector3 center, int count, float radius)
    {
        if (dust == null || count <= 0)
            return;

        if (!dust.isPlaying)
            dust.Play();

        ParticleSystem.EmitParams emitParams = new ParticleSystem.EmitParams { applyShapeToPosition = false };

        for (int i = 0; i < count; i++)
        {
            float angle = (i + NextRange(-0.3f, 0.3f)) / count * 2f * Mathf.PI;
            Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

            emitParams.position = center + direction * radius + Vector3.up * config.DustHeight;
            emitParams.velocity = direction * NextRange(config.DustSpeed.x, config.DustSpeed.y) + Vector3.up * config.DustUpwardSpeed;
            dust.Emit(emitParams, 1);
        }
    }

    private void PlayUpgradeStartEffects()
    {
        if (upgradeStartEffects == null)
            return;

        foreach (ParticleSystem effect in upgradeStartEffects)
        {
            if (effect == null)
                continue;

            effect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            effect.Play(true);
        }
    }

    private void PlaySfx(SfxId id, float pitch)
    {
        if (id != SfxId.None && audioService != null)
            audioService.Play(id, pitch);
    }

    private void HideProps(Transform[] props)
    {
        if (props == null)
            return;

        foreach (Transform prop in props)
        {
            if (prop != null)
                prop.localScale = Vector3.zero;
        }
    }

    private void CachePoses(Transform[] transforms)
    {
        if (transforms == null)
            return;

        foreach (Transform target in transforms)
            CachePose(target);
    }

    private void CachePose(Transform target)
    {
        if (target != null && !authoredPoses.ContainsKey(target))
            authoredPoses.Add(target, new LocalPose(target));
    }

    private void RestorePose(Transform target)
    {
        if (authoredPoses.TryGetValue(target, out LocalPose pose))
            pose.ApplyTo(target);
    }

    private static Vector3 NormalizedOrUp(Vector3 axis)
    {
        return axis.sqrMagnitude > 0.0001f ? axis.normalized : Vector3.up;
    }

    private float NextRange(float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}

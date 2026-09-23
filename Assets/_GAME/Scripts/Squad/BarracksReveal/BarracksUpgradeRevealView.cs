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
    private System.Random random;
    private Sequence sequence;
    private GameObject previousModel;
    private GameObject nextModel;

    [Inject]
    public void Construct(IAudioService audioService)
    {
        this.audioService = audioService;
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

    public void Play(GameObject previous, GameObject next)
    {
        Finish();

        previousModel = previous;
        nextModel = next;
        CachePose(previous.transform);
        random = new System.Random(config.TiltSeed);

        float switchTime = config.AnticipationDuration;

        sequence = DOTween.Sequence().SetLink(gameObject);
        sequence.Insert(0f, CreateAnticipation(previous.transform));
        sequence.InsertCallback(switchTime, SwitchModels);
        InsertBuilding(switchTime);
        InsertProps(crates, config.Crates, switchTime);
        InsertProps(barrels, config.Barrels, switchTime);
        InsertProps(equipment, config.Equipment, switchTime);
        sequence.OnComplete(OnSequenceCompleted);

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
            building.localScale = authoredPoses[building].Scale * config.BuildingStartScale;
            EmitDust(building.position, config.BuildingDustCount, config.BuildingDustRadius);
        }

        PlaySfx(config.BuildingSfx, 1f);
    }

    private void InsertBuilding(float startTime)
    {
        if (building == null)
            return;

        Vector3 scale = authoredPoses[building].Scale;
        float grownAt = startTime + config.BuildingGrowDuration;

        sequence.Insert(startTime, DOVirtual.Float(config.BuildingStartScale, config.BuildingOvershoot,
            config.BuildingGrowDuration, value => building.localScale = scale * value).SetEase(Ease.OutQuad));
        sequence.Insert(grownAt, DOVirtual.Float(config.BuildingOvershoot, 1f,
            config.BuildingSettleDuration, value => building.localScale = scale * value).SetEase(Ease.InOutQuad));
    }

    private void InsertProps(Transform[] props, BarracksPropMotion motion, float switchTime)
    {
        if (props == null)
            return;

        for (int i = 0; i < props.Length; i++)
        {
            Transform prop = props[i];
            if (prop == null)
                continue;

            float startTime = switchTime + motion.StartTime + motion.Stagger * i;
            InsertProp(prop, motion, startTime);
        }
    }

    private void InsertProp(Transform prop, BarracksPropMotion motion, float startTime)
    {
        LocalPose pose = authoredPoses[prop];
        Quaternion tilt = Quaternion.AngleAxis(NextRange(-motion.MaxTilt, motion.MaxTilt), RandomHorizontalAxis());
        Vector3 wobbleAxis = RandomHorizontalAxis();
        float landTime = startTime + motion.Duration;

        sequence.Insert(startTime, DOVirtual.Float(0f, 1f, motion.Duration, t =>
        {
            float move = DOVirtual.EasedValue(0f, 1f, t, motion.MoveEase);
            prop.localPosition = pose.Position + Vector3.up * (motion.StartHeight * (1f - move));
            prop.localRotation = Quaternion.Slerp(tilt, Quaternion.identity, DOVirtual.EasedValue(0f, 1f, t, Ease.OutQuad)) * pose.Rotation;
            prop.localScale = pose.Scale * Mathf.Lerp(motion.StartScale, 1f, Mathf.Clamp01(t * 2f));
        }));

        sequence.InsertCallback(landTime, () =>
        {
            pose.ApplyTo(prop);
            EmitDust(prop.position, motion.DustCount, motion.DustRadius);
            PlaySfx(motion.LandSfx, motion.LandPitch);
        });

        sequence.Insert(landTime, DOVirtual.Float(0f, 1f, motion.LandSettleDuration, t =>
            prop.localScale = Vector3.Scale(pose.Scale, Vector3.Lerp(motion.LandSquash, Vector3.one, t))).SetEase(Ease.OutQuad));

        if (motion.WobbleAngle <= 0f)
            return;

        sequence.Insert(landTime, DOVirtual.Float(0f, 1f, motion.WobbleDuration, t =>
        {
            float angle = motion.WobbleAngle * Mathf.Sin(t * motion.WobbleCycles * 2f * Mathf.PI) * (1f - t);
            prop.localRotation = Quaternion.AngleAxis(angle, wobbleAxis) * pose.Rotation;
        }));
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

    private Vector3 RandomHorizontalAxis()
    {
        float angle = NextRange(0f, 2f * Mathf.PI);
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }

    private float NextRange(float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}

using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;

public class BaseCombatAgentView : BaseCombatUnitView
{
    [field: SerializeField] public UnitConfig unitConfig { get; private set; }
    [field: SerializeField] public UnitRagdollView RagdollView { get; private set; }

    [Header("Death Sink")]
    [Min(0f)] [SerializeField] private float deathSinkDelay = 2f;
    [Min(0f)] [SerializeField] private float deathSinkDepth = 4f;
    [Min(0.01f)] [SerializeField] private float deathSinkDuration = 1f;
    [SerializeField] private Ease deathSinkEase = Ease.InQuad;

    private void Reset()
    {
        NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    public async UniTask PlayDeathSinkAsync()
    {
        var token = this.GetCancellationTokenOnDestroy();

        await UniTask.Delay(TimeSpan.FromSeconds(deathSinkDelay), cancellationToken: token);

        RagdollView.FreezeInPlace();

        var targetY = transform.position.y - deathSinkDepth;

        transform.DOMoveY(targetY, deathSinkDuration)
            .SetEase(deathSinkEase)
            .SetLink(gameObject);

        await UniTask.Delay(TimeSpan.FromSeconds(deathSinkDuration), cancellationToken: token);
    }
}

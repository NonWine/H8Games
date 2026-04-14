using System;
using UnityEngine;
using Zenject;

public class PlayerView : MonoBehaviour, ICombatTarget
{
    [SerializeField] private TeamId teamId;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform cameraAnchor;
    [SerializeField] private LayerMask detectionMask = ~0;
    [SerializeField] private WorldHealthBarView healthBarView;
    [SerializeField] private SimpleProjectileView projectilePrefab;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Animator animator;
    
    [Inject] private Joystick movementJoystick;
    [Inject] private IHeroStateReader stateReader;
    [Inject] private IHeroUpgradeAccess upgradeAccess;

    public event Action Died;
    public event Action<float, Vector3> DamageReceived;

    public HeroUpgradeService UpgradeService => upgradeAccess.UpgradeService;
    public TeamId TeamId => teamId;
    public bool IsAlive => stateReader.IsAlive;
    public Transform AttackOrigin => attackPoint;
    public Transform CameraAnchor => cameraAnchor;
    public LayerMask DetectionMask => detectionMask;
    public Joystick MovementJoystick => movementJoystick;
    public CharacterController CharacterController => characterController;
    public Animator Animator => animator;

    public void GetDamage(float damage, Vector3 sourceWorldPosition)
    {
        DamageReceived?.Invoke(damage, sourceWorldPosition);
    }

    public void SetHealth(float current, float max)
    {
        healthBarView.SetHealth(current, max);
    }

    public void RaiseDeath()
    {
        Died?.Invoke();
        gameObject.SetActive(false);
    }

    public void SpawnProjectileVisual(Transform target, float projectileSpeed)
    {
        SimpleProjectileView projectile = Instantiate(projectilePrefab, AttackOrigin.position, Quaternion.identity);
        projectile.Launch(target, projectileSpeed);
    }
}

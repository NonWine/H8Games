using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class UnitRagdollView : MonoBehaviour
{
        [Serializable]
        private sealed class BoneEntry
        {
            [SerializeField] private Transform transform;
            [SerializeField] private Rigidbody rigidbody;
            [SerializeField] private Collider[] colliders;

            public Transform Transform => transform;
            public Rigidbody Rigidbody => rigidbody;
            public Collider[] Colliders => colliders;

            public void Set(Transform valueTransform, Rigidbody valueRigidbody, Collider[] valueColliders)
            {
                transform = valueTransform;
                rigidbody = valueRigidbody;
                colliders = valueColliders;
            }
        }

        [Serializable]
        private sealed class DroppedWeaponEntry
        {
            [SerializeField] private Transform transform;
            [SerializeField] private Rigidbody rigidbody;
            [SerializeField] private Collider[] colliders;

            private Transform cachedParent;
            private Vector3 cachedLocalPosition;
            private Quaternion cachedLocalRotation;
            private Vector3 cachedLocalScale;
            private bool isCacheBuilt;
            private bool isPendingRelease;
            private bool isReleased;
            private float releaseTime;

            public Transform Transform => transform;
            public Rigidbody Rigidbody => rigidbody;
            public Collider[] Colliders => colliders;
            public bool IsPendingRelease => isPendingRelease;
            public bool IsReleased => isReleased;
            public float ReleaseTime => releaseTime;

            public void SetTransform(Transform value)
            {
                transform = value;
            }

            public void ResolveComponents()
            {
                if (transform == null)
                {
                    rigidbody = null;
                    colliders = Array.Empty<Collider>();

                    return;
                }

                if (rigidbody == null)
                {
                    rigidbody = transform.GetComponent<Rigidbody>();
                }

                colliders = transform.GetComponentsInChildren<Collider>(true);
            }

            public void CaptureInitialState()
            {
                if (isCacheBuilt || transform == null)
                {
                    return;
                }

                cachedParent = transform.parent;
                cachedLocalPosition = transform.localPosition;
                cachedLocalRotation = transform.localRotation;
                cachedLocalScale = transform.localScale;
                isCacheBuilt = true;
            }

            public void ResetStateImmediate(bool disableCollidersWhileAttached)
            {
                CaptureInitialState();

                isPendingRelease = false;
                isReleased = false;
                releaseTime = 0f;

                if (transform != null && cachedParent != null)
                {
                    transform.SetParent(cachedParent, false);
                    transform.localPosition = cachedLocalPosition;
                    transform.localRotation = cachedLocalRotation;
                    transform.localScale = cachedLocalScale;
                }

                if (rigidbody != null)
                {
                    rigidbody.isKinematic = true;
                    rigidbody.useGravity = false;
                    rigidbody.detectCollisions = false;
                    rigidbody.Sleep();
                }

                if (colliders == null || colliders.Length == 0)
                {
                    return;
                }

                for (var i = 0; i < colliders.Length; i++)
                {
                    var currentCollider = colliders[i];

                    if (currentCollider == null)
                    {
                        continue;
                    }

                    currentCollider.enabled = disableCollidersWhileAttached == false;
                }
            }

            public void ScheduleRelease(float delay)
            {
                isPendingRelease = true;
                isReleased = false;
                releaseTime = Time.time + delay;
            }

            public void MarkReleased()
            {
                isPendingRelease = false;
                isReleased = true;
                releaseTime = 0f;
            }
        }

        [Header("Components")]
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody rootRigidbody;
        [SerializeField] private CharacterJoint[] joints;
        [SerializeField] private BoneEntry[] bones;
        [SerializeField] private DroppedWeaponEntry[] droppedWeapons;

        [Header("Impact")]
        [Min(0f)] [SerializeField] private float impactForceMultiplier = 0.35f;
        [Min(0f)] [SerializeField] private float minImpactForce = 3f;
        [Min(0f)] [SerializeField] private float upwardDirectionFactor = 0.2f;
        [Min(0f)] [SerializeField] private float additionalUpwardImpulse = 1.5f;
        [Min(0f)] [SerializeField] private float randomTorqueForce = 1.5f;

        [Header("Settling")]
        [Min(0f)] [SerializeField] private float startLinearDamping = 0.05f;
        [Min(0f)] [SerializeField] private float startAngularDamping = 0.05f;
        [Min(0f)] [SerializeField] private float settledLinearDamping = 1.5f;
        [Min(0f)] [SerializeField] private float settledAngularDamping = 6f;
        [Min(0f)] [SerializeField] private float settleDelay = 0.35f;
        [Min(0.01f)] [SerializeField] private float settleDuration = 1.25f;
        [Min(0f)] [SerializeField] private float sleepLinearVelocityThreshold = 0.08f;
        [Min(0f)] [SerializeField] private float sleepAngularVelocityThreshold = 0.12f;
        [Min(0f)] [SerializeField] private float sleepAfterStillTime = 0.6f;

        [Header("Dropped Weapons")]
        [SerializeField] private bool disableWeaponCollidersWhileAttached = true;
        [Min(0f)] [SerializeField] private float weaponReleaseDelay = 0.08f;
        [Min(0f)] [SerializeField] private float weaponInheritedVelocityMultiplier = 1f;
        [Min(0f)] [SerializeField] private float weaponImpactForceMultiplier = 0.35f;
        [Min(0f)] [SerializeField] private float weaponMinImpactForce = 1.5f;
        [Min(0f)] [SerializeField] private float weaponUpwardDirectionFactor = 0.15f;
        [Min(0f)] [SerializeField] private float weaponAdditionalUpwardImpulse = 0.5f;
        [Min(0f)] [SerializeField] private float weaponRandomTorqueForce = 0.75f;

        private Vector3[] cachedLocalPositions;
        private Quaternion[] cachedLocalRotations;
        private float[] cachedLinearDampings;
        private float[] cachedAngularDampings;
        private bool isCacheBuilt;
        private bool isRagdollActive;
        private float ragdollEnabledTime;
        private float stillTimer;
        private UnitDamageData cachedDamageData;

        private void Awake()
        {
            CaptureInitialPose();
            ResetStateImmediate();
        }

        private void FixedUpdate()
        {
            UpdatePendingWeaponReleases();

            if (isRagdollActive == false)
            {
                return;
            }

            UpdateSettling();
        }

        public void EnableRagdoll(UnitDamageData damageData)
        {
            CaptureInitialPose();

            cachedDamageData = damageData;
            isRagdollActive = true;
            ragdollEnabledTime = Time.time;
            stillTimer = 0f;

            if (animator != null)
            {
                animator.enabled = false;
            }

            if (rootRigidbody != null)
            {
                rootRigidbody.isKinematic = true;
                rootRigidbody.useGravity = false;
            }

            if (bones != null && bones.Length > 0)
            {
                for (var i = 0; i < bones.Length; i++)
                {
                    var bone = bones[i];

                    if (bone == null)
                    {
                        continue;
                    }

                    var rigidbodyComponent = bone.Rigidbody;

                    if (rigidbodyComponent != null)
                    {
                        rigidbodyComponent.isKinematic = false;
                        rigidbodyComponent.useGravity = true;
                        rigidbodyComponent.detectCollisions = true;
                        rigidbodyComponent.linearDamping = startLinearDamping;
                        rigidbodyComponent.angularDamping = startAngularDamping;
                        rigidbodyComponent.WakeUp();
                    }

                    var colliders = bone.Colliders;

                    if (colliders == null || colliders.Length == 0)
                    {
                        continue;
                    }

                    for (var j = 0; j < colliders.Length; j++)
                    {
                        var currentCollider = colliders[j];

                        if (currentCollider == null)
                        {
                            continue;
                        }

                        currentCollider.enabled = true;
                    }
                }
            }

            PrepareDroppedWeapons();
            ApplyImpact(damageData);
        }

        public void ResetStateImmediate()
        {
            CaptureInitialPose();

            cachedDamageData = default;
            isRagdollActive = false;
            stillTimer = 0f;
            ragdollEnabledTime = 0f;

            if (bones != null && bones.Length > 0)
            {
                for (var i = 0; i < bones.Length; i++)
                {
                    var bone = bones[i];

                    if (bone == null)
                    {
                        continue;
                    }

                    var rigidbodyComponent = bone.Rigidbody;

                    if (rigidbodyComponent != null)
                    {
                        rigidbodyComponent.isKinematic = true;
                        rigidbodyComponent.useGravity = false;
                        rigidbodyComponent.detectCollisions = false;

                        if (cachedLinearDampings != null && i < cachedLinearDampings.Length)
                        {
                            rigidbodyComponent.linearDamping = cachedLinearDampings[i];
                        }

                        if (cachedAngularDampings != null && i < cachedAngularDampings.Length)
                        {
                            rigidbodyComponent.angularDamping = cachedAngularDampings[i];
                        }

                        rigidbodyComponent.Sleep();
                    }

                    var colliders = bone.Colliders;

                    if (colliders != null && colliders.Length > 0)
                    {
                        for (var j = 0; j < colliders.Length; j++)
                        {
                            var currentCollider = colliders[j];

                            if (currentCollider == null)
                            {
                                continue;
                            }

                            currentCollider.enabled = false;
                        }
                    }

                    if (cachedLocalPositions != null &&
                        cachedLocalRotations != null &&
                        i < cachedLocalPositions.Length &&
                        i < cachedLocalRotations.Length &&
                        bone.Transform != null)
                    {
                        bone.Transform.localPosition = cachedLocalPositions[i];
                        bone.Transform.localRotation = cachedLocalRotations[i];
                    }
                }
            }

            ResetDroppedWeapons();

            if (animator != null && animator.isActiveAndEnabled)
            {
                animator.Rebind();
                animator.Update(0f);
            }
        }

        [ContextMenu("AutoCollect")]
        private void AutoCollect()
        {
            var unitView = GetComponentInParent<BaseCombatAgentView>();
            var searchRoot = transform.root;

            animator ??= GetComponentInParent<Animator>();

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>(true);
            }

            rootRigidbody ??= unitView != null ? unitView.GetComponent<Rigidbody>() : null;

            if (rootRigidbody == null)
            {
                rootRigidbody = searchRoot.GetComponent<Rigidbody>();
            }

            joints = searchRoot.GetComponentsInChildren<CharacterJoint>(true);

            var childRigidbodies = searchRoot.GetComponentsInChildren<Rigidbody>(true);
            var collectedBones = new List<BoneEntry>();

            for (var i = 0; i < childRigidbodies.Length; i++)
            {
                var currentRigidbody = childRigidbodies[i];

                if (currentRigidbody == null || currentRigidbody == rootRigidbody)
                {
                    continue;
                }

                if (IsWeaponRigidbody(currentRigidbody))
                {
                    continue;
                }

                var ownedColliders = new List<Collider>();

                CollectOwnedCollidersRecursive(currentRigidbody.transform, currentRigidbody, ownedColliders);

                var entry = new BoneEntry();

                entry.Set(currentRigidbody.transform, currentRigidbody, ownedColliders.ToArray());
                collectedBones.Add(entry);
            }

            bones = collectedBones.ToArray();
            ResolveDroppedWeaponComponents();
            isCacheBuilt = false;

            CaptureInitialPose();
            ResetStateImmediate();
        }

        [ContextMenu("ResolveDroppedWeaponComponents")]
        private void ResolveDroppedWeaponComponents()
        {
            if (droppedWeapons == null || droppedWeapons.Length == 0)
            {
                return;
            }

            for (var i = 0; i < droppedWeapons.Length; i++)
            {
                var currentWeapon = droppedWeapons[i];

                if (currentWeapon == null)
                {
                    continue;
                }

                currentWeapon.ResolveComponents();
                currentWeapon.CaptureInitialState();
            }
        }

        private bool IsWeaponRigidbody(Rigidbody candidate)
        {
            if (candidate == null || droppedWeapons == null || droppedWeapons.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < droppedWeapons.Length; i++)
            {
                var currentWeapon = droppedWeapons[i];

                if (currentWeapon == null)
                {
                    continue;
                }

                if (currentWeapon.Rigidbody == candidate)
                {
                    return true;
                }

                if (currentWeapon.Transform != null && candidate.transform.IsChildOf(currentWeapon.Transform))
                {
                    return true;
                }
            }

            return false;
        }

        private void CollectOwnedCollidersRecursive(Transform currentTransform, Rigidbody ownerRigidbody, List<Collider> result)
        {
            if (currentTransform == null || ownerRigidbody == null || result == null)
            {
                return;
            }

            if (currentTransform != ownerRigidbody.transform &&
                currentTransform.TryGetComponent<Rigidbody>(out var nestedRigidbody) &&
                nestedRigidbody != ownerRigidbody)
            {
                return;
            }

            var currentColliders = currentTransform.GetComponents<Collider>();

            if (currentColliders != null && currentColliders.Length > 0)
            {
                for (var i = 0; i < currentColliders.Length; i++)
                {
                    var currentCollider = currentColliders[i];

                    if (currentCollider == null)
                    {
                        continue;
                    }

                    result.Add(currentCollider);
                }
            }

            for (var i = 0; i < currentTransform.childCount; i++)
            {
                CollectOwnedCollidersRecursive(currentTransform.GetChild(i), ownerRigidbody, result);
            }
        }

        private void CaptureInitialPose()
        {
            if (isCacheBuilt)
            {
                return;
            }

            if (bones == null || bones.Length == 0)
            {
                cachedLocalPositions = Array.Empty<Vector3>();
                cachedLocalRotations = Array.Empty<Quaternion>();
                cachedLinearDampings = Array.Empty<float>();
                cachedAngularDampings = Array.Empty<float>();
            }
            else
            {
                cachedLocalPositions = new Vector3[bones.Length];
                cachedLocalRotations = new Quaternion[bones.Length];
                cachedLinearDampings = new float[bones.Length];
                cachedAngularDampings = new float[bones.Length];

                for (var i = 0; i < bones.Length; i++)
                {
                    var bone = bones[i];

                    if (bone?.Transform != null)
                    {
                        cachedLocalPositions[i] = bone.Transform.localPosition;
                        cachedLocalRotations[i] = bone.Transform.localRotation;
                    }

                    if (bone?.Rigidbody != null)
                    {
                        cachedLinearDampings[i] = bone.Rigidbody.linearDamping;
                        cachedAngularDampings[i] = bone.Rigidbody.angularDamping;
                    }
                }
            }

            if (droppedWeapons != null && droppedWeapons.Length > 0)
            {
                for (var i = 0; i < droppedWeapons.Length; i++)
                {
                    var currentWeapon = droppedWeapons[i];

                    if (currentWeapon == null)
                    {
                        continue;
                    }

                    currentWeapon.ResolveComponents();
                    currentWeapon.CaptureInitialState();
                }
            }

            isCacheBuilt = true;
        }

        private void UpdateSettling()
        {
            if (bones == null || bones.Length == 0)
            {
                return;
            }

            var settleTime = Time.time - ragdollEnabledTime - settleDelay;
            var settleT = settleTime <= 0f ? 0f : Mathf.Clamp01(settleTime / settleDuration);
            var targetLinearDamping = Mathf.Lerp(startLinearDamping, settledLinearDamping, settleT);
            var targetAngularDamping = Mathf.Lerp(startAngularDamping, settledAngularDamping, settleT);
            var isAlmostStill = true;

            for (var i = 0; i < bones.Length; i++)
            {
                var rigidbodyComponent = bones[i]?.Rigidbody;

                if (rigidbodyComponent == null || rigidbodyComponent.isKinematic)
                {
                    continue;
                }

                rigidbodyComponent.linearDamping = targetLinearDamping;
                rigidbodyComponent.angularDamping = targetAngularDamping;

                if (rigidbodyComponent.linearVelocity.sqrMagnitude > sleepLinearVelocityThreshold * sleepLinearVelocityThreshold)
                {
                    isAlmostStill = false;
                }

                if (rigidbodyComponent.angularVelocity.sqrMagnitude > sleepAngularVelocityThreshold * sleepAngularVelocityThreshold)
                {
                    isAlmostStill = false;
                }
            }

            if (isAlmostStill)
            {
                stillTimer += Time.fixedDeltaTime;
            }
            else
            {
                stillTimer = 0f;
            }

            if (stillTimer < sleepAfterStillTime)
            {
                return;
            }

            SleepAllBones();
            isRagdollActive = false;
        }

        private void SleepAllBones()
        {
            if (bones == null || bones.Length == 0)
            {
                return;
            }

            for (var i = 0; i < bones.Length; i++)
            {
                var rigidbodyComponent = bones[i]?.Rigidbody;

                if (rigidbodyComponent == null || rigidbodyComponent.isKinematic)
                {
                    continue;
                }

                rigidbodyComponent.linearVelocity = Vector3.zero;
                rigidbodyComponent.angularVelocity = Vector3.zero;
                rigidbodyComponent.linearDamping = settledLinearDamping;
                rigidbodyComponent.angularDamping = settledAngularDamping;
                rigidbodyComponent.Sleep();
            }
        }

        private void PrepareDroppedWeapons()
        {
            if (droppedWeapons == null || droppedWeapons.Length == 0)
            {
                return;
            }

            for (var i = 0; i < droppedWeapons.Length; i++)
            {
                var currentWeapon = droppedWeapons[i];

                if (currentWeapon == null || currentWeapon.Transform == null)
                {
                    continue;
                }

                currentWeapon.ResolveComponents();
                currentWeapon.CaptureInitialState();

                if (currentWeapon.Rigidbody != null)
                {
                    currentWeapon.Rigidbody.linearVelocity = Vector3.zero;
                    currentWeapon.Rigidbody.angularVelocity = Vector3.zero;
                    currentWeapon.Rigidbody.isKinematic = true;
                    currentWeapon.Rigidbody.useGravity = false;
                    currentWeapon.Rigidbody.detectCollisions = false;
                }

                var colliders = currentWeapon.Colliders;

                if (colliders != null && colliders.Length > 0)
                {
                    for (var j = 0; j < colliders.Length; j++)
                    {
                        var currentCollider = colliders[j];

                        if (currentCollider == null)
                        {
                            continue;
                        }

                        currentCollider.enabled = disableWeaponCollidersWhileAttached == false;
                    }
                }

                currentWeapon.ScheduleRelease(weaponReleaseDelay);
            }
        }

        private void UpdatePendingWeaponReleases()
        {
            if (droppedWeapons == null || droppedWeapons.Length == 0)
            {
                return;
            }

            for (var i = 0; i < droppedWeapons.Length; i++)
            {
                var currentWeapon = droppedWeapons[i];

                if (currentWeapon == null || currentWeapon.IsPendingRelease == false)
                {
                    continue;
                }

                if (Time.time < currentWeapon.ReleaseTime)
                {
                    continue;
                }

                ReleaseDroppedWeapon(currentWeapon);
            }
        }

        private void ReleaseDroppedWeapon(DroppedWeaponEntry weaponEntry)
        {
            if (weaponEntry == null || weaponEntry.Transform == null)
            {
                return;
            }

            var weaponTransform = weaponEntry.Transform;
            var weaponRigidbody = weaponEntry.Rigidbody;
            var inheritedVelocity = ResolveInheritedVelocity(weaponTransform) * weaponInheritedVelocityMultiplier;
            var worldPosition = weaponTransform.position;
            var worldRotation = weaponTransform.rotation;

            weaponTransform.SetParent(null, true);
            weaponTransform.SetPositionAndRotation(worldPosition, worldRotation);

            if (weaponRigidbody != null)
            {
                if (!weaponRigidbody.isKinematic)
                {
                    weaponRigidbody.linearVelocity = inheritedVelocity;
                }
               
                weaponRigidbody.isKinematic = false;
                weaponRigidbody.useGravity = true;
                weaponRigidbody.detectCollisions = true;
                weaponRigidbody.WakeUp();
            }

            var colliders = weaponEntry.Colliders;

            if (colliders != null && colliders.Length > 0)
            {
                for (var i = 0; i < colliders.Length; i++)
                {
                    var currentCollider = colliders[i];

                    if (currentCollider == null)
                    {
                        continue;
                    }

                    currentCollider.enabled = true;
                }
            }

            ApplyWeaponImpact(weaponRigidbody);
            weaponEntry.MarkReleased();
        }

        private void ResetDroppedWeapons()
        {
            if (droppedWeapons == null || droppedWeapons.Length == 0)
            {
                return;
            }

            for (var i = 0; i < droppedWeapons.Length; i++)
            {
                var currentWeapon = droppedWeapons[i];

                currentWeapon?.ResetStateImmediate(disableWeaponCollidersWhileAttached);
            }
        }

        private Vector3 ResolveInheritedVelocity(Transform sourceTransform)
        {
            if (sourceTransform == null)
            {
                return Vector3.zero;
            }

            var currentTransform = sourceTransform.parent;

            while (currentTransform != null)
            {
                if (currentTransform.TryGetComponent<Rigidbody>(out var parentRigidbody))
                {
                    return parentRigidbody.linearVelocity;
                }

                currentTransform = currentTransform.parent;
            }

            return Vector3.zero;
        }

        private void ApplyImpact(UnitDamageData damageData)
        {
            if (damageData.HasImpact == false)
            {
                return;
            }

            var targetRigidbody = ResolveImpactRigidbody(damageData.ImpactPoint);

            if (targetRigidbody == null)
            {
                return;
            }

            var impulseDirection = damageData.ImpactDirection;

            if (upwardDirectionFactor > 0f)
            {
                impulseDirection = (impulseDirection + Vector3.up * upwardDirectionFactor).normalized;
            }

            var impulseForce = Mathf.Max(minImpactForce, damageData.ImpactStrength * impactForceMultiplier);
            var impactPoint = damageData.ImpactPoint;

            if (impactPoint == Vector3.zero)
            {
                impactPoint = targetRigidbody.worldCenterOfMass;
            }

            targetRigidbody.AddForceAtPosition(impulseDirection * impulseForce, impactPoint, ForceMode.Impulse);

            if (additionalUpwardImpulse > 0f)
            {
                targetRigidbody.AddForce(Vector3.up * additionalUpwardImpulse, ForceMode.Impulse);
            }

            if (randomTorqueForce > 0f)
            {
                targetRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * randomTorqueForce, ForceMode.Impulse);
            }
        }

        private void ApplyWeaponImpact(Rigidbody weaponRigidbody)
        {
            if (weaponRigidbody == null || cachedDamageData.HasImpact == false)
            {
                return;
            }

            var impulseDirection = cachedDamageData.ImpactDirection;

            if (weaponUpwardDirectionFactor > 0f)
            {
                impulseDirection = (impulseDirection + Vector3.up * weaponUpwardDirectionFactor).normalized;
            }

            var impulseForce = Mathf.Max(weaponMinImpactForce, cachedDamageData.ImpactStrength * weaponImpactForceMultiplier);

            weaponRigidbody.AddForce(impulseDirection * impulseForce, ForceMode.Impulse);

            if (weaponAdditionalUpwardImpulse > 0f)
            {
                weaponRigidbody.AddForce(Vector3.up * weaponAdditionalUpwardImpulse, ForceMode.Impulse);
            }

            if (weaponRandomTorqueForce > 0f)
            {
                weaponRigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * weaponRandomTorqueForce, ForceMode.Impulse);
            }
        }

        private Rigidbody ResolveImpactRigidbody(Vector3 impactPoint)
        {
            if (bones == null || bones.Length == 0)
            {
                return null;
            }

            var fallback = ResolveFallbackImpactRigidbody();

            if (impactPoint == Vector3.zero)
            {
                return fallback;
            }

            var bestDistance = float.MaxValue;
            var bestRigidbody = fallback;

            for (var i = 0; i < bones.Length; i++)
            {
                var rigidbodyComponent = bones[i]?.Rigidbody;

                if (rigidbodyComponent == null)
                {
                    continue;
                }

                var distance = (rigidbodyComponent.worldCenterOfMass - impactPoint).sqrMagnitude;

                if (distance >= bestDistance)
                {
                    continue;
                }

                bestDistance = distance;
                bestRigidbody = rigidbodyComponent;
            }

            return bestRigidbody;
        }

        private Rigidbody ResolveFallbackImpactRigidbody()
        {
            if (bones == null || bones.Length == 0)
            {
                return null;
            }

            for (var i = 0; i < bones.Length; i++)
            {
                var rigidbodyComponent = bones[i]?.Rigidbody;

                if (rigidbodyComponent == null)
                {
                    continue;
                }

                var lowerName = rigidbodyComponent.name.ToLowerInvariant();

                if (lowerName.Contains("hip") || lowerName.Contains("pelvis"))
                {
                    return rigidbodyComponent;
                }
            }

            for (var i = 0; i < bones.Length; i++)
            {
                var rigidbodyComponent = bones[i]?.Rigidbody;

                if (rigidbodyComponent != null)
                {
                    return rigidbodyComponent;
                }
            }

            return null;
        }
}
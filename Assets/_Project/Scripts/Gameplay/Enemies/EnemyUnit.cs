using System;
using System.Collections;
using System.Collections.Generic;
using Core.Combat;
using Core.Movement;
using Core.Pooling;
using UnityEngine;

namespace Gameplay.Enemies
{
    [RequireComponent(typeof(HealthComponent))]
    [RequireComponent(typeof(WaypointMotor))]
    public class EnemyUnit : MonoBehaviour, IPoolable, ITargetable
    {
        [Header("Configuration")]
        [SerializeField] private EnemyDataSO config;

        [Header("Visual Juice References")]
        [SerializeField] private MeshRenderer meshRenderer;

        // Public contracts
        public Transform TargetTransform => transform;
        public Vector3 AimPosition => transform.position + Vector3.up * aimHeightOffset;
        public bool IsTargetable => gameObject.activeInHierarchy && healthComponent != null && !healthComponent.IsDead;
        public TargetFaction Faction => TargetFaction.Enemy;

        public event Action<EnemyUnit> OnUnitDefeated;
        public event Action<EnemyUnit> OnUnitEscaped;

        private HealthComponent healthComponent;
        private IMover mover;
        private IPool<EnemyUnit> parentPool;
        private float aimHeightOffset = 0.5f;

        // Visual juice state
        private Vector3 originalBaseScale;
        private MaterialPropertyBlock propertyBlock;
        private static readonly int BaseColorPropId = Shader.PropertyToID("_BaseColor");
        private Color defaultTintColor = Color.white;
        private Coroutine hitJuiceCoroutine;

        private void Awake()
        {
            healthComponent = GetComponent<HealthComponent>();
            mover = GetComponent<IMover>();

            originalBaseScale = transform.localScale;
            propertyBlock = new MaterialPropertyBlock();

            if (meshRenderer == null)
            {
                meshRenderer = GetComponentInChildren<MeshRenderer>();
            }

            if (meshRenderer != null && meshRenderer.sharedMaterial != null)
            {
                if (meshRenderer.sharedMaterial.HasProperty(BaseColorPropId))
                {
                    defaultTintColor = meshRenderer.sharedMaterial.GetColor(BaseColorPropId);
                }
            }

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                aimHeightOffset = col.bounds.extents.y;
            }
        }

        private void OnEnable()
        {
            healthComponent.OnDamaged += HandleDamaged;
            healthComponent.OnKilled += HandleKilled;
            mover.OnDestinationReached += HandleDestinationReached;
        }

        private void OnDisable()
        {
            healthComponent.OnDamaged -= HandleDamaged;
            healthComponent.OnKilled -= HandleKilled;
            mover.OnDestinationReached -= HandleDestinationReached;

            if (hitJuiceCoroutine != null)
            {
                StopCoroutine(hitJuiceCoroutine);
                hitJuiceCoroutine = null;
            }

            transform.localScale = originalBaseScale;
            ResetMaterialColor();
        }

        public void Initialize(IReadOnlyList<Vector3> pathWaypoints, IPool<EnemyUnit> pool, EnemyDataSO overrideConfig = null)
        {
            if (overrideConfig != null)
            {
                config = overrideConfig;
            }

            parentPool = pool;

            // Apply data-driven stats
            if (config != null)
            {
                healthComponent.SetMaxHealth(config.maxHealth, resetCurrent: true);
                mover.Speed = config.moveSpeed;
            }
            else
            {
                healthComponent.ResetHealth();
            }

            transform.localScale = originalBaseScale;
            ResetMaterialColor();

            mover.SetPath(pathWaypoints);
        }

        // ==========================================
        // IPoolable Lifecycle Contract
        // ==========================================
        public void OnSpawnedFromPool()
        {
            SpatialTargetRegistry.Register(this);
        }

        public void OnReturnedToPool()
        {
            SpatialTargetRegistry.Unregister(this);
            mover.Stop();
        }

        // ==========================================
        // Event Handlers
        // ==========================================
        private void HandleDamaged(DamageData data)
        {
            if (hitJuiceCoroutine != null)
            {
                StopCoroutine(hitJuiceCoroutine);
            }
            hitJuiceCoroutine = StartCoroutine(HitJuiceRoutine());
        }

        private void HandleKilled(DamageData data)
        {
            OnUnitDefeated?.Invoke(this);
            RecycleSelf();
        }

        private void HandleDestinationReached()
        {
            OnUnitEscaped?.Invoke(this);
            RecycleSelf();
        }

        private void RecycleSelf()
        {
            if (parentPool != null)
            {
                parentPool.Release(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // ==========================================
        // Juice & Shaders
        // ==========================================
        private IEnumerator HitJuiceRoutine()
        {
            Color flash = config != null ? config.hitFlashColor : Color.white;
            Vector3 squash = config != null ? config.hitSquashScale : new Vector3(1.2f, 0.8f, 1.2f);

            SetMaterialColor(flash);
            transform.localScale = Vector3.Scale(originalBaseScale, squash);

            yield return new WaitForSeconds(0.08f);

            ResetMaterialColor();

            float elapsed = 0f;
            float duration = 0.08f;
            Vector3 squashed = transform.localScale;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(squashed, originalBaseScale, elapsed / duration);
                yield return null;
            }

            transform.localScale = originalBaseScale;
        }

        private void SetMaterialColor(Color color)
        {
            if (meshRenderer == null) return;
            meshRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorPropId, color);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }

        private void ResetMaterialColor()
        {
            SetMaterialColor(defaultTintColor);
        }
    }
}
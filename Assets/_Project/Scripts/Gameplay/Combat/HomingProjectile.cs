using Core.Combat;
using Core.Pooling;
using UnityEngine;

namespace Gameplay.Combat
{
    public class HomingProjectile : MonoBehaviour, IPoolable
    {
        [Header("Flight Dynamics")]
        [SerializeField] private float flightSpeed = 18f;
        [SerializeField] private float hitThreshold = 0.25f;

        [Header("Impact VFX")]
        [SerializeField] private ParticleSystem impactVFXPrefab;

        private Transform targetTransform;
        private DamageData assignedDamage;
        private IPool<HomingProjectile> parentPool;

        public void Initialize(Transform target, DamageData damage, IPool<HomingProjectile> pool)
        {
            targetTransform = target;
            assignedDamage = damage;
            parentPool = pool;
        }

        public void OnSpawnedFromPool() { }

        public void OnReturnedToPool()
        {
            targetTransform = null;
        }

        private void Update()
        {
            if (targetTransform == null || !targetTransform.gameObject.activeInHierarchy)
            {
                RecycleSelf();
                return;
            }

            Vector3 direction = targetTransform.position - transform.position;
            float stepDistance = flightSpeed * Time.deltaTime;

            if (direction.sqrMagnitude <= hitThreshold * hitThreshold)
            {
                DeliverImpact();
                return;
            }

            transform.position = Vector3.MoveTowards(transform.position, targetTransform.position, stepDistance);
            if (direction != Vector3.zero)
            {
                transform.forward = direction.normalized;
            }
        }

        private void DeliverImpact()
        {
            if (targetTransform != null && targetTransform.TryGetComponent<IDamageable>(out var damageReceiver))
            {
                damageReceiver.ApplyDamage(assignedDamage);
            }

            if (impactVFXPrefab != null)
            {
                ParticleSystem fx = Instantiate(impactVFXPrefab, transform.position, Quaternion.identity);
                Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
            }

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
    }
}
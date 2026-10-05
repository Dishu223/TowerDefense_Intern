using Core.Combat;
using Core.Pooling;
using Gameplay.Combat;
using UnityEngine;

namespace Gameplay.Towers
{
    public class TurretUnit : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private TurretDataSO turretData;
        [SerializeField] private HomingProjectile projectilePrefab;

        [Header("Mechanical Rigging")]
        [SerializeField] private Transform swivel;
        [SerializeField] private Transform firePoint;
        [SerializeField] private ParticleSystem muzzleFlash;

        [Header("Tracking Parameters")]
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float scanInterval = 0.12f;

        private ITargetable currentTarget;
        private float fireTimer = 0f;
        private IPool<HomingProjectile> bulletPool;

        private void Awake()
        {
            InitializeBulletPool();

            if (muzzleFlash == null && firePoint != null)
            {
                muzzleFlash = firePoint.GetComponentInChildren<ParticleSystem>();
            }
        }

        private void Start()
        {
            InvokeRepeating(nameof(ScanForTargets), 0f, scanInterval);
        }

        private void InitializeBulletPool()
        {
            if (projectilePrefab == null) return;

            bulletPool = new GenericPool<HomingProjectile>(
                createFunc: () => Instantiate(projectilePrefab, firePoint.position, firePoint.rotation),
                defaultCapacity: 8,
                maxSize: 32
            );
        }

        private void ScanForTargets()
        {
            float maxRange = turretData != null ? turretData.range : 5f;

            // Retain lock if valid and in range
            if (currentTarget != null && currentTarget.IsTargetable)
            {
                float sqrDist = (swivel.position - currentTarget.AimPosition).sqrMagnitude;
                if (sqrDist <= maxRange * maxRange)
                {
                    return;
                }
            }

            // Zero-allocation spatial registry lookup
            currentTarget = SpatialTargetRegistry.GetClosest(swivel != null ? swivel.position : transform.position, maxRange);
        }

        private void Update()
        {
            if (currentTarget == null) return;

            AimAtTarget();

            fireTimer -= Time.deltaTime;
            if (fireTimer <= 0f)
            {
                ExecuteShot();
                float rate = turretData != null ? turretData.fireRate : 1.5f;
                fireTimer = 1f / Mathf.Max(0.01f, rate);
            }
        }

        private void AimAtTarget()
        {
            if (swivel == null || currentTarget == null) return;

            Vector3 aimDirection = currentTarget.AimPosition - swivel.position;
            aimDirection.y = 0f;

            if (aimDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
                swivel.rotation = Quaternion.Lerp(swivel.rotation, targetRotation, turnSpeed * Time.deltaTime);
            }
        }

        private void ExecuteShot()
        {
            if (bulletPool == null || firePoint == null || currentTarget == null) return;

            HomingProjectile bullet = bulletPool.Get();
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;

            int dmgAmount = turretData != null ? turretData.damage : 25;
            DamageData damage = new DamageData(dmgAmount, DamageType.Physical, gameObject);

            bullet.Initialize(currentTarget.TargetTransform, damage, bulletPool);

            if (muzzleFlash != null)
            {
                muzzleFlash.Play();
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (turretData == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, turretData.range);
        }
    }
}
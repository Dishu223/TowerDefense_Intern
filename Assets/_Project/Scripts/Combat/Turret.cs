using UnityEngine;
using UnityEngine.Pool;

public class Turret : MonoBehaviour
{
    [Header("Data Configuration")]
    [SerializeField] private TurretDataSO turretData;

    [Header("Mechanical Rigging")]
    [Tooltip("The rotating upper head")]
    [SerializeField] private Transform swivel;
    [Tooltip("The tip of the barrel where bullets originate")]
    [SerializeField] private Transform firePoint;
    [Tooltip("The muzzle flash particle effect at the barrel tip")]
    [SerializeField] private ParticleSystem muzzleFlash;

    [Header("Rotation Settings")]
    [SerializeField] private float turnSpeed = 12f;

    private Transform currentTarget;
    private float fireCountdown = 0f;
    private IObjectPool<Projectile> bulletPool;

    private void Start()
    {
        InvokeRepeating(nameof(UpdateTarget), 0f, 0.15f);
        SetupBulletPool();

        if (muzzleFlash == null && firePoint != null)
        {
            muzzleFlash = firePoint.GetComponentInChildren<ParticleSystem>();
        }
    }

    private void SetupBulletPool()
    {
        if (turretData == null || turretData.projectilePrefab == null) return;

        bulletPool = new ObjectPool<Projectile>(
            createFunc: () => Instantiate(turretData.projectilePrefab, firePoint.position, firePoint.rotation),
            actionOnGet: p => p.gameObject.SetActive(true),
            actionOnRelease: p => p.gameObject.SetActive(false),
            actionOnDestroy: p =>
            {
                if (p != null) Destroy(p.gameObject);
            },
            defaultCapacity: 8,
            maxSize: 30
        );
    }

    private void UpdateTarget()
    {
        float sqrRange = turretData.range * turretData.range;

        // Keep current target if still active and within range
        if (currentTarget != null && currentTarget.gameObject.activeInHierarchy)
        {
            float sqrDistToCurrent = (transform.position - currentTarget.position).sqrMagnitude;
            if (sqrDistToCurrent <= sqrRange)
            {
                return;
            }
        }

        // Zero-allocation iteration through active enemies
        float shortestSqrDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        foreach (Enemy enemy in Enemy.ActiveEnemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy) continue;

            float sqrDist = (transform.position - enemy.transform.position).sqrMagnitude;
            if (sqrDist < shortestSqrDistance && sqrDist <= sqrRange)
            {
                shortestSqrDistance = sqrDist;
                nearestEnemy = enemy.transform;
            }
        }

        currentTarget = nearestEnemy;
    }

    private void Update()
    {
        if (currentTarget == null) return;

        AimAtTarget();

        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            Shoot();
            fireCountdown = 1f / turretData.fireRate;
        }
    }

    private void AimAtTarget()
    {
        if (swivel == null) return;

        Vector3 aimDirection = currentTarget.position - swivel.position;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(aimDirection);
            swivel.rotation = Quaternion.Lerp(swivel.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private void Shoot()
    {
        if (bulletPool == null || firePoint == null) return;

        Projectile bullet = bulletPool.Get();
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;
        bullet.Initialize(currentTarget, turretData.damage, bulletPool);

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
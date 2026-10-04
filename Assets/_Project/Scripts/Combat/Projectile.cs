using UnityEngine;
using UnityEngine.Pool;

public class Projectile : MonoBehaviour
{
    [Header("Flight Settings")]
    [SerializeField] private float speed = 18f;
    [SerializeField] private float hitDistanceThreshold = 0.25f;

    [Header("Effects")]
    [SerializeField] private ParticleSystem hitVFXPrefab;

    private Transform targetEnemy;
    private int damageAmount;
    private IObjectPool<Projectile> originPool;

    public void Initialize(Transform target, int damage, IObjectPool<Projectile> pool)
    {
        targetEnemy = target;
        damageAmount = damage;
        originPool = pool;
    }

    private void Update()
    {
        if (targetEnemy == null || !targetEnemy.gameObject.activeInHierarchy)
        {
            ReleaseBackToPool();
            return;
        }

        Vector3 direction = targetEnemy.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (direction.sqrMagnitude <= hitDistanceThreshold * hitDistanceThreshold)
        {
            HitTarget();
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetEnemy.position, distanceThisFrame);
        if (direction != Vector3.zero)
        {
            transform.forward = direction.normalized;
        }
    }

    private void HitTarget()
    {
        // 1. Deal damage to Enemy
        if (targetEnemy != null && targetEnemy.TryGetComponent<Enemy>(out var enemy))
        {
            enemy.TakeDamage(damageAmount);
        }

        // 2. Play particle effect at hit position if assigned
        if (hitVFXPrefab != null)
        {
            Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);
        }

        ReleaseBackToPool();
    }

    private void ReleaseBackToPool()
    {
        if (originPool != null)
        {
            originPool.Release(this);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
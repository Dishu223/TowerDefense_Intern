using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float reachThreshold = 0.1f;

    [Header("Health & Combat")]
    [SerializeField] private int maxHealth = 100;

    [Header("Juice & Feedback")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private Vector3 hitSquashScale = new Vector3(1.25f, 0.75f, 1.25f);

    public event Action<Enemy> OnGoalReached;
    public event Action<Enemy> OnEnemyDeath;

    private int currentHealth;
    private List<Vector3> pathPoints;
    private int currentPointIndex = 0;
    private float calculatedHeightOffset = 0f;
    private IObjectPool<Enemy> originPool;

    private Color originalColor;
    private Vector3 originalBaseScale;
    private Material runtimeMaterial;
    private Coroutine feedbackCoroutine;

    private void Awake()
    {
        CalculateHeightOffset();
        originalBaseScale = transform.localScale;

        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        if (meshRenderer != null)
        {
            runtimeMaterial = meshRenderer.material;
            originalColor = runtimeMaterial.color;
        }
    }

    private void CalculateHeightOffset()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            calculatedHeightOffset = col.bounds.extents.y;
            return;
        }

        Renderer rend = GetComponent<Renderer>();
        if (rend != null)
        {
            calculatedHeightOffset = rend.bounds.extents.y;
        }
    }

    public void Initialize(List<Vector3> waypoints, IObjectPool<Enemy> pool)
    {
        pathPoints = waypoints;
        currentPointIndex = 0;
        originPool = pool;
        currentHealth = maxHealth;

        transform.localScale = originalBaseScale;
        if (runtimeMaterial != null)
        {
            runtimeMaterial.color = originalColor;
        }

        if (pathPoints != null && pathPoints.Count > 0)
        {
            Vector3 startPos = pathPoints[0];
            startPos.y += calculatedHeightOffset;
            transform.position = startPos;
        }
    }

    private void Update()
    {
        if (pathPoints == null || pathPoints.Count == 0) return;
        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        Vector3 targetPosition = pathPoints[currentPointIndex];
        targetPosition.y += calculatedHeightOffset;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        float sqrDistance = (transform.position - targetPosition).sqrMagnitude;
        if (sqrDistance <= reachThreshold * reachThreshold)
        {
            currentPointIndex++;

            if (currentPointIndex >= pathPoints.Count)
            {
                ReachEnd();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (feedbackCoroutine != null)
        {
            StopCoroutine(feedbackCoroutine);
        }
        feedbackCoroutine = StartCoroutine(HitJuiceRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitJuiceRoutine()
    {
        if (runtimeMaterial != null) runtimeMaterial.color = flashColor;
        transform.localScale = Vector3.Scale(originalBaseScale, hitSquashScale);

        yield return new WaitForSeconds(0.08f);

        if (runtimeMaterial != null) runtimeMaterial.color = originalColor;

        float elapsed = 0f;
        float duration = 0.001f;
        Vector3 squashed = transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(squashed, originalBaseScale, elapsed / duration);
            yield return null;
        }

        transform.localScale = originalBaseScale;
    }

    private void Die()
    {
        OnEnemyDeath?.Invoke(this);
        ReturnToPool();
    }

    private void ReachEnd()
    {
        OnGoalReached?.Invoke(this);
        ReturnToPool();
    }

    private void ReturnToPool()
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
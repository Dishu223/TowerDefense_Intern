using System.Collections;
using UnityEngine;

public class PlacementIndicator : MonoBehaviour
{
    [Header("Visual Feedback Settings")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color validColor = new Color(0.2f, 0.85f, 0.6f, 0.45f);
    [SerializeField] private Color invalidColor = new Color(1.0f, 0.35f, 0.35f, 0.45f);
    [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.95f);

    [Header("Hover Motion Juice")]
    [SerializeField] private float snapLerpSpeed = 24f;
    [SerializeField] private float bobHeight = 0.04f;
    [SerializeField] private float bobSpeed = 6f;

    [Header("Dust VFX")]
    [SerializeField] private ParticleSystem buildDustPrefab;

    private static readonly int BaseColorProp = Shader.PropertyToID("_BaseColor");
    private MaterialPropertyBlock propBlock;
    private Coroutine flashCoroutine;
    private bool lastValidityState = true;

    private Vector3 targetAnchorPosition;
    private Vector3 baseLocalScale;

    private void Awake()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        propBlock = new MaterialPropertyBlock();
        baseLocalScale = transform.localScale;
        targetAnchorPosition = transform.position;
    }

    private void Update()
    {
        if (meshRenderer == null || !meshRenderer.enabled) return;

        // Subtle hover float + smooth snap to cell center
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        Vector3 targetPosWithBob = targetAnchorPosition + Vector3.up * bobOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosWithBob, Time.deltaTime * snapLerpSpeed);
    }

    public void SetTargetPosition(Vector3 worldCellCenter)
    {
        targetAnchorPosition = worldCellCenter;
    }

    public void SetVisible(bool isVisible)
    {
        if (meshRenderer != null && meshRenderer.enabled != isVisible)
        {
            meshRenderer.enabled = isVisible;
        }
    }

    public void SetStatus(bool isValid)
    {
        lastValidityState = isValid;

        if (flashCoroutine == null)
        {
            ApplyColor(isValid ? validColor : invalidColor);
        }
    }

    public void TriggerPlacementJuice(Vector3 position)
    {
        // 1. Spawn radial dust burst
        if (buildDustPrefab != null)
        {
            ParticleSystem dust = Instantiate(buildDustPrefab, position, Quaternion.identity);
            dust.Play();
            Destroy(dust.gameObject, dust.main.duration + dust.main.startLifetime.constantMax);
        }

        // 2. Trigger tile flash
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
        if (meshRenderer != null) meshRenderer.enabled = true;

        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        ApplyColor(flashColor);
        transform.localScale = baseLocalScale * 1.12f;

        float elapsed = 0f;
        float duration = 0.12f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            ApplyColor(Color.Lerp(flashColor, validColor, t));
            transform.localScale = Vector3.Lerp(baseLocalScale * 1.12f, baseLocalScale, t);
            yield return null;
        }

        transform.localScale = baseLocalScale;
        ApplyColor(lastValidityState ? validColor : invalidColor);
        flashCoroutine = null;
    }

    private void ApplyColor(Color col)
    {
        if (meshRenderer == null) return;
        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(BaseColorProp, col);
        meshRenderer.SetPropertyBlock(propBlock);
    }
}
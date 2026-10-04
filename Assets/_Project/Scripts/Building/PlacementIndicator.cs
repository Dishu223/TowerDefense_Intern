using System.Collections;
using UnityEngine;

public class PlacementIndicator : MonoBehaviour
{
    [Header("Visual Feedback Settings")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Color validColor = new Color(0.2f, 0.85f, 0.6f, 0.45f);
    [SerializeField] private Color invalidColor = new Color(1.0f, 0.35f, 0.35f, 0.45f);
    [SerializeField] private Color flashColor = new Color(1f, 1f, 1f, 0.95f);

    [Header("Dust VFX")]
    [SerializeField] private ParticleSystem buildDustPrefab;

    private Material runtimeMaterial;
    private Coroutine flashCoroutine;

    private void Awake()
    {
        if (meshRenderer == null)
        {
            meshRenderer = GetComponentInChildren<MeshRenderer>();
        }

        if (meshRenderer != null)
        {
            runtimeMaterial = meshRenderer.material;
        }
    }

    public void SetStatus(bool isValid)
    {
        if (runtimeMaterial != null && flashCoroutine == null)
        {
            runtimeMaterial.color = isValid ? validColor : invalidColor;
        }
    }

    public void TriggerPlacementJuice()
    {
        // 1. Spawn the radial dust ring slightly above the tile floor
        if (buildDustPrefab != null)
        {
            ParticleSystem dust = Instantiate(buildDustPrefab, transform.position, Quaternion.identity);
            dust.Play();
        }

        // 2. Trigger the quick tile flash
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        if (runtimeMaterial != null)
        {
            runtimeMaterial.color = flashColor;
        }

        float elapsed = 0f;
        float duration = 0.12f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (runtimeMaterial != null)
            {
                runtimeMaterial.color = Color.Lerp(flashColor, validColor, t);
            }

            yield return null;
        }

        if (runtimeMaterial != null)
        {
            runtimeMaterial.color = validColor;
        }

        flashCoroutine = null;
    }
}
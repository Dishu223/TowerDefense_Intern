using System.Collections;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Transform punchTarget;
    [SerializeField] private float rollDuration = 0.35f;

    private int displayedCoins = 0;
    private Coroutine rollCoroutine;
    private Vector3 originalScale;

    private void Awake()
    {
        if (punchTarget == null) punchTarget = transform;
        originalScale = punchTarget.localScale;
    }

    private void OnEnable()
    {
        CurrencyManager.OnCoinsModified += HandleCoinsModified;
    }

    private void OnDisable()
    {
        CurrencyManager.OnCoinsModified -= HandleCoinsModified;
    }

    private void HandleCoinsModified(int targetCoins, int delta)
    {
        if (rollCoroutine != null) StopCoroutine(rollCoroutine);
        rollCoroutine = StartCoroutine(RollNumberRoutine(targetCoins));

        if (delta != 0)
        {
            StartCoroutine(PunchRoutine(delta > 0));
        }
    }

    private IEnumerator RollNumberRoutine(int target)
    {
        int start = displayedCoins;
        float elapsed = 0f;

        while (elapsed < rollDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / rollDuration;
            // Smooth step ease out
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            displayedCoins = Mathf.RoundToInt(Mathf.Lerp(start, target, t));
            if (coinsText != null) coinsText.text = displayedCoins.ToString("N0");
            yield return null;
        }

        displayedCoins = target;
        if (coinsText != null) coinsText.text = displayedCoins.ToString("N0");
        rollCoroutine = null;
    }

    private IEnumerator PunchRoutine(bool isGain)
    {
        Vector3 punchScale = isGain ? originalScale * 1.2f : originalScale * 0.88f;
        punchTarget.localScale = punchScale;

        float elapsed = 0f;
        float duration = 0.18f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            punchTarget.localScale = Vector3.Lerp(punchScale, originalScale, elapsed / duration);
            yield return null;
        }

        punchTarget.localScale = originalScale;
    }
}
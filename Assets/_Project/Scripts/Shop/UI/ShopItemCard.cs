using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemCard : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Data")]
    [SerializeField] private TurretShopItemSO itemData;

    [Header("UI Bindings")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI stockCountText;
    [SerializeField] private GameObject activeSelectedBadge;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Motion Juice")]
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float hoverLiftY = 12f;
    [SerializeField] private float hoverScale = 1.05f;
    [SerializeField] private float tiltAngle = 4f;

    [Header("Exponential Hold-to-Buy")]
    [SerializeField] private float initialHoldDelay = 0.35f;
    [SerializeField] private float minInterval = 0.04f;
    [SerializeField] private float maxInterval = 0.35f;
    [SerializeField] private float accelerationRate = 2.2f;

    private Vector3 originalLocalPos;
    private Vector3 originalScale;
    private bool isHovered = false;
    private bool isPointerDown = false;
    private Coroutine holdRoutine;
    private Coroutine motionRoutine;

    public TurretShopItemSO ItemData => itemData;

    private void Awake()
    {
        if (visualRoot == null) visualRoot = transform;
        originalLocalPos = visualRoot.localPosition;
        originalScale = visualRoot.localScale;
    }

    private void OnEnable()
    {
        CurrencyManager.OnCoinsModified += OnCoinsChanged;
        CurrencyManager.OnInventoryStockChanged += OnStockChanged;
        RefreshUI();
    }

    private void OnDisable()
    {
        CurrencyManager.OnCoinsModified -= OnCoinsChanged;
        CurrencyManager.OnInventoryStockChanged -= OnStockChanged;
        StopAllCoroutines();
    }

    public void Setup(TurretShopItemSO data)
    {
        itemData = data;
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (itemData == null) return;

        if (nameText != null) nameText.text = itemData.displayName;
        if (costText != null) costText.text = $"{itemData.baseCost}";
        if (iconImage != null && itemData.icon != null) iconImage.sprite = itemData.icon;

        int stock = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetStock(itemData.itemId) : 0;
        if (stockCountText != null) stockCountText.text = $"x{stock}";

        UpdateAffordabilityVisuals();
    }

    private void OnCoinsChanged(int currentCoins, int delta)
    {
        UpdateAffordabilityVisuals();
    }

    private void OnStockChanged(string id, int count)
    {
        if (itemData != null && itemData.itemId == id && stockCountText != null)
        {
            stockCountText.text = $"x{count}";
        }
    }

    private void UpdateAffordabilityVisuals()
    {
        if (itemData == null || CurrencyManager.Instance == null) return;
        bool canAfford = CurrencyManager.Instance.CanAfford(itemData.baseCost);
        if (canvasGroup != null)
        {
            canvasGroup.alpha = canAfford ? 1f : 0.65f;
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (activeSelectedBadge != null)
        {
            activeSelectedBadge.SetActive(isSelected);
        }
    }

    // ==========================================
    // POINTER INTERFACES (JUICY INTERACTION)
    // ==========================================
    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        AnimateTo(originalLocalPos + Vector3.up * hoverLiftY, originalScale * hoverScale, Quaternion.Euler(0f, 0f, -tiltAngle));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;
        isPointerDown = false;
        if (holdRoutine != null) StopCoroutine(holdRoutine);
        AnimateTo(originalLocalPos, originalScale, Quaternion.identity);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        isPointerDown = true;
        AnimateTo(originalLocalPos, originalScale * 0.92f, Quaternion.identity);

        // Immediate single tap purchase
        ExecutePurchase();

        // Start exponential auto-buy routine
        if (holdRoutine != null) StopCoroutine(holdRoutine);
        holdRoutine = StartCoroutine(ExponentialHoldBuyRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPointerDown = false;
        if (holdRoutine != null) StopCoroutine(holdRoutine);

        Vector3 targetPos = isHovered ? originalLocalPos + Vector3.up * hoverLiftY : originalLocalPos;
        Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
        Quaternion targetRot = isHovered ? Quaternion.Euler(0f, 0f, -tiltAngle) : Quaternion.identity;

        AnimateTo(targetPos, targetScale, targetRot);
    }

    private void ExecutePurchase()
    {
        if (itemData == null || CurrencyManager.Instance == null) return;

        if (CurrencyManager.Instance.TrySpend(itemData.baseCost))
        {
            CurrencyManager.Instance.AddStock(itemData.itemId, 1);
            ShopManager.Instance?.SelectWeapon(itemData);
            StartCoroutine(CardPunchPopRoutine());
        }
        else
        {
            StartCoroutine(DeniedShakeRoutine());
        }
    }

    private IEnumerator ExponentialHoldBuyRoutine()
    {
        yield return new WaitForSeconds(initialHoldDelay);

        float currentInterval = maxInterval;
        float holdDuration = 0f;

        while (isPointerDown)
        {
            ExecutePurchase();

            holdDuration += currentInterval;
            // Exponential acceleration decay
            currentInterval = Mathf.Max(minInterval, maxInterval * Mathf.Exp(-accelerationRate * holdDuration));

            yield return new WaitForSeconds(currentInterval);
        }
    }

    // ==========================================
    // MICRO-ANIMATION COROUTINES
    // ==========================================
    private void AnimateTo(Vector3 targetPos, Vector3 targetScale, Quaternion targetRot)
    {
        if (motionRoutine != null) StopCoroutine(motionRoutine);
        motionRoutine = StartCoroutine(SmoothMotionRoutine(targetPos, targetScale, targetRot));
    }

    private IEnumerator SmoothMotionRoutine(Vector3 endPos, Vector3 endScale, Quaternion endRot)
    {
        float elapsed = 0f;
        float duration = 0.12f;
        Vector3 startPos = visualRoot.localPosition;
        Vector3 startScale = visualRoot.localScale;
        Quaternion startRot = visualRoot.localRotation;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            visualRoot.localPosition = Vector3.Lerp(startPos, endPos, t);
            visualRoot.localScale = Vector3.Lerp(startScale, endScale, t);
            visualRoot.localRotation = Quaternion.Slerp(startRot, endRot, t);
            yield return null;
        }

        visualRoot.localPosition = endPos;
        visualRoot.localScale = endScale;
        visualRoot.localRotation = endRot;
        motionRoutine = null;
    }

    private IEnumerator CardPunchPopRoutine()
    {
        Vector3 punchScale = visualRoot.localScale * 1.15f;
        visualRoot.localScale = punchScale;
        float elapsed = 0f;
        float duration = 0.14f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            visualRoot.localScale = Vector3.Lerp(punchScale, originalScale * (isHovered ? hoverScale : 1f), elapsed / duration);
            yield return null;
        }
    }

    private IEnumerator DeniedShakeRoutine()
    {
        Vector3 basePos = visualRoot.localPosition;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float xOffset = Mathf.Sin(elapsed * 50f) * 6f;
            visualRoot.localPosition = new Vector3(basePos.x + xOffset, basePos.y, basePos.z);
            yield return null;
        }

        visualRoot.localPosition = basePos;
    }
}
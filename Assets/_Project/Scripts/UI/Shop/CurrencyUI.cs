using TMPro;
using UnityEngine;
using Core.Economy;

namespace UI.Shop
{
    public class CurrencyUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text coinText;

        private void Awake()
        {
            if (coinText == null)
            {
                coinText = GetComponent<TMP_Text>();
                if (coinText == null)
                {
                    coinText = GetComponentInChildren<TMP_Text>();
                }
            }
        }

        private void Start()
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged += UpdateDisplay;
                UpdateDisplay(EconomyManager.Instance.CurrentBalance);
            }
        }

        private void OnDestroy()
        {
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged -= UpdateDisplay;
            }
        }

        private void UpdateDisplay(int balance)
        {
            if (coinText != null)
            {
                coinText.text = $"Coins: {balance}";
            }
        }
    }
}
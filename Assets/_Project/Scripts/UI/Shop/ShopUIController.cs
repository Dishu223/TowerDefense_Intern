using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Economy;
using Data.Shop;

namespace UI.Shop
{
    public class ShopUIController : MonoBehaviour
    {
        [Header("Data & Configuration")]
        [SerializeField] private ShopCatalogSO catalog;
        [SerializeField] private BuildManager buildManager;

        [Header("UI Prefabs & Containers")]
        [SerializeField] private ShopItemCardUI cardPrefab;
        [SerializeField] private Transform cardsContainer;
        [SerializeField] private Button sellModeButton;
        [SerializeField] private Image sellButtonActiveBorder;

        private readonly List<ShopItemCardUI> spawnedCards = new List<ShopItemCardUI>();

        private void Start()
        {
            if (buildManager == null)
            {
                buildManager = UnityEngine.Object.FindAnyObjectByType<BuildManager>();
            }

            BuildCards();

            if (sellModeButton != null)
            {
                sellModeButton.onClick.AddListener(() => buildManager.ToggleSellMode());
            }

            if (buildManager != null)
            {
                buildManager.OnTurretSelected += HandleTurretSelected;
                buildManager.OnSellModeChanged += HandleSellModeChanged;
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged += HandleCurrencyChanged;
                HandleCurrencyChanged(EconomyManager.Instance.CurrentBalance);
            }
        }

        private void OnDestroy()
        {
            if (buildManager != null)
            {
                buildManager.OnTurretSelected -= HandleTurretSelected;
                buildManager.OnSellModeChanged -= HandleSellModeChanged;
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        private void BuildCards()
        {
            if (catalog == null || cardPrefab == null || cardsContainer == null) return;

            foreach (var turretData in catalog.AvailableTurrets)
            {
                if (turretData == null) continue;

                ShopItemCardUI card = Instantiate(cardPrefab, cardsContainer);
                card.Bind(turretData, data => buildManager.SelectTurret(data));
                spawnedCards.Add(card);
            }
        }

        private void HandleTurretSelected(TurretDataSO selected)
        {
            for (int i = 0; i < spawnedCards.Count; i++)
            {
                spawnedCards[i].SetSelected(spawnedCards[i].BoundData == selected);
            }
        }

        private void HandleSellModeChanged(bool isSellActive)
        {
            if (sellButtonActiveBorder != null)
            {
                sellButtonActiveBorder.enabled = isSellActive;
            }
        }

        private void HandleCurrencyChanged(int newBalance)
        {
            for (int i = 0; i < spawnedCards.Count; i++)
            {
                spawnedCards[i].UpdateAffordability(newBalance);
            }
        }
    }
}
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Data.Shop;

namespace UI.Shop
{
    public class ShopItemCardUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Button selectButton;
        [SerializeField] private Image selectionBorder;

        private TurretDataSO boundData;
        private Action<TurretDataSO> onSelectedCallback;

        public TurretDataSO BoundData => boundData;

        public void Bind(TurretDataSO data, Action<TurretDataSO> onSelected)
        {
            boundData = data;
            onSelectedCallback = onSelected;

            if (nameText != null) nameText.text = data.displayName;
            if (costText != null) costText.text = $"{data.cost}G";
            if (iconImage != null && data.icon != null) iconImage.sprite = data.icon;

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelectedCallback?.Invoke(boundData));
            SetSelected(false);
        }

        public void SetSelected(bool isSelected)
        {
            if (selectionBorder != null)
            {
                selectionBorder.enabled = isSelected;
            }
        }

        public void UpdateAffordability(int currentCoins)
        {
            if (boundData == null) return;
            bool canAfford = currentCoins >= boundData.cost;
            selectButton.interactable = canAfford;
            if (costText != null)
            {
                costText.color = canAfford ? Color.white : new Color(1f, 0.4f, 0.4f);
            }
        }
    }
}
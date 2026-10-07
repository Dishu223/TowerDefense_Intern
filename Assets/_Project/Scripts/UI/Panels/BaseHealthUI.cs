using TMPro;
using UnityEngine;
using Gameplay.Base;

namespace UI.HUD
{
    public class BaseHealthUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text healthText;

        private void Start()
        {
            // Initial calculation
            RefreshDisplay();
        }

        private void Update()
        {
            // Active bases ka collective health update
            RefreshDisplay();
        }

        private void RefreshDisplay()
        {
            if (healthText == null) return;

            var bases = BaseCore.ActiveBases;
            if (bases == null || bases.Count == 0)
            {
                healthText.text = "Base HP: 0";
                return;
            }

            int totalCurrent = 0;
            int totalMax = 0;

            for (int i = 0; i < bases.Count; i++)
            {
                if (bases[i] != null)
                {
                    totalCurrent += bases[i].CurrentHealth;
                    totalMax += bases[i].MaxHealth;
                }
            }

            healthText.text = $"Base HP: {totalCurrent} / {totalMax}";
        }
    }
}
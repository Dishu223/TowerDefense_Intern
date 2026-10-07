using System;
using UnityEngine;

namespace Core.Economy
{
    public class EconomyManager : MonoBehaviour, ICurrencyService
    {
        [Header("Starting Economy")]
        [SerializeField] private int startingCoins = 250;

        public static EconomyManager Instance { get; private set; }

        public int CurrentBalance { get; private set; }
        public event Action<int> OnCurrencyChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            CurrentBalance = startingCoins;
        }

        private void Start()
        {
            // Fresh refresh across the whole game on start
            RefreshUI();
        }

        public void RefreshUI()
        {
            OnCurrencyChanged?.Invoke(CurrentBalance);
        }

        public bool CanAfford(int amount)
        {
            return CurrentBalance >= amount;
        }

        public bool TrySpend(int amount)
        {
            if (amount < 0) return false;
            if (!CanAfford(amount)) return false;

            CurrentBalance -= amount;
            OnCurrencyChanged?.Invoke(CurrentBalance);
            return true;
        }

        public void Add(int amount)
        {
            if (amount <= 0) return;

            CurrentBalance += amount;
            OnCurrencyChanged?.Invoke(CurrentBalance);
        }
    }
}
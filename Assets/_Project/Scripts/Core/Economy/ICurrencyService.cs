using System;

namespace Core.Economy
{
    public interface ICurrencyService
    {
        int CurrentBalance { get; }
        event Action<int> OnCurrencyChanged;

        bool CanAfford(int amount);
        bool TrySpend(int amount);
        void Add(int amount);
    }
}
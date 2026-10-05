using System;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Starting Economy")]
    [SerializeField] private int startingCoins = 350;

    public int CurrentCoins { get; private set; }

    // Events for UI juice
    public static event Action<int, int> OnCoinsModified; // (currentCoins, delta)
    public static event Action<string, int> OnInventoryStockChanged; // (itemId, currentStock)

    private readonly Dictionary<string, int> itemInventory = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        CurrentCoins = startingCoins;
    }

    private void Start()
    {
        OnCoinsModified?.Invoke(CurrentCoins, 0);
    }

    public bool CanAfford(int cost) => CurrentCoins >= cost;

    public bool TrySpend(int cost)
    {
        if (!CanAfford(cost)) return false;

        CurrentCoins -= cost;
        OnCoinsModified?.Invoke(CurrentCoins, -cost);
        return true;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        CurrentCoins += amount;
        OnCoinsModified?.Invoke(CurrentCoins, amount);
    }

    // Inventory Stack Management
    public int GetStock(string itemId)
    {
        return itemInventory.TryGetValue(itemId, out int count) ? count : 0;
    }

    public void AddStock(string itemId, int count = 1)
    {
        if (!itemInventory.ContainsKey(itemId))
        {
            itemInventory[itemId] = 0;
        }

        itemInventory[itemId] += count;
        OnInventoryStockChanged?.Invoke(itemId, itemInventory[itemId]);
    }

    public bool TryConsumeStock(string itemId)
    {
        if (GetStock(itemId) <= 0) return false;

        itemInventory[itemId]--;
        OnInventoryStockChanged?.Invoke(itemId, itemInventory[itemId]);
        return true;
    }
}
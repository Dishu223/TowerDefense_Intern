using UnityEngine;

public enum ShopItemType
{
    Weapon,
    Skin,
    Powerup
}

public abstract class ShopItemSO : ScriptableObject
{
    [Header("Core Identifiers")]
    public string itemId;
    public string displayName;
    public ShopItemType itemType = ShopItemType.Weapon;

    [Header("Economy & Display")]
    public int baseCost = 50;
    public Sprite icon;
    [TextArea(2, 4)] public string description;
}
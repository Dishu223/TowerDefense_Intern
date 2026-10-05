using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Catalog")]
    [SerializeField] private List<TurretShopItemSO> weaponCatalog = new List<TurretShopItemSO>();
    [SerializeField] private Transform cardContainer;
    [SerializeField] private ShopItemCard cardPrefab;

    public TurretShopItemSO SelectedWeapon { get; private set; }

    private readonly List<ShopItemCard> spawnedCards = new List<ShopItemCard>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        PopulateShop();
        if (weaponCatalog.Count > 0)
        {
            SelectWeapon(weaponCatalog[0]);
        }
    }

    private void PopulateShop()
    {
        if (cardContainer == null || cardPrefab == null) return;

        foreach (var weapon in weaponCatalog)
        {
            ShopItemCard card = Instantiate(cardPrefab, cardContainer);
            card.Setup(weapon);
            spawnedCards.Add(card);
        }
    }

    public void SelectWeapon(TurretShopItemSO weapon)
    {
        SelectedWeapon = weapon;
        foreach (var card in spawnedCards)
        {
            card.SetSelected(card.ItemData == weapon);
        }

        // Inform BuildManager
        BuildManager buildMgr = Object.FindAnyObjectByType<BuildManager>();
        if (buildMgr != null)
        {
            buildMgr.SetActiveTurretData(weapon);
        }
    }
}
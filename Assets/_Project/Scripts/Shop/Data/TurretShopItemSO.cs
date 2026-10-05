using UnityEngine;

[CreateAssetMenu(fileName = "NewTurretShopItem", menuName = "Tower Defense/Shop/Turret Item")]
public class TurretShopItemSO : ShopItemSO
{
    [Header("Turret Specifics")]
    public GameObject turretPrefab;
    public TurretDataSO turretData;

    private void OnValidate()
    {
        itemType = ShopItemType.Weapon;
        if (string.IsNullOrEmpty(itemId) && turretPrefab != null)
        {
            itemId = turretPrefab.name;
        }
    }
}
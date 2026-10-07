using System.Collections.Generic;
using UnityEngine;

namespace Data.Shop
{
    [CreateAssetMenu(fileName = "ShopCatalog", menuName = "Tower Defense/Shop/Shop Catalog")]
    public class ShopCatalogSO : ScriptableObject
    {
        [SerializeField] private List<TurretDataSO> availableTurrets = new List<TurretDataSO>();

        public IReadOnlyList<TurretDataSO> AvailableTurrets => availableTurrets;

        public TurretDataSO GetItem(string id)
        {
            for (int i = 0; i < availableTurrets.Count; i++)
            {
                if (availableTurrets[i] != null && availableTurrets[i].turretId.Equals(id, System.StringComparison.OrdinalIgnoreCase))
                {
                    return availableTurrets[i];
                }
            }
            return null;
        }
    }
}
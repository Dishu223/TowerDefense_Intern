using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Enemies
{
    [System.Serializable]
    public struct EnemyCatalogEntry
    {
        public string enemyId;
        public EnemyUnit unitPrefab;
    }

    [CreateAssetMenu(fileName = "EnemyCatalog", menuName = "Tower Defense/Enemies/Enemy Catalog")]
    public class EnemyCatalogSO : ScriptableObject
    {
        [SerializeField] private List<EnemyCatalogEntry> entries = new List<EnemyCatalogEntry>();

        private readonly Dictionary<string, EnemyUnit> lookupTable = new Dictionary<string, EnemyUnit>(StringComparer.OrdinalIgnoreCase);

        private void OnEnable()
        {
            BuildLookup();
        }

        private void BuildLookup()
        {
            lookupTable.Clear();
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.enemyId) && entry.unitPrefab != null)
                {
                    lookupTable[entry.enemyId] = entry.unitPrefab;
                }
            }
        }

        public EnemyUnit GetPrefab(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;

            if (lookupTable.Count == 0 && entries.Count > 0)
            {
                BuildLookup();
            }

            if (lookupTable.TryGetValue(id, out EnemyUnit prefab))
            {
                return prefab;
            }

            // Direct linear scan fallback (in case dictionary missed an editor domain refresh)
            foreach (var entry in entries)
            {
                if (string.Equals(entry.enemyId, id, StringComparison.OrdinalIgnoreCase))
                {
                    lookupTable[id] = entry.unitPrefab;
                    return entry.unitPrefab;
                }
            }

            Debug.LogError($"[EnemyCatalog] Enemy ID '{id}' not found in catalog!");
            return null;
        }

        public bool TryGetPrefab(string id, out EnemyUnit prefab)
        {
            if (lookupTable.Count == 0 && entries.Count > 0)
            {
                BuildLookup();
            }

            return lookupTable.TryGetValue(id, out prefab);
        }
    }
}
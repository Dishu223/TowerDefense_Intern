using System.Collections;
using System.Collections.Generic;
using Core.Navigation;
using Core.Pooling;
using Gameplay.Enemies;
using UnityEngine;

namespace Gameplay.Spawning
{
    public class ModularWaveSpawner : MonoBehaviour
    {
        [Header("Data Resolution")]
        [SerializeField] private EnemyCatalogSO enemyCatalog;

        [Header("Scene Dependencies")]
        [SerializeField] private NavigationManager navigationManager;
        [SerializeField] private TilemapGrid tilemapGrid;

        [Header("Wave Specifications")]
        [SerializeField] private List<MultiLaneWaveDataSO> waves = new List<MultiLaneWaveDataSO>();

        private int currentWaveIndex = 0;
        private readonly Dictionary<EnemyUnit, IPool<EnemyUnit>> unitPools = new Dictionary<EnemyUnit, IPool<EnemyUnit>>();
        private readonly Dictionary<int, List<Vector3>> cachedSpawnPaths = new Dictionary<int, List<Vector3>>();

        private void Start()
        {
            if (tilemapGrid == null) tilemapGrid = Object.FindAnyObjectByType<TilemapGrid>();
            if (navigationManager == null) navigationManager = Object.FindAnyObjectByType<NavigationManager>();

            CacheAllPaths();
            StartCoroutine(MasterWaveRoutine());
        }

        private void CacheAllPaths()
        {
            cachedSpawnPaths.Clear();
            if (tilemapGrid == null || navigationManager == null) return;

            foreach (int spawnId in tilemapGrid.GetAllSpawnIds())
            {
                List<Vector3> path = navigationManager.RequestPathForSpawn(spawnId);
                if (path != null && path.Count > 0)
                {
                    cachedSpawnPaths[spawnId] = path;
                }
                else
                {
                    Debug.LogWarning($"[ModularWaveSpawner] Path solving failed for Spawn ID: {spawnId}");
                }
            }
        }

        private IEnumerator MasterWaveRoutine()
        {
            while (currentWaveIndex < waves.Count)
            {
                MultiLaneWaveDataSO wave = waves[currentWaveIndex];
                yield return new WaitForSeconds(wave.timeBeforeWave);

                List<Coroutine> activeLaneCoroutines = new List<Coroutine>();

                foreach (LaneSpawnConfig lane in wave.lanes)
                {
                    activeLaneCoroutines.Add(StartCoroutine(SpawnLaneRoutine(lane)));
                }

                foreach (Coroutine routine in activeLaneCoroutines)
                {
                    yield return routine;
                }

                currentWaveIndex++;
            }
        }

        private IEnumerator SpawnLaneRoutine(LaneSpawnConfig lane)
        {
            if (lane.initialDelay > 0f)
            {
                yield return new WaitForSeconds(lane.initialDelay);
            }

            if (!cachedSpawnPaths.TryGetValue(lane.spawnId, out List<Vector3> path) || path.Count == 0)
            {
                yield break;
            }

            // Resolve prefab via catalog or optional override
            EnemyUnit unitPrefab = lane.optionalPrefabOverride != null 
                ? lane.optionalPrefabOverride 
                : (enemyCatalog != null ? enemyCatalog.GetPrefab(lane.enemyId) : null);

            if (unitPrefab == null)
            {
                Debug.LogError($"[ModularWaveSpawner] Could not resolve enemy for Lane {lane.spawnId} with ID '{lane.enemyId}'!");
                yield break;
            }

            IPool<EnemyUnit> pool = GetOrCreatePool(unitPrefab);

            for (int i = 0; i < lane.enemyCount; i++)
            {
                EnemyUnit enemy = pool.Get();
                enemy.Initialize(path, pool);

                yield return new WaitForSeconds(lane.spawnInterval);
            }
        }

        private IPool<EnemyUnit> GetOrCreatePool(EnemyUnit prefab)
        {
            if (unitPools.TryGetValue(prefab, out var existing))
            {
                return existing;
            }

            IPool<EnemyUnit> newPool = new GenericPool<EnemyUnit>(
                createFunc: () => Instantiate(prefab, transform),
                defaultCapacity: 12,
                maxSize: 60
            );

            unitPools.Add(prefab, newPool);
            return newPool;
        }
    }
}
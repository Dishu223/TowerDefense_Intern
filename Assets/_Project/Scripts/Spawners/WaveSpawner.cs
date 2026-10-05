/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WaveSpawner : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Pathfinder pathfinder;
    [SerializeField] private TilemapGrid tilemapGrid;

    [Header("Multi-Lane Waves")]
    [SerializeField] private List<MultiLaneWaveDataSO> waves = new List<MultiLaneWaveDataSO>();

    private int currentWaveIndex = 0;
    private Dictionary<Enemy, IObjectPool<Enemy>> pools = new Dictionary<Enemy, IObjectPool<Enemy>>();
    private Dictionary<int, List<Vector3>> cachedSpawnPaths = new Dictionary<int, List<Vector3>>();

    private void Start()
    {
        CacheAllPaths();
        StartCoroutine(MasterWaveRoutine());
    }

    private void CacheAllPaths()
    {
        cachedSpawnPaths.Clear();
        foreach (int spawnId in tilemapGrid.GetAllSpawnIds())
        {
            List<Vector3> path = pathfinder.CalculatePathForSpawn(spawnId);
            if (path.Count > 0)
            {
                cachedSpawnPaths[spawnId] = path;
            }
        }
    }

    private IEnumerator MasterWaveRoutine()
    {
        while (currentWaveIndex < waves.Count)
        {
            MultiLaneWaveDataSO currentWave = waves[currentWaveIndex];
            yield return new WaitForSeconds(currentWave.timeBeforeWave);

            Debug.Log($"[WaveSpawner] Starting {currentWave.waveName} across {currentWave.lanes.Count} lanes!");

            List<Coroutine> activeLaneCoroutines = new List<Coroutine>();

            // Run each lane emitter concurrently
            foreach (LaneSpawnConfig lane in currentWave.lanes)
            {
                activeLaneCoroutines.Add(StartCoroutine(SpawnLaneRoutine(lane)));
            }

            // Wait for all lane spawners in this wave to finish emitting
            foreach (Coroutine routine in activeLaneCoroutines)
            {
                yield return routine;
            }

            currentWaveIndex++;
        }

        Debug.Log("[WaveSpawner] All multi-lane waves completed!");
    }

    private IEnumerator SpawnLaneRoutine(LaneSpawnConfig lane)
    {
        if (lane.initialDelay > 0f)
        {
            yield return new WaitForSeconds(lane.initialDelay);
        }

        if (!cachedSpawnPaths.TryGetValue(lane.spawnId, out List<Vector3> path) || path.Count == 0)
        {
            Debug.LogError($"[WaveSpawner] Lane Spawn ID {lane.spawnId} has no calculated path! Skipping lane.");
            yield break;
        }

        IObjectPool<Enemy> pool = GetOrCreatePool(lane.enemyPrefab);

        for (int i = 0; i < lane.enemyCount; i++)
        {
            Enemy enemy = pool.Get();
            enemy.Initialize(path, pool);

            yield return new WaitForSeconds(lane.spawnInterval);
        }
    }

    private IObjectPool<Enemy> GetOrCreatePool(Enemy prefab)
    {
        if (pools.ContainsKey(prefab)) return pools[prefab];

        IObjectPool<Enemy> newPool = new ObjectPool<Enemy>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: e =>
            {
                if (e != null) e.gameObject.SetActive(true);
            },
            actionOnRelease: e =>
            {
                if (e != null) e.gameObject.SetActive(false);
            },
            actionOnDestroy: e =>
            {
                if (e != null) Destroy(e.gameObject);
            },
            defaultCapacity: 12,
            maxSize: 60
        );

        pools.Add(prefab, newPool);
        return newPool;
    }
} */
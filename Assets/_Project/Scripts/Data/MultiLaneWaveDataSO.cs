using System.Collections.Generic;
using Gameplay.Enemies;
using UnityEngine;

[System.Serializable]
public class LaneSpawnConfig
{
    public int spawnId = 1;

    [Tooltip("ID matching the EnemyCatalog (e.g., 'basic', 'tank', 'runner')")]
    public string enemyId = "basic";

    [Tooltip("Optional override if you want a custom one-off prefab")]
    public EnemyUnit optionalPrefabOverride;

    public int enemyCount = 8;
    public float spawnInterval = 0.8f;
    public float initialDelay = 0f;
}

[CreateAssetMenu(fileName = "NewMultiLaneWave", menuName = "Tower Defense/Waves/Multi-Lane Wave Data")]
public class MultiLaneWaveDataSO : ScriptableObject
{
    public string waveName = "Wave 1";
    public float timeBeforeWave = 3f;
    public List<LaneSpawnConfig> lanes = new List<LaneSpawnConfig>();
}
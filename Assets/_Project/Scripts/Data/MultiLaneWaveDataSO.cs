using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LaneSpawnConfig
{
    [Tooltip("The Spawn Tile ID this sub-wave will emit from")]
    public int spawnId;

    [Tooltip("Enemy prefab to spawn from this point")]
    public Enemy enemyPrefab;

    public int enemyCount;
    public float spawnInterval;

    [Tooltip("Delay in seconds before this specific lane starts spawning")]
    public float initialDelay;
}

[CreateAssetMenu(fileName = "NewMultiLaneWave", menuName = "Tower Defense/Multi-Lane Wave Data")]
public class MultiLaneWaveDataSO : ScriptableObject
{
    [Header("Wave Meta")]
    public string waveName = "Wave 1";
    public float timeBeforeWave = 3f;

    [Header("Lane Configurations")]
    [Tooltip("Add as many lanes/spawns as you want to emit during this wave!")]
    public List<LaneSpawnConfig> lanes = new List<LaneSpawnConfig>();
}
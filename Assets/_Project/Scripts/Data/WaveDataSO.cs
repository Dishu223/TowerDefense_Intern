using UnityEngine;

[CreateAssetMenu(fileName = "NewWaveData", menuName = "Tower Defense/Wave Data")]
public class WaveDataSO : ScriptableObject
{
    [Header("Lane Selection")]
    [Tooltip("Which lane/path this wave will travel on")]
    public PathDataSO pathConfig;

    [Header("Wave Configuration")]
    public Enemy enemyPrefab;
    public int enemyCount = 10;
    public float spawnInterval = 1f;
    public float timeBeforeWave = 3f;
}
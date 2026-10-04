using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewPathConfig", menuName = "Tower Defense/Path Config")]
public class PathDataSO : ScriptableObject
{
    [Header("Lane Anchors (Cell Coordinates)")]
    [Tooltip("Tile coordinate where enemies enter")]
    public Vector3Int startCell;

    [Tooltip("Tile coordinate where enemies exit")]
    public Vector3Int endCell;

    [Header("Optional Mid-Checkpoints (for Crossing/Forking Paths)")]
    [Tooltip("If paths cross, add intermediate turning points here to guide the pathfinder")]
    public List<Vector3Int> checkpoints = new List<Vector3Int>();
}
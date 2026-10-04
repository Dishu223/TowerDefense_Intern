using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapGrid : MonoBehaviour
{
    [Header("Tilemap Reference")]
    [SerializeField] private Tilemap tilemap;

    [Header("Elevation Settings")]
    [Tooltip("The world Y height where the top surface of the 3D tiles rests")]
    [SerializeField] private float floorSurfaceY = 0.2f;

    public Tilemap Tilemap => tilemap;
    public float FloorSurfaceY => floorSurfaceY;

    // Fast lookups indexed by Point ID
    private Dictionary<int, Vector3Int> exitPoints = new Dictionary<int, Vector3Int>();
    private Dictionary<int, (Vector3Int cellPos, int targetExitId)> spawnPoints = new Dictionary<int, (Vector3Int, int)>();

    private void Awake()
    {
        if (tilemap == null)
        {
            tilemap = GetComponentInChildren<Tilemap>();
        }

        ScanAndIndexRoutes();
    }

    public void ScanAndIndexRoutes()
    {
        exitPoints.Clear();
        spawnPoints.Clear();

        if (tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile is RoutingTile routingTile)
            {
                if (routingTile.role == TileRole.Exit)
                {
                    exitPoints[routingTile.pointId] = pos;
                }
                else if (routingTile.role == TileRole.Spawn)
                {
                    spawnPoints[routingTile.pointId] = (pos, routingTile.destinationExitId);
                }
            }
        }

        Debug.Log($"[TilemapGrid] Indexed {spawnPoints.Count} Spawn Points and {exitPoints.Count} Exit Points.");
    }

    public bool IsRoad(Vector3Int cellPosition)
    {
        if (tilemap == null) return false;
        TileBase tile = tilemap.GetTile(cellPosition);

        if (tile is RoutingTile) return true;

        return false;
    }

    public bool IsBuildable(Vector3Int cellPosition)
    {
        if (tilemap == null) return false;
        TileBase tile = tilemap.GetTile(cellPosition);

        // Buildable if the tile exists and is not a road/spawn/exit
        return tile != null && !(tile is RoutingTile);
    }

    public Vector3Int WorldToCell(Vector3 worldPosition)
    {
        return tilemap.WorldToCell(worldPosition);
    }

    public Vector3 GetWorldCenter(Vector3Int cellPosition)
    {
        Vector3 center = tilemap.GetCellCenterWorld(cellPosition);
        center.y = floorSurfaceY;
        return center;
    }

    public bool TryGetSpawnData(int spawnId, out Vector3Int spawnPos, out int targetExitId)
    {
        if (spawnPoints.TryGetValue(spawnId, out var data))
        {
            spawnPos = data.cellPos;
            targetExitId = data.targetExitId;
            return true;
        }

        spawnPos = Vector3Int.zero;
        targetExitId = -1;
        return false;
    }

    public bool TryGetExitPosition(int exitId, out Vector3Int exitPos)
    {
        return exitPoints.TryGetValue(exitId, out exitPos);
    }

    public IEnumerable<int> GetAllSpawnIds() => spawnPoints.Keys;
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (tilemap == null) return;

        BoundsInt bounds = tilemap.cellBounds;
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.fontStyle = FontStyle.Bold;
        style.alignment = TextAnchor.MiddleCenter;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = tilemap.GetTile(pos);
            if (tile is RoutingTile rt)
            {
                Vector3 worldPos = GetWorldCenter(pos) + Vector3.up * 0.4f;

                if (rt.role == TileRole.Spawn)
                {
                    style.normal.textColor = Color.cyan;
                    UnityEditor.Handles.Label(worldPos, $"S{rt.pointId} ➔ E{rt.destinationExitId}", style);
                }
                else if (rt.role == TileRole.Exit)
                {
                    style.normal.textColor = Color.red;
                    UnityEditor.Handles.Label(worldPos, $"EXIT {rt.pointId}", style);
                }
            }
        }
    }
#endif
}


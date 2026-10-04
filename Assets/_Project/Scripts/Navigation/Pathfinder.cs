using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    [SerializeField] private TilemapGrid tilemapGrid;

    public List<Vector3> CalculatePathForSpawn(int spawnId)
    {
        List<Vector3> worldPath = new List<Vector3>();

        if (!tilemapGrid.TryGetSpawnData(spawnId, out Vector3Int startCell, out int targetExitId))
        {
            Debug.LogError($"[Pathfinder] Spawn ID {spawnId} not found on the grid!");
            return worldPath;
        }

        if (!tilemapGrid.TryGetExitPosition(targetExitId, out Vector3Int targetEndCell))
        {
            Debug.LogError($"[Pathfinder] Target Exit ID {targetExitId} for Spawn {spawnId} not found on the grid!");
            return worldPath;
        }

        List<Vector3Int> cellPath = FindPathBFS(startCell, targetEndCell);

        if (cellPath == null || cellPath.Count == 0)
        {
            Debug.LogError($"[Pathfinder] No valid road connects Spawn {spawnId} ({startCell}) to Exit {targetExitId} ({targetEndCell})!");
            return worldPath;
        }

        foreach (Vector3Int cell in cellPath)
        {
            worldPath.Add(tilemapGrid.GetWorldCenter(cell));
        }

        return worldPath;
    }

    private List<Vector3Int> FindPathBFS(Vector3Int start, Vector3Int target)
    {
        Queue<Vector3Int> queue = new Queue<Vector3Int>();
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();

        queue.Enqueue(start);
        cameFrom[start] = start;

        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0)
        };

        bool foundTarget = false;

        while (queue.Count > 0)
        {
            Vector3Int current = queue.Dequeue();

            if (current == target)
            {
                foundTarget = true;
                break;
            }

            foreach (Vector3Int dir in directions)
            {
                Vector3Int neighbor = current + dir;

                if (!cameFrom.ContainsKey(neighbor))
                {
                    if (tilemapGrid.IsRoad(neighbor))
                    {
                        cameFrom[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        if (!foundTarget) return null;

        List<Vector3Int> path = new List<Vector3Int>();
        Vector3Int curr = target;

        while (curr != start)
        {
            path.Add(curr);
            curr = cameFrom[curr];
        }

        path.Add(start);
        path.Reverse();
        return path;
    }
}
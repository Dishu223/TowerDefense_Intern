using System.Collections.Generic;
using UnityEngine;

namespace Core.Navigation
{
    public class NavigationManager : MonoBehaviour
    {
        [SerializeField] private TilemapNavGridAdapter gridAdapter;
        [SerializeField] private TilemapGrid tilemapGrid;

        private IPathfinder pathfinder;

        private void Awake()
        {
            if (gridAdapter == null) gridAdapter = Object.FindAnyObjectByType<TilemapNavGridAdapter>();
            if (tilemapGrid == null) tilemapGrid = Object.FindAnyObjectByType<TilemapGrid>();

            pathfinder = new BFSPathfinder(gridAdapter);
        }

        public List<Vector3> RequestPathForSpawn(int spawnId)
        {
            if (tilemapGrid == null || pathfinder == null) return new List<Vector3>();

            // Matches TryGetSpawnData(int, out Vector3Int, out int)
            if (!tilemapGrid.TryGetSpawnData(spawnId, out Vector3Int startCell, out int destExitId))
            {
                return new List<Vector3>();
            }

            // Matches TryGetExitPosition(int, out Vector3Int)
            if (!tilemapGrid.TryGetExitPosition(destExitId, out Vector3Int endCell))
            {
                return new List<Vector3>();
            }

            if (pathfinder.TryFindPath(startCell, endCell, out List<Vector3> computedPath))
            {
                return computedPath;
            }

            return new List<Vector3>();
        }
    }
}
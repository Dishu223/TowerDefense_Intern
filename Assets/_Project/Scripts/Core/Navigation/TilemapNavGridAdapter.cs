using System.Collections.Generic;
using UnityEngine;

namespace Core.Navigation
{
    public class TileNode : INavNode
    {
        public Vector3Int Coordinate { get; }
        public Vector3 WorldPosition { get; }
        public bool IsTraversable { get; }

        public TileNode(Vector3Int coord, Vector3 worldPos, bool traversable)
        {
            Coordinate = coord;
            WorldPosition = worldPos;
            IsTraversable = traversable;
        }
    }

    public class TilemapNavGridAdapter : MonoBehaviour, INavGraph
    {
        [SerializeField] private TilemapGrid tilemapGrid;

        private static readonly Vector3Int[] Directions = {
            Vector3Int.up,
            Vector3Int.right,
            Vector3Int.down,
            Vector3Int.left
        };

        private void Awake()
        {
            if (tilemapGrid == null)
            {
                tilemapGrid = GetComponent<TilemapGrid>() ?? Object.FindAnyObjectByType<TilemapGrid>();
            }
        }

        public bool TryGetNode(Vector3Int coordinate, out INavNode node)
        {
            node = null;
            if (tilemapGrid == null) return false;

            bool isWalkable = tilemapGrid.IsRoad(coordinate);
            Vector3 worldPos = tilemapGrid.GetWorldCenter(coordinate);
            node = new TileNode(coordinate, worldPos, isWalkable);
            return true;
        }

        public void GetNeighbors(INavNode node, IList<INavNode> neighborsBuffer)
        {
            if (node == null || tilemapGrid == null) return;

            for (int i = 0; i < Directions.Length; i++)
            {
                Vector3Int nextCoord = node.Coordinate + Directions[i];
                if (tilemapGrid.IsRoad(nextCoord))
                {
                    neighborsBuffer.Add(new TileNode(
                        nextCoord,
                        tilemapGrid.GetWorldCenter(nextCoord),
                        true
                    ));
                }
            }
        }

        public Vector3 CellToWorldPosition(Vector3Int coordinate)
        {
            return tilemapGrid != null ? tilemapGrid.GetWorldCenter(coordinate) : Vector3.zero;
        }
    }
}
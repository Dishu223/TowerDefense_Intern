using System.Collections.Generic;
using UnityEngine;

namespace Core.Navigation
{
    public class BFSPathfinder : IPathfinder
    {
        private readonly INavGraph graph;
        private readonly Queue<Vector3Int> frontier = new Queue<Vector3Int>();
        private readonly Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        private readonly List<INavNode> neighborBuffer = new List<INavNode>(4);

        public BFSPathfinder(INavGraph navGraph)
        {
            graph = navGraph;
        }

        public bool TryFindPath(Vector3Int startCoord, Vector3Int endCoord, out List<Vector3> worldWaypoints)
        {
            worldWaypoints = new List<Vector3>();

            if (graph == null) return false;

            frontier.Clear();
            cameFrom.Clear();

            frontier.Enqueue(startCoord);
            cameFrom[startCoord] = startCoord;

            bool reachedGoal = false;

            while (frontier.Count > 0)
            {
                Vector3Int current = frontier.Dequeue();

                if (current == endCoord)
                {
                    reachedGoal = true;
                    break;
                }

                if (!graph.TryGetNode(current, out INavNode currentNode)) continue;

                neighborBuffer.Clear();
                graph.GetNeighbors(currentNode, neighborBuffer);

                for (int i = 0; i < neighborBuffer.Count; i++)
                {
                    INavNode neighbor = neighborBuffer[i];
                    if (neighbor == null || !neighbor.IsTraversable) continue;

                    Vector3Int nextCoord = neighbor.Coordinate;
                    if (!cameFrom.ContainsKey(nextCoord))
                    {
                        frontier.Enqueue(nextCoord);
                        cameFrom[nextCoord] = current;
                    }
                }
            }

            if (!reachedGoal) return false;

            // Reconstruct path backward
            Vector3Int trace = endCoord;
            List<Vector3> reversedPath = new List<Vector3>();

            while (trace != startCoord)
            {
                reversedPath.Add(graph.CellToWorldPosition(trace));
                trace = cameFrom[trace];
            }
            reversedPath.Add(graph.CellToWorldPosition(startCoord));

            // Reverse into start -> goal order
            for (int i = reversedPath.Count - 1; i >= 0; i--)
            {
                worldWaypoints.Add(reversedPath[i]);
            }

            return true;
        }
    }
}
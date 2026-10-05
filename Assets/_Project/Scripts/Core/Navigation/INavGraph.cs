using System.Collections.Generic;
using UnityEngine;

namespace Core.Navigation
{
    public interface INavGraph
    {
        bool TryGetNode(Vector3Int coordinate, out INavNode node);
        void GetNeighbors(INavNode node, IList<INavNode> neighborsBuffer);
        Vector3 CellToWorldPosition(Vector3Int coordinate);
    }
}
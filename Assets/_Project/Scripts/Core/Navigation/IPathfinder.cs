using System.Collections.Generic;
using UnityEngine;

namespace Core.Navigation
{
    public interface IPathfinder
    {
        bool TryFindPath(Vector3Int startCoord, Vector3Int endCoord, out List<Vector3> worldWaypoints);
    }
}
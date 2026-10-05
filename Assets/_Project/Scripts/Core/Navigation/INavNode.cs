using UnityEngine;

namespace Core.Navigation
{
    public interface INavNode
    {
        Vector3Int Coordinate { get; }
        Vector3 WorldPosition { get; }
        bool IsTraversable { get; }
    }
}
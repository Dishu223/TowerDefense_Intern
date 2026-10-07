using UnityEngine;

namespace Core.Combat
{
    public enum TargetFaction
    {
        Enemy,
        Friendly,
        Neutral
    }

    /// Contract for any entity that can be spatially tracked and locked onto by turrets or weapons.
    public interface ITargetable
    {
        Transform TargetTransform { get; }
        Vector3 AimPosition { get; }
        bool IsTargetable { get; }
        TargetFaction Faction { get; }
    }
}
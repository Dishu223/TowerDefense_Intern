using System.Collections.Generic;
using UnityEngine;

namespace Core.Combat
{
    /// <summary>
    /// High-performance spatial registry for targetable entities.
    /// Provides zero-allocation proximity queries.
    /// </summary>
    public static class SpatialTargetRegistry
    {
        private static readonly HashSet<ITargetable> activeTargets = new HashSet<ITargetable>();

        public static void Register(ITargetable target)
        {
            if (target != null)
            {
                activeTargets.Add(target);
            }
        }

        public static void Unregister(ITargetable target)
        {
            if (target != null)
            {
                activeTargets.Remove(target);
            }
        }

        /// <summary>
        /// Finds the closest valid target to an origin position within maxRange matching the specified faction.
        /// Zero allocations (avoids foreach enumerator heap boxing via HashSet struct enumerator).
        /// </summary>
        public static ITargetable GetClosest(Vector3 origin, float maxRange, TargetFaction targetFaction = TargetFaction.Enemy)
        {
            float shortestSqrDistance = maxRange * maxRange;
            ITargetable closest = null;

            foreach (ITargetable target in activeTargets)
            {
                if (target == null || !target.IsTargetable || target.Faction != targetFaction)
                {
                    continue;
                }

                float sqrDist = (origin - target.AimPosition).sqrMagnitude;
                if (sqrDist <= shortestSqrDistance)
                {
                    shortestSqrDistance = sqrDist;
                    closest = target;
                }
            }

            return closest;
        }

        /// <summary>
        /// Clears all entries. Useful on scene transitions.
        /// </summary>
        public static void Clear()
        {
            activeTargets.Clear();
        }
    }
}
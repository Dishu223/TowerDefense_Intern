using System.Collections.Generic;
using UnityEngine;

namespace Core.Combat
{
    public static class SpatialTargetRegistry
    {
        private static readonly HashSet<ITargetable> activeTargets = new HashSet<ITargetable>();
        private static readonly List<ITargetable> deadReferencesBuffer = new List<ITargetable>();

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

        public static ITargetable GetClosest(Vector3 origin, float maxRange, TargetFaction targetFaction = TargetFaction.Enemy)
        {
            float shortestSqrDistance = maxRange * maxRange;
            ITargetable closest = null;
            deadReferencesBuffer.Clear();

            foreach (ITargetable target in activeTargets)
            {
                // Unity-safe check: check if the underlying Unity Object was destroyed
                if (target is Object unityObj && unityObj == null)
                {
                    deadReferencesBuffer.Add(target);
                    continue;
                }

                if (!target.IsTargetable || target.Faction != targetFaction)
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

            // Clean up any destroyed objects automatically
            for (int i = 0; i < deadReferencesBuffer.Count; i++)
            {
                activeTargets.Remove(deadReferencesBuffer[i]);
            }

            return closest;
        }

        public static void Clear()
        {
            activeTargets.Clear();
            deadReferencesBuffer.Clear();
        }
    }
}
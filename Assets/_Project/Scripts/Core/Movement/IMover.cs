using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Movement
{
    /// Contract for any entity capable of autonomous or scripted locomotion.
    public interface IMover
    {
        float Speed { get; set; }
        bool IsMoving { get; }
        Vector3 Velocity { get; }

        void SetPath(IReadOnlyList<Vector3> waypoints);
        void Stop();
        void Resume();

        event Action OnDestinationReached;
        event Action<int> OnWaypointReached;
    }
}
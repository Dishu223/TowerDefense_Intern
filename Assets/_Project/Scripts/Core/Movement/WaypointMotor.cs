using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Movement
{
    /// <summary>
    /// Standalone motor that translates a Transform along a sequence of 3D waypoints.
    /// Completely decoupled from combat, health, or pooling logic.
    /// </summary>
    public class WaypointMotor : MonoBehaviour, IMover
    {
        [Header("Locomotion Tuning")]
        [SerializeField] private float baseSpeed = 3.5f;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float reachThreshold = 0.1f;

        public float Speed
        {
            get => baseSpeed;
            set => baseSpeed = Mathf.Max(0f, value);
        }

        public bool IsMoving { get; private set; }
        public Vector3 Velocity { get; private set; }

        public event Action OnDestinationReached;
        public event Action<int> OnWaypointReached;

        private IReadOnlyList<Vector3> currentPath;
        private int currentTargetIndex = 0;
        private Vector3 lastPosition;
        private float heightOffset = 0f;

        private void Awake()
        {
            CalculateHeightOffset();
            lastPosition = transform.position;
        }

        private void CalculateHeightOffset()
        {
            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                heightOffset = col.bounds.extents.y;
                return;
            }

            Renderer rend = GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                heightOffset = rend.bounds.extents.y;
            }
        }

        public void SetPath(IReadOnlyList<Vector3> waypoints)
        {
            if (waypoints == null || waypoints.Count == 0)
            {
                Stop();
                return;
            }

            currentPath = waypoints;
            currentTargetIndex = 0;
            IsMoving = true;

            // Snap immediately to initial waypoint
            Vector3 startPos = currentPath[0];
            startPos.y += heightOffset;
            transform.position = startPos;
            lastPosition = startPos;
        }

        public void Stop()
        {
            IsMoving = false;
            Velocity = Vector3.zero;
        }

        public void Resume()
        {
            if (currentPath != null && currentTargetIndex < currentPath.Count)
            {
                IsMoving = true;
            }
        }

        private void Update()
        {
            if (!IsMoving || currentPath == null || currentTargetIndex >= currentPath.Count)
            {
                Velocity = Vector3.zero;
                return;
            }

            AdvanceMovement();
        }

        private void AdvanceMovement()
        {
            Vector3 targetPos = currentPath[currentTargetIndex];
            targetPos.y += heightOffset;

            // Frame-rate independent translation
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                baseSpeed * Time.deltaTime
            );

            // Compute instantaneous velocity vector
            Velocity = (transform.position - lastPosition) / Mathf.Max(Time.deltaTime, 0.0001f);
            lastPosition = transform.position;

            // Smooth face-direction alignment
            Vector3 horizontalDirection = targetPos - transform.position;
            horizontalDirection.y = 0f;
            if (horizontalDirection.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(horizontalDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
            }

            // Check waypoint arrival via squared magnitude
            float sqrDist = (transform.position - targetPos).sqrMagnitude;
            if (sqrDist <= reachThreshold * reachThreshold)
            {
                OnWaypointReached?.Invoke(currentTargetIndex);
                currentTargetIndex++;

                if (currentTargetIndex >= currentPath.Count)
                {
                    IsMoving = false;
                    Velocity = Vector3.zero;
                    OnDestinationReached?.Invoke();
                }
            }
        }
    }
}
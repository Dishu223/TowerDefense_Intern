using UnityEngine;
using UnityEngine.InputSystem;

public class JuicyCameraController : MonoBehaviour
{
    [Header("Dependencies")]
    [Tooltip("Optional: If unassigned, automatically finds the active TilemapGrid in the scene")]
    [SerializeField] private TilemapGrid tilemapGrid;

    [Header("Pan Movement & Damping")]
    [SerializeField] private float moveSpeed = 18f;
    [SerializeField] private float smoothTime = 0.12f;
    [SerializeField] private bool enableEdgePanning = true;
    [Tooltip("Distance from screen edge in pixels to trigger panning")]
    [SerializeField] private float edgePanBorder = 25f;
    [SerializeField] private float dragSensitivity = 1.2f;

    [Header("Individual Level Edge Padding (In Tiles/Units)")]
    [Tooltip("Extra roaming allowance forward (towards Exits)")]
    [SerializeField] private float forwardPadding = 4.0f;
    [Tooltip("Extra roaming allowance backward (towards Spawns)")]
    [SerializeField] private float backPadding = 1.5f;
    [Tooltip("Extra roaming allowance left")]
    [SerializeField] private float leftPadding = 2.5f;
    [Tooltip("Extra roaming allowance right")]
    [SerializeField] private float rightPadding = 2.5f;

    [Header("Zoom Distance Settings")]
    [Range(0f, 100f)]
    [SerializeField] private float zoomSensitivity = 50f;
    [SerializeField] private float minDistance = 7f;
    [SerializeField] private float maxDistance = 22f;
    [SerializeField] private float zoomSmoothTime = 0.08f;

    [Header("Juice & Dynamic Tilt")]
    [SerializeField] private float maxRollAngle = 2.5f;
    [SerializeField] private float maxPitchOffset = 2.0f;
    [SerializeField] private float tiltSmoothSpeed = 8f;

    // Auto-calculated Level Geometry
    private Vector3 levelCenterWorld;
    private float maxPanLeft;
    private float maxPanRight;
    private float maxPanForward;
    private float maxPanBack;

    // Fixed Baseline Axes
    private Quaternion baseRotation;
    private Vector3 fixedCamForward;
    private Vector3 fixedCamRight;
    private Vector3 fixedViewDirection;

    // Movement state
    private float currentPanLateral = 0f;
    private float currentPanLongitudinal = 0f;

    private float targetDistance;
    private float currentDistance;
    private float distanceVelocity;

    private Vector3 targetGroundPivot;
    private Vector3 currentGroundPivot;
    private Vector3 pivotVelocity;

    private bool isMovementEnabled = true;
    private bool isDragging = false;
    private Vector2 lastMouseScreenPos;

    private void Start()
    {
        baseRotation = transform.rotation;
        fixedCamForward = Vector3.ProjectOnPlane(baseRotation * Vector3.forward, Vector3.up).normalized;
        fixedCamRight = Vector3.ProjectOnPlane(baseRotation * Vector3.right, Vector3.up).normalized;
        fixedViewDirection = (baseRotation * Vector3.forward).normalized;

        if (tilemapGrid == null)
        {
            tilemapGrid = Object.FindAnyObjectByType<TilemapGrid>();
        }

        AdaptToLevelBounds();

        targetDistance = Mathf.Clamp(Vector3.Distance(transform.position, levelCenterWorld), minDistance, maxDistance);
        currentDistance = targetDistance;
        targetGroundPivot = levelCenterWorld;
        currentGroundPivot = levelCenterWorld;
    }

    public void AdaptToLevelBounds()
    {
        if (tilemapGrid != null && tilemapGrid.Tilemap != null)
        {
            tilemapGrid.Tilemap.CompressBounds();
            BoundsInt cellBounds = tilemapGrid.Tilemap.cellBounds;

            Vector3 minWorld = tilemapGrid.Tilemap.CellToWorld(cellBounds.min);
            Vector3 maxWorld = tilemapGrid.Tilemap.CellToWorld(cellBounds.max);

            levelCenterWorld = (minWorld + maxWorld) * 0.5f;
            levelCenterWorld.y = 0f;

            float sizeX = Mathf.Abs(maxWorld.x - minWorld.x) * 0.5f;
            float sizeZ = Mathf.Abs(maxWorld.z - minWorld.z) * 0.5f;

            maxPanLeft = sizeX + leftPadding;
            maxPanRight = sizeX + rightPadding;
            maxPanForward = sizeZ + forwardPadding;
            maxPanBack = sizeZ + backPadding;
        }
        else
        {
            levelCenterWorld = Vector3.zero;
            maxPanLeft = 10f + leftPadding;
            maxPanRight = 10f + rightPadding;
            maxPanForward = 10f + forwardPadding;
            maxPanBack = 10f + backPadding;
        }
    }

    private void Update()
    {
        HandleHotkeys();

        if (isMovementEnabled)
        {
            HandlePanInput();
            HandleMouseDrag();
            HandleZoomInput();
        }

        ApplyMovement();
        ApplyDynamicTilt();
    }

    private void HandleHotkeys()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.cKey.wasPressedThisFrame)
        {
            isMovementEnabled = !isMovementEnabled;
            isDragging = false;
            Debug.Log($"[JuicyCamera] Camera movement: {(isMovementEnabled ? "<color=green>ENABLED</color>" : "<color=red>DISABLED</color>")}");
        }

        if (isMovementEnabled && keyboard.rKey.wasPressedThisFrame)
        {
            currentPanLateral = 0f;
            currentPanLongitudinal = 0f;
            targetDistance = (minDistance + maxDistance) * 0.5f;
            Debug.Log("[JuicyCamera] Camera reset to level center.");
        }
    }

    private void HandlePanInput()
    {
        float inputLateral = 0f;
        float inputLongitudinal = 0f;

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) inputLongitudinal += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) inputLongitudinal -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) inputLateral -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) inputLateral += 1f;
        }

        var mouse = Mouse.current;
        if (enableEdgePanning && mouse != null && !isDragging)
        {
            Vector2 mousePos = mouse.position.ReadValue();
            if (mousePos.x >= 0 && mousePos.x <= Screen.width && mousePos.y >= 0 && mousePos.y <= Screen.height)
            {
                if (mousePos.x <= edgePanBorder) inputLateral -= 1f;
                else if (mousePos.x >= Screen.width - edgePanBorder) inputLateral += 1f;

                if (mousePos.y <= edgePanBorder) inputLongitudinal -= 1f;
                else if (mousePos.y >= Screen.height - edgePanBorder) inputLongitudinal += 1f;
            }
        }

        Vector2 inputDir = new Vector2(inputLateral, inputLongitudinal);
        if (inputDir.sqrMagnitude > 1f) inputDir.Normalize();

        currentPanLateral += inputDir.x * (moveSpeed * Time.deltaTime);
        currentPanLongitudinal += inputDir.y * (moveSpeed * Time.deltaTime);

        ClampPanValues();
    }

    private void HandleMouseDrag()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        bool dragButtonDown = mouse.rightButton.isPressed || mouse.middleButton.isPressed;

        if (mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
        {
            isDragging = true;
            lastMouseScreenPos = mouse.position.ReadValue();
        }

        if (dragButtonDown && isDragging)
        {
            Vector2 currentMousePos = mouse.position.ReadValue();
            Vector2 delta = currentMousePos - lastMouseScreenPos;
            lastMouseScreenPos = currentMousePos;

            float heightFactor = (currentDistance / 15f) * (dragSensitivity * 0.02f);
            currentPanLateral -= delta.x * heightFactor;
            currentPanLongitudinal -= delta.y * heightFactor;

            ClampPanValues();
        }
        else
        {
            isDragging = false;
        }
    }

    private void HandleZoomInput()
    {
        var mouse = Mouse.current;
        if (mouse == null || zoomSensitivity <= 0f) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(scroll) > 0.01f)
        {
            float scrollStep = Mathf.Sign(scroll);
            float stepMultiplier = (zoomSensitivity / 100f) * 2.5f;

            targetDistance -= scrollStep * stepMultiplier;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }
    }

    private void ClampPanValues()
    {
        currentPanLateral = Mathf.Clamp(currentPanLateral, -maxPanLeft, maxPanRight);
        currentPanLongitudinal = Mathf.Clamp(currentPanLongitudinal, -maxPanBack, maxPanForward);
    }

    private void ApplyMovement()
    {
        targetGroundPivot = levelCenterWorld + (fixedCamRight * currentPanLateral) + (fixedCamForward * currentPanLongitudinal);
        currentGroundPivot = Vector3.SmoothDamp(currentGroundPivot, targetGroundPivot, ref pivotVelocity, smoothTime);

        currentDistance = Mathf.SmoothDamp(currentDistance, targetDistance, ref distanceVelocity, zoomSmoothTime);

        transform.position = currentGroundPivot - (fixedViewDirection * currentDistance);
    }

    private void ApplyDynamicTilt()
    {
        Vector3 localVel = Quaternion.Inverse(baseRotation) * pivotVelocity;

        float targetRoll = -Mathf.Clamp(localVel.x / moveSpeed, -1f, 1f) * maxRollAngle;
        float targetPitchOffset = Mathf.Clamp(localVel.z / moveSpeed, -1f, 1f) * maxPitchOffset;

        Quaternion dynamicTilt = baseRotation * Quaternion.Euler(targetPitchOffset, 0f, targetRoll);
        transform.rotation = Quaternion.Slerp(transform.rotation, dynamicTilt, Time.deltaTime * tiltSmoothSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? levelCenterWorld : transform.position;
        Quaternion rot = Application.isPlaying ? baseRotation : transform.rotation;

        Vector3 fwd = Vector3.ProjectOnPlane(rot * Vector3.forward, Vector3.up).normalized;
        Vector3 rgt = Vector3.ProjectOnPlane(rot * Vector3.right, Vector3.up).normalized;

        Gizmos.color = Color.cyan;
        Vector3 p1 = center + fwd * maxPanForward + rgt * maxPanRight;
        Vector3 p2 = center + fwd * maxPanForward - rgt * maxPanLeft;
        Vector3 p3 = center - fwd * maxPanBack - rgt * maxPanLeft;
        Vector3 p4 = center - fwd * maxPanBack + rgt * maxPanRight;

        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p4);
        Gizmos.DrawLine(p4, p1);
    }
}
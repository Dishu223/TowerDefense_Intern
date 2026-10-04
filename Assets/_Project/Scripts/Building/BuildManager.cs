using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TilemapGrid tilemapGrid;
    [SerializeField] private GameObject defaultTurretPrefab;
    [SerializeField] private GameObject placementIndicator;

    [Header("Painting Controls")]
    [Tooltip("Hold down left mouse button to paint turrets across tiles")]
    [SerializeField] private bool allowDragPainting = true;

    [Header("Placement Juice")]
    [Tooltip("Small scale bounce when turrets are placed")]
    [SerializeField] private bool animateTurretSpawn = true;

    private readonly HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
    private Vector3Int lastPaintedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
    private Plane groundPlane;
    private PlacementIndicator indicatorComponent;

    private void Start()
    {
        if (tilemapGrid == null)
        {
            tilemapGrid = Object.FindAnyObjectByType<TilemapGrid>();
        }

        // Automatic fallback if prefab asset was dragged instead of a scene instance
        if (placementIndicator != null && (!placementIndicator.scene.IsValid() || !placementIndicator.activeInHierarchy))
        {
            placementIndicator = Instantiate(placementIndicator, Vector3.zero, Quaternion.identity);
            placementIndicator.name = "TilePlacementIndicator (Runtime Instance)";
        }
        else if (placementIndicator == null)
        {
            PlacementIndicator found = Object.FindAnyObjectByType<PlacementIndicator>();
            if (found != null) placementIndicator = found.gameObject;
        }

        if (placementIndicator != null)
        {
            placementIndicator.SetActive(true);
            indicatorComponent = placementIndicator.GetComponent<PlacementIndicator>();
            if (indicatorComponent != null)
            {
                indicatorComponent.SetVisible(false);
            }
        }

        float surfaceY = tilemapGrid != null ? tilemapGrid.FloorSurfaceY : 0.2f;
        groundPlane = new Plane(Vector3.up, new Vector3(0f, surfaceY, 0f));
    }

    private void Update()
    {
        HandlePlacementInput();
    }

    private void HandlePlacementInput()
    {
        var mouse = Mouse.current;
        if (mouse == null || Camera.main == null) return;

        // Prevent building through UI buttons or HUD elements
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            if (indicatorComponent != null) indicatorComponent.SetVisible(false);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
        if (!groundPlane.Raycast(ray, out float enterDist))
        {
            if (indicatorComponent != null) indicatorComponent.SetVisible(false);
            return;
        }

        Vector3 hitPoint = ray.GetPoint(enterDist);
        Vector3Int cellPos = tilemapGrid.WorldToCell(hitPoint);
        Vector3 cellCenter = tilemapGrid.GetWorldCenter(cellPos);

        bool isBuildable = tilemapGrid.IsBuildable(cellPos);
        bool isOccupied = occupiedCells.Contains(cellPos);
        bool isValidPlacement = isBuildable && !isOccupied;

        // 1. Smooth Indicator Movement & Validity
        if (placementIndicator != null && indicatorComponent != null)
        {
            indicatorComponent.SetVisible(true);
            indicatorComponent.SetTargetPosition(cellCenter);
            indicatorComponent.SetStatus(isValidPlacement);
        }

        // 2. Drag-to-Paint & Single Click
        bool isBuilding = allowDragPainting ? mouse.leftButton.isPressed : mouse.leftButton.wasPressedThisFrame;

        if (isBuilding)
        {
            if (cellPos != lastPaintedCell)
            {
                if (isValidPlacement)
                {
                    PlaceTurret(cellPos, cellCenter);
                }
                lastPaintedCell = cellPos;
            }
        }
        else
        {
            lastPaintedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
        }
    }

    private void PlaceTurret(Vector3Int cellPos, Vector3 worldPos)
    {
        if (defaultTurretPrefab == null) return;

        GameObject turretObj = Instantiate(defaultTurretPrefab, worldPos, Quaternion.identity);
        occupiedCells.Add(cellPos);

        if (animateTurretSpawn)
        {
            StartCoroutine(PopSpawnRoutine(turretObj.transform));
        }

        if (indicatorComponent != null)
        {
            indicatorComponent.TriggerPlacementJuice(worldPos);
            indicatorComponent.SetStatus(false);
        }
    }

    private IEnumerator PopSpawnRoutine(Transform target)
    {
        Vector3 endScale = target.localScale;
        target.localScale = new Vector3(endScale.x * 0.1f, endScale.y * 1.4f, endScale.z * 0.1f);

        float elapsed = 0f;
        float duration = 0.18f;

        while (elapsed < duration && target != null)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // Elastic pop curve
            float curve = Mathf.Sin(t * Mathf.PI * 0.5f);
            target.localScale = Vector3.Lerp(target.localScale, endScale, curve);
            yield return null;
        }

        if (target != null) target.localScale = endScale;
    }
}
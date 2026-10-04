using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TilemapGrid tilemapGrid;
    [SerializeField] private Camera mainCamera;

    [Header("Active Selection")]
    [SerializeField] private TurretDataSO selectedTurretData;

    [Header("Indicator & Feedback")]
    [SerializeField] private PlacementIndicator indicatorPrefab;

    private HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();
    private PlacementIndicator activeIndicator;
    private Vector3Int currentHoveredCell;
    private bool isHoveringValid = false;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (indicatorPrefab != null)
        {
            activeIndicator = Instantiate(indicatorPrefab);
            activeIndicator.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateHoverState();

        if (WasPointerPressed())
        {
            TryPlaceTurret(currentHoveredCell);
        }
    }

    private void UpdateHoverState()
    {
        if (selectedTurretData == null || activeIndicator == null) return;

        Vector2 screenPos = GetPointerScreenPosition();
        if (screenPos == Vector2.zero)
        {
            activeIndicator.gameObject.SetActive(false);
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        float surfaceY = tilemapGrid != null ? tilemapGrid.FloorSurfaceY : 0f;
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0f, surfaceY, 0f));

        if (groundPlane.Raycast(ray, out float enterDistance))
        {
            Vector3 worldHitPoint = ray.GetPoint(enterDistance);
            worldHitPoint.y = surfaceY;

            currentHoveredCell = tilemapGrid.WorldToCell(worldHitPoint);

            // Check if cell is within painted tilemap boundaries
            if (tilemapGrid.Tilemap.HasTile(currentHoveredCell))
            {
                activeIndicator.gameObject.SetActive(true);

                // Elevate indicator by +0.02f above tile surface to avoid Z-fighting
                Vector3 indicatorPos = tilemapGrid.GetWorldCenter(currentHoveredCell);
                indicatorPos.y = surfaceY + 0.02f;
                activeIndicator.transform.position = indicatorPos;

                isHoveringValid = tilemapGrid.IsBuildable(currentHoveredCell) && !occupiedCells.Contains(currentHoveredCell);
                activeIndicator.SetStatus(isHoveringValid);
            }
            else
            {
                activeIndicator.gameObject.SetActive(false);
            }
        }
        else
        {
            activeIndicator.gameObject.SetActive(false);
        }
    }

    private bool WasPointerPressed()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            return true;
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private Vector2 GetPointerScreenPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            return Touchscreen.current.primaryTouch.position.ReadValue();
        }

        if (Mouse.current != null)
        {
            return Mouse.current.position.ReadValue();
        }

        return Vector2.zero;
    }

    public bool TryPlaceTurret(Vector3Int cellPos)
    {
        if (!tilemapGrid.IsBuildable(cellPos)) return false;
        if (occupiedCells.Contains(cellPos)) return false;

        Vector3 spawnPosition = tilemapGrid.GetWorldCenter(cellPos);

        GameObject placedTurret = Instantiate(selectedTurretData.prefab, spawnPosition, Quaternion.identity);
        placedTurret.name = $"{selectedTurretData.turretName}_{cellPos.x}_{cellPos.y}";

        occupiedCells.Add(cellPos);

        if (activeIndicator != null)
        {
            activeIndicator.TriggerPlacementJuice();
        }

        Debug.Log($"[BuildManager] Built {selectedTurretData.turretName} at cell {cellPos}!");
        return true;
    }

    public void SelectTurretToBuild(TurretDataSO turretData)
    {
        selectedTurretData = turretData;
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Core.Economy;
using Data.Shop;

public class BuildManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TilemapGrid tilemapGrid;
    [SerializeField] private GameObject placementIndicator;
    [SerializeField] private ShopCatalogSO shopCatalog;

    [Header("Painting Controls")]
    [Tooltip("Hold down left mouse button to paint turrets across tiles")]
    [SerializeField] private bool allowDragPainting = false; // Turned off by default for economy safety

    [Header("Placement Juice")]
    [SerializeField] private bool animateTurretSpawn = true;

    // Track both coordinate occupancy and the active instance + its data for refunds
    private struct PlacedTurretRecord
    {
        public GameObject Instance;
        public TurretDataSO Data;
    }

    private readonly Dictionary<Vector3Int, PlacedTurretRecord> placedTurrets = new Dictionary<Vector3Int, PlacedTurretRecord>();
    private Vector3Int lastPaintedCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
    private Plane groundPlane;
    private PlacementIndicator indicatorComponent;

    // Current State
    private TurretDataSO selectedTurretData;
    private bool isSellModeActive;

    public event Action<TurretDataSO> OnTurretSelected;
    public event Action<bool> OnSellModeChanged;

    private void Start()
    {
        if (tilemapGrid == null)
        {
            tilemapGrid = UnityEngine.Object.FindAnyObjectByType<TilemapGrid>();
        }

        if (placementIndicator != null && (!placementIndicator.scene.IsValid() || !placementIndicator.activeInHierarchy))
        {
            placementIndicator = Instantiate(placementIndicator, Vector3.zero, Quaternion.identity);
            placementIndicator.name = "TilePlacementIndicator (Runtime Instance)";
        }
        else if (placementIndicator == null)
        {
            PlacementIndicator found = UnityEngine.Object.FindAnyObjectByType<PlacementIndicator>();
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

        // Auto-select first turret in catalog as default
        if (shopCatalog != null && shopCatalog.AvailableTurrets.Count > 0)
        {
            SelectTurret(shopCatalog.AvailableTurrets[0]);
        }
    }

    private void Update()
    {
        HandlePlacementInput();
    }

    public void SelectTurret(TurretDataSO data)
    {
        isSellModeActive = false;
        selectedTurretData = data;
        OnSellModeChanged?.Invoke(false);
        OnTurretSelected?.Invoke(data);
    }

    public void ToggleSellMode()
    {
        isSellModeActive = !isSellModeActive;
        if (isSellModeActive)
        {
            selectedTurretData = null;
            OnTurretSelected?.Invoke(null);
        }
        OnSellModeChanged?.Invoke(isSellModeActive);
    }

    private void HandlePlacementInput()
    {
        var mouse = Mouse.current;
        if (mouse == null || Camera.main == null) return;

        // Block input if hovering UI
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
        bool isOccupied = placedTurrets.ContainsKey(cellPos);

        // Indicator Validity Status
        bool isValidAction = false;
        if (isSellModeActive)
        {
            isValidAction = isOccupied; // Valid to sell if tile has a turret
        }
        else if (selectedTurretData != null)
        {
            bool canAfford = EconomyManager.Instance == null || EconomyManager.Instance.CanAfford(selectedTurretData.cost);
            isValidAction = isBuildable && !isOccupied && canAfford;
        }

        if (placementIndicator != null && indicatorComponent != null)
        {
            indicatorComponent.SetVisible(true);
            indicatorComponent.SetTargetPosition(cellCenter);
            indicatorComponent.SetStatus(isValidAction);
        }

        // Action Trigger
        bool isTriggered = allowDragPainting && !isSellModeActive 
            ? mouse.leftButton.isPressed 
            : mouse.leftButton.wasPressedThisFrame;

        if (isTriggered)
        {
            if (cellPos != lastPaintedCell)
            {
                if (isSellModeActive)
                {
                    if (isOccupied) SellTurret(cellPos);
                }
                else if (isValidAction)
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
        if (selectedTurretData == null || selectedTurretData.turretPrefab == null) return;

        // 1. Transaction verification
        if (EconomyManager.Instance != null)
        {
            if (!EconomyManager.Instance.TrySpend(selectedTurretData.cost)) return;
        }

        // 2. Instantiate and register
        GameObject turretObj = Instantiate(selectedTurretData.turretPrefab, worldPos, Quaternion.identity);
        placedTurrets.Add(cellPos, new PlacedTurretRecord
        {
            Instance = turretObj,
            Data = selectedTurretData
        });

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

    private void SellTurret(Vector3Int cellPos)
    {
        if (!placedTurrets.TryGetValue(cellPos, out PlacedTurretRecord record)) return;

        // 1. Refund Coins
        if (EconomyManager.Instance != null && record.Data != null)
        {
            int refund = record.Data.CalculateRefundAmount();
            EconomyManager.Instance.Add(refund);
        }

        // 2. Cleanup Object
        if (record.Instance != null)
        {
            Destroy(record.Instance);
        }

        placedTurrets.Remove(cellPos);

        if (indicatorComponent != null)
        {
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
            float curve = Mathf.Sin(t * Mathf.PI * 0.5f);
            target.localScale = Vector3.Lerp(target.localScale, endScale, curve);
            yield return null;
        }

        if (target != null) target.localScale = endScale;
    }
}
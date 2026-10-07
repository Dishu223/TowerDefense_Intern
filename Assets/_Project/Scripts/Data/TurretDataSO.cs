using UnityEngine;

[CreateAssetMenu(fileName = "TurretData_", menuName = "Tower Defense/Turrets/Turret Data")]
public class TurretDataSO : ScriptableObject
{
    [Header("Display Info")]
    public string turretId = "gatling";
    public string turretName = "Gatling Cannon";
    public Sprite icon;

    [Header("Visual Prefab")]
    public GameObject prefab;

    [Header("Combat Stats")]
    public float range = 5f;
    public float fireRate = 5f;
    public int damage = 25;

    [Header("Ammunition")]
    public GameObject projectilePrefab;

    [Header("Economy")]
    public int buildCost = 100;
    [Range(0f, 1f)] public float sellRefundRatio = 0.7f;

    // Helper properties so both old and new scripts work smoothly
    public string displayName => turretName;
    public int cost => buildCost;
    public GameObject turretPrefab => prefab;

    public int CalculateRefundAmount()
    {
        return Mathf.RoundToInt(buildCost * sellRefundRatio);
    }
}
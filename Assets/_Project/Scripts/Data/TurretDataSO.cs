using UnityEngine;

[CreateAssetMenu(fileName = "NewTurretData", menuName = "Tower Defense/Turret Data")]
public class TurretDataSO : ScriptableObject
{
    [Header("Display Info")]
    public string turretName = "Standard Turret";

    [Header("Visual Prefab")]
    public GameObject prefab;

    [Header("Combat Stats")]
    public float range = 4f;
    public float fireRate = 2f; // Shots per second
    public int damage = 25;

    [Header("Ammunition")]
    public Projectile projectilePrefab;

    [Header("Economy")]
    public int buildCost = 100;
}
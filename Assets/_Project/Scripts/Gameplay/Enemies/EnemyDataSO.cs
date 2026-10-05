using UnityEngine;

namespace Gameplay.Enemies
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "Tower Defense/Enemies/Enemy Data")]
    public class EnemyDataSO : ScriptableObject
    {
        [Header("Identity")]
        public string enemyId;
        public string displayName = "Standard Drone";

        [Header("Attributes")]
        public int maxHealth = 100;
        public float moveSpeed = 3.5f;

        [Header("Economy Rewards")]
        public int bountyReward = 15;

        [Header("Visual Feedback")]
        public Color hitFlashColor = Color.white;
        public Vector3 hitSquashScale = new Vector3(1.25f, 0.75f, 1.25f);
    }
}
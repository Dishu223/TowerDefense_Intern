using System.Collections.Generic;
using UnityEngine;

namespace Core.Levels
{
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Tower Defense/Levels/Level Catalog")]
    public class LevelCatalogSO : ScriptableObject
    {
        [Header("Scene Configuration")]
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private List<string> levelSceneNames = new List<string>();

        public string MainMenuSceneName => mainMenuSceneName;
        public IReadOnlyList<string> LevelSceneNames => levelSceneNames;

        public bool HasNextLevel(int currentLevelIndex)
        {
            return currentLevelIndex + 1 < levelSceneNames.Count;
        }

        public string GetLevelScene(int levelIndex)
        {
            if (levelIndex >= 0 && levelIndex < levelSceneNames.Count)
            {
                return levelSceneNames[levelIndex];
            }
            return string.Empty;
        }
    }
}
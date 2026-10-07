using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Levels
{
    public class LevelManager : MonoBehaviour, ILevelService
    {
        [SerializeField] private LevelCatalogSO levelCatalog;

        public static LevelManager Instance { get; private set; }

        public int CurrentLevelIndex { get; private set; } = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void LoadFirstLevel()
        {
            CurrentLevelIndex = 0;
            LoadScene(levelCatalog.GetLevelScene(0));
        }

        public void LoadNextLevel()
        {
            if (levelCatalog != null && levelCatalog.HasNextLevel(CurrentLevelIndex))
            {
                CurrentLevelIndex++;
                LoadScene(levelCatalog.GetLevelScene(CurrentLevelIndex));
            }
            else
            {
                ReturnToMainMenu();
            }
        }

        public void RestartFromLevelOne()
        {
            LoadFirstLevel();
        }

        public void ReloadCurrentLevel()
        {
            if (levelCatalog != null)
            {
                LoadScene(levelCatalog.GetLevelScene(CurrentLevelIndex));
            }
        }

        public void ReturnToMainMenu()
        {
            if (levelCatalog != null)
            {
                LoadScene(levelCatalog.MainMenuSceneName);
            }
        }

        private void LoadScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            Time.timeScale = 1f; // Ensure time is moving when entering the new scene
            SceneManager.LoadScene(sceneName);
        }
    }
}
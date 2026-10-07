using UnityEngine;
using UnityEngine.UI;
using Core.GameFlow;
using Core.Levels;

namespace UI.Panels
{
    public class GameOverPanelUI : MonoBehaviour
    {
        [Header("UI Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Buttons")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (restartButton != null)
            {
                restartButton.onClick.RemoveAllListeners();
                restartButton.onClick.AddListener(() => LevelManager.Instance?.RestartFromLevelOne());
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners();
                mainMenuButton.onClick.AddListener(() => LevelManager.Instance?.ReturnToMainMenu());
            }

            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnStateChanged += HandleStateChanged;
            }
        }

        private void OnDestroy()
        {
            if (GameFlowManager.Instance != null)
            {
                GameFlowManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            if (panelRoot != null)
            {
                bool isGameOver = (state == GameState.GameOver);
                panelRoot.SetActive(isGameOver);

                if (isGameOver)
                {
                    panelRoot.transform.SetAsLastSibling(); // Pop over all UI
                }
            }
        }
    }
}
using UnityEngine;
using UnityEngine.UI;
using Core.GameFlow;
using Core.Levels;

namespace UI.Panels
{
    public class PausePanelUI : MonoBehaviour
    {
        [Header("UI Root")]
        [SerializeField] private GameObject panelRoot;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (panelRoot != null) panelRoot.SetActive(false);

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(() => GameFlowManager.Instance?.Resume());
            }

            if (mainMenuButton != null)
            {
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
                panelRoot.SetActive(state == GameState.Paused);
            }
        }
    }
}
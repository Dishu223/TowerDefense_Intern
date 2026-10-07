using UnityEngine;
using UnityEngine.UI;
using Core.Levels;

namespace UI.MainMenu
{
    public class MainMenuUIController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(() =>
                {
                    if (LevelManager.Instance != null)
                    {
                        LevelManager.Instance.LoadFirstLevel();
                    }
                });
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(() =>
                {
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#else
                    Application.Quit();
#endif
                });
            }
        }
    }
}
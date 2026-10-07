using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Gameplay.Base;

namespace Core.GameFlow
{
    public class GameFlowManager : MonoBehaviour, IGameStateService
    {
        public static GameFlowManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Playing;
        public event Action<GameState> OnStateChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Time.timeScale = 1f;
        }

        private void Start()
        {
            Time.timeScale = 1f;
            SetState(GameState.Playing);

            // Hook into all active BaseCores in the scene
            BaseCore.OnAnyBaseDestroyed += HandleAnyBaseDestroyed;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            BaseCore.OnAnyBaseDestroyed -= HandleAnyBaseDestroyed;
            Time.timeScale = 1f;
        }

        private void Update()
        {
            bool escapePressed = false;

            if (Keyboard.current != null)
            {
                escapePressed = Keyboard.current.escapeKey.wasPressedThisFrame;
            }
            else
            {
                escapePressed = Input.GetKeyDown(KeyCode.Escape);
            }

            if (escapePressed)
            {
                TogglePause();
            }
        }

        public void Pause()
        {
            if (CurrentState != GameState.Playing) return;

            Time.timeScale = 0f;
            SetState(GameState.Paused);
        }

        public void Resume()
        {
            if (CurrentState != GameState.Paused) return;

            Time.timeScale = 1f;
            SetState(GameState.Playing);
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing) Pause();
            else if (CurrentState == GameState.Paused) Resume();
        }

        private void HandleAnyBaseDestroyed(BaseCore baseCore)
        {
            TriggerGameOver();
        }

        public void TriggerGameOver()
        {
            if (CurrentState == GameState.GameOver) return;

            Time.timeScale = 0f;
            SetState(GameState.GameOver);
        }

        public void TriggerVictory()
        {
            if (CurrentState == GameState.Victory || CurrentState == GameState.GameOver) return;

            Time.timeScale = 0f;
            SetState(GameState.Victory);
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
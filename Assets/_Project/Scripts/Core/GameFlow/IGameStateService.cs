using System;

namespace Core.GameFlow
{
    public interface IGameStateService
    {
        GameState CurrentState { get; }
        event Action<GameState> OnStateChanged;

        void Pause();
        void Resume();
        void TriggerGameOver();
        void TriggerVictory();
    }
}
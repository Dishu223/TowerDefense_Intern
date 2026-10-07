namespace Core.Levels
{
    public interface ILevelService
    {
        int CurrentLevelIndex { get; }
        void LoadFirstLevel();
        void LoadNextLevel();
        void RestartFromLevelOne();
        void ReloadCurrentLevel();
        void ReturnToMainMenu();
    }
}
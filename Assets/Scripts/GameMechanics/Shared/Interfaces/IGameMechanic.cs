using UnityEngine;

namespace MechanicGames.Shared
{
    public interface IGameMechanic
    {
        // Game state
        bool IsGameActive { get; }
        bool IsPaused { get; }
        int CurrentScore { get; }
        int HighScore { get; }
        
        // Game control
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void EndGame();
        void ResetGame();
        
        // Score management
        void UpdateScore(int newScore);
        void AddScore(int points);
        
        // Events
        System.Action OnGameStart { get; set; }
        System.Action OnGamePause { get; set; }
        System.Action OnGameResume { get; set; }
        System.Action OnGameOver { get; set; }
        System.Action<int> OnScoreChanged { get; set; }
    }
    
    public interface IGameMechanicConfig
    {
        // Configuration properties
        bool IsMobileBuild { get; }
        float TargetFrameRate { get; }
        bool EnableVSync { get; }
        float MobileEffectScale { get; }
        int MaxParticles { get; }
    }
    
    public interface IGameMechanicUI
    {
        // UI management
        void ShowMainMenu();
        void ShowGamePanel();
        void ShowPausePanel();
        void ShowGameOverPanel();
        void ShowSettingsPanel();
        
        // UI updates
        void UpdateScoreDisplay(int score);
        void UpdateHighScoreDisplay(int highScore);
        void UpdateLevelDisplay(int level);
        void UpdateProgressBar(float progress);
    }
}


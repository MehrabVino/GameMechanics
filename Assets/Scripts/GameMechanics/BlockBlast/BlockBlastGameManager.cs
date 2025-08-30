using UnityEngine;
using UnityEngine.Events;

namespace MechanicGames.BlockBlast
{
    public class BlockBlastGameManager : MonoBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private BlockBlastConfig gameConfig;
        
        [Header("System References")]
        [SerializeField] private BlockBlastMechanic gameMechanic;
        [SerializeField] private BlockBlastGameUI gameUI;
        [SerializeField] private BlockBlastEffects effects;
        [SerializeField] private UIResponsiveLayout responsiveLayout;
        
        [Header("Events")]
        [SerializeField] private UnityEvent onGameStart;
        [SerializeField] private UnityEvent onGamePause;
        [SerializeField] private UnityEvent onGameResume;
        [SerializeField] private UnityEvent onGameOver;
        [SerializeField] private UnityEvent onScoreChanged;
        [SerializeField] private UnityEvent onComboChanged;
        
        // Game state
        private bool isGameActive = false;
        private bool isPaused = false;
        private int currentScore = 0;
        private int highScore = 0;
        
        // Performance monitoring
        private float lastFrameTime = 0f;
        private float averageFrameTime = 0f;
        private int frameCount = 0;
        
        public static BlockBlastGameManager Instance { get; private set; }
        public BlockBlastConfig Config => gameConfig;
        public bool IsGameActive => isGameActive;
        public bool IsPaused => isPaused;
        public int CurrentScore => currentScore;
        public int HighScore => highScore;
        public float AverageFrameTime => averageFrameTime;
        
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeGameManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            SetupPerformanceSettings();
            LoadGameData();
            SetupSystemConnections();
        }
        
        void Update()
        {
            UpdatePerformanceMonitoring();
            HandleGlobalInput();
        }
        
        void InitializeGameManager()
        {
            // Create default config if none assigned
            if (gameConfig == null)
            {
                gameConfig = ScriptableObject.CreateInstance<BlockBlastConfig>();
            }
            
            // Auto-detect mobile platform
            #if UNITY_ANDROID || UNITY_IOS
            if (gameConfig != null)
            {
                // Force mobile settings
                // Note: This would require making the config editable at runtime
            }
            #endif
        }
        
        void SetupPerformanceSettings()
        {
            if (gameConfig == null) return;
            
            Application.targetFrameRate = Mathf.RoundToInt(gameConfig.TargetFrameRate);
            QualitySettings.vSyncCount = gameConfig.EnableVSync ? 1 : 0;
            
            // Mobile-specific optimizations
            if (gameConfig.IsMobileBuild)
            {
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.antiAliasing = 0;
                QualitySettings.softParticles = false;
                QualitySettings.realtimeReflectionProbes = false;
            }
        }
        
        void LoadGameData()
        {
            highScore = PlayerPrefs.GetInt("BlockBlast_HighScore", 0);
        }
        
        void SetupSystemConnections()
        {
            if (gameMechanic != null)
            {
                gameMechanic.SetGameManager(this);
            }
            
            if (gameUI != null)
            {
                gameUI.SetGameManager(this);
            }
            
            if (effects != null)
            {
                effects.SetGameManager(this);
            }
        }
        
        void UpdatePerformanceMonitoring()
        {
            frameCount++;
            float currentFrameTime = Time.unscaledDeltaTime;
            averageFrameTime = Mathf.Lerp(averageFrameTime, currentFrameTime, 0.1f);
            lastFrameTime = currentFrameTime;
            
            // Auto-adjust quality if performance is poor
            if (gameConfig.IsMobileBuild && averageFrameTime > 0.033f) // Below 30 FPS
            {
                // Could implement dynamic quality adjustment here
            }
        }
        
        void HandleGlobalInput()
        {
            // Global pause/resume
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isGameActive)
                {
                    TogglePause();
                }
            }
            
            // Performance monitoring
            if (Input.GetKeyDown(KeyCode.F1))
            {
                Debug.Log($"Performance: Avg Frame Time: {averageFrameTime:F4}s, FPS: {1f/averageFrameTime:F1}");
            }
        }
        
        // Public API
        public void StartGame()
        {
            isGameActive = true;
            isPaused = false;
            currentScore = 0;
            
            if (gameMechanic != null)
            {
                gameMechanic.ResetGame();
            }
            
            onGameStart?.Invoke();
        }
        
        public void PauseGame()
        {
            if (!isGameActive) return;
            
            isPaused = true;
            Time.timeScale = 0f;
            onGamePause?.Invoke();
        }
        
        public void ResumeGame()
        {
            if (!isGameActive) return;
            
            isPaused = false;
            Time.timeScale = 1f;
            onGameResume?.Invoke();
        }
        
        public void EndGame()
        {
            isGameActive = false;
            isPaused = false;
            Time.timeScale = 1f;
            
            // Check for new high score
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("BlockBlast_HighScore", highScore);
                PlayerPrefs.Save();
            }
            
            onGameOver?.Invoke();
        }
        
        public void UpdateScore(int newScore)
        {
            currentScore = newScore;
            onScoreChanged?.Invoke();
        }
        
        public void UpdateCombo(int comboCount, float multiplier)
        {
            onComboChanged?.Invoke();
        }
        
        public void TogglePause()
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
        
        // Performance API
        public float GetPerformanceRating()
        {
            if (gameConfig == null) return 1f;
            
            float targetFrameTime = 1f / gameConfig.TargetFrameRate;
            return Mathf.Clamp01(targetFrameTime / averageFrameTime);
        }
        
        public bool ShouldReduceEffects()
        {
            return gameConfig.IsMobileBuild && GetPerformanceRating() < 0.7f;
        }
        
        // Configuration API
        public void SetMobileMode(bool isMobile)
        {
            if (gameConfig != null)
            {
                // This would require making the config editable at runtime
                // For now, we'll just log the change
                Debug.Log($"Mobile mode set to: {isMobile}");
            }
        }
        
        void OnApplicationPause(bool pauseStatus)
        {
            if (isGameActive && !isPaused)
            {
                PauseGame();
            }
        }
        
        void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && isGameActive && !isPaused)
            {
                PauseGame();
            }
        }
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MechanicGames.BlockBlast
{
    public class BlockBlastGameUI : MonoBehaviour
    {
        [Header("UI Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject gamePanel;
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private GameObject settingsPanel;
        
        [Header("Main Menu")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI titleText;
        
        [Header("Game Panel")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI movesText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI comboMultiplierText;
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button menuButton;
        [SerializeField] private Slider progressBar;
        
        [Header("Next Tiles Display")]
        [SerializeField] private Transform nextTilesContainer;
        [SerializeField] private GameObject nextTilePrefab;
        [SerializeField] private TextMeshProUGUI nextTilesLabel;
        
        [Header("Game Over Panel")]
        [SerializeField] private TextMeshProUGUI finalScoreText;
        [SerializeField] private TextMeshProUGUI newHighScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        
        [Header("Pause Panel")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button pauseSettingsButton;
        [SerializeField] private Button pauseMainMenuButton;
        
        [Header("Settings Panel")]
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [SerializeField] private Toggle gridLinesToggle;
        [SerializeField] private Toggle particlesToggle;
        [SerializeField] private Toggle screenShakeToggle;
        [SerializeField] private Button settingsBackButton;
        
        [Header("Game Reference")]
        [SerializeField] private BlockBlastMechanic gameMechanic;
        
        [Header("UI Animation")]
        [SerializeField] private float animationDuration = 0.3f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private CanvasGroup[] panelGroups;
        private int currentScore = 0;
        private int highScore = 0;
        private int currentLevel = 1;
        private int currentMoves = 0;
        private bool isPaused = false;
        
        void Start()
        {
            InitializeUI();
            SetupButtonListeners();
            LoadSettings();
            ShowMainMenu();
        }
        
        void Update()
        {
            if (gameMechanic != null && gamePanel.activeInHierarchy)
            {
                UpdateGameUI();
            }
        }
        
        void InitializeUI()
        {
            // Get all panel canvas groups for animations
            panelGroups = new CanvasGroup[]
            {
                GetOrAddCanvasGroup(mainMenuPanel),
                GetOrAddCanvasGroup(gamePanel),
                GetOrAddCanvasGroup(pausePanel),
                GetOrAddCanvasGroup(gameOverPanel),
                GetOrAddCanvasGroup(settingsPanel)
            };
            
            // Load high score
            highScore = PlayerPrefs.GetInt("BlockBlast_HighScore", 0);
            UpdateHighScoreDisplay();
        }
        
        CanvasGroup GetOrAddCanvasGroup(GameObject panel)
        {
            if (panel == null) return null;
            
            CanvasGroup group = panel.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = panel.AddComponent<CanvasGroup>();
            }
            return group;
        }
        
        void SetupButtonListeners()
        {
            // Main Menu
            if (playButton != null) playButton.onClick.AddListener(StartGame);
            if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettings);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
            
            // Game Panel
            if (pauseButton != null) pauseButton.onClick.AddListener(PauseGame);
            if (menuButton != null) menuButton.onClick.AddListener(ReturnToMainMenu);
            
            // Pause Panel
            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (pauseSettingsButton != null) pauseSettingsButton.onClick.AddListener(ShowSettings);
            if (pauseMainMenuButton != null) pauseMainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
            // Game Over Panel
            if (restartButton != null) restartButton.onClick.AddListener(RestartGame);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(ReturnToMainMenu);
            
            // Settings Panel
            if (settingsBackButton != null) settingsBackButton.onClick.AddListener(CloseSettings);
            if (gridLinesToggle != null) gridLinesToggle.onValueChanged.AddListener(OnGridLinesToggled);
            if (particlesToggle != null) particlesToggle.onValueChanged.AddListener(OnParticlesToggled);
            if (screenShakeToggle != null) screenShakeToggle.onValueChanged.AddListener(OnScreenShakeToggled);
        }
        
        void LoadSettings()
        {
            if (gridLinesToggle != null)
                gridLinesToggle.isOn = PlayerPrefs.GetInt("GridLines", 1) == 1;
            if (particlesToggle != null)
                particlesToggle.isOn = PlayerPrefs.GetInt("Particles", 1) == 1;
            if (screenShakeToggle != null)
                screenShakeToggle.isOn = PlayerPrefs.GetInt("ScreenShake", 1) == 1;
            if (musicVolumeSlider != null)
                musicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            if (sfxVolumeSlider != null)
                sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        }
        
        void SaveSettings()
        {
            PlayerPrefs.SetInt("GridLines", gridLinesToggle != null && gridLinesToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt("Particles", particlesToggle != null && particlesToggle.isOn ? 1 : 0);
            PlayerPrefs.SetInt("ScreenShake", screenShakeToggle != null && screenShakeToggle.isOn ? 1 : 0);
            PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider != null ? musicVolumeSlider.value : 0.7f);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolumeSlider != null ? sfxVolumeSlider.value : 0.8f);
            PlayerPrefs.Save();
        }
        
        void UpdateGameUI()
        {
            if (gameMechanic == null) return;
            
            int newScore = gameMechanic.Score;
            if (newScore != currentScore)
            {
                currentScore = newScore;
                UpdateScoreDisplay();
                UpdateLevelDisplay();
                UpdateProgressBar();
            }
            
            // Update combo display
            UpdateComboDisplay();
        }
        
        void UpdateScoreDisplay()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore:N0}";
            }
            
            // Check for new high score
            if (currentScore > highScore)
            {
                highScore = currentScore;
                PlayerPrefs.SetInt("BlockBlast_HighScore", highScore);
                PlayerPrefs.Save();
                UpdateHighScoreDisplay();
            }
        }
        
        void UpdateHighScoreDisplay()
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"High Score: {highScore:N0}";
            }
        }
        
        void UpdateLevelDisplay()
        {
            int newLevel = (currentScore / 1000) + 1;
            if (newLevel != currentLevel)
            {
                currentLevel = newLevel;
                if (levelText != null)
                {
                    levelText.text = $"Level: {currentLevel}";
                }
            }
        }
        
        void UpdateProgressBar()
        {
            if (progressBar != null)
            {
                float progress = (float)(currentScore % 1000) / 1000f;
                progressBar.value = progress;
            }
        }
        
        void UpdateComboDisplay()
        {
            if (gameMechanic == null) return;
            
            if (comboText != null)
            {
                int comboCount = gameMechanic.ComboCount;
                if (comboCount > 0)
                {
                    comboText.text = $"Combo: {comboCount}x";
                    comboText.gameObject.SetActive(true);
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }
            
            if (comboMultiplierText != null)
            {
                float multiplier = gameMechanic.ComboMultiplier;
                if (multiplier > 1f)
                {
                    comboMultiplierText.text = $"×{multiplier:F1}";
                    comboMultiplierText.gameObject.SetActive(true);
                }
                else
                {
                    comboMultiplierText.gameObject.SetActive(false);
                }
            }
        }
        
        public void AddMove()
        {
            currentMoves++;
            if (movesText != null)
            {
                movesText.text = $"Moves: {currentMoves}";
            }
        }
        
        void ShowMainMenu()
        {
            SetActivePanel(mainMenuPanel);
            ResetGameState();
        }
        
        void StartGame()
        {
            SetActivePanel(gamePanel);
            ResetGameState();
            
            if (gameMechanic != null)
            {
                gameMechanic.ResetGame();
            }
        }
        
        void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            SetActivePanel(pausePanel);
        }
        
        void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            SetActivePanel(gamePanel);
        }
        
        void ShowSettings()
        {
            SetActivePanel(settingsPanel);
        }
        
        void CloseSettings()
        {
            SaveSettings();
            if (isPaused)
            {
                SetActivePanel(pausePanel);
            }
            else
            {
                SetActivePanel(gamePanel);
            }
        }
        
        void ShowGameOver()
        {
            SetActivePanel(gameOverPanel);
            
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {currentScore:N0}";
            }
            
            if (newHighScoreText != null)
            {
                newHighScoreText.text = currentScore >= highScore ? "NEW HIGH SCORE!" : "";
            }
        }
        
        void RestartGame()
        {
            StartGame();
        }
        
        void ReturnToMainMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;
            ShowMainMenu();
        }
        
        void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
        
        void ResetGameState()
        {
            currentScore = 0;
            currentLevel = 1;
            currentMoves = 0;
            UpdateScoreDisplay();
            UpdateLevelDisplay();
            if (movesText != null) movesText.text = "Moves: 0";
            if (progressBar != null) progressBar.value = 0f;
        }
        
        void SetActivePanel(GameObject activePanel)
        {
            // Hide all panels
            mainMenuPanel.SetActive(false);
            gamePanel.SetActive(false);
            pausePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            settingsPanel.SetActive(false);
            
            // Show active panel
            activePanel.SetActive(true);
            
            // Animate panel appearance
            CanvasGroup activeGroup = activePanel.GetComponent<CanvasGroup>();
            if (activeGroup != null)
            {
                StartCoroutine(AnimatePanelFade(activeGroup, 0f, 1f));
            }
        }
        
        System.Collections.IEnumerator AnimatePanelFade(CanvasGroup group, float fromAlpha, float toAlpha)
        {
            float elapsed = 0f;
            group.alpha = fromAlpha;
            
            while (elapsed < animationDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / animationDuration;
                float curveValue = animationCurve.Evaluate(progress);
                group.alpha = Mathf.Lerp(fromAlpha, toAlpha, curveValue);
                yield return null;
            }
            
            group.alpha = toAlpha;
        }
        
        // Settings callbacks
        void OnGridLinesToggled(bool isOn)
        {
            if (gameMechanic != null)
            {
                // You can add a method to toggle grid lines in the mechanic
                // gameMechanic.SetGridLinesVisible(isOn);
            }
        }
        
        void OnParticlesToggled(bool isOn)
        {
            // Toggle particle effects based on isOn value
        }
        
        void OnScreenShakeToggled(bool isOn)
        {
            // Toggle screen shake effects based on isOn value
        }
        
        // Public method to be called from game mechanic when game ends
        public void OnGameOver()
        {
            ShowGameOver();
        }
        
        // Method to set the game mechanic reference
        public void SetGameMechanic(BlockBlastMechanic mechanic)
        {
            gameMechanic = mechanic;
        }
        
        // Method to set the game manager reference
        public void SetGameManager(BlockBlastGameManager manager)
        {
            // Store reference to game manager if needed for future use
            // Currently not used but added for interface consistency
        }
    }
}

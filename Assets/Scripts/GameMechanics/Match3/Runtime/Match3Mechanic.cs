using MechanicGames.Shared;
using UnityEngine;
using System.Collections;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Main controller for the Match3 game mechanic, implementing IGameMechanic and IMatch3Context.
    /// </summary>
    public sealed class Match3Mechanic : MonoBehaviour, IGameMechanic, IMatch3Context
    {
        [Header("Board Configuration")]
        [SerializeField] private Match3Board board = new Match3Board();
        [SerializeField] private float cellSize = 1.0f;
        [SerializeField] private Vector2 boardOriginLocal = Vector2.zero;

        [Header("Game Settings")]
        [SerializeField] private int scorePerTile = 10;
        [SerializeField] private int bonusPerChain = 5;
        [SerializeField] private bool drawGizmos = true;

        [Header("Visual Theme")]
        [SerializeField] private Match3Theme theme;

        [Header("Game Systems")]
        [SerializeField] private Match3PowerUp powerUpSystem;
        [SerializeField] private Match3Objective objectiveSystem;
        [SerializeField] private Match3AudioManager audioManager;

        // Game state
        private bool isActive;
        private bool hasSelection;
        private Vector2Int selectedCell;
        private int currentScore;

        // Events
        public System.Action OnGameStart { get; set; }
        public System.Action OnGamePause { get; set; }
        public System.Action OnGameResume { get; set; }
        public System.Action OnGameOver { get; set; }
        public System.Action<int> OnScoreChanged { get; set; }

        // IGameMechanic implementation
        public bool IsGameActive => isActive;
        public bool IsPaused => false; // Match3 doesn't support pause
        public int CurrentScore => currentScore;
        public int Score => currentScore; // Alias for compatibility
        public int HighScore => PlayerPrefs.GetInt("Match3_HighScore", 0);

        // IMatch3Context implementation
        public IMatch3BoardReadOnly Board => board;
        public int BoardWidth => board?.Width ?? 0;
        public int BoardHeight => board?.Height ?? 0;
        public float CellSize => cellSize;
        public Vector3 BoardOriginWorld => transform.TransformPoint(new Vector3(boardOriginLocal.x, boardOriginLocal.y, 0f));
        public Match3Theme Theme => theme;
        public System.Collections.Generic.IReadOnlyList<Vector2Int> ClearedCells => board?.LastClearedCells;
        public System.Collections.Generic.IReadOnlyList<Vector2Int> SpawnedCells => board?.LastSpawnedCells;
        public System.Collections.Generic.IReadOnlyList<Vector2Int> SpecialTilesCreated => board?.SpecialTilesCreated;

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeMechanic();
            InitializeGameSystems();
        }

        private void Start()
        {
            // Start the game automatically
            StartGame();
        }

        private void Update()
        {
            if (!isActive) return;
            HandleInput();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the game mechanic and board.
        /// </summary>
        public void InitializeMechanic()
        {
            if (board == null)
            {
                board = new Match3Board();
            }

            // Set theme on board if available
            if (theme != null)
            {
                board.SetTheme(theme);
                Debug.Log($"Match3Mechanic: Applied theme '{theme.name}' to board");
            }
            else
            {
                board.SetTileTypeCount(6); // Default fallback
                Debug.Log("Match3Mechanic: No theme assigned, using default tile types");
            }

            board.Initialize();
            currentScore = 0;

            Debug.Log($"Match3Mechanic: Initialized with board {BoardWidth}x{BoardHeight}");
        }

        /// <summary>
        /// Set the visual theme for the game.
        /// </summary>
        public void SetTheme(Match3Theme newTheme)
        {
            Debug.Log($"Match3Mechanic: Setting theme to {(newTheme != null ? newTheme.name : "null")}");
            theme = newTheme;
            
            if (board != null)
            {
                board.SetTheme(theme);
                board.Initialize();
            }

            // Notify the view to refresh
            var view = GetComponentInChildren<Match3RuntimeView>();
            if (view != null)
            {
                view.RefreshFromMechanic();
                // Also force a theme refresh for existing tiles
                view.ForceThemeRefresh();
            }
        }

        /// <summary>
        /// Force refresh the visual theme for all tiles (useful for debugging).
        /// </summary>
        [ContextMenu("Refresh Theme")]
        public void RefreshTheme()
        {
            var view = GetComponentInChildren<Match3RuntimeView>();
            if (view != null)
            {
                view.ForceThemeRefresh();
            }
        }

        /// <summary>
        /// Initialize game systems (power-ups, objectives, audio).
        /// </summary>
        private void InitializeGameSystems()
        {
            // Initialize power-up system
            if (powerUpSystem == null)
            {
                powerUpSystem = GetComponent<Match3PowerUp>();
            }

            // Initialize objective system
            if (objectiveSystem == null)
            {
                objectiveSystem = GetComponent<Match3Objective>();
            }

            // Initialize audio manager
            if (audioManager == null)
            {
                audioManager = FindObjectOfType<Match3AudioManager>();
            }

            // Bind events
            BindGameSystemEvents();
        }

        /// <summary>
        /// Bind events between game systems.
        /// </summary>
        private void BindGameSystemEvents()
        {
            // Power-up events
            if (powerUpSystem != null)
            {
                powerUpSystem.OnPowerUpUsed += OnPowerUpUsed;
            }

            // Objective events
            if (objectiveSystem != null)
            {
                objectiveSystem.OnObjectiveCompleted += OnObjectiveCompleted;
                objectiveSystem.OnAllObjectivesCompleted += OnAllObjectivesCompleted;
                objectiveSystem.OnGameOver += OnObjectiveGameOver;
            }
        }

        #endregion

        #region Game Control

        /// <summary>
        /// Start the game.
        /// </summary>
        public void StartGame()
        {
            if (board == null)
            {
                InitializeMechanic();
            }

            isActive = true;
            enabled = true;
            currentScore = 0;
            OnGameStart?.Invoke();

            Debug.Log("Match3Mechanic: Game started");
        }

        /// <summary>
        /// Pause the game (not supported in Match3, but required by interface).
        /// </summary>
        public void PauseGame()
        {
            OnGamePause?.Invoke();
        }

        /// <summary>
        /// Resume the game (not supported in Match3, but required by interface).
        /// </summary>
        public void ResumeGame()
        {
            OnGameResume?.Invoke();
        }

        /// <summary>
        /// End the game and save high score.
        /// </summary>
        public void EndGame()
        {
            isActive = false;
            enabled = false;

            if (currentScore > HighScore)
            {
                PlayerPrefs.SetInt("Match3_HighScore", currentScore);
                PlayerPrefs.Save();
            }

            OnGameOver?.Invoke();
            Debug.Log($"Match3Mechanic: Game ended with score {currentScore}");
        }

        /// <summary>
        /// Reset the game to initial state.
        /// </summary>
        public void ResetGame()
        {
            currentScore = 0;
            if (board != null)
            {
                board.Initialize();
            }
            Debug.Log("Match3Mechanic: Game reset");
        }

        #endregion

        #region Scoring

        /// <summary>
        /// Add points to the current score.
        /// </summary>
        public void AddScore(int points)
        {
            currentScore += points;
            OnScoreChanged?.Invoke(currentScore);
        }

        /// <summary>
        /// Update the score to a specific value.
        /// </summary>
        public void UpdateScore(int newScore)
        {
            currentScore = newScore;
            OnScoreChanged?.Invoke(currentScore);
        }

        #endregion

        #region Input Handling

        /// <summary>
        /// Handle mouse input for tile selection and swapping.
        /// </summary>
        private void HandleInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (TryGetCellUnderMouse(out Vector2Int cell))
                {
                    hasSelection = true;
                    selectedCell = cell;
                }
            }

            if (hasSelection && Input.GetMouseButtonUp(0))
            {
                if (TryGetCellUnderMouse(out Vector2Int releaseCell))
                {
                    if (board != null && board.AreAdjacent(selectedCell, releaseCell))
                    {
                        StartCoroutine(HandleSwapAndResolve(selectedCell, releaseCell));
                    }
                }
                hasSelection = false;
            }
        }

        /// <summary>
        /// Try to get the cell position under the mouse cursor.
        /// </summary>
        private bool TryGetCellUnderMouse(out Vector2Int cell)
        {
            cell = default;

            if (board == null) return false;

            Camera camera = Camera.main;
            if (camera == null) return false;

            Vector3 worldPos = camera.ScreenToWorldPoint(Input.mousePosition);
            Vector3 localPos = transform.InverseTransformPoint(worldPos);
            Vector2 start = boardOriginLocal;

            float xLocal = localPos.x - start.x;
            float yLocal = localPos.y - start.y;

            if (xLocal < 0f || yLocal < 0f) return false;

            int x = Mathf.FloorToInt(xLocal / cellSize);
            int y = Mathf.FloorToInt(yLocal / cellSize);

            if (x < 0 || y < 0 || x >= board.Width || y >= board.Height) return false;

            cell = new Vector2Int(x, y);
            return true;
        }

        #endregion

        #region Game Logic

        /// <summary>
        /// Handle the tile swap and resolve any resulting matches.
        /// </summary>
        private IEnumerator HandleSwapAndResolve(Vector2Int a, Vector2Int b)
        {
            if (board == null) yield break;

            // Use a move
            if (objectiveSystem != null && !objectiveSystem.UseMove())
            {
                yield break; // No moves remaining
            }

            // Play swap sound
            if (audioManager != null)
            {
                audioManager.PlayTileSwapSound();
            }

            // Trigger swap animation in view
            var view = GetComponentInChildren<Match3RuntimeView>();
            if (view != null)
            {
                yield return view.AnimateSwap(a, b, cellSize, BoardOriginWorld);
            }

            // Attempt swap and resolve
            if (board.TrySwapAndResolve(a, b, out int cleared, out int chains))
            {
                // Play match sound
                if (audioManager != null)
                {
                    audioManager.PlayTileMatchSound();
                }

                // Calculate score
                int baseScore = cleared * scorePerTile;
                int chainBonus = Mathf.Max(0, chains - 1) * bonusPerChain * cleared;
                AddScore(baseScore + chainBonus);

                // Update objectives
                if (objectiveSystem != null)
                {
                    objectiveSystem.UpdateObjectiveProgress(ObjectiveType.ClearTiles, cleared);
                    objectiveSystem.UpdateObjectiveProgress(ObjectiveType.ScoreTarget, currentScore);
                }

                // Show effects
                if (view != null)
                {
                    Vector3 popupPos = BoardOriginWorld + new Vector3(
                        (a.x + b.x + 1f) * 0.5f * cellSize,
                        (a.y + b.y + 1f) * 0.5f * cellSize,
                        0f
                    );
                    view.ShowScorePopup(popupPos, baseScore + chainBonus);
                    view.PulseClears();
                    view.PulseNeighborsOfClears();
                    view.FlashSpawned();
                }

                Debug.Log($"Match3Mechanic: Swap successful! Cleared: {cleared}, Chains: {chains}, Score: {baseScore + chainBonus}");
            }
            else
            {
                // Play error sound
                if (audioManager != null)
                {
                    audioManager.PlayErrorSound();
                }

                // Invalid swap - animate back
                if (view != null)
                {
                    yield return view.AnimateSwapBack(a, b, cellSize, BoardOriginWorld);
                }
                Debug.Log("Match3Mechanic: Invalid swap - no matches created");
            }
        }

        /// <summary>
        /// Get the tile value at the specified position.
        /// </summary>
        public int GetTileValue(int x, int y)
        {
            return board?.GetTile(x, y) ?? -1;
        }

        /// <summary>
        /// Get the special tile at the specified position.
        /// </summary>
        public SpecialTile GetSpecialTile(int x, int y)
        {
            return board?.GetSpecialTile(x, y) ?? new SpecialTile(-1, SpecialTileType.None);
        }

        #endregion

        #region Game System Event Handlers

        /// <summary>
        /// Handle power-up usage.
        /// </summary>
        private void OnPowerUpUsed(PowerUpType powerUpType)
        {
            Debug.Log($"Match3Mechanic: Power-up {powerUpType} was used");
            
            // Play power-up sound
            if (audioManager != null)
            {
                audioManager.PlayPowerUpUseSound();
            }
        }

        /// <summary>
        /// Handle objective completion.
        /// </summary>
        private void OnObjectiveCompleted(Objective objective)
        {
            Debug.Log($"Match3Mechanic: Objective {objective.type} completed!");
            
            // Play objective complete sound
            if (audioManager != null)
            {
                audioManager.PlayObjectiveCompleteSound();
            }
        }

        /// <summary>
        /// Handle all objectives completed.
        /// </summary>
        private void OnAllObjectivesCompleted()
        {
            Debug.Log("Match3Mechanic: All objectives completed! Level passed!");
            
            // Play victory music
            if (audioManager != null)
            {
                audioManager.PlayVictoryMusic();
            }
            
            // End the game with victory
            EndGame();
        }

        /// <summary>
        /// Handle objective game over (no moves/time remaining).
        /// </summary>
        private void OnObjectiveGameOver()
        {
            Debug.Log("Match3Mechanic: Game over - no moves or time remaining");
            
            // Play game over music
            if (audioManager != null)
            {
                audioManager.PlayGameOverMusic();
            }
            
            // End the game
            EndGame();
        }

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {
            if (!drawGizmos || board == null) return;

            Vector3 originWorld = BoardOriginWorld;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(originWorld + new Vector3(board.Width * cellSize * 0.5f, board.Height * cellSize * 0.5f, 0f),
                                new Vector3(board.Width * cellSize, board.Height * cellSize, 0.1f));

            // Draw individual tiles
            for (int y = 0; y < board.Height; y++)
            {
                for (int x = 0; x < board.Width; x++)
                {
                    int value = board.GetTile(x, y);
                    if (value >= 0)
                    {
                        Color color = GetColorForValue(value);
                        Gizmos.color = color;
                        Vector3 center = originWorld + new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0f);
                        Gizmos.DrawCube(center, new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f));
                    }
                }
            }
        }

        /// <summary>
        /// Get a color for a tile value (fallback when no theme is available).
        /// </summary>
        public static Color GetColorForValue(int value)
        {
            if (value < 0) return new Color(0f, 0f, 0f, 0.1f);

            int idx = value % 6;
            return idx switch
            {
                0 => Color.red,
                1 => Color.green,
                2 => Color.blue,
                3 => Color.yellow,
                4 => new Color(0.6f, 0f, 0.8f), // Purple
                _ => new Color(1f, 0.5f, 0f)     // Orange
            };
        }

        #endregion
    }
}

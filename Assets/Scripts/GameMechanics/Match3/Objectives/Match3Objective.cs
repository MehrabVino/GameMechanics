using UnityEngine;
using System.Collections.Generic;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Types of objectives in Match-3 games.
    /// </summary>
    public enum ObjectiveType
    {
        None = 0,
        ClearTiles = 1,         // Clear a specific number of tiles
        ClearColor = 2,         // Clear a specific number of tiles of a certain color
        CreateSpecialTiles = 3, // Create a specific number of special tiles
        ScoreTarget = 4,        // Reach a target score
        MovesLimit = 5,         // Complete objectives within a move limit
        TimeLimit = 6,          // Complete objectives within a time limit
        ClearObstacles = 7      // Clear specific obstacle tiles
    }

    /// <summary>
    /// Data structure for game objectives.
    /// </summary>
    [System.Serializable]
    public struct Objective
    {
        public ObjectiveType type;
        public int targetValue;
        public int currentValue;
        public int targetColor; // For color-specific objectives
        public SpecialTileType targetSpecialType; // For special tile objectives
        public bool isCompleted;

        public Objective(ObjectiveType type, int targetValue, int targetColor = -1, SpecialTileType targetSpecialType = SpecialTileType.None)
        {
            this.type = type;
            this.targetValue = targetValue;
            this.currentValue = 0;
            this.targetColor = targetColor;
            this.targetSpecialType = targetSpecialType;
            this.isCompleted = false;
        }

        public float Progress => targetValue > 0 ? (float)currentValue / targetValue : 0f;
    }

    /// <summary>
    /// Objective system for Match-3 games.
    /// </summary>
    public sealed class Match3Objective : MonoBehaviour
    {
        [Header("Objective Settings")]
        [SerializeField] private int maxObjectives = 3;
        [SerializeField] private bool autoComplete = true;

        // Objective tracking
        private List<Objective> objectives;
        private int movesRemaining = 30;
        private float timeRemaining = 300f; // 5 minutes default
        private bool isTimeLimitActive = false;

        // Events
        public System.Action<Objective> OnObjectiveCompleted { get; set; }
        public System.Action<Objective> OnObjectiveProgress { get; set; }
        public System.Action OnAllObjectivesCompleted { get; set; }
        public System.Action OnGameOver { get; set; }
        public System.Action<int> OnMovesChanged { get; set; }
        public System.Action<float> OnTimeChanged { get; set; }

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeObjectives();
        }

        private void Update()
        {
            if (isTimeLimitActive)
            {
                UpdateTimeLimit();
            }
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initialize the objective system.
        /// </summary>
        private void InitializeObjectives()
        {
            objectives = new List<Objective>();
        }

        /// <summary>
        /// Set up a new level with objectives.
        /// </summary>
        public void SetupLevel(Objective[] levelObjectives, int moves = 30, float timeLimit = 0f)
        {
            objectives.Clear();
            objectives.AddRange(levelObjectives);
            
            movesRemaining = moves;
            timeRemaining = timeLimit;
            isTimeLimitActive = timeLimit > 0f;
            
            Debug.Log($"Match3Objective: Level setup with {objectives.Count} objectives, {moves} moves, {timeLimit}s time limit");
        }

        #endregion

        #region Objective Management

        /// <summary>
        /// Add an objective to the current level.
        /// </summary>
        public void AddObjective(Objective objective)
        {
            if (objectives.Count >= maxObjectives)
            {
                Debug.LogWarning("Match3Objective: Maximum objectives reached");
                return;
            }
            
            objectives.Add(objective);
            Debug.Log($"Match3Objective: Added {objective.type} objective");
        }

        /// <summary>
        /// Update objective progress based on game events.
        /// </summary>
        public void UpdateObjectiveProgress(ObjectiveType type, int value, int color = -1, SpecialTileType specialType = SpecialTileType.None)
        {
            for (int i = 0; i < objectives.Count; i++)
            {
                Objective obj = objectives[i];
                
                if (obj.isCompleted) continue;
                
                bool shouldUpdate = false;
                
                switch (type)
                {
                    case ObjectiveType.ClearTiles:
                        if (obj.type == ObjectiveType.ClearTiles)
                        {
                            obj.currentValue += value;
                            shouldUpdate = true;
                        }
                        break;
                        
                    case ObjectiveType.ClearColor:
                        if (obj.type == ObjectiveType.ClearColor && obj.targetColor == color)
                        {
                            obj.currentValue += value;
                            shouldUpdate = true;
                        }
                        break;
                        
                    case ObjectiveType.CreateSpecialTiles:
                        if (obj.type == ObjectiveType.CreateSpecialTiles && obj.targetSpecialType == specialType)
                        {
                            obj.currentValue += value;
                            shouldUpdate = true;
                        }
                        break;
                        
                    case ObjectiveType.ScoreTarget:
                        if (obj.type == ObjectiveType.ScoreTarget)
                        {
                            obj.currentValue = value;
                            shouldUpdate = true;
                        }
                        break;
                }
                
                if (shouldUpdate)
                {
                    obj.currentValue = Mathf.Min(obj.currentValue, obj.targetValue);
                    
                    if (obj.currentValue >= obj.targetValue && !obj.isCompleted)
                    {
                        obj.isCompleted = true;
                        OnObjectiveCompleted?.Invoke(obj);
                        Debug.Log($"Match3Objective: Completed {obj.type} objective");
                    }
                    else
                    {
                        OnObjectiveProgress?.Invoke(obj);
                    }
                    
                    objectives[i] = obj;
                }
            }
            
            CheckAllObjectivesCompleted();
        }

        /// <summary>
        /// Check if all objectives are completed.
        /// </summary>
        private void CheckAllObjectivesCompleted()
        {
            bool allCompleted = true;
            foreach (Objective obj in objectives)
            {
                if (!obj.isCompleted)
                {
                    allCompleted = false;
                    break;
                }
            }
            
            if (allCompleted)
            {
                OnAllObjectivesCompleted?.Invoke();
                Debug.Log("Match3Objective: All objectives completed!");
            }
        }

        /// <summary>
        /// Use a move (decrement move counter).
        /// </summary>
        public bool UseMove()
        {
            if (movesRemaining <= 0)
            {
                OnGameOver?.Invoke();
                return false;
            }
            
            movesRemaining--;
            OnMovesChanged?.Invoke(movesRemaining);
            
            if (movesRemaining <= 0)
            {
                OnGameOver?.Invoke();
            }
            
            return true;
        }

        /// <summary>
        /// Update time limit.
        /// </summary>
        private void UpdateTimeLimit()
        {
            if (timeRemaining > 0f)
            {
                timeRemaining -= Time.deltaTime;
                OnTimeChanged?.Invoke(timeRemaining);
                
                if (timeRemaining <= 0f)
                {
                    timeRemaining = 0f;
                    OnGameOver?.Invoke();
                }
            }
        }

        #endregion

        #region Getters

        /// <summary>
        /// Get all current objectives.
        /// </summary>
        public IReadOnlyList<Objective> Objectives => objectives;

        /// <summary>
        /// Get remaining moves.
        /// </summary>
        public int MovesRemaining => movesRemaining;

        /// <summary>
        /// Get remaining time.
        /// </summary>
        public float TimeRemaining => timeRemaining;

        /// <summary>
        /// Check if time limit is active.
        /// </summary>
        public bool IsTimeLimitActive => isTimeLimitActive;

        /// <summary>
        /// Get completion percentage for all objectives.
        /// </summary>
        public float OverallProgress
        {
            get
            {
                if (objectives.Count == 0) return 1f;
                
                float totalProgress = 0f;
                foreach (Objective obj in objectives)
                {
                    totalProgress += obj.Progress;
                }
                
                return totalProgress / objectives.Count;
            }
        }

        #endregion

        #region Debug

        /// <summary>
        /// Create sample objectives for testing.
        /// </summary>
        [ContextMenu("Create Sample Objectives")]
        public void CreateSampleObjectives()
        {
            objectives.Clear();
            
            // Clear 50 tiles
            AddObjective(new Objective(ObjectiveType.ClearTiles, 50));
            
            // Clear 20 red tiles (assuming red is color 0)
            AddObjective(new Objective(ObjectiveType.ClearColor, 20, 0));
            
            // Create 3 bomb special tiles
            AddObjective(new Objective(ObjectiveType.CreateSpecialTiles, 3, -1, SpecialTileType.Bomb));
            
            Debug.Log("Match3Objective: Created sample objectives");
        }

        /// <summary>
        /// Complete all objectives for testing.
        /// </summary>
        [ContextMenu("Complete All Objectives")]
        public void CompleteAllObjectives()
        {
            for (int i = 0; i < objectives.Count; i++)
            {
                Objective obj = objectives[i];
                obj.currentValue = obj.targetValue;
                obj.isCompleted = true;
                objectives[i] = obj;
            }
            
            OnAllObjectivesCompleted?.Invoke();
            Debug.Log("Match3Objective: All objectives completed via debug");
        }

        #endregion
    }
}

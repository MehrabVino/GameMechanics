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
        ClearTiles = 1,
        ClearColor = 2,
        CreateSpecialTiles = 3,
        ScoreTarget = 4,
        MovesLimit = 5,
        TimeLimit = 6,
        ClearObstacles = 7
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
        public int targetColor;
        public SpecialTileType targetSpecialType;
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
        [SerializeField] private ObjectiveConfig objectiveConfig;

        private List<Objective> objectives = new List<Objective>();
        private int movesRemaining;
        private float timeRemaining;
        private bool isTimeLimitActive;

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

        private void InitializeObjectives()
        {
            objectives.Clear();
        }

        /// <summary>
        /// Set up a new level with objectives.
        /// </summary>
        public void SetupLevel()
        {
            objectives.AddRange(objectiveConfig.LevelObjectives);
            movesRemaining = objectiveConfig.Moves;
            timeRemaining = objectiveConfig.TimeLimit;
            isTimeLimitActive = timeRemaining > 0f;
        }

        #endregion

        #region Objective Management

        /// <summary>
        /// Add an objective to the current level.
        /// </summary>
        public void AddObjective(Objective objective)
        {
            if (objectives.Count >= objectiveConfig.MaxObjectives)
            {
                return;
            }
            
            objectives.Add(objective);
        }

        /// <summary>
        /// Update objective progress based on game events.
        /// </summary>
        public void UpdateObjectiveProgress(ObjectiveType type, int value, int color = -1, SpecialTileType specialType = SpecialTileType.None)
        {
            bool updated = false;
            for (int i = 0; i < objectives.Count; i++)
            {
                Objective obj = objectives[i];
                if (obj.isCompleted || !IsMatchingObjective(obj, type, color, specialType)) continue;

                obj.currentValue = Mathf.Min(obj.currentValue + value, obj.targetValue);
                objectives[i] = obj;
                updated = true;

                if (obj.currentValue >= obj.targetValue)
                {
                    obj.isCompleted = true;
                    objectives[i] = obj;
                    OnObjectiveCompleted?.Invoke(obj);
                }
                else
                {
                    OnObjectiveProgress?.Invoke(obj);
                }
            }

            if (updated) CheckAllObjectivesCompleted();
        }

        private bool IsMatchingObjective(Objective obj, ObjectiveType type, int color, SpecialTileType specialType)
        {
            switch (type)
            {
                case ObjectiveType.ClearTiles:
                    return obj.type == ObjectiveType.ClearTiles;
                case ObjectiveType.ClearColor:
                    return obj.type == ObjectiveType.ClearColor && obj.targetColor == color;
                case ObjectiveType.CreateSpecialTiles:
                    return obj.type == ObjectiveType.CreateSpecialTiles && obj.targetSpecialType == specialType;
                case ObjectiveType.ScoreTarget:
                    return obj.type == ObjectiveType.ScoreTarget;
                default:
                    return false;
            }
        }

        private void CheckAllObjectivesCompleted()
        {
            if (objectives.TrueForAll(o => o.isCompleted))
            {
                OnAllObjectivesCompleted?.Invoke();
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

        public IReadOnlyList<Objective> Objectives => objectives;
        public int MovesRemaining => movesRemaining;
        public float TimeRemaining => timeRemaining;
        public bool IsTimeLimitActive => isTimeLimitActive;

        public float OverallProgress
        {
            get
            {
                if (objectives.Count == 0) return 1f;
                
                float total = 0f;
                foreach (Objective obj in objectives)
                {
                    total += obj.Progress;
                }
                
                return total / objectives.Count;
            }
        }

        #endregion
    }

    /// <summary>
    /// ScriptableObject for objective configuration.
    /// </summary>
    [CreateAssetMenu(fileName = "ObjectiveConfig", menuName = "MechanicGames/Objective Config", order = 2)]
    public class ObjectiveConfig : ScriptableObject
    {
        [Header("Objective Settings")]
        public int MaxObjectives = 3;
        public Objective[] LevelObjectives;
        public int Moves = 30;
        public float TimeLimit = 300f;
    }
}
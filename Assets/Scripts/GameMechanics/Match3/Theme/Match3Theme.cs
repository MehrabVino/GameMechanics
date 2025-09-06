using UnityEngine;
using System.Collections.Generic;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Theme data for Match-3 visuals with integrated tile spawning logic.
    /// Uses only Tile Definitions for modern, flexible tile management.
    /// </summary>
    [CreateAssetMenu(fileName = "Match3Theme", menuName = "MechanicGames/Match3 Theme", order = 0)]
    public sealed class Match3Theme : ScriptableObject
    {
        [System.Serializable]
        public sealed class TileDefinition
        {
            [Header("Tile Identity")]
            [Tooltip("Unique identifier for this tile type")]
            public string id;
            [Tooltip("Tile value (0, 1, 2, etc.) - must be unique")]
            public int tileValue;
            
            [Header("Visual Properties")]
            [Tooltip("Sprite for this tile (optional - will use color if null)")]
            public Sprite sprite;
            [Tooltip("Color for this tile (used when sprite is null or as tint)")]
            public Color color = Color.white;
            
            [Header("Spawn Settings")]
            [Range(0f, 1f)]
            [Tooltip("Relative spawn weight (higher = more likely to spawn)")]
            public float spawnWeight = 1f;
            [Tooltip("Whether this tile can spawn on the board")]
            public bool canSpawn = true;
        }

        [Header("Tile Definitions")]
        [Tooltip("Define each tile type with its properties. Tile values should be 0, 1, 2, etc.")]
        public TileDefinition[] tileDefinitions;

        [Header("Background")]
        [Tooltip("Background sprite for the board (optional)")]
        public Sprite backgroundSprite;
        [Tooltip("Background color for the board")]
        public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        [Header("Spawn Settings")]
        [Tooltip("Whether to use weighted spawning for tile definitions")]
        public bool useWeightedSpawning = true;
        [Tooltip("Seed for consistent random generation")]
        public int spawnSeed = 0;

        // Cached data for performance
        private System.Random spawnRandom;
        private float[] cumulativeWeights;
        private bool weightsCalculated = false;

        /// <summary>
        /// Get the number of different tile types available.
        /// </summary>
        public int TileTypeCount
        {
            get
            {
                if (tileDefinitions != null && tileDefinitions.Length > 0)
                {
                    return tileDefinitions.Length;
                }
                
                return 1; // Default fallback
            }
        }

        /// <summary>
        /// Get a random tile value based on the theme's spawn rules.
        /// </summary>
        public int GetRandomTileValue()
        {
            if (tileDefinitions != null && tileDefinitions.Length > 0)
            {
                if (useWeightedSpawning)
                {
                    return GetWeightedRandomTileValue();
                }
                else
                {
                    return GetUniformRandomTileValue();
                }
            }
            
            // Fallback if no tile definitions
            return 0;
        }

        /// <summary>
        /// Get a tile value using weighted spawning based on spawnWeight.
        /// </summary>
        private int GetWeightedRandomTileValue()
        {
            if (!weightsCalculated)
            {
                CalculateCumulativeWeights();
            }

            if (cumulativeWeights == null || cumulativeWeights.Length == 0)
            {
                return 0;
            }

            float randomValue = (float)spawnRandom.NextDouble() * cumulativeWeights[cumulativeWeights.Length - 1];
            
            for (int i = 0; i < cumulativeWeights.Length; i++)
            {
                if (randomValue <= cumulativeWeights[i])
                {
                    return tileDefinitions[i].tileValue;
                }
            }
            
            return tileDefinitions[tileDefinitions.Length - 1].tileValue;
        }

        /// <summary>
        /// Get a tile value using uniform random selection.
        /// </summary>
        private int GetUniformRandomTileValue()
        {
            if (tileDefinitions == null || tileDefinitions.Length == 0)
                return 0;

            List<int> validIndices = new List<int>();
            for (int i = 0; i < tileDefinitions.Length; i++)
            {
                if (tileDefinitions[i].canSpawn)
                {
                    validIndices.Add(i);
                }
            }

            if (validIndices.Count == 0)
                return 0;

            int randomIndex = validIndices[spawnRandom.Next(validIndices.Count)];
            return tileDefinitions[randomIndex].tileValue;
        }

        /// <summary>
        /// Calculate cumulative weights for weighted spawning.
        /// </summary>
        private void CalculateCumulativeWeights()
        {
            if (tileDefinitions == null || tileDefinitions.Length == 0)
            {
                cumulativeWeights = null;
                weightsCalculated = true;
                return;
            }

            cumulativeWeights = new float[tileDefinitions.Length];
            float totalWeight = 0f;

            for (int i = 0; i < tileDefinitions.Length; i++)
            {
                if (tileDefinitions[i].canSpawn)
                {
                    totalWeight += tileDefinitions[i].spawnWeight;
                }
                cumulativeWeights[i] = totalWeight;
            }

            weightsCalculated = true;
        }

        /// <summary>
        /// Initialize the spawn random generator.
        /// </summary>
        public void InitializeSpawnRandom()
        {
            spawnRandom = spawnSeed == 0 ? new System.Random() : new System.Random(spawnSeed);
            weightsCalculated = false;
        }

        /// <summary>
        /// Get tile definition for a specific tile value.
        /// </summary>
        public TileDefinition GetTileDefinition(int tileValue)
        {
            if (tileDefinitions == null) return null;
            
            for (int i = 0; i < tileDefinitions.Length; i++)
            {
                if (tileDefinitions[i].tileValue == tileValue)
                {
                    return tileDefinitions[i];
                }
            }
            
            return null;
        }

        /// <summary>
        /// Get sprite for a tile value.
        /// </summary>
        public Sprite GetTileSprite(int tileValue)
        {
            TileDefinition def = GetTileDefinition(tileValue);
            return def?.sprite;
        }

        /// <summary>
        /// Get color for a tile value.
        /// </summary>
        public Color GetTileColor(int tileValue)
        {
            TileDefinition def = GetTileDefinition(tileValue);
            return def?.color ?? Color.white;
        }

        /// <summary>
        /// Validate the theme configuration.
        /// </summary>
        public bool IsValid()
        {
            if (tileDefinitions != null && tileDefinitions.Length > 0)
            {
                // Check if tile definitions are properly configured
                for (int i = 0; i < tileDefinitions.Length; i++)
                {
                    if (tileDefinitions[i] == null) return false;
                    if (string.IsNullOrEmpty(tileDefinitions[i].id)) return false;
                }
                return true;
            }

            return false;
        }

        private void OnValidate()
        {
            // Reset weights when theme is modified in editor
            weightsCalculated = false;
        }
    }
}




using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Special tile types that can be created through special matches.
    /// </summary>
    public enum SpecialTileType
    {
        None = 0,
        Bomb = 1,        // Clears 3x3 area
        Lightning = 2,   // Clears entire row or column
        Rainbow = 3,     // Clears all tiles of one color
        Star = 4         // Clears all tiles in cross pattern
    }

    /// <summary>
    /// Data structure for special tiles.
    /// </summary>
    [Serializable]
    public struct SpecialTile
    {
        public int baseValue;        // The original tile value
        public SpecialTileType type; // The special tile type
        public int power;            // Power level (for scaling effects)

        public SpecialTile(int baseValue, SpecialTileType type, int power = 1)
        {
            this.baseValue = baseValue;
            this.type = type;
            this.power = power;
        }

        public bool IsSpecial => type != SpecialTileType.None;
    }

    /// <summary>
    /// Pure logic container for a match-3 grid with efficient match detection and cascade resolution.
    /// Integrates with Match3Theme for tile spawning and supports special tiles.
    /// </summary>
    [Serializable]
    public sealed class Match3Board : IMatch3BoardReadOnly
    {
        [Header("Board Configuration")]
        [SerializeField] private int width = 8;
        [SerializeField] private int height = 8;
        [SerializeField] private int tileTypeCount = 6;
        [SerializeField] private int seed = 0;

        [Header("Theme Integration")]
        [SerializeField] private Match3Theme theme;

        // Internal state
        private int[,] tiles;
        private SpecialTile[,] specialTiles;
        private System.Random random;
        private readonly List<Vector2Int> lastClearedCells = new List<Vector2Int>();
        private readonly List<Vector2Int> lastSpawnedCells = new List<Vector2Int>();
        private readonly List<Vector2Int> specialTilesCreated = new List<Vector2Int>();

        // Public properties
        public int Width => width;
        public int Height => height;
        public int TileTypeCount => tileTypeCount;
        public IReadOnlyList<Vector2Int> LastClearedCells => lastClearedCells;
        public IReadOnlyList<Vector2Int> LastSpawnedCells => lastSpawnedCells;
        public IReadOnlyList<Vector2Int> SpecialTilesCreated => specialTilesCreated;
        public Match3Theme Theme => theme;

        /// <summary>
        /// Set the theme for this board.
        /// </summary>
        public void SetTheme(Match3Theme newTheme)
        {
            theme = newTheme;
            if (theme != null)
            {
                theme.InitializeSpawnRandom();
                tileTypeCount = theme.TileTypeCount;
            }
        }

        /// <summary>
        /// Initialize the board with random tiles, ensuring no immediate matches.
        /// </summary>
        public void Initialize()
        {
            random = seed == 0 ? new System.Random() : new System.Random(seed);
            tiles = new int[width, height];
            specialTiles = new SpecialTile[width, height];
            
            if (theme != null)
            {
                theme.InitializeSpawnRandom();
            }
            
            FillBoardWithoutImmediateMatches();
        }

        /// <summary>
        /// Set the number of different tile types for variety.
        /// </summary>
        public void SetTileTypeCount(int count)
        {
            tileTypeCount = Mathf.Max(1, count);
        }

        /// <summary>
        /// Get the tile value at the specified position.
        /// </summary>
        public int GetTile(int x, int y)
        {
            if (!IsValidPosition(x, y)) return -1;
            return tiles[x, y];
        }

        /// <summary>
        /// Get the special tile at the specified position.
        /// </summary>
        public SpecialTile GetSpecialTile(int x, int y)
        {
            if (!IsValidPosition(x, y)) return new SpecialTile(-1, SpecialTileType.None);
            return specialTiles[x, y];
        }

        /// <summary>
        /// Set a special tile at the specified position.
        /// </summary>
        public void SetSpecialTile(int x, int y, SpecialTile specialTile)
        {
            if (!IsValidPosition(x, y)) return;
            specialTiles[x, y] = specialTile;
            if (specialTile.IsSpecial)
            {
                specialTilesCreated.Add(new Vector2Int(x, y));
            }
        }

        /// <summary>
        /// Set a tile value at the specified position.
        /// </summary>
        public void SetTile(int x, int y, int value)
        {
            if (!IsValidPosition(x, y)) return;
            tiles[x, y] = value;
        }

        private bool IsValidPosition(int x, int y) => x >= 0 && x < width && y >= 0 && y < height;

        /// <summary>
        /// Check if two cells are adjacent (horizontally or vertically).
        /// </summary>
        public bool AreAdjacent(Vector2Int a, Vector2Int b)
        {
            int dx = Mathf.Abs(a.x - b.x);
            int dy = Mathf.Abs(a.y - b.y);
            return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
        }

        /// <summary>
        /// Attempt to swap two adjacent tiles and resolve all resulting matches and cascades.
        /// </summary>
        public bool TrySwapAndResolve(Vector2Int a, Vector2Int b, out int cleared, out int chains)
        {
            cleared = 0;
            chains = 0;

            if (!AreAdjacent(a, b) || !IsValidPosition(a.x, a.y) || !IsValidPosition(b.x, b.y))
                return false;

            SwapTiles(a, b);

            if (!HasAnyMatch())
            {
                SwapTiles(a, b); // Revert swap
                return false;
            }

            while (ResolveMatchesAndCascade(ref cleared, ref chains)) { }

            return true;
        }

        private bool ResolveMatchesAndCascade(ref int cleared, ref int chains)
        {
            lastClearedCells.Clear();
            lastSpawnedCells.Clear();
            specialTilesCreated.Clear();

            List<Vector2Int> matches = FindAllMatches();
            if (matches.Count == 0) return false;

            ProcessSpecialTiles(matches);
            ClearMatchedTiles(matches);
            cleared += matches.Count;
            chains++;
            CollapseAndRefill();

            return true;
        }

        /// <summary>
        /// Find all matches (3 or more tiles in a row or column).
        /// </summary>
        private List<Vector2Int> FindAllMatches()
        {
            List<Vector2Int> matches = new List<Vector2Int>();

            // Check horizontal matches
            for (int y = 0; y < height; y++)
            {
                int matchCount = 1;
                int matchValue = -1;
                for (int x = 0; x < width; x++)
                {
                    int currentValue = tiles[x, y];
                    if (currentValue == matchValue && currentValue >= 0)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int i = x - matchCount; i < x; i++)
                            {
                                matches.Add(new Vector2Int(i, y));
                            }
                            if (matchCount >= 4) // Create special tile for larger matches
                            {
                                CreateSpecialTile(x - 1, y, matchCount, isHorizontal: true);
                            }
                        }
                        matchValue = currentValue;
                        matchCount = 1;
                    }
                }
                if (matchCount >= 3)
                {
                    for (int i = width - matchCount; i < width; i++)
                    {
                        matches.Add(new Vector2Int(i, y));
                    }
                    if (matchCount >= 4)
                    {
                        CreateSpecialTile(width - 1, y, matchCount, isHorizontal: true);
                    }
                }
            }

            // Check vertical matches
            for (int x = 0; x < width; x++)
            {
                int matchCount = 1;
                int matchValue = -1;
                for (int y = 0; y < height; y++)
                {
                    int currentValue = tiles[x, y];
                    if (currentValue == matchValue && currentValue >= 0)
                    {
                        matchCount++;
                    }
                    else
                    {
                        if (matchCount >= 3)
                        {
                            for (int i = y - matchCount; i < y; i++)
                            {
                                matches.Add(new Vector2Int(x, i));
                            }
                            if (matchCount >= 4)
                            {
                                CreateSpecialTile(x, y - 1, matchCount, isHorizontal: false);
                            }
                        }
                        matchValue = currentValue;
                        matchCount = 1;
                    }
                }
                if (matchCount >= 3)
                {
                    for (int i = height - matchCount; i < height; i++)
                    {
                        matches.Add(new Vector2Int(x, i));
                    }
                    if (matchCount >= 4)
                    {
                        CreateSpecialTile(x, height - 1, matchCount, isHorizontal: false);
                    }
                }
            }

            return matches;
        }

        /// <summary>
        /// Create a special tile based on match size and orientation.
        /// </summary>
        private void CreateSpecialTile(int x, int y, int matchCount, bool isHorizontal)
        {
            SpecialTileType type = matchCount switch
            {
                4 => isHorizontal ? SpecialTileType.Bomb : SpecialTileType.Lightning,
                5 => SpecialTileType.Rainbow,
                >= 6 => SpecialTileType.Star,
                _ => SpecialTileType.None
            };

            if (type != SpecialTileType.None)
            {
                SetSpecialTile(x, y, new SpecialTile(tiles[x, y], type));
            }
        }

        /// <summary>
        /// Process special tiles in the matched positions.
        /// </summary>
        private void ProcessSpecialTiles(List<Vector2Int> matches)
        {
            foreach (Vector2Int pos in matches)
            {
                SpecialTile special = specialTiles[pos.x, pos.y];
                if (!special.IsSpecial) continue;

                switch (special.type)
                {
                    case SpecialTileType.Bomb:
                        ClearBombArea(pos.x, pos.y);
                        break;
                    case SpecialTileType.Lightning:
                        ClearLine(pos.x, pos.y);
                        break;
                    case SpecialTileType.Rainbow:
                        ClearColor(tiles[pos.x, pos.y]);
                        break;
                    case SpecialTileType.Star:
                        ClearCross(pos.x, pos.y);
                        break;
                }
            }
        }

        private void ClearBombArea(int x, int y)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    int nx = x + dx;
                    int ny = y + dy;
                    if (IsValidPosition(nx, ny))
                    {
                        lastClearedCells.Add(new Vector2Int(nx, ny));
                    }
                }
            }
        }

        private void ClearLine(int x, int y)
        {
            for (int i = 0; i < width; i++)
            {
                lastClearedCells.Add(new Vector2Int(i, y));
            }
            for (int i = 0; i < height; i++)
            {
                lastClearedCells.Add(new Vector2Int(x, i));
            }
        }

        private void ClearColor(int targetColor)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (tiles[x, y] == targetColor)
                    {
                        lastClearedCells.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        private void ClearCross(int x, int y)
        {
            for (int i = 0; i < width; i++)
            {
                lastClearedCells.Add(new Vector2Int(i, y));
            }
            for (int i = 0; i < height; i++)
            {
                if (i != y) lastClearedCells.Add(new Vector2Int(x, i));
            }
        }

        /// <summary>
        /// Clear matched tiles from the board.
        /// </summary>
        private void ClearMatchedTiles(List<Vector2Int> matches)
        {
            foreach (Vector2Int pos in matches)
            {
                if (IsValidPosition(pos.x, pos.y))
                {
                    tiles[pos.x, pos.y] = -1;
                    specialTiles[pos.x, pos.y] = new SpecialTile(-1, SpecialTileType.None);
                    lastClearedCells.Add(pos);
                }
            }
        }

        /// <summary>
        /// Collapse columns and refill with new tiles.
        /// </summary>
        private void CollapseAndRefill()
        {
            for (int x = 0; x < width; x++)
            {
                int emptyCount = 0;
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y] == -1)
                    {
                        emptyCount++;
                    }
                    else if (emptyCount > 0)
                    {
                        tiles[x, y - emptyCount] = tiles[x, y];
                        specialTiles[x, y - emptyCount] = specialTiles[x, y];
                        tiles[x, y] = -1;
                        specialTiles[x, y] = new SpecialTile(-1, SpecialTileType.None);
                    }
                }

                for (int i = 0; i < emptyCount; i++)
                {
                    int y = height - emptyCount + i;
                    tiles[x, y] = GetRandomTileValue();
                    specialTiles[x, y] = new SpecialTile(tiles[x, y], SpecialTileType.None);
                    lastSpawnedCells.Add(new Vector2Int(x, y));
                }
            }
        }

        /// <summary>
        /// Check if there are any matches on the board.
        /// </summary>
        public bool HasAnyMatch()
        {
            return FindAllMatches().Count > 0;
        }

        /// <summary>
        /// Fill the board with random tiles, ensuring no immediate matches.
        /// </summary>
        private void FillBoardWithoutImmediateMatches()
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int value;
                    do
                    {
                        value = GetRandomTileValue();
                    } while (WouldCreateMatch(x, y, value));
                    tiles[x, y] = value;
                    specialTiles[x, y] = new SpecialTile(value, SpecialTileType.None);
                }
            }
        }

        /// <summary>
        /// Check if placing a tile would create an immediate match.
        /// </summary>
        private bool WouldCreateMatch(int x, int y, int value)
        {
            if (x >= 2 && tiles[x - 1, y] == value && tiles[x - 2, y] == value)
                return true;
            if (y >= 2 && tiles[x, y - 1] == value && tiles[x, y - 2] == value)
                return true;
            return false;
        }

        /// <summary>
        /// Swap two tiles on the board.
        /// </summary>
        private void SwapTiles(Vector2Int a, Vector2Int b)
        {
            int tempTile = tiles[a.x, a.y];
            SpecialTile tempSpecial = specialTiles[a.x, a.y];

            tiles[a.x, a.y] = tiles[b.x, b.y];
            specialTiles[a.x, a.y] = specialTiles[b.x, b.y];

            tiles[b.x, b.y] = tempTile;
            specialTiles[b.x, b.y] = tempSpecial;
        }

        /// <summary>
        /// Get a random tile value, prioritizing theme-based spawning.
        /// </summary>
        private int GetRandomTileValue()
        {
            if (theme != null && theme.IsValid())
            {
                return theme.GetRandomTileValue();
            }
            
            return random.Next(0, tileTypeCount);
        }
    }
}
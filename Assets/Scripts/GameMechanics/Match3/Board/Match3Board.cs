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
    [System.Serializable]
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
    /// Now integrates with Match3Theme for proper tile spawning and supports special tiles.
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
            if (x < 0 || x >= width || y < 0 || y >= height)
                return -1;
            return tiles[x, y];
        }

        /// <summary>
        /// Get the special tile at the specified position.
        /// </summary>
        public SpecialTile GetSpecialTile(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return new SpecialTile(-1, SpecialTileType.None);
            return specialTiles[x, y];
        }

        /// <summary>
        /// Set a special tile at the specified position.
        /// </summary>
        public void SetSpecialTile(int x, int y, SpecialTile specialTile)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            specialTiles[x, y] = specialTile;
        }

        /// <summary>
        /// Set a tile value at the specified position.
        /// </summary>
        public void SetTile(int x, int y, int value)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            tiles[x, y] = value;
        }

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
        /// Returns false if the swap doesn't produce any matches.
        /// </summary>
        public bool TrySwapAndResolve(Vector2Int a, Vector2Int b, out int totalCleared, out int chainCount)
        {
            totalCleared = 0;
            chainCount = 0;
            lastClearedCells.Clear();
            lastSpawnedCells.Clear();
            specialTilesCreated.Clear();

            // Validate swap
            if (!AreAdjacent(a, b))
                return false;

            // Perform swap
            SwapTiles(a, b);

            // Check if swap produces any matches
            if (!HasAnyMatch())
            {
                // Revert swap if no matches
                SwapTiles(a, b);
                return false;
            }

            // Resolve all cascades
            bool found;
            do
            {
                bool[,] matched = FindAllMatches();
                found = ClearMatches(matched, out int cleared);
                totalCleared += cleared;
                if (found)
                {
                    chainCount++;
                    CreateSpecialTiles(matched);
                    CollapseAndRefill();
                }
            } while (found);

            return true;
        }

        /// <summary>
        /// Find all matches on the board (horizontal, vertical, L-shapes, and T-shapes).
        /// </summary>
        private bool[,] FindAllMatches()
        {
            bool[,] matched = new bool[width, height];

            // Check horizontal matches (3+ in a row)
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - 2; x++)
                {
                    int value = tiles[x, y];
                    if (value >= 0 && value == tiles[x + 1, y] && value == tiles[x + 2, y])
                    {
                        // Extend the match to find all consecutive tiles
                        int matchLength = 3;
                        while (x + matchLength < width && tiles[x + matchLength, y] == value)
                        {
                            matchLength++;
                        }
                        
                        // Mark all tiles in the match
                        for (int i = 0; i < matchLength; i++)
                        {
                            matched[x + i, y] = true;
                        }
                    }
                }
            }

            // Check vertical matches (3+ in a column)
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 2; y++)
                {
                    int value = tiles[x, y];
                    if (value >= 0 && value == tiles[x, y + 1] && value == tiles[x, y + 2])
                    {
                        // Extend the match to find all consecutive tiles
                        int matchLength = 3;
                        while (y + matchLength < height && tiles[x, y + matchLength] == value)
                        {
                            matchLength++;
                        }
                        
                        // Mark all tiles in the match
                        for (int i = 0; i < matchLength; i++)
                        {
                            matched[x, y + i] = true;
                        }
                    }
                }
            }

            // Check L-shapes and T-shapes
            FindLAndTShapes(matched);

            return matched;
        }

        /// <summary>
        /// Find L-shapes and T-shapes for special tile creation.
        /// </summary>
        private void FindLAndTShapes(bool[,] matched)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int value = tiles[x, y];
                    if (value < 0) continue;

                    // Check for L-shapes (3 horizontal + 3 vertical with intersection)
                    if (IsLShape(x, y, value))
                    {
                        // Mark the L-shape tiles
                        MarkLShape(matched, x, y, value);
                    }

                    // Check for T-shapes (3 horizontal + 3 vertical with intersection)
                    if (IsTShape(x, y, value))
                    {
                        // Mark the T-shape tiles
                        MarkTShape(matched, x, y, value);
                    }
                }
            }
        }

        /// <summary>
        /// Check if position forms an L-shape.
        /// </summary>
        private bool IsLShape(int x, int y, int value)
        {
            // Check horizontal line (3+ tiles)
            bool hasHorizontal = false;
            int hStart = x, hEnd = x;
            
            // Extend left
            while (hStart > 0 && tiles[hStart - 1, y] == value) hStart--;
            // Extend right
            while (hEnd < width - 1 && tiles[hEnd + 1, y] == value) hEnd++;
            
            if (hEnd - hStart >= 2) hasHorizontal = true;

            // Check vertical line (3+ tiles)
            bool hasVertical = false;
            int vStart = y, vEnd = y;
            
            // Extend down
            while (vStart > 0 && tiles[x, vStart - 1] == value) vStart--;
            // Extend up
            while (vEnd < height - 1 && tiles[x, vEnd + 1] == value) vEnd++;
            
            if (vEnd - vStart >= 2) hasVertical = true;

            return hasHorizontal && hasVertical;
        }

        /// <summary>
        /// Check if position forms a T-shape.
        /// </summary>
        private bool IsTShape(int x, int y, int value)
        {
            // Check if we have a horizontal line of 3+ with a vertical line of 3+ intersecting
            bool hasHorizontal = false;
            int hStart = x, hEnd = x;
            
            // Extend left
            while (hStart > 0 && tiles[hStart - 1, y] == value) hStart--;
            // Extend right
            while (hEnd < width - 1 && tiles[hEnd + 1, y] == value) hEnd++;
            
            if (hEnd - hStart >= 2) hasHorizontal = true;

            // Check vertical line (3+ tiles)
            bool hasVertical = false;
            int vStart = y, vEnd = y;
            
            // Extend down
            while (vStart > 0 && tiles[x, vStart - 1] == value) vStart--;
            // Extend up
            while (vEnd < height - 1 && tiles[x, vEnd + 1] == value) vEnd++;
            
            if (vEnd - vStart >= 2) hasVertical = true;

            // T-shape requires the intersection point to be in the middle of the horizontal line
            return hasHorizontal && hasVertical && (x > hStart && x < hEnd);
        }

        /// <summary>
        /// Mark L-shape tiles for clearing.
        /// </summary>
        private void MarkLShape(bool[,] matched, int x, int y, int value)
        {
            // Mark horizontal line
            int hStart = x, hEnd = x;
            while (hStart > 0 && tiles[hStart - 1, y] == value) hStart--;
            while (hEnd < width - 1 && tiles[hEnd + 1, y] == value) hEnd++;
            
            for (int i = hStart; i <= hEnd; i++)
            {
                matched[i, y] = true;
            }

            // Mark vertical line
            int vStart = y, vEnd = y;
            while (vStart > 0 && tiles[x, vStart - 1] == value) vStart--;
            while (vEnd < height - 1 && tiles[x, vEnd + 1] == value) vEnd++;
            
            for (int i = vStart; i <= vEnd; i++)
            {
                matched[x, i] = true;
            }
        }

        /// <summary>
        /// Mark T-shape tiles for clearing.
        /// </summary>
        private void MarkTShape(bool[,] matched, int x, int y, int value)
        {
            // Mark horizontal line
            int hStart = x, hEnd = x;
            while (hStart > 0 && tiles[hStart - 1, y] == value) hStart--;
            while (hEnd < width - 1 && tiles[hEnd + 1, y] == value) hEnd++;
            
            for (int i = hStart; i <= hEnd; i++)
            {
                matched[i, y] = true;
            }

            // Mark vertical line
            int vStart = y, vEnd = y;
            while (vStart > 0 && tiles[x, vStart - 1] == value) vStart--;
            while (vEnd < height - 1 && tiles[x, vEnd + 1] == value) vEnd++;
            
            for (int i = vStart; i <= vEnd; i++)
            {
                matched[x, i] = true;
            }
        }

        /// <summary>
        /// Clear matched tiles and return the count of cleared tiles.
        /// </summary>
        private bool ClearMatches(bool[,] matched, out int cleared)
        {
            cleared = 0;
            bool anyCleared = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (matched[x, y])
                    {
                        lastClearedCells.Add(new Vector2Int(x, y));
                        
                        // Handle special tile effects before clearing
                        SpecialTile specialTile = specialTiles[x, y];
                        if (specialTile.IsSpecial)
                        {
                            ActivateSpecialTile(x, y, specialTile);
                        }
                        
                        tiles[x, y] = -1; // Mark as empty
                        specialTiles[x, y] = new SpecialTile(-1, SpecialTileType.None); // Clear special tile
                        cleared++;
                        anyCleared = true;
                    }
                }
            }

            return anyCleared;
        }

        /// <summary>
        /// Create special tiles based on match patterns.
        /// </summary>
        private void CreateSpecialTiles(bool[,] matched)
        {
            // Find intersection points for L and T shapes
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (matched[x, y])
                    {
                        int value = tiles[x, y];
                        if (value < 0) continue;

                        // Check for 5+ in a line (creates Lightning)
                        if (IsFiveInLine(x, y, value))
                        {
                            CreateLightningTile(x, y, value);
                        }
                        // Check for L-shape (creates Bomb)
                        else if (IsLShape(x, y, value))
                        {
                            CreateBombTile(x, y, value);
                        }
                        // Check for T-shape (creates Star)
                        else if (IsTShape(x, y, value))
                        {
                            CreateStarTile(x, y, value);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Check if position is part of a 5+ tile line.
        /// </summary>
        private bool IsFiveInLine(int x, int y, int value)
        {
            // Check horizontal line
            int hStart = x, hEnd = x;
            while (hStart > 0 && tiles[hStart - 1, y] == value) hStart--;
            while (hEnd < width - 1 && tiles[hEnd + 1, y] == value) hEnd++;
            
            if (hEnd - hStart >= 4) return true;

            // Check vertical line
            int vStart = y, vEnd = y;
            while (vStart > 0 && tiles[x, vStart - 1] == value) vStart--;
            while (vEnd < height - 1 && tiles[x, vEnd + 1] == value) vEnd++;
            
            if (vEnd - vStart >= 4) return true;

            return false;
        }

        /// <summary>
        /// Create a lightning tile at the specified position.
        /// </summary>
        private void CreateLightningTile(int x, int y, int value)
        {
            specialTiles[x, y] = new SpecialTile(value, SpecialTileType.Lightning, 1);
            specialTilesCreated.Add(new Vector2Int(x, y));
        }

        /// <summary>
        /// Create a bomb tile at the specified position.
        /// </summary>
        private void CreateBombTile(int x, int y, int value)
        {
            specialTiles[x, y] = new SpecialTile(value, SpecialTileType.Bomb, 1);
            specialTilesCreated.Add(new Vector2Int(x, y));
        }

        /// <summary>
        /// Create a star tile at the specified position.
        /// </summary>
        private void CreateStarTile(int x, int y, int value)
        {
            specialTiles[x, y] = new SpecialTile(value, SpecialTileType.Star, 1);
            specialTilesCreated.Add(new Vector2Int(x, y));
        }

        /// <summary>
        /// Activate a special tile's effect.
        /// </summary>
        private void ActivateSpecialTile(int x, int y, SpecialTile specialTile)
        {
            switch (specialTile.type)
            {
                case SpecialTileType.Bomb:
                    ActivateBombEffect(x, y, specialTile.power);
                    break;
                case SpecialTileType.Lightning:
                    ActivateLightningEffect(x, y, specialTile.power);
                    break;
                case SpecialTileType.Star:
                    ActivateStarEffect(x, y, specialTile.power);
                    break;
                case SpecialTileType.Rainbow:
                    ActivateRainbowEffect(x, y, specialTile.power);
                    break;
            }
        }

        /// <summary>
        /// Activate bomb effect (clears 3x3 area).
        /// </summary>
        private void ActivateBombEffect(int x, int y, int power)
        {
            int radius = 1 + power; // 3x3 for power 1, 5x5 for power 2, etc.
            
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int targetX = x + dx;
                    int targetY = y + dy;
                    
                    if (targetX >= 0 && targetX < width && targetY >= 0 && targetY < height)
                    {
                        if (tiles[targetX, targetY] >= 0)
                        {
                            lastClearedCells.Add(new Vector2Int(targetX, targetY));
                            tiles[targetX, targetY] = -1;
                            specialTiles[targetX, targetY] = new SpecialTile(-1, SpecialTileType.None);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Activate lightning effect (clears entire row or column).
        /// </summary>
        private void ActivateLightningEffect(int x, int y, int power)
        {
            // Clear entire row
            for (int i = 0; i < width; i++)
            {
                if (tiles[i, y] >= 0)
                {
                    lastClearedCells.Add(new Vector2Int(i, y));
                    tiles[i, y] = -1;
                    specialTiles[i, y] = new SpecialTile(-1, SpecialTileType.None);
                }
            }
            
            // Clear entire column
            for (int i = 0; i < height; i++)
            {
                if (tiles[x, i] >= 0)
                {
                    lastClearedCells.Add(new Vector2Int(x, i));
                    tiles[x, i] = -1;
                    specialTiles[x, i] = new SpecialTile(-1, SpecialTileType.None);
                }
            }
        }

        /// <summary>
        /// Activate star effect (clears cross pattern).
        /// </summary>
        private void ActivateStarEffect(int x, int y, int power)
        {
            // Clear horizontal line
            for (int i = 0; i < width; i++)
            {
                if (tiles[i, y] >= 0)
                {
                    lastClearedCells.Add(new Vector2Int(i, y));
                    tiles[i, y] = -1;
                    specialTiles[i, y] = new SpecialTile(-1, SpecialTileType.None);
                }
            }
            
            // Clear vertical line
            for (int i = 0; i < height; i++)
            {
                if (tiles[x, i] >= 0)
                {
                    lastClearedCells.Add(new Vector2Int(x, i));
                    tiles[x, i] = -1;
                    specialTiles[x, i] = new SpecialTile(-1, SpecialTileType.None);
                }
            }
        }

        /// <summary>
        /// Activate rainbow effect (clears all tiles of one color).
        /// </summary>
        private void ActivateRainbowEffect(int x, int y, int power)
        {
            int targetValue = tiles[x, y];
            if (targetValue < 0) return;
            
            for (int ty = 0; ty < height; ty++)
            {
                for (int tx = 0; tx < width; tx++)
                {
                    if (tiles[tx, ty] == targetValue)
                    {
                        lastClearedCells.Add(new Vector2Int(tx, ty));
                        tiles[tx, ty] = -1;
                        specialTiles[tx, ty] = new SpecialTile(-1, SpecialTileType.None);
                    }
                }
            }
        }

        /// <summary>
        /// Collapse tiles down to fill empty spaces and spawn new tiles at the top.
        /// </summary>
        private void CollapseAndRefill()
        {
            // Collapse tiles down
            for (int x = 0; x < width; x++)
            {
                int writeY = 0;
                for (int y = 0; y < height; y++)
                {
                    if (tiles[x, y] >= 0)
                    {
                        if (writeY != y)
                        {
                            tiles[x, writeY] = tiles[x, y];
                            specialTiles[x, writeY] = specialTiles[x, y];
                            tiles[x, y] = -1;
                            specialTiles[x, y] = new SpecialTile(-1, SpecialTileType.None);
                        }
                        writeY++;
                    }
                }

                // Fill empty spaces with new tiles
                for (int y = writeY; y < height; y++)
                {
                    tiles[x, y] = GetRandomTileValue();
                    specialTiles[x, y] = new SpecialTile(-1, SpecialTileType.None);
                    lastSpawnedCells.Add(new Vector2Int(x, y));
                }
            }
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
            
            // Fallback to legacy random generation
            return random.Next(0, tileTypeCount);
        }

        /// <summary>
        /// Swap two tiles in the array.
        /// </summary>
        private void SwapTiles(Vector2Int a, Vector2Int b)
        {
            // Swap regular tiles
            int temp = tiles[a.x, a.y];
            tiles[a.x, a.y] = tiles[b.x, b.y];
            tiles[b.x, b.y] = temp;

            // Swap special tiles
            SpecialTile tempSpecial = specialTiles[a.x, a.y];
            specialTiles[a.x, a.y] = specialTiles[b.x, b.y];
            specialTiles[b.x, b.y] = tempSpecial;
        }

        /// <summary>
        /// Check if there are any matches on the board.
        /// </summary>
        private bool HasAnyMatch()
        {
            // Quick check for horizontal matches
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width - 2; x++)
                {
                    int value = tiles[x, y];
                    if (value >= 0 && value == tiles[x + 1, y] && value == tiles[x + 2, y])
                        return true;
                }
            }

            // Quick check for vertical matches
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height - 2; y++)
                {
                    int value = tiles[x, y];
                    if (value >= 0 && value == tiles[x, y + 1] && value == tiles[x, y + 2])
                        return true;
                }
            }

            return false;
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
                    int attempts = 0;
                    int value;
                    do
                    {
                        value = GetRandomTileValue();
                        attempts++;
                    } while (attempts < 10 && WouldCreateMatch(x, y, value));
                    
                    tiles[x, y] = value;
                    specialTiles[x, y] = new SpecialTile(-1, SpecialTileType.None);
                }
            }
        }

        /// <summary>
        /// Check if placing a tile at the given position would create an immediate match.
        /// </summary>
        private bool WouldCreateMatch(int x, int y, int value)
        {
            // Check horizontal matches
            if (x >= 2 && value == tiles[x - 1, y] && value == tiles[x - 2, y])
                return true;
            if (x >= 1 && x < width - 1 && value == tiles[x - 1, y] && value == tiles[x + 1, y])
                return true;
            if (x < width - 2 && value == tiles[x + 1, y] && value == tiles[x + 2, y])
                return true;

            // Check vertical matches
            if (y >= 2 && value == tiles[x, y - 1] && value == tiles[x, y - 2])
                return true;
            if (y >= 1 && y < height - 1 && value == tiles[x, y - 1] && value == tiles[x, y + 1])
                return true;
            if (y < height - 2 && value == tiles[x, y + 1] && value == tiles[x, y + 2])
                return true;

            return false;
        }
    }
}

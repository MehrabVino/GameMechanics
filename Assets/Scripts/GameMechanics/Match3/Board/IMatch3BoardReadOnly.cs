using System.Collections.Generic;
using UnityEngine;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Read-only interface for accessing Match3Board data.
    /// </summary>
    public interface IMatch3BoardReadOnly
    {
        int Width { get; }
        int Height { get; }
        int TileTypeCount { get; }
        int GetTile(int x, int y);
        SpecialTile GetSpecialTile(int x, int y);
        bool AreAdjacent(Vector2Int a, Vector2Int b);
        IReadOnlyList<Vector2Int> LastClearedCells { get; }
        IReadOnlyList<Vector2Int> LastSpawnedCells { get; }
        IReadOnlyList<Vector2Int> SpecialTilesCreated { get; }
    }
}




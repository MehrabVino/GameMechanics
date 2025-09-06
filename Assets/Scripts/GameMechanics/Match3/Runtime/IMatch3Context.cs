using UnityEngine;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Interface providing access to Match3 game context data.
    /// </summary>
    public interface IMatch3Context
    {
        IMatch3BoardReadOnly Board { get; }
        int BoardWidth { get; }
        int BoardHeight { get; }
        float CellSize { get; }
        Vector3 BoardOriginWorld { get; }
        Match3Theme Theme { get; }
        System.Collections.Generic.IReadOnlyList<Vector2Int> ClearedCells { get; }
        System.Collections.Generic.IReadOnlyList<Vector2Int> SpawnedCells { get; }
        System.Collections.Generic.IReadOnlyList<Vector2Int> SpecialTilesCreated { get; }
        int GetTileValue(int x, int y);
        SpecialTile GetSpecialTile(int x, int y);
    }
}


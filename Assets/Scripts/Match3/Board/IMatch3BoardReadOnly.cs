using System.Collections.Generic;
using UnityEngine;

namespace MechanicGames.Match3
{
	/// <summary>
	/// Read-only board contract to follow Interface Segregation and reduce coupling.
	/// </summary>
	public interface IMatch3BoardReadOnly
	{
		int Width { get; }
		int Height { get; }
		int GetTile(int x, int y);
		IReadOnlyList<Vector2Int> LastClearedCells { get; }
		IReadOnlyList<Vector2Int> LastSpawnedCells { get; }
	}
}




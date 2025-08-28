using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechanicGames.BlockBlast
{
	[Serializable]
	public sealed class BlockBlastBoard
	{
		[SerializeField]
		private int width = 9;

		[SerializeField]
		private int height = 9;

		[SerializeField]
		private bool[] occupied;

		[NonSerialized]
		public readonly List<Vector2Int> LastPlacedCells = new List<Vector2Int>();

		[NonSerialized]
		public readonly List<int> LastClearedRows = new List<int>();

		[NonSerialized]
		public readonly List<int> LastClearedCols = new List<int>();

		public int Width => width;
		public int Height => height;
		public bool IsReady => occupied != null && occupied.Length == width * height && width > 0 && height > 0;

		public void Initialize()
		{
			if (width <= 0) width = 9;
			if (height <= 0) height = 9;
			occupied = new bool[width * height];
			LastPlacedCells.Clear();
			LastClearedRows.Clear();
			LastClearedCols.Clear();
		}

		private int Index(int x, int y) => y * width + x;

		public bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < width && y < height;

		public bool IsOccupied(int x, int y)
		{
			if (!IsInside(x, y) || !IsReady) return false;
			return occupied[Index(x, y)];
		}

		public bool TryPlace(IReadOnlyList<Vector2Int> localCells, Vector2Int boardPos, out int cleared)
		{
			cleared = 0;
			if (!IsReady || localCells == null || localCells.Count == 0) return false;
			for (int i = 0; i < localCells.Count; i++)
			{
				int x = boardPos.x + localCells[i].x;
				int y = boardPos.y + localCells[i].y;
				if (!IsInside(x, y) || IsOccupied(x, y)) return false;
			}
			LastPlacedCells.Clear();
			for (int i = 0; i < localCells.Count; i++)
			{
				int x = boardPos.x + localCells[i].x;
				int y = boardPos.y + localCells[i].y;
				occupied[Index(x, y)] = true;
				LastPlacedCells.Add(new Vector2Int(x, y));
			}
			// Clear full rows/cols
			LastClearedRows.Clear();
			LastClearedCols.Clear();
			for (int y = 0; y < height; y++)
			{
				bool full = true;
				for (int x = 0; x < width; x++) if (!occupied[Index(x, y)]) { full = false; break; }
				if (full) LastClearedRows.Add(y);
			}
			for (int x = 0; x < width; x++)
			{
				bool full = true;
				for (int y = 0; y < height; y++) if (!occupied[Index(x, y)]) { full = false; break; }
				if (full) LastClearedCols.Add(x);
			}
			for (int i = 0; i < LastClearedRows.Count; i++)
			{
				int y = LastClearedRows[i];
				for (int x = 0; x < width; x++) occupied[Index(x, y)] = false;
			}
			for (int i = 0; i < LastClearedCols.Count; i++)
			{
				int x = LastClearedCols[i];
				for (int y = 0; y < height; y++) occupied[Index(x, y)] = false;
			}
			cleared = LastClearedRows.Count + LastClearedCols.Count;
			return true;
		}
	}
}



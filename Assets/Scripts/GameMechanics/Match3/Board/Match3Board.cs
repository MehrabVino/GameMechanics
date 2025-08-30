using System;
using System.Collections.Generic;
using UnityEngine;

namespace MechanicGames.Match3
{
	/// <summary>
	/// Pure logic container for a match-3 grid.
	/// </summary>
	[Serializable]
	public sealed class Match3Board : IMatch3BoardReadOnly
	{
		public int Width => width;
		public int Height => height;

		[SerializeField]
		private int width = 8;

		[SerializeField]
		private int height = 8;

		[SerializeField]
		private int numberOfTileTypes = 6;

		[SerializeField]
		private int seed = 0;

		private System.Random random;
		private int[,] tiles;
		private readonly List<Vector2Int> lastClearedCells = new List<Vector2Int>();
		private readonly List<Vector2Int> lastSpawnedCells = new List<Vector2Int>();

		public IReadOnlyList<Vector2Int> LastClearedCells => lastClearedCells;
		public IReadOnlyList<Vector2Int> LastSpawnedCells => lastSpawnedCells;

		public void Initialize()
		{
			random = seed == 0 ? new System.Random() : new System.Random(seed);
			tiles = new int[width, height];
			FillBoardWithoutImmediateMatches();
		}

		public void SetTileTypeCount(int count)
		{
			numberOfTileTypes = Mathf.Max(1, count);
		}

		public int GetTile(int x, int y)
		{
			return tiles[x, y];
		}

		public bool TrySwapAndResolve(Vector2Int a, Vector2Int b, out int totalCleared)
		{
			return TrySwapAndResolve(a, b, out totalCleared, out _);
		}

		/// <summary>
		/// Swap two cells, resolve all cascades, and report tiles cleared and cascade chain count.
		/// Returns false if the swap does not produce any match (and reverts the swap).
		/// </summary>
		public bool TrySwapAndResolve(Vector2Int a, Vector2Int b, out int totalCleared, out int chainCount)
		{
			totalCleared = 0;
			chainCount = 0;
			lastClearedCells.Clear();
			lastSpawnedCells.Clear();
			if (!AreAdjacent(a, b))
			{
				return false;
			}

			Swap(a, b);

			if (!HasAnyMatch())
			{
				Swap(a, b);
				return false;
			}

			bool found;
			do
			{
				bool[,] matched = FindMatches();
				found = ClearMatches(matched, out int cleared);
				totalCleared += cleared;
				if (found)
				{
					chainCount++;
				}
				if (found)
				{
					CollapseAndRefill();
				}
			}
			while (found);

			return true;
		}

		public bool AreAdjacent(Vector2Int a, Vector2Int b)
		{
			int dx = Mathf.Abs(a.x - b.x);
			int dy = Mathf.Abs(a.y - b.y);
			return dx + dy == 1;
		}

		private void FillBoardWithoutImmediateMatches()
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					int value;
					int guard = 0;
					do
					{
						value = random.Next(0, numberOfTileTypes);
						guard++;
					}
					while (CreatesImmediateMatch(x, y, value) && guard < 100);
					tiles[x, y] = value;
				}
			}
		}

		private bool CreatesImmediateMatch(int x, int y, int value)
		{
			if (x >= 2)
			{
				if (tiles[x - 1, y] == value && tiles[x - 2, y] == value)
				{
					return true;
				}
			}
			if (y >= 2)
			{
				if (tiles[x, y - 1] == value && tiles[x, y - 2] == value)
				{
					return true;
				}
			}
			return false;
		}

		private void Swap(Vector2Int a, Vector2Int b)
		{
			int temp = tiles[a.x, a.y];
			tiles[a.x, a.y] = tiles[b.x, b.y];
			tiles[b.x, b.y] = temp;
		}

		private bool HasAnyMatch()
		{
			bool[,] matched = FindMatches();
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (matched[x, y])
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool[,] FindMatches()
		{
			bool[,] matched = new bool[width, height];

			for (int y = 0; y < height; y++)
			{
				int runLength = 1;
				for (int x = 1; x < width; x++)
				{
					if (tiles[x, y] == tiles[x - 1, y])
					{
						runLength++;
					}
					else
					{
						if (runLength >= 3)
						{
							for (int k = 0; k < runLength; k++)
							{
								matched[x - 1 - k, y] = true;
							}
						}
						runLength = 1;
					}
				}
				if (runLength >= 3)
				{
					for (int k = 0; k < runLength; k++)
					{
						matched[width - 1 - k, y] = true;
					}
				}
			}

			for (int x = 0; x < width; x++)
			{
				int runLength = 1;
				for (int y = 1; y < height; y++)
				{
					if (tiles[x, y] == tiles[x, y - 1])
					{
						runLength++;
					}
					else
					{
						if (runLength >= 3)
						{
							for (int k = 0; k < runLength; k++)
							{
								matched[x, y - 1 - k] = true;
							}
						}
						runLength = 1;
					}
				}
				if (runLength >= 3)
				{
					for (int k = 0; k < runLength; k++)
					{
						matched[x, height - 1 - k] = true;
					}
				}
			}

			return matched;
		}

		private bool ClearMatches(bool[,] matched, out int clearedCount)
		{
			bool any = false;
			int count = 0;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (matched[x, y])
					{
						tiles[x, y] = -1;
						any = true;
						count++;
						lastClearedCells.Add(new Vector2Int(x, y));
					}
				}
			}
			clearedCount = count;
			return any;
		}

		private void CollapseAndRefill()
		{
			for (int x = 0; x < width; x++)
			{
				int writeY = 0;
				for (int y = 0; y < height; y++)
				{
					if (tiles[x, y] != -1)
					{
						if (y != writeY)
						{
							tiles[x, writeY] = tiles[x, y];
							tiles[x, y] = -1;
						}
						writeY++;
					}
				}
				for (int y = writeY; y < height; y++)
				{
					tiles[x, y] = random.Next(0, numberOfTileTypes);
					lastSpawnedCells.Add(new Vector2Int(x, y));
				}
			}
		}
	}
}




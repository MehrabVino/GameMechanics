using MechanicGames.Shared;
using UnityEngine;

namespace MechanicGames.Match3
{
	/// <summary>
	/// MonoBehaviour wrapper that exposes a Match3 board with simple mouse input and Gizmos rendering.
	/// </summary>
	public sealed class Match3Mechanic : MonoBehaviour, IGameMechanic, IMatch3Context
	{
		[SerializeField]
		private MechanicGames.Match3.Match3Board board = new MechanicGames.Match3.Match3Board();

		[SerializeField]
		private float cellSize = 1.0f;

		[SerializeField]
		private Vector2 boardOriginLocal = Vector2.zero;

		[SerializeField]
		private bool drawGizmos = true;

		[SerializeField]
		private Match3Theme theme;

		private bool isActive;
		private bool hasSelection;
		private Vector2Int selectedCell;

		[SerializeField]
		private int scorePerTile = 10;

		[SerializeField]
		private int bonusPerChain = 5;

		private int currentScore;

		public int Score => currentScore;

		// IGameMechanic interface implementation
		public bool IsGameActive => isActive;
		public bool IsPaused => false; // Match3 doesn't have pause functionality
		public int CurrentScore => currentScore;
		public int HighScore => PlayerPrefs.GetInt("Match3_HighScore", 0);
		
		// Events
		public System.Action OnGameStart { get; set; }
		public System.Action OnGamePause { get; set; }
		public System.Action OnGameResume { get; set; }
		public System.Action OnGameOver { get; set; }
		public System.Action<int> OnScoreChanged { get; set; }

		public int BoardWidth => board != null ? board.Width : 0;
		public int BoardHeight => board != null ? board.Height : 0;
		public float CellSize => cellSize;
		public Vector3 BoardOriginWorld => transform.TransformPoint(new Vector3(boardOriginLocal.x, boardOriginLocal.y, 0f));
		public Match3Theme Theme => theme;
		public System.Collections.Generic.IReadOnlyList<Vector2Int> ClearedCells => board != null ? board.LastClearedCells : null;
		public System.Collections.Generic.IReadOnlyList<Vector2Int> SpawnedCells => board != null ? board.LastSpawnedCells : null;

		IMatch3BoardReadOnly IMatch3Context.Board => board;

		public void SetTheme(Match3Theme newTheme)
		{
			theme = newTheme;
			if (theme != null)
			{
				board.SetTileTypeCount(theme.TileTypeCount);
			}
			board.Initialize();
			Match3RuntimeView view = GetComponentInChildren<Match3RuntimeView>();
			if (view != null)
			{
				view.RefreshFromMechanic();
			}
		}

		public int GetTileValue(int x, int y)
		{
			return board.GetTile(x, y);
		}

		public void InitializeMechanic()
		{
			if (theme != null)
			{
				board.SetTileTypeCount(theme.TileTypeCount);
			}
			board.Initialize();
			currentScore = 0;
		}

		public void SetMechanicActive(bool value)
		{
			isActive = value;
			enabled = value;
		}

		// IGameMechanic interface methods
		public void StartGame()
		{
			isActive = true;
			enabled = true;
			currentScore = 0;
			OnGameStart?.Invoke();
		}

		public void PauseGame()
		{
			// Match3 doesn't support pause, but we'll implement it for interface compliance
			OnGamePause?.Invoke();
		}

		public void ResumeGame()
		{
			// Match3 doesn't support pause, but we'll implement it for interface compliance
			OnGameResume?.Invoke();
		}

		public void EndGame()
		{
			isActive = false;
			enabled = false;
			// Save high score
			if (currentScore > HighScore)
			{
				PlayerPrefs.SetInt("Match3_HighScore", currentScore);
				PlayerPrefs.Save();
			}
			OnGameOver?.Invoke();
		}

		public void ResetGame()
		{
			currentScore = 0;
			if (board != null)
			{
				board.Initialize();
			}
		}

		public void UpdateScore(int newScore)
		{
			currentScore = newScore;
			OnScoreChanged?.Invoke(currentScore);
		}

		public void AddScore(int points)
		{
			currentScore += points;
			OnScoreChanged?.Invoke(currentScore);
		}

		private void Update()
		{
			if (!isActive)
			{
				return;
			}

			if (Input.GetMouseButtonDown(0))
			{
				if (TryGetCellUnderMouse(out Vector2Int cell))
				{
					hasSelection = true;
					selectedCell = cell;
				}
			}

			if (hasSelection && Input.GetMouseButtonUp(0))
			{
				if (TryGetCellUnderMouse(out Vector2Int releaseCell))
				{
					if (board.AreAdjacent(selectedCell, releaseCell))
					{
						StartCoroutine(HandleSwapAndResolve(selectedCell, releaseCell));
					}
					else if (selectedCell == releaseCell)
					{
						// No-op click on the same cell.
					}
					else
					{
						// If not adjacent, treat this as a new selection next time.
					}
				}
				hasSelection = false;
			}
		}

		private System.Collections.IEnumerator HandleSwapAndResolve(Vector2Int a, Vector2Int b)
		{
			// Trigger swap animation in view if available
			Match3RuntimeView view = GetComponentInChildren<Match3RuntimeView>();
			if (view != null)
			{
				yield return view.AnimateSwap(a, b, CellSize, BoardOriginWorld);
			}
			if (board.TrySwapAndResolve(a, b, out int cleared, out int chains))
			{
				int baseScore = cleared * scorePerTile;
				int chainBonus = Mathf.Max(0, chains - 1) * bonusPerChain * cleared;
				AddScore(baseScore + chainBonus);
				if (view != null)
				{
					Vector3 popupPos = BoardOriginWorld + new Vector3((a.x + b.x + 1f) * 0.5f * CellSize, (a.y + b.y + 1f) * 0.5f * CellSize, 0f);
					view.ShowScorePopup(popupPos, baseScore + chainBonus);
					view.PulseClears();
					view.PulseNeighborsOfClears();
					view.FlashSpawned();
				}
			}
			else
			{
				// invalid swap -> swap back animation
				if (view != null)
				{
					yield return view.AnimateSwapBack(a, b, CellSize, BoardOriginWorld);
				}
			}
			yield break;
		}

		private bool TryGetCellUnderMouse(out Vector2Int cell)
		{
			cell = default;
			Camera camera = Camera.main;
			if (camera == null)
			{
				return false;
			}
			Vector3 world = camera.ScreenToWorldPoint(Input.mousePosition);
			Vector3 local = transform.InverseTransformPoint(world);
			Vector2 start = boardOriginLocal;
			float xLocal = local.x - start.x;
			float yLocal = local.y - start.y;
			if (xLocal < 0f || yLocal < 0f)
			{
				return false;
			}
			int x = Mathf.FloorToInt(xLocal / cellSize);
			int y = Mathf.FloorToInt(yLocal / cellSize);
			if (x < 0 || y < 0 || x >= board.Width || y >= board.Height)
			{
				return false;
			}
			cell = new Vector2Int(x, y);
			return true;
		}

		private void OnDrawGizmos()
		{
			if (!drawGizmos)
			{
				return;
			}

			if (board == null)
			{
				return;
			}

			Vector3 originWorld = transform.TransformPoint(new Vector3(boardOriginLocal.x, boardOriginLocal.y, 0f));
			for (int y = 0; y < board.Height; y++)
			{
				for (int x = 0; x < board.Width; x++)
				{
					int v;
					try
					{
						v = board.GetTile(x, y);
					}
					catch
					{
						continue;
					}

					Color color = GetColorForValue(v);
					Gizmos.color = color;
					Vector3 center = originWorld + new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0f);
					Vector3 size = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 0.1f);
					Gizmos.DrawCube(center, size);
					Gizmos.color = Color.black;
					Gizmos.DrawWireCube(center, size);
				}
			}
		}

		private void OnGUI()
		{
			GUIStyle style = new GUIStyle(GUI.skin.label);
			style.fontSize = 18;
			style.normal.textColor = Color.white;
			Rect r = new Rect(12, 12, 300, 40);
			GUI.Label(r, $"Score: {currentScore}", style);
		}

		public static Color GetColorForValue(int value)
		{
			if (value < 0)
			{
				return new Color(0f, 0f, 0f, 0.1f);
			}
			int idx = value % 6;
			switch (idx)
			{
				case 0: return Color.red;
				case 1: return Color.green;
				case 2: return Color.blue;
				case 3: return Color.yellow;
				case 4: return new Color(0.6f, 0f, 0.8f);
				default: return new Color(1f, 0.5f, 0f);
			}
		}
	}
}




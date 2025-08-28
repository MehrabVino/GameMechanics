using System.Collections.Generic;
using MechanicGames.Core;
using UnityEngine;

namespace MechanicGames.BlockBlast
{
	public sealed class BlockBlastMechanic : MonoBehaviour, IGameMechanic
	{
		[SerializeField]
		private BlockBlastBoard board = new BlockBlastBoard();

		[SerializeField]
		private float cellSize = 1.0f;

		[SerializeField]
		private Vector2 boardOriginLocal = Vector2.zero;

		[SerializeField]
		private GameObject tilePrefab; // optional prefab

		[SerializeField]
		private QuadTileFactory quadTileFactory; // fallback factory

		[SerializeField]
		private GameObject popupPrefab; // TextMesh popup (optional)

		private readonly List<List<Vector2Int>> queue = new List<List<Vector2Int>>(3);
		private int selected = 0;
		private int score;
		private Transform tilesRoot;
		private readonly List<Renderer> renderers = new List<Renderer>();
		private readonly List<Renderer> placeholder = new List<Renderer>();
		private SimpleGameObjectPool popupPool;

		// Cell border/background grid so cells are visible even when empty
		[SerializeField]
		private Color cellBackgroundColor = new Color(0f, 0f, 0f, 0.25f);

		[SerializeField]
		private float backgroundZ = -0.05f;

		private Transform backgroundsRoot;
		private MeshFilter backgroundMeshFilter;
		private MeshRenderer backgroundMeshRenderer;

		public int Score => score;
		public float CellSize => cellSize;
		public Vector3 BoardOriginWorld => transform.TransformPoint(new Vector3(boardOriginLocal.x, boardOriginLocal.y, 0f));

		public void InitializeMechanic()
		{
			board.Initialize();
			score = 0;
			queue.Clear();
			for (int i = 0; i < 3; i++) queue.Add(GenerateShape());
			selected = 0;
			EnsureTiles();
			EnsureBackgrounds();
		}

		public void SetMechanicActive(bool value)
		{
			enabled = value;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Alpha1)) selected = 0;
			if (Input.GetKeyDown(KeyCode.Alpha2)) selected = 1;
			if (Input.GetKeyDown(KeyCode.Alpha3)) selected = 2;
			if (Input.GetKeyDown(KeyCode.R)) RotateSelected();

			// Hover placeholder
			if (TryGetCellUnderMouse(out Vector2Int hoverCell))
			{
				RenderPlaceholder(hoverCell);
			}
			else
			{
				HidePlaceholder();
			}

			if (Input.GetMouseButtonDown(0) && TryGetCellUnderMouse(out Vector2Int cell))
			{
				List<Vector2Int> shape = queue[selected];
				if (board.TryPlace(shape, cell, out int cleared))
				{
					score += shape.Count + cleared * 10;
					queue[selected] = GenerateShape();
					if (popupPrefab != null)
					{
						if (popupPool == null) popupPool = new SimpleGameObjectPool(popupPrefab, transform);
						Vector3 p = BoardOriginWorld + new Vector3((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize, 0f);
						GameObject go = popupPool.Activate();
						go.transform.position = p;
						TextMesh tm = go.GetComponentInChildren<TextMesh>();
						if (tm != null) tm.text = $"+{shape.Count + cleared * 10}";
						StartCoroutine(RecycleAfter(go, 0.8f));
					}
					StartCoroutine(PopAnimation(board.LastPlacedCells));
					if (board.LastClearedRows.Count > 0 || board.LastClearedCols.Count > 0)
					{
						StartCoroutine(FlashClears());
					}
					SyncTiles();
				}
			}
		}

		private System.Collections.IEnumerator PopAnimation(IReadOnlyList<Vector2Int> cells)
		{
			if (cells == null) yield break;
			float t = 0f;
			while (t < 0.15f)
			{
				t += Time.deltaTime;
				float s = 1f + Mathf.Sin((t / 0.15f) * Mathf.PI) * 0.25f;
				for (int i = 0; i < cells.Count; i++)
				{
					int idx = cells[i].y * board.Width + cells[i].x;
					Renderer r = idx < renderers.Count ? renderers[idx] : null;
					if (r != null)
					{
						r.transform.localScale = new Vector3(cellSize * s, cellSize * s, 1f);
					}
				}
				yield return null;
			}
			for (int i = 0; i < cells.Count; i++)
			{
				int idx = cells[i].y * board.Width + cells[i].x;
				Renderer r = idx < renderers.Count ? renderers[idx] : null;
				if (r != null) r.transform.localScale = new Vector3(cellSize, cellSize, 1f);
			}
		}

		private System.Collections.IEnumerator FlashClears()
		{
			float t = 0f;
			while (t < 0.15f)
			{
				t += Time.deltaTime;
				float a = Mathf.PingPong(t * 8f, 1f);
				// Pulse cleared rows
				for (int i = 0; i < board.LastClearedRows.Count; i++)
				{
					int y = board.LastClearedRows[i];
					for (int x = 0; x < board.Width; x++)
					{
						int idx = y * board.Width + x;
						Renderer r = idx < renderers.Count ? renderers[idx] : null;
						if (r != null && r.enabled)
						{
							Color c = r.sharedMaterial != null ? r.sharedMaterial.color : Color.white;
							SetRendererColor(r, new Color(c.r, c.g, c.b, Mathf.Lerp(0.4f, 1f, a)));
						}
					}
				}
				// Pulse cleared columns
				for (int i = 0; i < board.LastClearedCols.Count; i++)
				{
					int x = board.LastClearedCols[i];
					for (int y = 0; y < board.Height; y++)
					{
						int idx = y * board.Width + x;
						Renderer r = idx < renderers.Count ? renderers[idx] : null;
						if (r != null && r.enabled)
						{
							Color c = r.sharedMaterial != null ? r.sharedMaterial.color : Color.white;
							SetRendererColor(r, new Color(c.r, c.g, c.b, Mathf.Lerp(0.4f, 1f, a)));
						}
					}
				}
				yield return null;
			}
			// restore alpha
			for (int y = 0; y < board.Height; y++)
			{
				for (int x = 0; x < board.Width; x++)
				{
					int idx = y * board.Width + x;
					Renderer r = idx < renderers.Count ? renderers[idx] : null;
					if (r != null && r.sharedMaterial != null)
					{
						Color c = r.sharedMaterial.color;
						SetRendererColor(r, new Color(c.r, c.g, c.b, 1f));
					}
				}
			}
		}

		private void SetRendererColor(Renderer r, Color color)
		{
			if (r == null) return;
			// instance material per renderer to avoid global changes
			if (r.material != null)
			{
				r.material.color = color;
			}
		}

		private void RenderPlaceholder(Vector2Int baseCell)
		{
			EnsurePlaceholderSize(queue[selected].Count);
			Vector3 origin = BoardOriginWorld;
			for (int i = 0; i < queue[selected].Count; i++)
			{
				Renderer r = placeholder[i];
				if (r == null) continue;
				Vector2Int s = queue[selected][i];
				int x = baseCell.x + s.x;
				int y = baseCell.y + s.y;
				bool inside = board.IsInside(x, y) && !board.IsOccupied(x, y);
				r.enabled = inside;
				Vector3 center = origin + new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, -0.1f);
				r.transform.position = center;
				r.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1f);
			}
		}

		private void HidePlaceholder()
		{
			for (int i = 0; i < placeholder.Count; i++) if (placeholder[i] != null) placeholder[i].enabled = false;
		}

		private void EnsurePlaceholderSize(int count)
		{
			while (placeholder.Count < count)
			{
				GameObject go = tilePrefab != null ? Instantiate(tilePrefab, transform) : GameObject.CreatePrimitive(PrimitiveType.Quad);
				go.name = "Placeholder";
				Renderer r = go.GetComponent<Renderer>();
				if (r == null) r = go.AddComponent<MeshRenderer>();
				Collider col = go.GetComponent<Collider>();
				if (col != null) Destroy(col);
				if (r.sharedMaterial != null)
				{
					Color c = r.sharedMaterial.color;
					r.sharedMaterial = new Material(r.sharedMaterial);
					r.sharedMaterial.color = new Color(c.r, c.g, c.b, 0.35f);
				}
				placeholder.Add(r);
			}
		}

		private System.Collections.IEnumerator RecycleAfter(GameObject go, float seconds)
		{
			yield return new WaitForSeconds(seconds);
			if (popupPool != null && go != null) popupPool.Deactivate(go);
		}

		private List<Vector2Int> GenerateShape()
		{
			switch (Random.Range(0, 6))
			{
				case 0: return new List<Vector2Int> { new Vector2Int(0,0) };
				case 1: return new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0) };
				case 2: return new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1) };
				case 3: return new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0) };
				case 4: return new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,0), new Vector2Int(1,1) };
				default: return new List<Vector2Int> { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(0,1) };
			}
		}

		private void RotateSelected()
		{
			List<Vector2Int> s = queue[selected];
			for (int i = 0; i < s.Count; i++)
			{
				Vector2Int c = s[i];
				s[i] = new Vector2Int(c.y, -c.x);
			}
		}

		private bool TryGetCellUnderMouse(out Vector2Int cell)
		{
			cell = default;
			Camera cam = Camera.main;
			if (cam == null) return false;
			Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
			Vector3 local = transform.InverseTransformPoint(world);
			float xLocal = local.x - boardOriginLocal.x;
			float yLocal = local.y - boardOriginLocal.y;
			if (xLocal < 0f || yLocal < 0f) return false;
			int x = Mathf.FloorToInt(xLocal / cellSize);
			int y = Mathf.FloorToInt(yLocal / cellSize);
			if (!board.IsInside(x, y)) return false;
			cell = new Vector2Int(x, y);
			return true;
		}

		private void EnsureTiles()
		{
			if (tilesRoot == null)
			{
				GameObject tr = new GameObject("Tiles");
				tr.transform.SetParent(transform, false);
				tilesRoot = tr.transform;
			}
			if (renderers.Count != board.Width * board.Height)
			{
				for (int i = 0; i < renderers.Count; i++)
				{
					if (renderers[i] != null) Destroy(renderers[i].gameObject);
				}
				renderers.Clear();
				for (int y = 0; y < board.Height; y++)
				{
					for (int x = 0; x < board.Width; x++)
					{
						Renderer r = null;
						if (tilePrefab != null)
						{
							GameObject go = Instantiate(tilePrefab, tilesRoot);
							r = go.GetComponent<Renderer>();
							if (r == null) r = go.AddComponent<MeshRenderer>();
							Collider pc = go.GetComponent<Collider>();
							if (pc != null) Destroy(pc);
						}
						else if (quadTileFactory != null)
						{
							r = quadTileFactory.CreateTile(tilesRoot);
						}
						else
						{
							GameObject go = GameObject.CreatePrimitive(PrimitiveType.Quad);
							go.transform.SetParent(tilesRoot, false);
							Collider pc = go.GetComponent<Collider>();
							if (pc != null) Destroy(pc);
							r = go.GetComponent<Renderer>();
							if (r == null) r = go.AddComponent<MeshRenderer>();
						}
						r.name = $"Cell_{x}_{y}";
						renderers.Add(r);
					}
				}
			}
			SyncTiles();
		}

		private void EnsureBackgrounds()
		{
			if (backgroundsRoot == null)
			{
				GameObject br = new GameObject("Backgrounds");
				br.transform.SetParent(transform, false);
				backgroundsRoot = br.transform;
			}
			if (backgroundMeshFilter == null)
			{
				backgroundMeshFilter = backgroundsRoot.gameObject.GetComponent<MeshFilter>();
				if (backgroundMeshFilter == null) backgroundMeshFilter = backgroundsRoot.gameObject.AddComponent<MeshFilter>();
			}
			if (backgroundMeshRenderer == null)
			{
				backgroundMeshRenderer = backgroundsRoot.gameObject.GetComponent<MeshRenderer>();
				if (backgroundMeshRenderer == null) backgroundMeshRenderer = backgroundsRoot.gameObject.AddComponent<MeshRenderer>();
			}
			if (backgroundMeshRenderer.sharedMaterial == null)
			{
				Material mat = new Material(Shader.Find("Unlit/Color"));
				mat.color = cellBackgroundColor;
				backgroundMeshRenderer.sharedMaterial = mat;
			}
			backgroundMeshFilter.sharedMesh = GridBackgroundMesh.BuildGridQuads(board.Width, board.Height, cellSize, BoardOriginWorld, backgroundZ);
		}

		private void SyncTiles()
		{
			if (!board.IsReady) return;
			Vector3 origin = BoardOriginWorld;
			for (int y = 0; y < board.Height; y++)
			{
				for (int x = 0; x < board.Width; x++)
				{
					int idx = y * board.Width + x;
					Renderer r = idx < renderers.Count ? renderers[idx] : null;
					if (r == null) continue;
					bool occ = board.IsOccupied(x, y);
					r.enabled = occ;
					Vector3 center = origin + new Vector3((x + 0.5f) * cellSize, (y + 0.5f) * cellSize, 0f);
					r.transform.position = center;
					// slightly smaller than background to create a visible border
					r.transform.localScale = new Vector3(cellSize * 0.9f, cellSize * 0.9f, 1f);
				}
			}
			SyncBackgrounds();
		}

		private void SyncBackgrounds()
		{
			if (!board.IsReady || backgroundMeshFilter == null) return;
			backgroundMeshFilter.sharedMesh = GridBackgroundMesh.BuildGridQuads(board.Width, board.Height, cellSize, BoardOriginWorld, backgroundZ);
			if (backgroundMeshRenderer != null)
			{
				backgroundMeshRenderer.enabled = true;
				backgroundMeshRenderer.sharedMaterial.color = cellBackgroundColor;
			}
		}
	}
}



using UnityEngine;

namespace MechanicGames.Match3
{
	/// <summary>
	/// Runtime tile rendering for Match-3 using generated SpriteRenderers (no external assets required).
	/// </summary>
	public sealed class Match3RuntimeView : MonoBehaviour
	{
		private Match3Mechanic mechanic;
		private Sprite tileSprite;
		private GameObject[,] tileObjects;
		private Vector3[,] restingPositions;
		private int[,] lastValues;
		[SerializeField]
		private float lastPulseTime;
		[SerializeField]
		private float[,] cellPulseUntil;
		[SerializeField]
		private float[,] cellNeighborPulseUntil;
		[SerializeField]
		private float[,] cellGlowUntil;
		[SerializeField]
		private Color[,] baseColors;
		private readonly System.Collections.Generic.List<GameObject> activePopups = new System.Collections.Generic.List<GameObject>();
		private GameObject backgroundObject;

		public void Bind(Match3Mechanic targetMechanic)
		{
			mechanic = targetMechanic;
			RebuildTiles();
		}

		private void Awake()
		{
			if (mechanic == null)
			{
				mechanic = GetComponentInParent<Match3Mechanic>();
			}
		}

		private void Update()
		{
			if (mechanic == null || mechanic.BoardWidth == 0 || mechanic.BoardHeight == 0)
			{
				return;
			}
			EnsureSprite();
			if (tileObjects == null || tileObjects.GetLength(0) != mechanic.BoardWidth || tileObjects.GetLength(1) != mechanic.BoardHeight)
			{
				RebuildTiles();
			}

			for (int y = 0; y < mechanic.BoardHeight; y++)
			{
				for (int x = 0; x < mechanic.BoardWidth; x++)
				{
					GameObject go = tileObjects[x, y];
					if (go == null)
					{
						continue;
					}
					int v = mechanic.GetTileValue(x, y);
					SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
					if (v < 0)
					{
						sr.enabled = false;
					}
					else
					{
						sr.enabled = true;
						if (lastValues[x, y] != v)
						{
							ApplyThemeToSprite(sr, v);
							lastValues[x, y] = v;
							if (baseColors != null)
							{
								baseColors[x, y] = sr.color;
							}
						}
						go.transform.position = Vector3.Lerp(go.transform.position, restingPositions[x, y], 12f * Time.deltaTime);
						float timeLeft = cellPulseUntil != null ? (cellPulseUntil[x, y] - Time.time) : 0f;
						float neighLeft = cellNeighborPulseUntil != null ? (cellNeighborPulseUntil[x, y] - Time.time) : 0f;
						float pulse = Mathf.Max(
							Mathf.Clamp01(timeLeft),
							Mathf.Clamp01(neighLeft * 0.6f)
						);
						float scale = 1f + pulse * 0.15f;
						go.transform.localScale = new Vector3(scale, scale, 1f);
						// Glow newly spawned tiles
						if (cellGlowUntil != null && baseColors != null)
						{
							float glowLeft = cellGlowUntil[x, y] - Time.time;
							if (glowLeft > 0f)
							{
								float t = Mathf.Clamp01(glowLeft);
								Color c = baseColors[x, y];
								sr.color = Color.Lerp(c, Color.white, 0.5f * t);
							}
							else
							{
								sr.color = baseColors[x, y];
							}
						}
					}
				}
			}

			for (int i = activePopups.Count - 1; i >= 0; i--)
			{
				GameObject p = activePopups[i];
				if (p == null)
				{
					activePopups.RemoveAt(i);
					continue;
				}
				TextMesh tm = p.GetComponent<TextMesh>();
				Color c = tm.color;
				p.transform.position += new Vector3(0f, Time.deltaTime * 0.8f, 0f);
				c.a -= Time.deltaTime * 1.2f;
				tm.color = c;
				if (c.a <= 0f)
				{
					Destroy(p);
					activePopups.RemoveAt(i);
				}
			}
		}



		public System.Collections.IEnumerator AnimateSwapBack(Vector2Int a, Vector2Int b, float cellSize, Vector3 origin)
		{
			if (tileObjects == null)
			{
				yield break;
			}
			GameObject aObj = tileObjects[a.x, a.y];
			GameObject bObj = tileObjects[b.x, b.y];
			if (aObj == null || bObj == null)
			{
				yield break;
			}
			Vector3 bRest = origin + new Vector3((a.x + 0.5f) * cellSize, (a.y + 0.5f) * cellSize, 0f);
			Vector3 aRest = origin + new Vector3((b.x + 0.5f) * cellSize, (b.y + 0.5f) * cellSize, 0f);
			float t = 0f;
			while (t < 1f)
			{
				t += Time.deltaTime * 8f;
				aObj.transform.position = Vector3.Lerp(aRest, bRest, t);
				bObj.transform.position = Vector3.Lerp(bRest, aRest, t);
				yield return null;
			}
			aObj.transform.position = bRest;
			bObj.transform.position = aRest;
		}

		private void RebuildTiles()
		{
			if (mechanic == null)
			{
				return;
			}
			EnsureSprite();
			DestroyExistingChildren();
			int w = mechanic.BoardWidth;
			int h = mechanic.BoardHeight;
			tileObjects = new GameObject[w, h];
			restingPositions = new Vector3[w, h];
			lastValues = new int[w, h];
			cellPulseUntil = new float[w, h];
			cellNeighborPulseUntil = new float[w, h];
			cellGlowUntil = new float[w, h];
			baseColors = new Color[w, h];
			Transform parent = transform;
			Vector3 originWorld = mechanic.BoardOriginWorld;
			float cell = mechanic.CellSize;
			// Background
			Match3Theme theme = mechanic != null ? mechanic.Theme : null;
			if (theme != null)
			{
				backgroundObject = new GameObject("Background");
				backgroundObject.transform.SetParent(parent, false);
				backgroundObject.transform.position = originWorld + new Vector3((w * cell) * 0.5f, (h * cell) * 0.5f, 1f);
				SpriteRenderer bsr = backgroundObject.AddComponent<SpriteRenderer>();
				bsr.sprite = theme.backgroundSprite != null ? theme.backgroundSprite : CreateSolidSprite(theme.backgroundColor);
				bsr.color = Color.white;
				bsr.drawMode = SpriteDrawMode.Sliced;
				bsr.size = new Vector2(w * cell * 1.05f, h * cell * 1.05f);
				bsr.sortingOrder = -10;
			}
			for (int y = 0; y < h; y++)
			{
				for (int x = 0; x < w; x++)
				{
					GameObject child = new GameObject($"Tile_{x}_{y}");
					child.transform.SetParent(parent, false);
					Vector3 center = originWorld + new Vector3((x + 0.5f) * cell, (y + 0.5f) * cell, 0f);
					child.transform.position = center;
					restingPositions[x, y] = center;
					SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
					sr.sprite = tileSprite;
					sr.drawMode = SpriteDrawMode.Sliced;
					sr.size = new Vector2(cell * 0.9f, cell * 0.9f);
					sr.sortingOrder = 0;
					lastValues[x, y] = int.MinValue;
					baseColors[x, y] = sr.color;
					tileObjects[x, y] = child;
				}
			}
		}

		private void EnsureSprite()
		{
			if (tileSprite != null)
			{
				return;
			}
			Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
			tex.Apply();
			tileSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
		}

		private void DestroyExistingChildren()
		{
			if (tileObjects != null)
			{
				for (int y = 0; y < tileObjects.GetLength(1); y++)
				{
					for (int x = 0; x < tileObjects.GetLength(0); x++)
					{
						GameObject go = tileObjects[x, y];
						if (go != null)
						{
							DestroyImmediate(go);
						}
					}
				}
			}
			if (backgroundObject != null)
			{
				DestroyImmediate(backgroundObject);
				backgroundObject = null;
			}
			for (int i = transform.childCount - 1; i >= 0; i--)
			{
				DestroyImmediate(transform.GetChild(i).gameObject);
			}
		}

		private void ApplyThemeToSprite(SpriteRenderer sr, int value)
		{
			Match3Theme theme = mechanic != null ? mechanic.Theme : null;
			if (theme != null)
			{
				if (theme.tileDefinitions != null && theme.tileDefinitions.Length > 0)
				{
					int idx = Mathf.Abs(value) % theme.tileDefinitions.Length;
					var def = theme.tileDefinitions[idx];
					sr.sprite = def.sprite != null ? def.sprite : sr.sprite;
					sr.color = def.sprite != null ? Color.white : def.color;
					return;
				}
				if (theme.tileSprites != null && theme.tileSprites.Length > 0)
				{
					sr.sprite = theme.tileSprites[value % theme.tileSprites.Length];
					sr.color = Color.white;
					return;
				}
				if (theme.tileColors != null && theme.tileColors.Length > 0)
				{
					sr.color = theme.tileColors[value % theme.tileColors.Length];
					return;
				}
			}
			// Fallback
			sr.color = Match3Mechanic.GetColorForValue(value);
		}

		private Sprite CreateSolidSprite(Color color)
		{
			Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
			tex.SetPixels(new Color[] { color, color, color, color });
			tex.Apply();
			return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
		}

		public void ShowScorePopup(Vector3 worldPosition, int points)
		{
			GameObject go = new GameObject($"Score_{points}");
			go.transform.SetParent(transform, false);
			go.transform.position = worldPosition + new Vector3(0f, 0.1f, 0f);
			TextMesh tm = go.AddComponent<TextMesh>();
			tm.text = $"+{points}";
			tm.color = new Color(1f, 1f, 1f, 1f);
			tm.fontSize = 32;
			tm.anchor = TextAnchor.MiddleCenter;
			tm.alignment = TextAlignment.Center;
			activePopups.Add(go);
		}

		public void RefreshFromMechanic()
		{
			RebuildTiles();
		}

		public void FlashSpawned()
		{
			if (mechanic == null)
			{
				return;
			}
			var spawned = mechanic.SpawnedCells;
			float until = Time.time + 0.6f;
			for (int i = 0; i < spawned.Count; i++)
			{
				Vector2Int c = spawned[i];
				if (c.x >= 0 && c.x < mechanic.BoardWidth && c.y >= 0 && c.y < mechanic.BoardHeight)
				{
					cellGlowUntil[c.x, c.y] = until;
				}
			}
		}

		public void PulseNeighborsOfClears()
		{
			if (mechanic == null)
			{
				return;
			}
			var cleared = mechanic.ClearedCells;
			float until = Time.time + 0.5f;
			for (int i = 0; i < cleared.Count; i++)
			{
				Vector2Int c = cleared[i];
				TrySetNeighborPulse(c.x - 1, c.y, until);
				TrySetNeighborPulse(c.x + 1, c.y, until);
				TrySetNeighborPulse(c.x, c.y - 1, until);
				TrySetNeighborPulse(c.x, c.y + 1, until);
			}
		}

		private void TrySetNeighborPulse(int x, int y, float until)
		{
			if (x < 0 || y < 0 || x >= mechanic.BoardWidth || y >= mechanic.BoardHeight)
			{
				return;
			}
			if (cellNeighborPulseUntil != null)
			{
				cellNeighborPulseUntil[x, y] = until;
			}
		}

		public System.Collections.IEnumerator AnimateSwap(Vector2Int a, Vector2Int b, float cellSize, Vector3 origin)
		{
			if (tileObjects == null)
			{
				yield break;
			}
			GameObject aObj = tileObjects[a.x, a.y];
			GameObject bObj = tileObjects[b.x, b.y];
			if (aObj == null || bObj == null)
			{
				yield break;
			}
			Vector3 aRest = origin + new Vector3((a.x + 0.5f) * cellSize, (a.y + 0.5f) * cellSize, 0f);
			Vector3 bRest = origin + new Vector3((b.x + 0.5f) * cellSize, (b.y + 0.5f) * cellSize, 0f);
			float t = 0f;
			while (t < 1f)
			{
				t += Time.deltaTime * 6f;
				aObj.transform.position = Vector3.Lerp(aRest, bRest, t);
				bObj.transform.position = Vector3.Lerp(bRest, aRest, t);
				yield return null;
			}
			aObj.transform.position = bRest;
			bObj.transform.position = aRest;
		}

		public void PulseClears()
		{
			lastPulseTime = Time.time;
		}
	}
}




using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Handles the visual representation and animations for the Match3 game.
    /// Now properly integrates with Match3Theme for tile spawning and visual updates.
    /// </summary>
    public sealed class Match3RuntimeView : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float swapAnimationDuration = 0.3f;
        [SerializeField] private float pulseIntensity = 0.15f;
        [SerializeField] private float glowIntensity = 0.5f;

        // References
        private Match3Mechanic mechanic;
        private Sprite tileSprite;

        // Tile objects and state
        private GameObject[,] tileObjects;
        private GameObject[,] specialTileIndicators;
        private Vector3[,] restingPositions;
        private int[,] lastValues;
        private SpecialTile[,] lastSpecialTiles;
        private float[,] cellPulseUntil;
        private float[,] cellNeighborPulseUntil;
        private float[,] cellGlowUntil;
        private Color[,] baseColors;

        // Effects
        private GameObject backgroundObject;
        private readonly List<GameObject> activePopups = new List<GameObject>();

        #region Unity Lifecycle

        private void Awake()
        {
            // Try to find the mechanic in parent
            mechanic = GetComponentInParent<Match3Mechanic>();
        }

        private void Start()
        {
            // Ensure we have a mechanic and bind to it
            if (mechanic == null)
            {
                mechanic = GetComponentInParent<Match3Mechanic>();
            }

            if (mechanic != null)
            {
                Bind(mechanic);
            }
            else
            {
                Debug.LogError("Match3RuntimeView: No Match3Mechanic found in parent hierarchy!");
            }
        }

        private void Update()
        {
            if (mechanic == null)
            {
                // Try to find the mechanic if we don't have one
                mechanic = GetComponentInParent<Match3Mechanic>();
                if (mechanic == null) return;
            }

            if (mechanic.BoardWidth == 0 || mechanic.BoardHeight == 0)
            {
                // Board not initialized yet, try to bind again
                Bind(mechanic);
                return;
            }

            UpdateTiles();
            UpdatePopups();
        }

        #endregion

        #region Binding and Initialization

        /// <summary>
        /// Bind this view to a Match3Mechanic.
        /// </summary>
        public void Bind(Match3Mechanic targetMechanic)
        {
            if (targetMechanic == null)
            {
                Debug.LogError("Match3RuntimeView: Cannot bind to null mechanic!");
                return;
            }

            mechanic = targetMechanic;

            // Ensure the mechanic is initialized before rebuilding tiles
            if (mechanic.BoardWidth == 0 || mechanic.BoardHeight == 0)
            {
                Debug.LogWarning("Match3RuntimeView: Mechanic board not initialized yet. Waiting for initialization...");
                return;
            }

            Debug.Log($"Match3RuntimeView: Binding to mechanic with board {mechanic.BoardWidth}x{mechanic.BoardHeight}");
            RebuildTiles();
        }

        /// <summary>
        /// Refresh the view from the current mechanic state.
        /// </summary>
        public void RefreshFromMechanic()
        {
            if (mechanic != null)
            {
                Debug.Log("Match3RuntimeView: Refreshing from mechanic");
                RebuildTiles();
            }
        }

        /// <summary>
        /// Force refresh all tiles with current theme (useful when theme changes).
        /// </summary>
        public void ForceThemeRefresh()
        {
            if (mechanic == null || tileObjects == null) return;

            Debug.Log("Match3RuntimeView: Force refreshing theme for all tiles");
            
            for (int y = 0; y < mechanic.BoardHeight; y++)
            {
                for (int x = 0; x < mechanic.BoardWidth; x++)
                {
                    GameObject go = tileObjects[x, y];
                    if (go != null)
                    {
                        SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            int currentValue = mechanic.GetTileValue(x, y);
                            if (currentValue >= 0)
                            {
                                ApplyThemeToSprite(sr, currentValue);
                                // Update base color for animations
                                if (baseColors != null)
                                {
                                    baseColors[x, y] = sr.color;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Tile Management

        /// <summary>
        /// Rebuild all tiles based on the current board state.
        /// </summary>
        private void RebuildTiles()
        {
            if (mechanic == null)
            {
                Debug.LogWarning("Match3RuntimeView: Cannot rebuild tiles - mechanic is null");
                return;
            }

            Debug.Log($"Match3RuntimeView: Rebuilding tiles for board {mechanic.BoardWidth}x{mechanic.BoardHeight}");

            EnsureSprite();
            DestroyExistingChildren();

            int w = mechanic.BoardWidth;
            int h = mechanic.BoardHeight;
            float cell = mechanic.CellSize;
            Vector3 originWorld = mechanic.BoardOriginWorld;

            // Initialize arrays
            tileObjects = new GameObject[w, h];
            specialTileIndicators = new GameObject[w, h];
            restingPositions = new Vector3[w, h];
            lastValues = new int[w, h];
            lastSpecialTiles = new SpecialTile[w, h];
            cellPulseUntil = new float[w, h];
            cellNeighborPulseUntil = new float[w, h];
            cellGlowUntil = new float[w, h];
            baseColors = new Color[w, h];

            // Create background
            CreateBackground(w, h, cell, originWorld);

            Debug.Log($"Match3RuntimeView: Creating {w * h} tiles at origin {originWorld} with cell size {cell}");

            // Create tiles
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    CreateTile(x, y, w, h, cell, originWorld);
                }
            }

            Debug.Log($"Match3RuntimeView: Successfully created {w * h} tiles. Tile objects array size: {tileObjects?.GetLength(0)}x{tileObjects?.GetLength(1)}");
        }

        /// <summary>
        /// Create a single tile at the specified position.
        /// </summary>
        private void CreateTile(int x, int y, int w, int h, float cell, Vector3 originWorld)
        {
            GameObject child = new GameObject($"Tile_{x}_{y}");
            child.transform.SetParent(transform, false);

            Vector3 center = originWorld + new Vector3((x + 0.5f) * cell, (y + 0.5f) * cell, 0f);
            child.transform.position = center;
            child.transform.localScale = new Vector3(cell * 0.9f, cell * 0.9f, 1f);
            restingPositions[x, y] = center;

            SpriteRenderer sr = child.AddComponent<SpriteRenderer>();
            sr.sprite = tileSprite;
            sr.sortingOrder = 0;

            // Apply theme immediately if available
            if (mechanic?.Theme != null)
            {
                int tileValue = mechanic.GetTileValue(x, y);
                if (tileValue >= 0)
                {
                    ApplyThemeToSprite(sr, tileValue);
                    Debug.Log($"Match3RuntimeView: Applied theme to tile at ({x}, {y}) with value {tileValue}");
                }
            }
            else
            {
                // Fallback to default white color
                sr.color = Color.white;
            }

            lastValues[x, y] = int.MinValue;
            lastSpecialTiles[x, y] = new SpecialTile(-1, SpecialTileType.None);
            baseColors[x, y] = sr.color;
            tileObjects[x, y] = child;

            // Create special tile indicator
            CreateSpecialTileIndicator(x, y, w, h, cell, originWorld);
        }

        /// <summary>
        /// Create a special tile indicator at the specified position.
        /// </summary>
        private void CreateSpecialTileIndicator(int x, int y, int w, int h, float cell, Vector3 originWorld)
        {
            GameObject indicator = new GameObject($"SpecialIndicator_{x}_{y}");
            indicator.transform.SetParent(transform, false);

            Vector3 center = originWorld + new Vector3((x + 0.5f) * cell, (y + 0.5f) * cell, -0.1f);
            indicator.transform.position = center;
            indicator.transform.localScale = new Vector3(cell * 0.8f, cell * 0.8f, 1f);

            SpriteRenderer sr = indicator.AddComponent<SpriteRenderer>();
            sr.sprite = tileSprite;
            sr.color = new Color(1f, 1f, 1f, 0f); // Start invisible
            sr.sortingOrder = 1;

            specialTileIndicators[x, y] = indicator;
        }

        /// <summary>
        /// Create the background object.
        /// </summary>
        private void CreateBackground(int w, int h, float cell, Vector3 originWorld)
        {
            Match3Theme theme = mechanic?.Theme;
            if (theme != null)
            {
                backgroundObject = new GameObject("Background");
                backgroundObject.transform.SetParent(transform, false);
                backgroundObject.transform.position = originWorld + new Vector3((w * cell) * 0.5f, (h * cell) * 0.5f, 1f);
                backgroundObject.transform.localScale = new Vector3(w * cell * 1.05f, h * cell * 1.05f, 1f);

                SpriteRenderer bsr = backgroundObject.AddComponent<SpriteRenderer>();
                
                // Use background sprite if available, otherwise create solid color sprite
                if (theme.backgroundSprite != null)
                {
                    bsr.sprite = theme.backgroundSprite;
                    bsr.color = Color.white; // Use sprite as-is
                }
                else
                {
                    bsr.sprite = CreateSolidSprite(theme.backgroundColor);
                    bsr.color = Color.white; // Color is already applied to the sprite
                }
                
                bsr.sortingOrder = -10;
                Debug.Log($"Match3RuntimeView: Created background with color {theme.backgroundColor}");
            }
        }

        /// <summary>
        /// Update all tiles with current values and animations.
        /// </summary>
        private void UpdateTiles()
        {
            if (tileObjects == null) return;

            for (int y = 0; y < mechanic.BoardHeight; y++)
            {
                for (int x = 0; x < mechanic.BoardWidth; x++)
                {
                    UpdateTile(x, y);
                }
            }
        }

        /// <summary>
        /// Update a single tile.
        /// </summary>
        private void UpdateTile(int x, int y)
        {
            GameObject go = tileObjects[x, y];
            if (go == null) return;

            int currentValue = mechanic.GetTileValue(x, y);
            SpecialTile currentSpecial = mechanic.GetSpecialTile(x, y);
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();

            if (currentValue < 0)
            {
                sr.enabled = false;
                UpdateSpecialTileIndicator(x, y, new SpecialTile(-1, SpecialTileType.None));
                return;
            }

            sr.enabled = true;

            // Update tile appearance if value changed
            if (lastValues[x, y] != currentValue)
            {
                ApplyThemeToSprite(sr, currentValue);
                lastValues[x, y] = currentValue;
                if (baseColors != null)
                {
                    baseColors[x, y] = sr.color;
                }
            }

            // Update special tile indicator if changed
            if (lastSpecialTiles[x, y].type != currentSpecial.type || 
                lastSpecialTiles[x, y].baseValue != currentSpecial.baseValue)
            {
                UpdateSpecialTileIndicator(x, y, currentSpecial);
                lastSpecialTiles[x, y] = currentSpecial;
            }

            // Update position (smooth movement)
            go.transform.position = Vector3.Lerp(go.transform.position, restingPositions[x, y], 12f * Time.deltaTime);

            // Update animations
            UpdateTileAnimations(x, y, sr);
        }

        /// <summary>
        /// Update special tile indicator appearance.
        /// </summary>
        private void UpdateSpecialTileIndicator(int x, int y, SpecialTile specialTile)
        {
            GameObject indicator = specialTileIndicators[x, y];
            if (indicator == null) return;

            SpriteRenderer sr = indicator.GetComponent<SpriteRenderer>();
            if (sr == null) return;

            if (specialTile.IsSpecial)
            {
                sr.enabled = true;
                sr.color = GetSpecialTileColor(specialTile.type);
                
                // Add pulsing animation for special tiles
                float pulse = Mathf.Sin(Time.time * 3f) * 0.1f + 0.9f;
                indicator.transform.localScale = new Vector3(pulse, pulse, 1f);
            }
            else
            {
                sr.enabled = false;
            }
        }

        /// <summary>
        /// Get color for special tile type.
        /// </summary>
        private Color GetSpecialTileColor(SpecialTileType type)
        {
            return type switch
            {
                SpecialTileType.Bomb => new Color(1f, 0.2f, 0.2f, 0.8f),      // Red
                SpecialTileType.Lightning => new Color(1f, 1f, 0.2f, 0.8f),   // Yellow
                SpecialTileType.Star => new Color(0.2f, 1f, 1f, 0.8f),        // Cyan
                SpecialTileType.Rainbow => new Color(1f, 0.2f, 1f, 0.8f),     // Magenta
                _ => new Color(1f, 1f, 1f, 0f)                                // Invisible
            };
        }

        /// <summary>
        /// Update tile animations (pulse, glow, etc.).
        /// </summary>
        private void UpdateTileAnimations(int x, int y, SpriteRenderer sr)
        {
            // Pulse animation
            float pulse = GetPulseValue(x, y);
            float scale = 1f + pulse * pulseIntensity;
            tileObjects[x, y].transform.localScale = new Vector3(scale, scale, 1f);

            // Glow animation for newly spawned tiles
            if (cellGlowUntil != null && baseColors != null)
            {
                float glowLeft = cellGlowUntil[x, y] - Time.time;
                if (glowLeft > 0f)
                {
                    float t = Mathf.Clamp01(glowLeft);
                    Color baseColor = baseColors[x, y];
                    sr.color = Color.Lerp(baseColor, Color.white, glowIntensity * t);
                }
                else
                {
                    sr.color = baseColors[x, y];
                }
            }
        }

        /// <summary>
        /// Get the current pulse value for a tile.
        /// </summary>
        private float GetPulseValue(int x, int y)
        {
            float timeLeft = cellPulseUntil != null ? (cellPulseUntil[x, y] - Time.time) : 0f;
            float neighLeft = cellNeighborPulseUntil != null ? (cellNeighborPulseUntil[x, y] - Time.time) : 0f;
            return Mathf.Max(
                Mathf.Clamp01(timeLeft),
                Mathf.Clamp01(neighLeft * 0.6f)
            );
        }

        #endregion

        #region Theme and Visuals

        /// <summary>
        /// Apply theme to a sprite renderer.
        /// </summary>
        private void ApplyThemeToSprite(SpriteRenderer sr, int value)
        {
            Match3Theme theme = mechanic?.Theme;
            if (theme != null && theme.IsValid())
            {
                var def = theme.GetTileDefinition(value);
                if (def != null)
                {
                    // Use sprite if available, otherwise use fallback sprite
                    sr.sprite = def.sprite != null ? def.sprite : tileSprite;
                    
                    // Always apply the color from tile definition
                    // If sprite is provided, use color as tint
                    // If no sprite, use color as the main color
                    sr.color = def.color;
                    
                    Debug.Log($"Match3RuntimeView: Applied tile definition '{def.id}' for value {value} - Sprite: {(def.sprite != null ? "Yes" : "No")}, Color: {def.color}");
                    return;
                }
            }

            // Fallback to default colors
            Color fallbackColor = Match3Mechanic.GetColorForValue(value);
            sr.sprite = tileSprite;
            sr.color = fallbackColor;
            Debug.Log($"Match3RuntimeView: Applied fallback color ({fallbackColor}) for value {value}");
        }

        /// <summary>
        /// Ensure we have a fallback tile sprite.
        /// </summary>
        private void EnsureSprite()
        {
            if (tileSprite != null) return;

            Debug.Log("Match3RuntimeView: Creating fallback tile sprite");
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            tileSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// Create a solid color sprite.
        /// </summary>
        private Sprite CreateSolidSprite(Color color)
        {
            Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new Color[] { color, color, color, color });
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 100f);
        }

        #endregion

        #region Animations

        /// <summary>
        /// Animate tile swap.
        /// </summary>
        public IEnumerator AnimateSwap(Vector2Int a, Vector2Int b, float cellSize, Vector3 origin)
        {
            if (tileObjects == null) yield break;

            GameObject aObj = tileObjects[a.x, a.y];
            GameObject bObj = tileObjects[b.x, b.y];
            if (aObj == null || bObj == null) yield break;

            Vector3 aRest = origin + new Vector3((a.x + 0.5f) * cellSize, (a.y + 0.5f) * cellSize, 0f);
            Vector3 bRest = origin + new Vector3((b.x + 0.5f) * cellSize, (b.y + 0.5f) * cellSize, 0f);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / swapAnimationDuration;
                aObj.transform.position = Vector3.Lerp(aRest, bRest, t);
                bObj.transform.position = Vector3.Lerp(bRest, aRest, t);
                yield return null;
            }

            aObj.transform.position = bRest;
            bObj.transform.position = aRest;
        }

        /// <summary>
        /// Animate tile swap back (for invalid moves).
        /// </summary>
        public IEnumerator AnimateSwapBack(Vector2Int a, Vector2Int b, float cellSize, Vector3 origin)
        {
            if (tileObjects == null) yield break;

            GameObject aObj = tileObjects[a.x, a.y];
            GameObject bObj = tileObjects[b.x, b.y];
            if (aObj == null || bObj == null) yield break;

            Vector3 bRest = origin + new Vector3((a.x + 0.5f) * cellSize, (a.y + 0.5f) * cellSize, 0f);
            Vector3 aRest = origin + new Vector3((b.x + 0.5f) * cellSize, (b.y + 0.5f) * cellSize, 0f);

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / swapAnimationDuration;
                aObj.transform.position = Vector3.Lerp(aRest, bRest, t);
                bObj.transform.position = Vector3.Lerp(bRest, aRest, t);
                yield return null;
            }

            aObj.transform.position = bRest;
            bObj.transform.position = aRest;
        }

        #endregion

        #region Effects

        /// <summary>
        /// Show a score popup at the specified position.
        /// </summary>
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

        /// <summary>
        /// Update score popups.
        /// </summary>
        private void UpdatePopups()
        {
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

        /// <summary>
        /// Pulse cleared tiles.
        /// </summary>
        public void PulseClears()
        {
            if (mechanic == null) return;

            var cleared = mechanic.ClearedCells;
            float until = Time.time + 0.5f;

            for (int i = 0; i < cleared.Count; i++)
            {
                Vector2Int c = cleared[i];
                if (c.x >= 0 && c.x < mechanic.BoardWidth && c.y >= 0 && c.y < mechanic.BoardHeight)
                {
                    cellPulseUntil[c.x, c.y] = until;
                }
            }
        }

        /// <summary>
        /// Pulse neighbors of cleared tiles.
        /// </summary>
        public void PulseNeighborsOfClears()
        {
            if (mechanic == null) return;

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

        /// <summary>
        /// Try to set neighbor pulse.
        /// </summary>
        private void TrySetNeighborPulse(int x, int y, float until)
        {
            if (x < 0 || y < 0 || x >= mechanic.BoardWidth || y >= mechanic.BoardHeight) return;
            if (cellNeighborPulseUntil != null)
            {
                cellNeighborPulseUntil[x, y] = until;
            }
        }

        /// <summary>
        /// Flash newly spawned tiles.
        /// </summary>
        public void FlashSpawned()
        {
            if (mechanic == null) return;

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

        #endregion

        #region Cleanup

        /// <summary>
        /// Destroy existing children and clean up arrays.
        /// </summary>
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

            if (specialTileIndicators != null)
            {
                for (int y = 0; y < specialTileIndicators.GetLength(1); y++)
                {
                    for (int x = 0; x < specialTileIndicators.GetLength(0); x++)
                    {
                        GameObject go = specialTileIndicators[x, y];
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

        #endregion
    }
}

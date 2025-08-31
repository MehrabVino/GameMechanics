using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MechanicGames.Shared;

namespace MechanicGames.BlockBlast
{
    public class BlockBlastMechanic : MonoBehaviour, IGameMechanic
    {
        [Header("System References")]
        [SerializeField] private BlockBlastGameManager gameManager;
        [SerializeField] private BlockBlastConfig gameConfig;
        
        [Header("UI References")]
        [SerializeField] private Canvas gameCanvas;
        [SerializeField] private RectTransform boardContainer;
        [SerializeField] private Text scoreText;
        
        [Header("Tile Settings")]
        [SerializeField] private Sprite tileSprite;
        
        [Header("Background Settings")]
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        [SerializeField] private Color gridLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private bool showGridLines = true;
        [SerializeField] private float gridLineThickness = 1f;
        
        [Header("Placeholder Settings")]
        [SerializeField] private Color placeholderFillColor = new Color(0.2f, 0.8f, 1f, 0.3f);
        [SerializeField] private Color placeholderBorderColor = new Color(0.2f, 0.8f, 1f, 0.8f);
        [SerializeField] private float placeholderPulseSpeed = 2f;
        [SerializeField] private float placeholderPulseScale = 0.05f;
        
        [Header("Next Tiles")]
        [SerializeField] private RectTransform nextTilesContainer;
        
        [Header("Effects")]
        [SerializeField] private GameObject explosionPrefab;
        [SerializeField] private ParticleSystem clearParticles;
        
        private int[,] board;
        private Image[,] tileImages;
        private int score = 0;
        private List<Vector2Int> currentShape;
        private List<Image> shapePreviews;
        private Vector2Int hoverPosition;
        private bool isAnimating = false;
        
        // Placeholder system
        private List<GameObject> placeholderObjects;
        private List<Image> placeholderFillImages;
        private List<Image> placeholderBorderImages;
        private float placeholderPulseTime = 0f;
        
        // Next tiles system
        private List<List<Vector2Int>> nextShapes;
        private List<List<Image>> nextTilePreviews;
        
        // Background system
        private List<Image> backgroundTiles;
        private List<Image> gridLines;
        
        // Combo system
        private int comboCount = 0;
        private float currentComboMultiplier = 1f;
        private float lastComboTime = 0f;
        
                // Configuration properties
        private int boardWidth => gameConfig != null ? gameConfig.BoardWidth : 8;
        private int boardHeight => gameConfig != null ? gameConfig.BoardHeight : 8;
        private float cellSize => gameConfig != null ? gameConfig.CellSize : 80f;
        private Color[] tileColors => gameConfig != null ? gameConfig.TileColors : new Color[] { Color.red, Color.blue, Color.green };
        private float popScale => gameConfig != null ? gameConfig.PopScale : 1.3f;
        private float popDuration => gameConfig != null ? gameConfig.PopDuration : 0.2f;
        private float shakeIntensity => gameConfig != null ? gameConfig.ShakeIntensity : 5f;
        private float tileDropSpeed => gameConfig != null ? gameConfig.TileDropSpeed : 0.1f;
        private float tileDropBounce => gameConfig != null ? gameConfig.TileDropBounce : 0.2f;
        private float tileDropBounceHeight => gameConfig != null ? gameConfig.TileDropBounceHeight : 20f;
        private float comboMultiplier => gameConfig != null ? gameConfig.ComboMultiplier : 1.2f;
        private int maxCombo => gameConfig != null ? gameConfig.MaxComboLevel : 5;
        private float comboDecayTime => gameConfig != null ? gameConfig.ComboDecayTime : 2f;
        private Color[] comboColors => gameConfig != null ? gameConfig.ComboColors : new Color[] { Color.white, Color.yellow };
        private int nextTilesCount => gameConfig != null ? gameConfig.NextTilesCount : 3;
        private float nextTileSize => gameConfig != null ? gameConfig.NextTileSize : 60f;
        private float nextTileSpacing => gameConfig != null ? gameConfig.NextTileSpacing : 10f;
        private float tileGlowDuration => gameConfig != null ? gameConfig.TileGlowDuration : 0.5f;
        private float tileGlowIntensity => gameConfig != null ? gameConfig.TileGlowIntensity : 0.3f;
        
        // Public properties for UI access
        public int Score => score;
        public int ComboCount => comboCount;
        public float ComboMultiplier => currentComboMultiplier;
        
        // IGameMechanic interface properties
        public bool IsGameActive => gameManager != null ? gameManager.IsGameActive : false;
        public bool IsPaused => gameManager != null ? gameManager.IsPaused : false;
        public int CurrentScore => score;
        public int HighScore => gameManager != null ? gameManager.HighScore : 0;
        
        // Events
        public System.Action OnGameStart { get; set; }
        public System.Action OnGamePause { get; set; }
        public System.Action OnGameResume { get; set; }
        public System.Action OnGameOver { get; set; }
        public System.Action<int> OnScoreChanged { get; set; }
        
        void Start()
        {
            // Auto-assign game manager if not set
            if (gameManager == null)
            {
                gameManager = BlockBlastGameManager.Instance;
            }
            
            // Auto-assign config if not set
            if (gameConfig == null && gameManager != null)
            {
                gameConfig = gameManager.Config;
            }
            
            InitializeGame();
        }
        
        public void SetGameManager(BlockBlastGameManager manager)
        {
            gameManager = manager;
            if (gameConfig == null && manager != null)
            {
                gameConfig = manager.Config;
            }
        }
        
        void Update()
        {
            if (isAnimating) return;
            
            // Update placeholder pulse animation
            placeholderPulseTime += Time.deltaTime * placeholderPulseSpeed;
            
            // Update combo system
            UpdateComboSystem();
            
            HandleInput();
            UpdateHover();
        }
        
        void InitializeGame()
        {
            board = new int[boardWidth, boardHeight];
            tileImages = new Image[boardWidth, boardHeight];
            
            // Initialize placeholder system
            placeholderObjects = new List<GameObject>();
            placeholderFillImages = new List<Image>();
            placeholderBorderImages = new List<Image>();
            
            // Initialize next tiles system
            nextShapes = new List<List<Vector2Int>>();
            nextTilePreviews = new List<List<Image>>();
            
            // Initialize background system
            backgroundTiles = new List<Image>();
            gridLines = new List<Image>();
            
            if (boardContainer == null)
            {
                GameObject container = new GameObject("BoardContainer");
                container.transform.SetParent(gameCanvas.transform, false);
                boardContainer = container.AddComponent<RectTransform>();
                boardContainer.anchorMin = new Vector2(0.5f, 0.5f);
                boardContainer.anchorMax = new Vector2(0.5f, 0.5f);
                boardContainer.sizeDelta = new Vector2(boardWidth * cellSize, boardHeight * cellSize);
            }
            
            CreateBackgroundTiles();
            CreateBoardTiles();
            InitializeNextTiles();
            GenerateNewShape();
            UpdateUI();
        }
        
        // IGameMechanic interface methods
        public void StartGame()
        {
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
            OnGameStart?.Invoke();
        }
        
        public void PauseGame()
        {
            if (gameManager != null)
            {
                gameManager.PauseGame();
            }
            OnGamePause?.Invoke();
        }
        
        public void ResumeGame()
        {
            if (gameManager != null)
            {
                gameManager.ResumeGame();
            }
            OnGameResume?.Invoke();
        }
        
        public void EndGame()
        {
            if (gameManager != null)
            {
                gameManager.EndGame();
            }
            OnGameOver?.Invoke();
        }
        
        public void ResetGame()
        {
            score = 0;
            ResetCombo();
            ClearBoard();
            InitializeNextTiles();
            GenerateNewShape();
            UpdateUI();
        }
        
        public void UpdateScore(int newScore)
        {
            score = newScore;
            OnScoreChanged?.Invoke(score);
        }
        
        public void AddScore(int points)
        {
            score += points;
            OnScoreChanged?.Invoke(score);
        }
        
        void ClearBoard()
        {
            if (board == null) return;
            
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    board[x, y] = 0;
                    if (tileImages[x, y] != null)
                    {
                        tileImages[x, y].color = Color.clear;
                    }
                }
            }
        }
        
        void CreateBackgroundTiles()
        {
            // Create background tiles
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    GameObject bgTileObj = new GameObject($"BackgroundTile_{x}_{y}");
                    bgTileObj.transform.SetParent(boardContainer, false);
                    
                    Image bgTileImage = bgTileObj.AddComponent<Image>();
                    bgTileImage.sprite = tileSprite;
                    bgTileImage.color = backgroundColor;
                    bgTileImage.raycastTarget = false;
                    
                    RectTransform rect = bgTileObj.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(cellSize, cellSize);
                    rect.anchoredPosition = new Vector2(
                        (x - boardWidth * 0.5f + 0.5f) * cellSize,
                        (y - boardHeight * 0.5f + 0.5f) * cellSize
                    );
                    
                    // Set background tiles to render behind game tiles
                    bgTileImage.transform.SetSiblingIndex(0);
                    
                    backgroundTiles.Add(bgTileImage);
                }
            }
            
            // Create grid lines if enabled
            if (showGridLines)
            {
                CreateGridLines();
            }
        }
        
        void CreateGridLines()
        {
            // Vertical lines
            for (int x = 0; x <= boardWidth; x++)
            {
                GameObject lineObj = new GameObject($"GridLine_V_{x}");
                lineObj.transform.SetParent(boardContainer, false);
                
                Image lineImage = lineObj.AddComponent<Image>();
                lineImage.sprite = tileSprite;
                lineImage.color = gridLineColor;
                lineImage.raycastTarget = false;
                
                RectTransform rect = lineObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(gridLineThickness, boardHeight * cellSize);
                rect.anchoredPosition = new Vector2(
                    (x - boardWidth * 0.5f) * cellSize,
                    0
                );
                
                lineImage.transform.SetSiblingIndex(1);
                gridLines.Add(lineImage);
            }
            
            // Horizontal lines
            for (int y = 0; y <= boardHeight; y++)
            {
                GameObject lineObj = new GameObject($"GridLine_H_{y}");
                lineObj.transform.SetParent(boardContainer, false);
                
                Image lineImage = lineObj.AddComponent<Image>();
                lineImage.sprite = tileSprite;
                lineImage.color = gridLineColor;
                lineImage.raycastTarget = false;
                
                RectTransform rect = lineObj.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(boardWidth * cellSize, gridLineThickness);
                rect.anchoredPosition = new Vector2(
                    0,
                    (y - boardHeight * 0.5f) * cellSize
                );
                
                lineImage.transform.SetSiblingIndex(1);
                gridLines.Add(lineImage);
            }
        }
        
        void CreateBoardTiles()
        {
            for (int x = 0; x < boardWidth; x++)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    GameObject tileObj = new GameObject($"Tile_{x}_{y}");
                    tileObj.transform.SetParent(boardContainer, false);
                    
                    Image tileImage = tileObj.AddComponent<Image>();
                    tileImage.sprite = tileSprite;
                    tileImage.color = Color.clear;
                    tileImage.raycastTarget = false;
                    
                    RectTransform rect = tileObj.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(cellSize * 0.9f, cellSize * 0.9f);
                    rect.anchoredPosition = new Vector2(
                        (x - boardWidth * 0.5f + 0.5f) * cellSize,
                        (y - boardHeight * 0.5f + 0.5f) * cellSize
                    );
                    
                    // Set game tiles to render above background
                    tileImage.transform.SetSiblingIndex(2);
                    
                    tileImages[x, y] = tileImage;
                }
            }
        }
        
        void GenerateNewShape()
        {
            currentShape = GenerateRandomShape();
            CreateShapePreviews();
        }
        
        List<Vector2Int> GenerateRandomShape()
        {
            int shapeType = Random.Range(0, 4);
            List<Vector2Int> shape = new List<Vector2Int>();
            
            switch (shapeType)
            {
                case 0: // Single
                    shape.Add(new Vector2Int(0, 0));
                    break;
                case 1: // Line
                    shape.Add(new Vector2Int(0, 0));
                    shape.Add(new Vector2Int(1, 0));
                    break;
                case 2: // L-shape
                    shape.Add(new Vector2Int(0, 0));
                    shape.Add(new Vector2Int(1, 0));
                    shape.Add(new Vector2Int(0, 1));
                    break;
                case 3: // Square
                    shape.Add(new Vector2Int(0, 0));
                    shape.Add(new Vector2Int(1, 0));
                    shape.Add(new Vector2Int(0, 1));
                    shape.Add(new Vector2Int(1, 1));
                    break;
            }
            
            return shape;
        }
        
        void CreateShapePreviews()
        {
            if (shapePreviews != null)
            {
                foreach (var preview in shapePreviews)
                {
                    if (preview != null) Destroy(preview.gameObject);
                }
            }
            
            shapePreviews = new List<Image>();
            
            for (int i = 0; i < currentShape.Count; i++)
            {
                GameObject previewObj = new GameObject($"Preview_{i}");
                previewObj.transform.SetParent(transform);
                
                Image previewImage = previewObj.AddComponent<Image>();
                previewImage.sprite = tileSprite;
                previewImage.color = tileColors[Random.Range(0, tileColors.Length)];
                previewImage.raycastTarget = false;
                
                shapePreviews.Add(previewImage);
            }
        }
        
        void HandleInput()
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateShape();
            }
            
            if (Input.GetMouseButtonDown(0))
            {
                TryPlaceShape();
            }
        }
        
        void UpdateHover()
        {
            Vector2Int hoverCell = GetCellUnderMouse();
            if (hoverCell != hoverPosition)
            {
                hoverPosition = hoverCell;
                UpdateHoverPreviews();
            }
        }
        
        Vector2Int GetCellUnderMouse()
        {
            Vector2 mousePos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                boardContainer, Input.mousePosition, gameCanvas.worldCamera, out mousePos);
            
            int x = Mathf.FloorToInt((mousePos.x + boardWidth * cellSize * 0.5f) / cellSize);
            int y = Mathf.FloorToInt((mousePos.y + boardHeight * cellSize * 0.5f) / cellSize);
            
            return new Vector2Int(x, y);
        }
        
        void UpdateHoverPreviews()
        {
            if (currentShape == null || shapePreviews == null) return;
            
            // Hide all placeholders first
            HidePlaceholders();
            
            for (int i = 0; i < currentShape.Count; i++)
            {
                if (shapePreviews[i] == null) continue;
                
                Vector2Int cellPos = hoverPosition + currentShape[i];
                bool isValid = IsValidPlacement(cellPos);
                
                if (isValid)
                {
                    shapePreviews[i].gameObject.SetActive(true);
                    Vector3 worldPos = GetWorldPosition(cellPos);
                    shapePreviews[i].transform.position = worldPos;
                    
                    Color previewColor = tileColors[Random.Range(0, tileColors.Length)];
                    previewColor.a = 0.6f;
                    shapePreviews[i].color = previewColor;
                    
                    // Show placeholder at this position
                    ShowPlaceholder(cellPos, i);
                }
                else
                {
                    shapePreviews[i].gameObject.SetActive(false);
                }
            }
        }
        
        bool IsValidPlacement(Vector2Int cell)
        {
            if (cell.x < 0 || cell.x >= boardWidth || cell.y < 0 || cell.y >= boardHeight)
                return false;
            
            return board[cell.x, cell.y] == 0;
        }
        
        Vector3 GetWorldPosition(Vector2Int cell)
        {
            Vector2 localPos = new Vector2(
                (cell.x - boardWidth * 0.5f + 0.5f) * cellSize,
                (cell.y - boardHeight * 0.5f + 0.5f) * cellSize
            );
            
            return boardContainer.TransformPoint(localPos);
        }
        
        void RotateShape()
        {
            if (currentShape == null) return;
            
            for (int i = 0; i < currentShape.Count; i++)
            {
                Vector2Int pos = currentShape[i];
                currentShape[i] = new Vector2Int(-pos.y, pos.x);
            }
            
            UpdateHoverPreviews();
        }
        
        void TryPlaceShape()
        {
            if (currentShape == null) return;
            
            for (int i = 0; i < currentShape.Count; i++)
            {
                Vector2Int cellPos = hoverPosition + currentShape[i];
                if (!IsValidPlacement(cellPos))
                    return;
            }
            
            StartCoroutine(PlaceShapeSequence());
        }
        
        IEnumerator PlaceShapeSequence()
        {
            isAnimating = true;
            
            List<Vector2Int> placedCells = new List<Vector2Int>();
            for (int i = 0; i < currentShape.Count; i++)
            {
                Vector2Int cellPos = hoverPosition + currentShape[i];
                board[cellPos.x, cellPos.y] = 1;
                placedCells.Add(cellPos);
                
                Color tileColor = tileColors[Random.Range(0, tileColors.Length)];
                tileImages[cellPos.x, cellPos.y].color = tileColor;
                
                StartCoroutine(PopTile(tileImages[cellPos.x, cellPos.y]));
            }
            
            foreach (var preview in shapePreviews)
            {
                if (preview != null) preview.gameObject.SetActive(false);
            }
            
            yield return StartCoroutine(CheckAndClearLines());
            
            // Advance to next shape
            AdvanceNextTiles();
            currentShape = nextShapes[0];
            CreateShapePreviews();
            
            score += currentShape.Count * 10;
            UpdateUI();
            
            isAnimating = false;
        }
        
        IEnumerator PopTile(Image tile)
        {
            Vector3 originalScale = tile.transform.localScale;
            float elapsed = 0f;
            
            while (elapsed < popDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / popDuration;
                float scale = Mathf.Lerp(1f, popScale, Mathf.Sin(progress * Mathf.PI));
                tile.transform.localScale = originalScale * scale;
				yield return null;
			}
            
            tile.transform.localScale = originalScale;
        }
        
        IEnumerator CheckAndClearLines()
        {
            List<int> rowsToClear = new List<int>();
            List<int> colsToClear = new List<int>();
            
            for (int y = 0; y < boardHeight; y++)
            {
                bool fullRow = true;
                for (int x = 0; x < boardWidth; x++)
                {
                    if (board[x, y] == 0)
                    {
                        fullRow = false;
                        break;
                    }
                }
                if (fullRow) rowsToClear.Add(y);
            }
            
            for (int x = 0; x < boardWidth; x++)
            {
                bool fullCol = true;
                for (int y = 0; y < boardHeight; y++)
                {
                    if (board[x, y] == 0)
                    {
                        fullCol = false;
                        break;
                    }
                }
                if (fullCol) colsToClear.Add(x);
            }
            
            if (rowsToClear.Count > 0 || colsToClear.Count > 0)
            {
                // Increase combo
                IncreaseCombo();
                
                StartCoroutine(ScreenShake());
                yield return StartCoroutine(ClearLines(rowsToClear, colsToClear));
                yield return StartCoroutine(DropTiles());
                
                int bonus = (rowsToClear.Count + colsToClear.Count) * 50;
                int comboBonus = Mathf.RoundToInt(bonus * currentComboMultiplier);
                score += comboBonus;
                
                // Show combo effect
                StartCoroutine(ShowComboEffect(comboBonus));
            }
            else
            {
                // Reset combo if no lines cleared
                ResetCombo();
            }
        }
        
        IEnumerator ClearLines(List<int> rows, List<int> cols)
        {
            float flashDuration = 0.3f;
            float elapsed = 0f;
            
            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.PingPong(elapsed * 8f, 1f);
                
                foreach (int row in rows)
                {
                    for (int x = 0; x < boardWidth; x++)
                    {
                        if (tileImages[x, row] != null)
                        {
                            Color c = tileImages[x, row].color;
                            tileImages[x, row].color = new Color(c.r, c.g, c.b, alpha);
                        }
                    }
                }
                
                foreach (int col in cols)
                {
                    for (int y = 0; y < boardHeight; y++)
                    {
                        if (tileImages[col, y] != null)
                        {
                            Color c = tileImages[col, y].color;
                            tileImages[col, y].color = new Color(c.r, c.g, c.b, alpha);
                        }
                    }
                }
                
                yield return null;
            }
            
            foreach (int row in rows)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    board[x, row] = 0;
                    tileImages[x, row].color = Color.clear;
                }
            }
            
            foreach (int col in cols)
            {
                for (int y = 0; y < boardHeight; y++)
                {
                    board[col, y] = 0;
                    tileImages[col, y].color = Color.clear;
                }
            }
            
            if (clearParticles != null)
            {
                clearParticles.Play();
            }
        }
        
        IEnumerator DropTiles()
        {
            bool tilesDropped;
            do
            {
                tilesDropped = false;
                
                for (int x = 0; x < boardWidth; x++)
                {
                    for (int y = 1; y < boardHeight; y++)
                    {
                        if (board[x, y] == 1 && board[x, y - 1] == 0)
                        {
                            board[x, y - 1] = board[x, y];
                            board[x, y] = 0;
                            
                            Color tileColor = tileImages[x, y].color;
                            tileImages[x, y - 1].color = tileColor;
                            tileImages[x, y].color = Color.clear;
                            
                            // Animate tile drop with bounce
                            StartCoroutine(AnimateTileDrop(tileImages[x, y - 1], y, y - 1));
                            
                            tilesDropped = true;
                        }
                    }
                }
                
                yield return new WaitForSeconds(tileDropSpeed);
                
            } while (tilesDropped);
        }
        
        IEnumerator AnimateTileDrop(Image tile, int fromY, int toY)
        {
            Vector3 startPos = tile.transform.position;
            Vector3 endPos = GetWorldPosition(new Vector2Int(0, toY));
            endPos.x = startPos.x; // Keep X position
            
            float elapsed = 0f;
            float duration = tileDropSpeed;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                // Add bounce effect
                float bounceOffset = Mathf.Sin(progress * Mathf.PI) * tileDropBounceHeight;
                Vector3 currentPos = Vector3.Lerp(startPos, endPos, progress);
                currentPos.y += bounceOffset;
                
                tile.transform.position = currentPos;
                yield return null;
            }
            
            tile.transform.position = endPos;
        }
        
        IEnumerator ScreenShake()
        {
            Vector3 originalPos = Camera.main.transform.position;
            float elapsed = 0f;
            float duration = 0.2f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                float intensity = shakeIntensity * (1f - progress);
                
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-intensity, intensity),
                    Random.Range(-intensity, intensity),
                    0f
                );
                
                Camera.main.transform.position = originalPos + shakeOffset;
                yield return null;
            }
            
            Camera.main.transform.position = originalPos;
        }
        
        void UpdateUI()
        {
            if (scoreText != null) scoreText.text = $"Score: {score}";
        }
        
        void InitializeNextTiles()
        {
            if (nextTilesContainer == null) return;
            
            // Generate initial next shapes
            for (int i = 0; i < nextTilesCount; i++)
            {
                nextShapes.Add(GenerateRandomShape());
            }
            
            // Create next tile previews
            for (int i = 0; i < nextTilesCount; i++)
            {
                List<Image> tileList = new List<Image>();
                List<Vector2Int> shape = nextShapes[i];
                
                for (int j = 0; j < shape.Count; j++)
                {
                    GameObject tileObj = new GameObject($"NextTile_{i}_{j}");
                    tileObj.transform.SetParent(nextTilesContainer, false);
                    
                    Image tileImage = tileObj.AddComponent<Image>();
                    tileImage.sprite = tileSprite;
                    tileImage.color = tileColors[Random.Range(0, tileColors.Length)];
                    tileImage.raycastTarget = false;
                    
                    RectTransform rect = tileObj.GetComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(nextTileSize, nextTileSize);
                    
                    // Position next tiles horizontally
                    float xPos = i * (nextTileSize + nextTileSpacing);
                    rect.anchoredPosition = new Vector2(xPos, 0);
                    
                    tileList.Add(tileImage);
                }
                
                nextTilePreviews.Add(tileList);
            }
        }
        
        void ShowPlaceholder(Vector2Int cellPos, int index)
        {
            // Ensure we have enough placeholder objects
            while (placeholderObjects.Count <= index)
            {
                CreatePlaceholderObject();
            }
            
            if (placeholderObjects[index] == null) return;
            
            // Position placeholder
            Vector3 worldPos = GetWorldPosition(cellPos);
            placeholderObjects[index].transform.position = worldPos;
            placeholderObjects[index].SetActive(true);
            
            // Apply pulse animation
            float pulseScale = 1f + Mathf.Sin(placeholderPulseTime + index * 0.5f) * placeholderPulseScale;
            placeholderObjects[index].transform.localScale = Vector3.one * pulseScale;
        }
        
        void HidePlaceholders()
        {
            foreach (var placeholder in placeholderObjects)
            {
                if (placeholder != null)
                    placeholder.SetActive(false);
            }
        }
        
        void CreatePlaceholderObject()
        {
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(transform);
            
            // Create fill image
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(placeholderObj.transform, false);
            fillObj.transform.localPosition = Vector3.zero;
            
            Image fillImage = fillObj.AddComponent<Image>();
            if (tileSprite != null)
                fillImage.sprite = tileSprite;
            fillImage.color = placeholderFillColor;
            
            // Create border image (slightly larger)
            GameObject borderObj = new GameObject("Border");
            borderObj.transform.SetParent(placeholderObj.transform, false);
            borderObj.transform.localPosition = Vector3.zero;
            borderObj.transform.localScale = Vector3.one * 1.05f;
            
            Image borderImage = borderObj.AddComponent<Image>();
            if (tileSprite != null)
                borderImage.sprite = tileSprite;
            borderImage.color = placeholderBorderColor;
            
            // Set render order
            fillImage.transform.SetSiblingIndex(0);
            borderImage.transform.SetSiblingIndex(1);
            
            placeholderObjects.Add(placeholderObj);
            placeholderFillImages.Add(fillImage);
            placeholderBorderImages.Add(borderImage);
        }
        
        void AdvanceNextTiles()
        {
            // Remove current shape from next shapes
            if (nextShapes.Count > 0)
            {
                nextShapes.RemoveAt(0);
            }
            
            // Generate new shape
            nextShapes.Add(GenerateRandomShape());
            
            // Update next tile previews
            UpdateNextTilePreviews();
        }
        
        void UpdateNextTilePreviews()
        {
            for (int i = 0; i < nextTilePreviews.Count; i++)
            {
                if (i >= nextShapes.Count) continue;
                
                List<Image> tileList = nextTilePreviews[i];
                List<Vector2Int> shape = nextShapes[i];
                
                // Update colors for new shapes
                for (int j = 0; j < Mathf.Min(tileList.Count, shape.Count); j++)
                {
                    if (tileList[j] != null)
                    {
                        tileList[j].color = tileColors[Random.Range(0, tileColors.Length)];
                    }
                }
            }
        }
        
        // Combo System Methods
        void UpdateComboSystem()
        {
            if (Time.time - lastComboTime > comboDecayTime && comboCount > 0)
            {
                ResetCombo();
            }
        }
        
        void IncreaseCombo()
        {
            comboCount++;
            lastComboTime = Time.time;
            
            if (comboCount <= maxCombo)
            {
                currentComboMultiplier = Mathf.Pow(comboMultiplier, comboCount - 1);
            }
        }
        
        void ResetCombo()
        {
            comboCount = 0;
            currentComboMultiplier = 1f;
        }
        
        IEnumerator ShowComboEffect(int comboBonus)
        {
            // Create floating combo text
            GameObject comboTextObj = new GameObject("ComboText");
            comboTextObj.transform.SetParent(transform);
            comboTextObj.transform.position = Camera.main.transform.position + Vector3.forward * 5f;
            
            Text comboText = comboTextObj.AddComponent<Text>();
            comboText.text = $"COMBO! +{comboBonus}";
            comboText.fontSize = 48;
            comboText.color = comboColors[Mathf.Min(comboCount - 1, comboColors.Length - 1)];
            comboText.alignment = TextAnchor.MiddleCenter;
            
            // Animate combo text
            float elapsed = 0f;
            float duration = 1.5f;
            Vector3 startPos = comboTextObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * 100f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / duration;
                
                comboTextObj.transform.position = Vector3.Lerp(startPos, endPos, progress);
                comboText.color = new Color(comboText.color.r, comboText.color.g, comboText.color.b, 1f - progress);
                
                yield return null;
            }
            
            Destroy(comboTextObj);
        }
        
        // Enhanced tile effects
        IEnumerator GlowTile(Image tile, Color glowColor)
        {
            Color originalColor = tile.color;
            float elapsed = 0f;
            
            while (elapsed < tileGlowDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / tileGlowDuration;
                float glowIntensity = Mathf.PingPong(progress * 4f, 1f) * tileGlowIntensity;
                
                tile.color = Color.Lerp(originalColor, glowColor, glowIntensity);
                yield return null;
            }
            
            tile.color = originalColor;
        }
    }
}

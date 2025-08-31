using UnityEngine;

namespace MechanicGames.BlockBlast
{
    [CreateAssetMenu(fileName = "BlockBlastConfig", menuName = "BlockBlast/Game Config")]
    public class BlockBlastConfig : ScriptableObject
    {
        [Header("Game Settings")]
        [SerializeField] private int boardWidth = 8;
        [SerializeField] private int boardHeight = 8;
        [SerializeField] private float cellSize = 80f;
        
        [Header("Mobile Optimization")]
        [SerializeField] private bool isMobileBuild = true;
        [SerializeField] private float mobileEffectScale = 0.5f;
        [SerializeField] private int maxParticles = 50;
        [SerializeField] private bool enableAdvancedEffects = true;
        
        [Header("Performance Settings")]
        [SerializeField] private float targetFrameRate = 60f;
        [SerializeField] private bool enableVSync = true;
        [SerializeField] private int maxComboLevel = 5;
        [SerializeField] private float animationSpeedMultiplier = 1f;
        
        [Header("Visual Settings")]
        [SerializeField] private Color[] tileColors = new Color[] 
        {
            Color.red, Color.blue, Color.green, Color.yellow, Color.magenta, Color.cyan
        };
        [SerializeField] private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
        [SerializeField] private Color gridLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
        [SerializeField] private bool showGridLines = true;
        [SerializeField] private float gridLineThickness = 1f;
        
        [Header("Animation Settings")]
        [SerializeField] private float popScale = 1.3f;
        [SerializeField] private float popDuration = 0.2f;
        [SerializeField] private float shakeIntensity = 5f;
        [SerializeField] private float tileDropSpeed = 0.1f;
        [SerializeField] private float tileDropBounce = 0.2f;
        [SerializeField] private float tileDropBounceHeight = 20f;
        [SerializeField] private float tileGlowDuration = 0.5f;
        [SerializeField] private float tileGlowIntensity = 0.3f;
        
        [Header("Combo System")]
        [SerializeField] private float comboMultiplier = 1.2f;
        [SerializeField] private float comboDecayTime = 2f;
        [SerializeField] private Color[] comboColors = new Color[]
        {
            Color.white, Color.yellow, new Color(1f, 0.5f, 0f, 1f), Color.red, Color.magenta
        };
        
        [Header("Placeholder Settings")]
        [SerializeField] private Color placeholderFillColor = new Color(0.2f, 0.8f, 1f, 0.3f);
        [SerializeField] private Color placeholderBorderColor = new Color(0.2f, 0.8f, 1f, 0.8f);
        [SerializeField] private float placeholderPulseSpeed = 2f;
        [SerializeField] private float placeholderPulseScale = 0.05f;
        
        [Header("Next Tiles")]
        [SerializeField] private int nextTilesCount = 3;
        [SerializeField] private float nextTileSize = 60f;
        [SerializeField] private float nextTileSpacing = 10f;
        
        // Public properties with mobile optimization
        public int BoardWidth => boardWidth;
        public int BoardHeight => boardHeight;
        public float CellSize => cellSize;
        public bool IsMobileBuild => isMobileBuild;
        public float MobileEffectScale => mobileEffectScale;
        public int MaxParticles => maxParticles;
        public bool EnableAdvancedEffects => enableAdvancedEffects && !isMobileBuild;
        public float TargetFrameRate => targetFrameRate;
        public bool EnableVSync => enableVSync;
        public int MaxComboLevel => isMobileBuild ? Mathf.Min(maxComboLevel, 3) : maxComboLevel;
        public float AnimationSpeedMultiplier => animationSpeedMultiplier;
        public Color[] TileColors => tileColors;
        public Color BackgroundColor => backgroundColor;
        public Color GridLineColor => gridLineColor;
        public bool ShowGridLines => showGridLines;
        public float GridLineThickness => gridLineThickness;
        public float PopScale => isMobileBuild ? popScale * mobileEffectScale : popScale;
        public float PopDuration => popDuration / animationSpeedMultiplier;
        public float ShakeIntensity => isMobileBuild ? shakeIntensity * mobileEffectScale : shakeIntensity;
        public float TileDropSpeed => tileDropSpeed / animationSpeedMultiplier;
        public float TileDropBounce => isMobileBuild ? tileDropBounce * mobileEffectScale : tileDropBounce;
        public float TileDropBounceHeight => isMobileBuild ? tileDropBounceHeight * mobileEffectScale : tileDropBounceHeight;
        public float TileGlowDuration => tileGlowDuration;
        public float TileGlowIntensity => tileGlowIntensity;
        public float ComboMultiplier => comboMultiplier;
        public float ComboDecayTime => comboDecayTime;
        public Color[] ComboColors => comboColors;
        public Color PlaceholderFillColor => placeholderFillColor;
        public Color PlaceholderBorderColor => placeholderBorderColor;
        public float PlaceholderPulseSpeed => placeholderPulseSpeed;
        public float PlaceholderPulseScale => isMobileBuild ? placeholderPulseScale * mobileEffectScale : placeholderPulseScale;
        public int NextTilesCount => nextTilesCount;
        public float NextTileSize => nextTileSize;
        public float NextTileSpacing => nextTileSpacing;
        
        void OnValidate()
        {
            // Ensure reasonable limits for mobile
            if (isMobileBuild)
            {
                boardWidth = Mathf.Clamp(boardWidth, 6, 10);
                boardHeight = Mathf.Clamp(boardHeight, 6, 10);
                cellSize = Mathf.Clamp(cellSize, 60f, 100f);
                maxParticles = Mathf.Clamp(maxParticles, 20, 100);
                maxComboLevel = Mathf.Clamp(maxComboLevel, 2, 5);
            }
        }
    }
}

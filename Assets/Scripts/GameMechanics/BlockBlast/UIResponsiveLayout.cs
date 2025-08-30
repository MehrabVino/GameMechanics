using UnityEngine;
using UnityEngine.UI;

namespace MechanicGames.BlockBlast
{
    public class UIResponsiveLayout : MonoBehaviour
    {
        [Header("Layout Settings")]
        [SerializeField] private CanvasScaler canvasScaler;
        [SerializeField] private RectTransform safeArea;
        [SerializeField] private bool useSafeArea = true;
        
        [Header("Responsive Elements")]
        [SerializeField] private RectTransform[] responsiveElements;
        [SerializeField] private float mobileScale = 0.8f;
        [SerializeField] private float tabletScale = 1.0f;
        [SerializeField] private float desktopScale = 1.2f;
        
        [Header("Orientation Handling")]
        [SerializeField] private bool handleOrientation = true;
        [SerializeField] private Vector2 portraitLayout;
        [SerializeField] private Vector2 landscapeLayout;
        
        private Vector2 lastScreenSize;
        private ScreenOrientation lastOrientation;
        
        void Start()
        {
            InitializeResponsiveLayout();
        }
        
        void Update()
        {
            CheckScreenChanges();
        }
        
        void InitializeResponsiveLayout()
        {
            if (canvasScaler == null)
            {
                canvasScaler = GetComponentInParent<CanvasScaler>();
            }
            
            if (safeArea == null)
            {
                safeArea = GetComponent<RectTransform>();
            }
            
            SetupCanvasScaler();
            ApplySafeArea();
            ScaleElementsForDevice();
            lastScreenSize = new Vector2(Screen.width, Screen.height);
            lastOrientation = Screen.orientation;
        }
        
        void SetupCanvasScaler()
        {
            if (canvasScaler == null) return;
            
            // Set up canvas scaler for responsive design
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }
        
        void ApplySafeArea()
        {
            if (!useSafeArea || safeArea == null) return;
            
            Rect safeRect = Screen.safeArea;
            Vector2 anchorMin = safeRect.position;
            Vector2 anchorMax = safeRect.position + safeRect.size;
            
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            
            safeArea.anchorMin = anchorMin;
            safeArea.anchorMax = anchorMax;
        }
        
        void ScaleElementsForDevice()
        {
            float targetScale = GetDeviceScale();
            
            foreach (var element in responsiveElements)
            {
                if (element != null)
                {
                    element.localScale = Vector3.one * targetScale;
                }
            }
        }
        
        float GetDeviceScale()
        {
            float aspectRatio = (float)Screen.width / Screen.height;
            
            // Determine device type based on aspect ratio and resolution
            if (Screen.width >= 1920 || Screen.height >= 1080)
            {
                // Desktop or high-res device
                return desktopScale;
            }
            else if (aspectRatio > 1.5f || aspectRatio < 0.7f)
            {
                // Mobile device
                return mobileScale;
            }
            else
            {
                // Tablet device
                return tabletScale;
            }
        }
        
        void CheckScreenChanges()
        {
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            ScreenOrientation currentOrientation = Screen.orientation;
            
            bool screenChanged = currentScreenSize != lastScreenSize;
            bool orientationChanged = currentOrientation != lastOrientation;
            
            if (screenChanged)
            {
                OnScreenSizeChanged(currentScreenSize);
                lastScreenSize = currentScreenSize;
            }
            
            if (orientationChanged && handleOrientation)
            {
                OnOrientationChanged(currentOrientation);
                lastOrientation = currentOrientation;
            }
        }
        
        void OnScreenSizeChanged(Vector2 newSize)
        {
            // Recalculate safe area
            ApplySafeArea();
            
            // Rescale elements if needed
            ScaleElementsForDevice();
            
            // Adjust layout for new screen size
            AdjustLayoutForScreenSize(newSize);
        }
        
        void OnOrientationChanged(ScreenOrientation newOrientation)
        {
            Vector2 targetLayout;
            
            switch (newOrientation)
            {
                case ScreenOrientation.Portrait:
                case ScreenOrientation.PortraitUpsideDown:
                    targetLayout = portraitLayout;
                    break;
                case ScreenOrientation.LandscapeLeft:
                case ScreenOrientation.LandscapeRight:
                    targetLayout = landscapeLayout;
                    break;
                default:
                    targetLayout = Vector2.one;
                    break;
            }
            
            ApplyOrientationLayout(targetLayout);
        }
        
        void AdjustLayoutForScreenSize(Vector2 screenSize)
        {
            float aspectRatio = screenSize.x / screenSize.y;
            
            // Adjust spacing and sizing based on aspect ratio
            if (aspectRatio > 2.0f)
            {
                // Ultra-wide screens
                ApplyUltraWideLayout();
            }
            else if (aspectRatio < 0.5f)
            {
                // Very tall screens
                ApplyTallScreenLayout();
            }
        }
        
        void ApplyOrientationLayout(Vector2 layout)
        {
            // Apply different layouts for portrait vs landscape
            foreach (var element in responsiveElements)
            {
                if (element != null)
                {
                    // Adjust element positioning based on orientation
                    Vector2 currentPos = element.anchoredPosition;
                    element.anchoredPosition = new Vector2(
                        currentPos.x * layout.x,
                        currentPos.y * layout.y
                    );
                }
            }
        }
        
        void ApplyUltraWideLayout()
        {
            // Adjust layout for ultra-wide screens
            // This could involve spreading elements horizontally
        }
        
        void ApplyTallScreenLayout()
        {
            // Adjust layout for very tall screens
            // This could involve stacking elements vertically
        }
        
        // Public methods for external control
        public void SetElementScale(float scale)
        {
            foreach (var element in responsiveElements)
            {
                if (element != null)
                {
                    element.localScale = Vector3.one * scale;
                }
            }
        }
        
        public void RefreshLayout()
        {
            InitializeResponsiveLayout();
        }
        
        public void SetSafeAreaEnabled(bool enabled)
        {
            useSafeArea = enabled;
            ApplySafeArea();
        }
        
        public void SetOrientationHandling(bool enabled)
        {
            handleOrientation = enabled;
        }
        
        // Editor helper methods
        #if UNITY_EDITOR
        [ContextMenu("Refresh Layout")]
        void EditorRefreshLayout()
        {
            InitializeResponsiveLayout();
        }
        
        [ContextMenu("Test Portrait Layout")]
        void TestPortraitLayout()
        {
            ApplyOrientationLayout(portraitLayout);
        }
        
        [ContextMenu("Test Landscape Layout")]
        void TestLandscapeLayout()
        {
            ApplyOrientationLayout(landscapeLayout);
        }
        #endif
    }
}

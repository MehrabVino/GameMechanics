using UnityEngine;

namespace MechanicGames.Match3
{
    /// <summary>
    /// Example script showing how to set up a Match3Theme with Tile Definitions.
    /// This demonstrates the proper way to configure themes after removing legacy systems.
    /// </summary>
    public class Match3ThemeSetupExample : MonoBehaviour
    {
        [Header("Theme Setup")]
        [SerializeField] private Match3Theme theme;
        
        [Header("Example Tile Sprites (Optional)")]
        [SerializeField] private Sprite[] exampleSprites;
        
        [Header("Example Colors")]
        [SerializeField] private Color[] exampleColors = {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            new Color(1f, 0.5f, 0f), // Orange
            new Color(0.6f, 0f, 0.8f) // Purple
        };

        [ContextMenu("Setup Example Theme")]
        public void SetupExampleTheme()
        {
            if (theme == null)
            {
                Debug.LogError("No theme assigned! Please assign a Match3Theme asset.");
                return;
            }

            // Create tile definitions
            var tileDefinitions = new Match3Theme.TileDefinition[6];
            
            for (int i = 0; i < 6; i++)
            {
                tileDefinitions[i] = new Match3Theme.TileDefinition
                {
                    id = $"Tile_{i}",
                    tileValue = i,
                    sprite = (exampleSprites != null && i < exampleSprites.Length) ? exampleSprites[i] : null,
                    color = exampleColors[i],
                    spawnWeight = 1f,
                    canSpawn = true
                };
            }

            // Apply to theme (this would normally be done in the Inspector)
            // For demonstration, we'll show what the setup should look like
            Debug.Log("=== Match3Theme Setup Example ===");
            Debug.Log("To set up your theme properly:");
            Debug.Log("1. Create a Match3Theme asset (Right-click → Create → MechanicGames → Match3 Theme)");
            Debug.Log("2. In the Inspector, set up Tile Definitions:");
            
            for (int i = 0; i < tileDefinitions.Length; i++)
            {
                var def = tileDefinitions[i];
                Debug.Log($"   Tile {i}: ID='{def.id}', Value={def.tileValue}, Color={def.color}, Sprite={(def.sprite != null ? "Assigned" : "None")}");
            }
            
            Debug.Log("3. Set Background Color to your desired board background");
            Debug.Log("4. Assign the theme to your Match3Mechanic component");
            Debug.Log("5. The system will now use Tile Definitions exclusively!");
        }

        [ContextMenu("Create Sample Theme Asset")]
        public void CreateSampleThemeAsset()
        {
            // This would create a theme asset programmatically
            // In practice, you'd create it through the Unity menu
            Debug.Log("To create a theme asset:");
            Debug.Log("1. Right-click in Project window");
            Debug.Log("2. Create → MechanicGames → Match3 Theme");
            Debug.Log("3. Name it 'DefaultTheme' or similar");
            Debug.Log("4. Configure the Tile Definitions in the Inspector");
        }

        void Start()
        {
            // Show setup instructions when the game starts
            if (theme == null)
            {
                Debug.LogWarning("Match3ThemeSetupExample: No theme assigned. Use the context menu to see setup instructions.");
            }
        }
    }
}

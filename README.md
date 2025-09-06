# MechanicGames - Unity Game Mechanics Framework

A comprehensive Unity framework providing modular, reusable game mechanics for different game types. This project includes implementations for **Match3** and **BlockBlast** game mechanics with a clean, extensible architecture.

## 🎯 Project Overview

MechanicGames is designed to provide a solid foundation for building various game types in Unity. Each mechanic is self-contained, follows SOLID principles, and can be easily integrated into existing projects or used as a starting point for new games.

## 🎮 Featured Game Mechanics

### 🟦 Match3 - Classic Puzzle Game
A fully-featured match-3 puzzle game mechanic with:
- **Grid-based board system** with customizable dimensions (default: 8x8)
- **Smart match detection** for horizontal, vertical, and diagonal patterns
- **Cascade system** with chain reactions and combo multipliers
- **Score calculation** with configurable points per tile and chain bonuses
- **Theme system** for easy visual customization with sprites and colors
- **Event-driven architecture** for smooth UI updates and animations
- **Mouse-based input** with drag-and-drop tile swapping
- **Gizmo rendering** for easy debugging and development
- **Clean, modular architecture** with clear separation of concerns

**Key Components:**
- `Match3Mechanic`: Main game controller implementing `IGameMechanic` (MonoBehaviour)
- `Match3Board`: Pure C# logic class for grid management and match resolution (Serializable)
- `Match3RuntimeView`: Visual representation and animation controller (MonoBehaviour, auto-connects to mechanic)
- `Match3Theme`: ScriptableObject-based theming system
- `Match3HUD`: Simple score display UI component (MonoBehaviour)

**Gameplay Features:**
- Swap adjacent tiles to create matches of 3 or more
- Automatic cascade resolution with chain reactions
- Score multipliers for longer chains
- Configurable tile types and board dimensions
- Built-in high score system using PlayerPrefs

### 🟨 BlockBlast - Block-Matching Puzzle
A block-matching puzzle game mechanic featuring:
- **Shape-based gameplay** with falling block pieces
- **Combo system** with multipliers and chain reactions
- **Progressive difficulty** with increasing complexity
- **Visual effects** including screen shake and particle systems
- **Responsive UI** that adapts to different screen sizes
- **Object pooling** for optimal performance

**Key Components:**
- `BlockBlastMechanic`: Core game logic implementing `IGameMechanic`
- `BlockBlastGameManager`: Game state and progression management
- `BlockBlastEffects`: Visual effects and screen shake system
- `BlockBlastGameUI`: Comprehensive UI system with multiple panels
- `BlockBlastObjectPool`: Performance-optimized object pooling

## 📁 Project Structure

```
Assets/
├── Scripts/
│   └── GameMechanics/
│       ├── Shared/                          # Common interfaces and utilities
│       │   ├── Interfaces/                  # Core interfaces for game mechanics
│       │   │   └── IGameMechanic.cs        # Base interface for all game mechanics
│       │   └── Utilities/                   # Shared utility classes
│       │       └── GameObjectPool.cs        # Generic object pooling system
│       │
│       ├── Match3/                          # Match3 game mechanic
│       │   ├── Board/                       # Board logic and management
│       │   │   ├── IMatch3BoardReadOnly.cs # Read-only board interface
│       │   │   └── Match3Board.cs          # Pure C# board logic class
│       │   ├── Runtime/                     # Core game logic
│       │   │   ├── IMatch3Context.cs       # Game context interface
│       │   │   ├── Match3Mechanic.cs       # Main game mechanic
│       │   │   └── Match3RuntimeView.cs    # Runtime view controller
│       │   ├── Theme/                       # Visual theming system
│       │   │   └── Match3Theme.cs          # Theme configuration
│       │   ├── UI/                          # User interface components
│       │   │   └── Match3HUD.cs            # Heads-up display
│       │   └── Utilities/                   # Match3-specific utilities
│       │
│       └── BlockBlast/                      # BlockBlast game mechanic
│           ├── Runtime/                     # Core game logic
│           │   ├── BlockBlastMechanic.cs    # Main game mechanic
│           │   ├── BlockBlastGameManager.cs # Game state management
│           │   └── BlockBlastConfig.cs      # Configuration settings
│           ├── UI/                          # User interface components
│           │   ├── BlockBlastGameUI.cs      # Main game UI
│           │   └── UIResponsiveLayout.cs    # Responsive layout system
│           ├── Effects/                      # Visual and audio effects
│           │   └── BlockBlastEffects.cs     # Particle and screen effects
│           └── Utilities/                   # BlockBlast-specific utilities
│               └── BlockBlastObjectPool.cs  # Object pooling system
├── Scenes/
│   ├── Match3Scene.unity                   # Ready-to-play Match3 scene
│   └── Match3_Scene.unity                  # Alternative Match3 scene
├── Resources/
│   └── Match3/
│       └── DefaultTheme.asset              # Default Match3 theme configuration
└── Prefabs/
    └── BlockBlast/                         # BlockBlast prefab assets
```

## 🚀 Quick Start - Match3

### Option 1: Use Existing Scene (Recommended)
1. **Open Unity** and load the project
2. **Navigate to** `Assets/Scenes/` folder
3. **Double-click** on `Match3Scene.unity` or `Match3_Scene.unity`
4. **Press Play** button to start playing immediately

### Option 2: Create New Scene from Scratch

#### 1. Basic Scene Setup
```
Scene Hierarchy:
├── Main Camera (Orthographic)
├── Directional Light
├── Canvas (UI)
├── GameMechanics (Empty GameObject)
│   └── Match3
└── UI (Empty GameObject)
    └── Match3HUD
```

#### 2. Configure Match3 Mechanic
1. **Create Empty GameObject** → "Match3"
2. **Add Component** → `Match3Mechanic`
3. **Configure in Inspector:**
   - **Board Width**: 8 (default)
   - **Board Height**: 8 (default)
   - **Cell Size**: 1.0
   - **Score Per Tile**: 10
   - **Bonus Per Chain**: 5
   - **Draw Gizmos**: true (for debugging)
4. **Board automatically initializes** when the component starts (no manual setup needed)

#### 3. Configure Match3 Board
The `Match3Board` is a pure C# class that gets instantiated automatically by `Match3Mechanic`. You can configure the board settings directly in the Match3Mechanic inspector:
- **Board Width**: 8 (default)
- **Board Height**: 8 (default)
- **Number of Tile Types**: 6 (default)
- **Seed**: 0 (for random generation, or set specific value for reproducible boards)

#### 4. Create Match3 Theme
1. **Right-click** in Project window → Create → ScriptableObject → Match3 Theme
2. **Configure** the theme with:
   - **Tile Definitions**: Array of tile types with sprites and colors
   - **Background Sprite**: Board background image
   - **Background Color**: Board background color

#### 5. Set Up Match3 View
1. **Create Empty** child under Match3 → "View"
2. **Add Component** → `Match3RuntimeView`
3. **The view automatically connects** to the parent Match3Mechanic and gets all necessary data from it
4. **No manual reference assignment needed** - the view finds the mechanic automatically

#### 6. Configure Camera
1. **Select Main Camera**
2. **Set Projection** to "Orthographic"
3. **Set Size** to 5-10 (adjust based on board size)
4. **Position** at (0, 0, -10)

#### 7. Set Up UI
1. **Create UI → Canvas**
2. **Set Render Mode** to "Screen Space - Overlay"
3. **Add Canvas Scaler** component
4. **Set UI Scale Mode** to "Scale With Screen Size"
5. **Create UI → Text** under Canvas
6. **Add Component** → `Match3HUD`
7. **Assign References**:
   - **Score Text**: Drag the Text component
   - **Game Mechanic**: Drag the Match3Mechanic

## 🎮 How to Play Match3

### Controls
- **Mouse**: Click and drag adjacent tiles to swap them
- **Goal**: Create matches of 3 or more identical tiles
- **Scoring**: Earn points for each tile cleared + chain bonuses

### Gameplay Mechanics
1. **Tile Swapping**: Click and drag a tile to an adjacent position
2. **Match Detection**: Automatic detection of horizontal, vertical, and diagonal matches
3. **Cascade System**: Matched tiles disappear, new tiles fall down
4. **Chain Reactions**: Multiple matches in sequence create combos
5. **Score Multipliers**: Longer chains provide bonus points

### Configuration Options
- **Board Size**: Customizable width and height
- **Tile Types**: Configurable number of different tile types
- **Scoring**: Adjustable points per tile and chain bonuses
- **Visual Theme**: Customizable colors, sprites, and backgrounds

## 🔧 Core Architecture

### IGameMechanic Interface
All game mechanics implement the `IGameMechanic` interface, providing a consistent API:

```csharp
public interface IGameMechanic
{
    // Game State
    bool IsGameActive { get; }
    bool IsPaused { get; }
    int CurrentScore { get; }
    int HighScore { get; }
    
    // Events
    System.Action OnGameStart { get; set; }
    System.Action OnGamePause { get; set; }
    System.Action OnGameResume { get; set; }
    System.Action OnGameOver { get; set; }
    System.Action<int> OnScoreChanged { get; set; }
    
    // Game Control
    void StartGame();
    void PauseGame();
    void ResumeGame();
    void EndGame();
    void ResetGame();
    void AddScore(int points);
}
```

### Match3-Specific Interfaces
```csharp
public interface IMatch3Context
{
    IMatch3BoardReadOnly Board { get; }
    int BoardWidth { get; }
    int BoardHeight { get; }
    float CellSize { get; }
    Vector3 BoardOriginWorld { get; }
    Match3Theme Theme { get; }
    IReadOnlyList<Vector2Int> ClearedCells { get; }
    IReadOnlyList<Vector2Int> SpawnedCells { get; }
}

public interface IMatch3BoardReadOnly
{
    int Width { get; }
    int Height { get; }
    int GetTile(int x, int y);
    IReadOnlyList<Vector2Int> LastClearedCells { get; }
    IReadOnlyList<Vector2Int> LastSpawnedCells { get; }
}
```

### Match3 Architecture Notes
- **`Match3Board`**: Pure C# class marked with `[Serializable]`, instantiated directly in `Match3Mechanic`
- **`Match3Mechanic`**: MonoBehaviour that contains the board instance and handles Unity-specific logic
- **`Match3RuntimeView`**: Automatically connects to parent mechanic, no manual reference assignment needed
- **Separation of Concerns**: Board logic is pure C# (testable, portable), Unity integration is in the Mechanic

### New Clean Implementation Features
- **Modular Design**: Each component has a single responsibility and clear interfaces
- **Comprehensive Documentation**: Extensive XML documentation and inline comments
- **Error Handling**: Robust null checks and graceful fallbacks throughout
- **Performance Optimized**: Efficient algorithms for match detection and cascade resolution
- **Animation System**: Smooth tile movements, pulse effects, and visual feedback
- **Theme Support**: Flexible theming system with fallback to default colors
- **Debug Support**: Built-in gizmo rendering and comprehensive logging
- **Complete Theme Integration**: Full theme support with automatic tile spawning, colors, and sprites

### Enhanced Theme System
- **Smart Tile Spawning**: Theme-aware tile generation with weighted spawning support
- **Automatic Visual Updates**: Tiles automatically get correct sprites and colors from theme
- **Priority-Based Rendering**: Tile Definitions → Legacy Sprites → Legacy Colors → Fallback
- **Performance Optimized**: Cached theme data and efficient sprite/color lookups
- **Editor Integration**: Full Unity inspector support with validation and tooltips

### Shared Utilities
- **GameObjectPool**: Generic object pooling system for performance optimization
- **Event System**: Consistent event handling across all mechanics
- **Configuration**: Centralized configuration management

## 🎨 Customization

### Match3 Visual Theming
1. **Create Theme Asset**: Right-click → Create → ScriptableObject → Match3 Theme
2. **Configure Tile Definitions**:
   ```csharp
   [System.Serializable]
   public sealed class TileDefinition
   {
       public string id;
       public Sprite sprite;
       public Color color = Color.white;
   }
   ```
3. **Set Background**: Choose background sprite and color
4. **Assign to Mechanic**: Drag theme asset to Match3Mechanic's Theme field

### Gameplay Balancing
- **Score Values**: Adjust `scorePerTile` and `bonusPerChain` in Match3Mechanic inspector
- **Board Dimensions**: Modify board settings in Match3Mechanic inspector (board is automatically instantiated)
- **Tile Types**: Change `numberOfTileTypes` in the board configuration
- **Board Seed**: Set specific seed value for reproducible board layouts

### UI Customization
- **Score Format**: Modify `scoreFormat` string in Match3HUD
- **Text Styling**: Customize Text component appearance
- **Layout**: Adjust Canvas settings for different screen sizes

## 🚀 Performance Optimization

### Built-in Optimizations
- **Object Pooling**: Generic GameObjectPool system
- **Efficient Match Detection**: Optimized algorithms for finding matches
- **Event-Driven Updates**: UI updates only when needed
- **Minimal Garbage Collection**: Careful memory management

### Best Practices
1. **Use Object Pooling** for frequently created/destroyed objects
2. **Implement Efficient Match Detection** algorithms
3. **Minimize Garbage Collection** with proper memory management
4. **Use Events** instead of Update loops for UI updates
5. **Batch Rendering** by keeping similar sprites in the same material

## 🔍 Troubleshooting

### Common Issues

#### Scripts Not Found
- **Solution**: Ensure all scripts are in the correct folders
- **Check**: Scripts should be in `Assets/Scripts/GameMechanics/`

#### Missing References
- **Solution**: Check Inspector for missing field assignments
- **Check**: Ensure all required components are added
- **Note**: Match3RuntimeView automatically connects to parent Match3Mechanic

#### UI Not Displaying
- **Solution**: Verify Canvas setup and UI component references
- **Check**: Ensure UI elements are children of Canvas

#### Game Not Responding to Input
- **Solution**: Check Input System package installation
- **Check**: Verify camera setup and positioning

#### Match3 Board Not Visible
- **Solution**: Enable "Draw Gizmos" in Match3Mechanic
- **Check**: Ensure camera is positioned correctly
- **Check**: Verify board dimensions and cell size

#### NullReferenceException in Match3Board.GetTile
- **Solution**: The board is automatically initialized in Awake() method
- **Check**: Ensure Match3Mechanic component is properly added to GameObject
- **Check**: Verify the scene hierarchy has Match3Mechanic as parent of Match3RuntimeView

#### Tiles Not Rendering
- **Solution**: Check Console for debug messages from Match3RuntimeView
- **Check**: Ensure Match3RuntimeView is a child of Match3Mechanic GameObject
- **Check**: Verify camera is positioned correctly and "Draw Gizmos" is enabled
- **Check**: Look for "Rebuilding tiles" and "Successfully created X tiles" messages in Console

#### Sprite Tiling Warning
- **Solution**: Fixed by using SpriteDrawMode.Simple instead of Sliced
- **Note**: Dynamically generated sprites work better with Simple mode
- **Check**: Tiles now use transform scaling instead of sprite size for dimensions

### Debug Tools
- **Unity Console**: Check for compilation and runtime errors
- **Unity Profiler**: Monitor performance metrics
- **Scene Hierarchy**: Verify component relationships
- **Inspector**: Check serialized field assignments
- **Gizmos**: Visual debugging for Match3 board layout

## 🎨 Enhanced Theme System

### **Smart Tile Spawning**
The new theme system automatically handles tile spawning with theme-aware generation:
- **Weighted Spawning**: Control tile rarity with spawn weights (0.0 to 1.0)
- **Consistent Generation**: Use spawn seeds for reproducible results
- **Performance Optimized**: Cached theme data for smooth gameplay

### **Automatic Visual Updates**
Tiles now automatically get the correct sprites and colors:
- **Priority System**: Tile Definitions → Legacy Sprites → Legacy Colors → Fallback
- **Real-time Updates**: Theme changes immediately affect all tiles
- **Animation Compatible**: Base colors are maintained for smooth effects

### **How to Use**

#### **Option 1: Tile Definitions (Recommended)**
1. Create a `Match3Theme` asset with `tileDefinitions` array
2. Configure each tile with:
   - `id`: Unique identifier (e.g., "RedGem", "BlueGem")
   - `tileValue`: Numeric value (0, 1, 2, etc.)
   - `sprite`: Custom sprite for the tile
   - `color`: Fallback color if no sprite
   - `spawnWeight`: Probability of spawning (0.0 to 1.0)
   - `canSpawn`: Whether this tile type can appear
3. Assign the theme to your `Match3Mechanic`
4. Tiles automatically spawn with correct visuals and weights

#### **Option 2: Legacy Arrays (Fallback)**
1. Use `tileSprites` array for sprite-based themes
2. Use `tileColors` array for color-only themes
3. Assign the theme to your `Match3Mechanic`
4. System automatically falls back to legacy mode

### **Advanced Features**
- **Theme Validation**: Automatic validation ensures proper configuration
- **Editor Integration**: Full Unity inspector support with tooltips
- **Performance**: Efficient sprite/color lookups with caching
- **Flexibility**: Mix and match different theme approaches

## 📋 Requirements

- **Unity Version**: 2022.3 LTS or later
- **Render Pipeline**: Universal Render Pipeline (URP) recommended
- **Input System**: New Input System package
- **TextMeshPro**: For UI text rendering
- **Platforms**: Windows, macOS, Linux, Android, iOS, WebGL

## 🎯 Next Steps

### Immediate Actions
1. **Test** the existing Match3 scene
2. **Customize** visual appearance using themes
3. **Adjust** gameplay parameters for balance
4. **Add** sound effects and music

### Advanced Features
1. **Save/Load System**: Implement persistent high scores
2. **Level System**: Create multiple board configurations
3. **Power-ups**: Add special tile types and abilities
4. **Multiplayer**: Implement turn-based or real-time multiplayer
5. **Analytics**: Track player behavior and performance

### Integration
1. **Add to Existing Projects**: Copy GameMechanics folder
2. **Create New Games**: Use as foundation for puzzle games
3. **Extend Framework**: Add new game mechanics following the pattern
4. **Customize for Platforms**: Optimize for mobile, console, or PC

## 🤝 Contributing

This framework is designed to be easily extensible. When adding new mechanics or features:

1. Follow the existing code structure and naming conventions
2. Implement the `IGameMechanic` interface for consistency
3. Add comprehensive documentation and comments
4. Test thoroughly across different platforms
5. Update this README with new information

## 📄 License

This project is provided as-is for educational and development purposes. Feel free to use, modify, and distribute according to your project's needs.

---

**Happy Game Development! 🎮✨**

*Ready to create the next great puzzle game? Start with the Match3 scene and customize it to your heart's content!*




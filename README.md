# GameMechanics - Unity Game Mechanics Framework

A comprehensive Unity framework providing modular, reusable game mechanics for different game types. This project includes implementations for Match3 and BlockBlast game mechanics with a clean, extensible architecture.

## 🎯 Project Overview

GameMechanics is designed to provide a solid foundation for building various game types in Unity. Each mechanic is self-contained, follows SOLID principles, and can be easily integrated into existing projects or used as a starting point for new games.

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
│       │   │   └── Match3Board.cs          # Main board implementation
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
```

## 🎮 Available Game Mechanics

### 1. Match3 Mechanic
A classic match-3 puzzle game mechanic with:
- **Grid-based board system** with customizable dimensions
- **Match detection** for horizontal, vertical, and diagonal patterns
- **Chain reaction system** for cascading matches
- **Score calculation** with combo multipliers
- **Theme system** for easy visual customization
- **Event-driven architecture** for UI updates

**Key Features:**
- `Match3Board`: Handles board logic, tile placement, and match detection
- `Match3Mechanic`: Main game controller implementing `IGameMechanic`
- `Match3Theme`: Visual theming system for different game styles
- `Match3RuntimeView`: Manages runtime visual updates and animations

### 2. BlockBlast Mechanic
A block-matching puzzle game mechanic featuring:
- **Shape-based gameplay** with falling block pieces
- **Combo system** with multipliers and chain reactions
- **Progressive difficulty** with increasing complexity
- **Visual effects** including screen shake and particle systems
- **Responsive UI** that adapts to different screen sizes
- **Object pooling** for optimal performance

**Key Features:**
- `BlockBlastMechanic`: Core game logic implementing `IGameMechanic`
- `BlockBlastGameManager`: Game state and progression management
- `BlockBlastEffects`: Visual effects and screen shake system
- `BlockBlastGameUI`: Comprehensive UI system with multiple panels
- `BlockBlastObjectPool`: Performance-optimized object pooling

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

### Shared Utilities
- **GameObjectPool**: Generic object pooling system for performance optimization
- **Event System**: Consistent event handling across all mechanics
- **Configuration**: Centralized configuration management

## 🚀 Implementation Guide

### Adding a New Game Mechanic

1. **Create the mechanic directory structure:**
   ```
   Assets/Scripts/GameMechanics/YourMechanic/
   ├── Runtime/          # Core game logic
   ├── UI/              # User interface components
   ├── Effects/          # Visual/audio effects
   └── Utilities/        # Mechanic-specific utilities
   ```

2. **Implement IGameMechanic interface:**
   ```csharp
   public class YourMechanic : MonoBehaviour, IGameMechanic
   {
       // Implement all required properties and methods
       public bool IsGameActive => isActive;
       public bool IsPaused => isPaused;
       // ... other properties
       
       public void StartGame() { /* Implementation */ }
       public void PauseGame() { /* Implementation */ }
       // ... other methods
   }
   ```

3. **Create configuration class:**
   ```csharp
   [CreateAssetMenu(fileName = "YourMechanicConfig", menuName = "GameMechanics/YourMechanic/Config")]
   public class YourMechanicConfig : ScriptableObject
   {
       [Header("Game Settings")]
       [SerializeField] private float gameSpeed = 1f;
       // ... other configurable parameters
   }
   ```

4. **Add UI components:**
   ```csharp
   public class YourMechanicUI : MonoBehaviour
   {
       [SerializeField] private YourMechanic gameMechanic;
       // ... UI implementation
   }
   ```

## 🎮 How to Run and Use the Project

### Prerequisites Setup

1. **Unity Version**: Ensure you have Unity 2022.3 LTS or later installed
2. **Required Packages**: The project will automatically import these packages:
   - **TextMeshPro**: For UI text rendering
   - **Universal Render Pipeline**: For modern rendering and effects
   - **Input System**: For input handling (if not already installed)

### Quick Start - Running the Project

#### Option 1: Using Existing Scenes
1. **Open Unity** and load the project
2. **Navigate to** `Assets/Scenes/` folder
3. **Double-click** on any existing scene (e.g., `SampleScene.unity`)
4. **Press Play** button to test the mechanics

#### Option 2: Creating a New Scene from Scratch

### Step-by-Step Scene Setup

#### 1. Create Basic Scene Structure
```
Scene Hierarchy:
├── Main Camera (Orthographic)
├── Directional Light
├── Canvas (UI)
├── GameMechanics (Empty GameObject)
│   ├── Match3
│   └── BlockBlast
└── UI (Empty GameObject)
    ├── Match3HUD
    └── BlockBlastHUD
```

#### 2. Set Up Camera
1. **Select Main Camera** in the hierarchy
2. **Set Projection** to "Orthographic"
3. **Set Size** to 5-10 (adjust based on your board size)
4. **Position** at (0, 0, -10)

#### 3. Set Up Canvas
1. **Create UI → Canvas** (right-click in hierarchy)
2. **Set Render Mode** to "Screen Space - Overlay"
3. **Add Canvas Scaler** component
4. **Set UI Scale Mode** to "Scale With Screen Size"

#### 4. Implement Match3 Mechanic

##### A. Create Match3 GameObject
1. **Right-click** on GameMechanics → Create Empty
2. **Rename** to "Match3"
3. **Add Component** → `Match3Mechanic`

##### B. Configure Match3Mechanic
1. **Select** the Match3 GameObject
2. **In Inspector**, configure these fields:
   - **Board Width**: 8 (default)
   - **Board Height**: 8 (default)
   - **Cell Size**: 1.0
   - **Score Per Tile**: 10
   - **Bonus Per Chain**: 5

##### C. Create Match3 Board
1. **Create Empty** child under Match3 → "Board"
2. **Add Component** → `Match3Board`
3. **Configure** board dimensions to match mechanic settings

##### D. Create Match3 View
1. **Create Empty** child under Match3 → "View"
2. **Add Component** → `Match3RuntimeView`
3. **Assign References**:
   - **Board**: Drag the Board GameObject
   - **Theme**: Create or assign a Match3Theme asset

##### E. Create Match3 Theme
1. **Right-click** in Project window → Create → ScriptableObject → Match3Theme
2. **Configure** the theme with:
   - **Tile Colors**: Array of colors for different tile types
   - **Background Color**: Board background color
   - **Cell Size**: Should match mechanic setting

#### 5. Implement BlockBlast Mechanic

##### A. Create BlockBlast GameObject
1. **Right-click** on GameMechanics → Create Empty
2. **Rename** to "BlockBlast"
3. **Add Component** → `BlockBlastMechanic`

##### B. Configure BlockBlastMechanic
1. **Select** the BlockBlast GameObject
2. **In Inspector**, configure:
   - **Board Width**: 10 (default)
   - **Board Height**: 20 (default)
   - **Tile Size**: 60
   - **Next Tiles Count**: 3

##### C. Create BlockBlast Game Manager
1. **Create Empty** child under BlockBlast → "GameManager"
2. **Add Component** → `BlockBlastGameManager`
3. **Assign References**:
   - **Game Mechanic**: Drag the BlockBlastMechanic
   - **Game UI**: Will be created next

##### D. Create BlockBlast UI
1. **Create Empty** child under BlockBlast → "UI"
2. **Add Component** → `BlockBlastGameUI`
3. **Configure UI Panels** (create these as child GameObjects):
   - **Main Menu Panel**
   - **Game Panel**
   - **Pause Panel**
   - **Game Over Panel**
   - **Settings Panel**

##### E. Create BlockBlast Effects
1. **Create Empty** child under BlockBlast → "Effects"
2. **Add Component** → `BlockBlastEffects`
3. **Configure** particle systems and screen shake settings

#### 6. Set Up UI Components

##### A. Match3 HUD
1. **Create UI → Text** under Canvas
2. **Rename** to "Match3HUD"
3. **Add Component** → `Match3HUD`
4. **Assign References**:
   - **Score Text**: Drag the Text component
   - **Game Mechanic**: Drag the Match3Mechanic

##### B. BlockBlast HUD
1. **Create UI → Text** under Canvas
2. **Rename** to "BlockBlastHUD"
3. **Add Component** → `BlockBlastHUD`
4. **Assign References**:
   - **Score Text**: Drag the Text component
   - **Game Mechanic**: Drag the BlockBlastMechanic

#### 7. Create Configuration Assets

##### A. BlockBlast Config
1. **Right-click** in Project window → Create → ScriptableObject → BlockBlastConfig
2. **Configure** game parameters:
   - **Board Settings**: Width, height, tile size
   - **Game Settings**: Score values, combo multipliers
   - **Animation Settings**: Durations, intensities

##### B. Assign Configs
1. **Select** BlockBlastMechanic
2. **Drag** the BlockBlastConfig to the "Game Config" field
3. **Select** Match3Mechanic
4. **Drag** the Match3Theme to the "Theme" field

### Running and Testing

#### 1. Test Match3
1. **Press Play** in Unity
2. **Click and drag** adjacent tiles to swap them
3. **Watch** for matches and cascading effects
4. **Check** score updates in the HUD

#### 2. Test BlockBlast
1. **Press Play** in Unity
2. **Use mouse** to place blocks
3. **Press R** to rotate blocks
4. **Press 1-3** to select from the queue
5. **Watch** for line clears and combos

### Troubleshooting Common Setup Issues

#### Issue: Scripts Not Found
- **Solution**: Ensure all scripts are in the correct folders
- **Check**: Scripts should be in `Assets/Scripts/GameMechanics/`

#### Issue: Missing References
- **Solution**: Check Inspector for missing field assignments
- **Check**: Ensure all required components are added

#### Issue: UI Not Displaying
- **Solution**: Verify Canvas setup and UI component references
- **Check**: Ensure UI elements are children of Canvas

#### Issue: Game Not Responding to Input
- **Solution**: Check Input System package installation
- **Check**: Verify camera setup and positioning

### Performance Optimization Tips

1. **Object Pooling**: Use the built-in object pooling systems
2. **Batch Rendering**: Keep similar sprites in the same material
3. **Efficient Updates**: Use Update() sparingly, prefer events
4. **Memory Management**: Avoid creating objects in Update loops

### Customization Examples

#### Changing Match3 Colors
1. **Select** your Match3Theme asset
2. **Modify** the Tile Colors array
3. **Add/Remove** colors to change tile types
4. **Adjust** Background Color for board appearance

#### Modifying BlockBlast Difficulty
1. **Select** your BlockBlastConfig asset
2. **Adjust** Combo Multiplier values
3. **Modify** Score Per Tile settings
4. **Change** Board dimensions

### Building for Different Platforms

1. **File → Build Settings**
2. **Select Target Platform** (PC, Mobile, WebGL)
3. **Add Scenes** to build
4. **Configure** platform-specific settings
5. **Build** the project

### Integration with Existing Projects

1. **Copy** the `GameMechanics` folder to your project
2. **Import** required packages (TextMeshPro, URP)
3. **Follow** the setup steps above
4. **Customize** mechanics to fit your game's needs
5. **Test** thoroughly before production use

### Next Steps After Setup

1. **Test** all mechanics thoroughly
2. **Customize** visual appearance and gameplay
3. **Add** sound effects and music
4. **Implement** save/load systems
5. **Add** analytics and achievements
6. **Optimize** for target platforms
7. **Create** additional game mechanics using the framework

---

### Integrating with Existing Projects

1. **Copy the GameMechanics folder** to your project's Assets/Scripts directory
2. **Import required dependencies:**
   - Unity UI (for UI components)
   - TextMeshPro (for text rendering)
   - Universal Render Pipeline (for effects)
3. **Set up the scene hierarchy** following the provided prefab structure
4. **Configure the mechanic** using the ScriptableObject configuration files

### Customization

#### Visual Theming
- **Match3**: Use `Match3Theme` ScriptableObject to customize colors, sprites, and visual effects
- **BlockBlast**: Modify `BlockBlastConfig` for visual parameters and adjust particle systems

#### Gameplay Balancing
- **Match3**: Adjust score values, combo multipliers, and board dimensions
- **BlockBlast**: Modify combo system, difficulty progression, and scoring mechanics

#### UI Layout
- **Responsive Design**: Use the `UIResponsiveLayout` system for adaptive UI
- **Panel System**: Leverage the panel-based UI architecture for easy customization

## 📋 Requirements

- **Unity Version**: 2022.3 LTS or later
- **Render Pipeline**: Universal Render Pipeline (URP) recommended
- **Input System**: New Input System package
- **TextMeshPro**: For UI text rendering
- **Platforms**: Windows, macOS, Linux, Android, iOS, WebGL

## 🎨 Best Practices

1. **Performance Optimization**
   - Use object pooling for frequently created/destroyed objects
   - Implement efficient match detection algorithms
   - Minimize garbage collection with proper memory management

2. **Code Organization**
   - Keep mechanics self-contained and modular
   - Use interfaces for loose coupling
   - Implement event-driven architecture for UI updates

3. **User Experience**
   - Provide smooth animations and transitions
   - Implement responsive controls
   - Use visual feedback for user actions

4. **Extensibility**
   - Design for easy modification and extension
   - Use ScriptableObjects for configuration
   - Implement plugin architecture for new features

## 🔍 Troubleshooting

### Common Issues

1. **Compilation Errors**
   - Ensure all required packages are imported
   - Check namespace references in using statements
   - Verify interface implementation completeness

2. **Runtime Errors**
   - Check component references in the Inspector
   - Verify configuration asset assignments
   - Ensure proper scene hierarchy setup

3. **Performance Issues**
   - Monitor object creation/destruction
   - Use Unity Profiler to identify bottlenecks
   - Implement object pooling where appropriate

### Debug Tools

- **Unity Console**: Check for compilation and runtime errors
- **Unity Profiler**: Monitor performance metrics
- **Scene Hierarchy**: Verify component relationships
- **Inspector**: Check serialized field assignments

## 📚 Additional Resources

- **Unity Documentation**: [docs.unity3d.com](https://docs.unity3d.com)
- **Unity Manual**: [manual.unity3d.com](https://manual.unity3d.com)
- **Unity Scripting Reference**: [docs.unity3d.com/ScriptReference](https://docs.unity3d.com/ScriptReference)

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




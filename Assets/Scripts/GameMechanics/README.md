# GameMechanics Project

## 🎯 Overview

A modular Unity project containing multiple game mechanics with shared utilities and interfaces. This project provides a scalable foundation for developing various game mechanics while maintaining consistency and performance.

## 📁 Project Structure

```
Assets/Scripts/GameMechanics/
├── Shared/                          # Shared components
│   ├── Interfaces/
│   │   └── IGameMechanic.cs        # Common game mechanic interfaces
│   └── Utilities/
│       └── GameObjectPool.cs       # Shared object pooling system
├── BlockBlast/                      # BlockBlast game mechanic
│   ├── BlockBlastConfig.cs         # Configuration system
│   ├── BlockBlastGameManager.cs    # Game management
│   ├── BlockBlastObjectPool.cs     # Memory management
│   ├── BlockBlastMechanic.cs       # Core game logic
│   ├── BlockBlastEffects.cs        # Visual effects
│   ├── BlockBlastGameUI.cs         # UI system
│   ├── UIResponsiveLayout.cs       # Responsive design
│   └── README.md                   # BlockBlast documentation
├── Match3/                          # Match3 game mechanic
│   ├── Board/
│   │   ├── Match3Board.cs          # Board logic
│   │   └── IMatch3BoardReadOnly.cs # Board interface
│   ├── Runtime/
│   │   ├── Match3Mechanic.cs       # Core game logic
│   │   ├── Match3RuntimeView.cs    # Runtime view
│   │   └── IMatch3Context.cs       # Context interface
│   ├── UI/
│   │   └── Match3HUD.cs            # UI system
│   └── Theme/
│       └── Match3Theme.cs          # Theming system
├── PROJECT_STRUCTURE.md            # Detailed structure overview
└── README.md                       # This file
```

## 🎮 Available Game Mechanics

### 1. **BlockBlast**
- **Type**: Puzzle/Strategy game
- **Features**: 
  - Mobile-optimized performance
  - Combo system with visual effects
  - Comprehensive UI with responsive design
  - Object pooling for memory efficiency
  - Configuration-driven settings
- **Status**: Complete and fully documented
- **Documentation**: See `BlockBlast/README.md`

### 2. **Match3**
- **Type**: Classic match-3 puzzle game
- **Features**:
  - Modular board system
  - Runtime view management
  - Theming system for customization
  - UI integration
- **Status**: Core system implemented
- **Documentation**: See `Match3/` directory

## 🔧 Shared Components

### Interfaces (`Shared/Interfaces/`)
- **IGameMechanic**: Common interface for all game mechanics
- **IGameMechanicConfig**: Configuration management interface
- **IGameMechanicUI**: UI consistency interface

### Utilities (`Shared/Utilities/`)
- **GameObjectPool**: Efficient object pooling system
- **Performance monitoring**: Built-in performance tracking
- **Mobile optimization**: Platform-specific optimizations

## 🚀 Getting Started

### 1. **Setup BlockBlast**
```bash
# Follow the setup guide in BlockBlast/README.md
# Create configuration assets
# Set up the scene with required components
```

### 2. **Setup Match3**
```bash
# Create Match3 scene
# Configure board settings
# Set up UI components
```

### 3. **Add New Game Mechanic**
```csharp
// 1. Create new folder in GameMechanics/
// 2. Implement IGameMechanic interface
// 3. Use shared utilities
// 4. Follow established patterns
```

## 📱 Mobile Optimization

### Shared Optimizations
- **Automatic platform detection**
- **Performance monitoring**
- **Memory management**
- **Quality settings adjustment**

### Mechanic-Specific Optimizations
- **BlockBlast**: Effect scaling, combo limits, mobile UI
- **Match3**: Board size limits, animation optimization
- **Future mechanics**: Follow established patterns

## 🔄 Development Workflow

### 1. **Adding New Mechanics**
1. Create new folder in `GameMechanics/`
2. Implement required interfaces
3. Use shared utilities where possible
4. Follow existing patterns and conventions

### 2. **Using Shared Components**
```csharp
// Object pooling
var pooledObject = GameObjectPool.Instance.GetPooledObject("tag", position, rotation);

// Interface implementation
public class NewMechanic : MonoBehaviour, IGameMechanic
{
    // Implement required methods
}
```

### 3. **Configuration Management**
- Use ScriptableObject-based configuration
- Implement mobile optimization
- Follow UI patterns from existing mechanics

## 📚 Documentation

### BlockBlast
- Complete setup and usage guide
- Mobile optimization details
- Performance considerations
- UI system documentation

### Match3
- Board system overview
- Runtime system documentation
- UI integration guide
- Theming system

### Shared Components
- Interface documentation
- Utility usage examples
- Best practices guide

## 🎯 Benefits

### 1. **Modularity**
- Each mechanic is self-contained
- Easy to add/remove mechanics
- Independent development possible

### 2. **Consistency**
- Shared interfaces ensure consistent behavior
- Common utilities reduce code duplication
- Standardized patterns across mechanics

### 3. **Performance**
- Shared object pooling
- Mobile optimization built-in
- Efficient memory management

### 4. **Scalability**
- Easy to add new game mechanics
- Shared components reduce development time
- Maintainable architecture

## 🔧 Technical Requirements

- **Unity Version**: 2022.3 LTS or newer
- **Platform**: Mobile (Android/iOS) and Desktop
- **Rendering**: Universal Render Pipeline (URP)
- **UI**: Unity UI (UGUI) with TextMeshPro

## 📄 License

This project is part of the MechanicGames framework and follows Unity's standard licensing terms.

---

For detailed information about each game mechanic, see their respective documentation folders.


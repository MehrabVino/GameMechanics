MechanicGames - Modular Mechanics and Match-3 Sample

Overview
This Unity project demonstrates a small, modular mechanic framework and a fully playable Match-3 mechanic that follows SOLID principles. It includes:
- Core pluggable mechanic interface and controller
- Match-3 board logic (swap, match, cascade, refill)
- Runtime view with animations (swap, neighbor pulse, spawn glow) and score popups
- Theming via ScriptableObject (tile sprites/colors, background)
- Simple UI HUD to show score

Unity Version
- Use the same Unity version listed in ProjectSettings/ProjectVersion.txt

Folder Structure (relevant)
- Assets/Scripts/Core: core mechanic interface and controller
- Assets/Scripts/Match3: match-3 logic, theme, runtime view, HUD

How to Run
1) Open Scenes/SampleScene.
2) In Hierarchy, create an empty GameObject "Mechanics" and add MechanicController.
3) Create an empty GameObject "Match3" and add Match3Mechanic.
4) Under "Match3", create empty GameObject "Match3View" and add Match3RuntimeView.
5) In MechanicController, add the Match3 GameObject to "Mechanic Behaviours".
6) Create a Canvas (UI) and Text to display score. Add Match3HUD to the Text (or a parent) and assign:
   - Mechanic: the Match3 GameObject
   - Score Text: the Text component
7) Ensure the main camera is tagged MainCamera and set to Orthographic so the board is visible.
8) Press Play. Click-drag (or click-release) adjacent tiles to swap. Valid swaps clear, cascade, and refill.

Theming
1) Create a Match3Theme asset: Right-click in Project window → Create → MechanicGames → Match3 Theme.
2) Set either Tile Definitions (preferred), or tileSprites/tileColors as fallback.
3) Optionally set a background sprite or color.
4) Assign the theme to Match3Mechanic → Theme. The board adjusts tile type count automatically.
5) You can switch themes at runtime via Match3Mechanic.SetTheme(newThemeAsset).

UI
- Match3HUD uses Unity UI (Text) to show score. For TextMeshPro, create a TMP variant and bind TMP_Text.

Controls
- Left mouse: select/drag adjacent tile to swap.

Architecture and SOLID

Single Responsibility
- Match3Board: pure board logic (state, matching, cascading, spawning). No rendering or input.
- Match3Mechanic: coordinates the board lifecycle, input handling, scoring, and communicates with the view.
- Match3RuntimeView: renders tiles, plays VFX/animations, and shows score popups.
- Match3Theme: data-only ScriptableObject for appearance.
- MechanicController: orchestrates mechanics in a scene.

Open/Closed
- New mechanics implement IGameMechanic and can be added to MechanicController without changing existing code.
- Match-3 visuals are themed via ScriptableObject without altering logic.

Liskov Substitution / Interface Segregation
- IMatch3BoardReadOnly exposes only what the view needs (Width, Height, GetTile, cleared/spawned lists).
- IMatch3Context exposes a narrow context (Board, Theme, CellSize, BoardOriginWorld).

Dependency Inversion
- The view reads through IMatch3Context/IMatch3BoardReadOnly rather than concrete types. Mechanic implements the context.

Extending
- Add UI polish (TextMeshPro, animated counters, buttons) by replacing Match3HUD.
- Add special tiles: extend Match3Board to mark special effects and teach view to animate them.
- Add new mechanics: create a component implementing IGameMechanic and plug it into MechanicController.

Notes
- Gizmos are for scene visualization; runtime view renders in Game view using SpriteRenderers.
- All timings/intensities for pulses/glow are editable in Match3RuntimeView.




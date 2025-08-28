MechanicGames - Modular Mechanics (Match-3 + BlockBlast)

Overview
This Unity project provides a small, modular mechanic framework with two sample mechanics:
- Core pluggable mechanic interface and controller
- Match-3: swap, match, cascade, refill with runtime view and HUD
- BlockBlast: place shapes and clear full rows/columns with previews, pop/flash feedback

Unity Version
- Use the same Unity version listed in ProjectSettings/ProjectVersion.txt

Folder Structure (relevant)
- Assets/Scripts/Core: interfaces, controller, pooling, tile factory (quad)
- Assets/Scripts/Match3: board logic, runtime view, HUD
- Assets/Scripts/BlockBlast: board logic, mechanic, HUD

Quick Run Roadmap
1) Open Scenes/SampleScene.
2) Create GameObject "Mechanics" → add `MechanicController`.
3) Create GameObject "Match3" → add `Match3Mechanic` → child "Match3View" with `Match3RuntimeView`.
4) Create GameObject "BlockBlast" → add `BlockBlastMechanic`.
   - Optional: assign `tilePrefab` (quad with material) or create a `QuadTileFactory` asset and assign to the mechanic.
5) In `MechanicController`, add both "Match3" and "BlockBlast" to Mechanic Behaviours.
6) Create Canvas → Text for Match3 score → add `Match3HUD` and bind score text. Create another Text for BlockBlast score → add `BlockBlastHUD` and bind.
7) Ensure Main Camera is Orthographic and looks at the board areas.
8) Play.
   - Match-3: drag adjacent tiles to swap; valid swaps clear and cascade.
   - BlockBlast: hover shows placeholder; LMB places; R rotates; 1/2/3 selects from queue.

Theming
1) Create a Match3Theme asset: Right-click in Project window → Create → MechanicGames → Match3 Theme.
2) Set either Tile Definitions (preferred), or tileSprites/tileColors as fallback.
3) Optionally set a background sprite or color.
4) Assign the theme to Match3Mechanic → Theme. The board adjusts tile type count automatically.
5) You can switch themes at runtime via Match3Mechanic.SetTheme(newThemeAsset).

UI
- `Match3HUD`: Unity UI (Text) score binding. For TMP, swap to TMP_Text in code.
- `BlockBlastHUD`: Unity UI (Text) score binding; mechanic handles previews and feedback.

Controls
- Left mouse: select/drag adjacent tile to swap.
- BlockBlast: Left mouse to place shape, R to rotate, 1-3 to select from queue.

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

Scripts Overview
- Core
  - `IGameMechanic`: interface for pluggable mechanics.
  - `MechanicController`: bootstraps and toggles mechanics.
  - `SimpleGameObjectPool`: basic pooled instantiation for popups.
  - `ITileFactory` / `QuadTileFactory`: modular tile creation (quad + material by default).
- Match3
  - `Match3Board` (read-only used by view), `Match3Mechanic`, `Match3RuntimeView`, `Match3HUD`.
- BlockBlast
  - `BlockBlastBoard`: occupancy grid; placement and full-line clearing; exposes last placed/cleared.
  - `BlockBlastMechanic`: input, queue/rotate, placeholder preview, tile rendering, pop/flash feedback.
  - `BlockBlastHUD`: score binding.

Notes
- Gizmos are for scene visualization; runtime view renders in Game view using SpriteRenderers.
- All timings/intensities for pulses/glow are editable in Match3RuntimeView. BlockBlastRuntimeView uses simple popups/Debug.DrawLine flashes.




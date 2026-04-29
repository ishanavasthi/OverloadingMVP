# FinishLine — Script Explainer

## Purpose
`FinishLine` is the trigger volume placed at the end of the road. When the truck enters it, the game is won. It also procedurally builds a black-and-white checker pattern so the finish line is visually recognizable without requiring imported textures.

## Trigger Detection
The script uses `OnTriggerEnter`. It checks whether the entering collider has a `TruckController` in its parent hierarchy — this ensures only the player truck (not random debris or child colliders) can trigger a win.

On detection, it calls `gameManager.Win()`.

## Checker Visual Builder
At `Awake()`, the script calls `BuildCheckerVisual()`:

1. Hides the original renderer (the host cube becomes invisible).
2. Creates a grid of small cubes (`checkerColumns × checkerRows`, default 12×3) as children.
3. Alternates black and white materials in a checkerboard pattern.
4. Removes colliders from tile cubes so they don't interfere with physics.
5. Uses a guard check (`transform.Find("Checker_00_00")`) to avoid duplicating tiles if called again.

## Material Handling
Materials are created at runtime using `Universal Render Pipeline/Lit` shader (falls back to `Standard` if URP isn't available). Two materials are shared across all tiles — one white, one black.

## Serialized Fields
| Field | Default | Description |
|---|---|---|
| `gameManager` | auto-found | Reference to the GameManager |
| `checkerColumns` | 12 | Number of columns in the checker grid |
| `checkerRows` | 3 | Number of rows in the checker grid |

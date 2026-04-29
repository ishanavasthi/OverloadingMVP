# GameManager — Script Explainer

## Purpose
`GameManager` owns the game loop state machine: **Playing → Won** or **Playing → Lost**. It keeps game-state decisions out of the physics scripts and provides the central win/loss/restart flow.

## State Machine
```
Playing ──(truck reaches finish)──► Won
Playing ──(topple / fall / hazard)──► Lost
Won or Lost ──(press R)──► Restart (scene reload)
```

## Lose Conditions
Three checks run every `Update()` while in the `Playing` state:

1. **Topple Detection** (`CheckTopple`): Measures the angle between the truck's local up and world up. If the angle exceeds `toppleAngle` (65°) or the dot product drops below 0.15 for longer than `toppleDelay` (0.8s), the truck is considered toppled.

2. **Fall Detection** (`CheckFall`): If the truck's Y position drops below `fallY` (-4), it has fallen off the road.

3. **Finish Distance Backup** (`CheckFinishDistance`): If the truck's Z position passes `finishZ` (61.5), trigger a win. This is a backup in case the FinishLine trigger is missed.

## Win / Loss Flow
- **Win**: Disables truck controls, stops motion, shows a congratulations modal.
- **Lose**: Disables truck controls, stops motion, freezes the Rigidbody (sets kinematic), shows a game-over modal with the failure reason and a gameplay tip.

Both states guard against double-triggering — once the state leaves `Playing`, subsequent calls are no-ops.

## Restart
Pressing `R` at any time calls `SceneManager.LoadScene()` to reload the active scene — a clean full reset.

## HUD Updates
Every frame during play, the manager updates:
- **Speed** display (m/s → km/h)
- **Lean angle** display (absolute roll degrees)
- **Debug text** (raw input values, controls state, velocity)

## Modal System
Win/loss messages are displayed via Unity's `OnGUI` immediate-mode system — a centered box with a title and body message. Styles are lazily initialized on first use.

## References
The GameManager holds serialized references to the `TruckController` and five `Text` UI components. If the truck reference is missing at Awake, it uses `FindAnyObjectByType<TruckController>()` as a fallback.

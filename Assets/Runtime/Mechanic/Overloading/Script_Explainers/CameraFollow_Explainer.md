# CameraFollow — Script Explainer

## Purpose
`CameraFollow` provides a smooth third-person chase camera that tracks the truck. It runs in `LateUpdate` so it always reads the truck's final position after physics and gameplay updates.

## How It Works

### Position Following
The camera computes a desired position using `target.TransformPoint(followOffset)` — this places the camera behind and above the truck in the truck's local space (default offset: `(0, 5.5, -9)`).

It then uses `Vector3.SmoothDamp` to glide toward that position, avoiding jarring snaps when the truck turns or accelerates. The `followSmoothTime` (0.15s) controls how quickly the camera catches up.

### Look-Ahead Rotation
Instead of looking directly at the truck, the camera looks at a point ahead of the truck: `target.TransformPoint(lookOffset)` (default: `(0, 1.2, 4)`). This gives the player a view of upcoming road, turns, and obstacles.

The rotation uses `Quaternion.Slerp` with `rotationLerp` for smooth angular transitions.

## Serialized Fields
| Field | Default | Description |
|---|---|---|
| `target` | set by scene builder | The Transform to follow (truck) |
| `followOffset` | (0, 5.5, -9) | Camera position relative to truck |
| `lookOffset` | (0, 1.2, 4) | Look-at point relative to truck |
| `followSmoothTime` | 0.15 | SmoothDamp time (lower = snappier) |
| `rotationLerp` | 10 | Rotation interpolation speed |

## Public API
| Method | Description |
|---|---|
| `SetTarget(Transform)` | Allows changing the follow target at runtime |

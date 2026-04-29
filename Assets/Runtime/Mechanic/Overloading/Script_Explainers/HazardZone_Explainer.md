# HazardZone — Script Explainer

## Purpose
`HazardZone` is a simple component that marks any GameObject as a hazard. When the truck collides with or enters a trigger on an object carrying this component, the game is lost.

## Detection
The script listens to both collision types:

- **`OnCollisionEnter`**: For solid physics collisions (barriers, obstacles).
- **`OnTriggerEnter`**: For trigger volumes (out-of-bounds zones, pits).

Both methods check `GetComponentInParent<TruckController>()` to ensure only the player truck causes a game over — not other physics objects bouncing around.

## Customizable Fail Reason
Each hazard instance has its own `failReason` string (default: "The truck hit a hazard."). The scene builder sets specific messages per hazard:
- Barriers: "You scraped the roadside barrier."
- Cargo blocks: "You hit the cargo blocks."
- Narrow gates: "You clipped the narrow gate."
- Final obstacle: "You hit the final obstacle."

This makes the game-over screen informative about what specifically went wrong.

## Serialized Fields
| Field | Default | Description |
|---|---|---|
| `gameManager` | auto-found | Reference to the GameManager |
| `failReason` | "The truck hit a hazard." | Message shown on game over |

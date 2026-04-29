# TruckController — Script Explainer

## Purpose
`TruckController` is the core physics controller for the player's overloaded truck. It reads WASD keyboard input and translates it into Rigidbody forces, torques, and direct position adjustments to create a top-heavy, unstable driving feel.

## Key Mechanic: Top-Heavy Balance
The defining game mechanic lives here. Three systems work together to make the truck feel dangerously overloaded:

1. **Raised Center of Mass** (`centerOfMassOffset`): The Rigidbody's center of mass is pushed upward (default Y = 1.1), making the truck inherently unstable — just like a real overloaded vehicle.

2. **Lateral Tip Force** (`lateralTipForce`): When steering at speed, a sideways force is applied proportional to speed × steer input. This simulates centrifugal load shift — faster turns push the truck sideways harder.

3. **Steering Roll Torque** (`steeringRollTorque`): On top of the lateral push, an additional roll torque makes the truck lean into turns. Turning right rolls the truck to the left, and vice versa.

## Stability Assist
To prevent the truck from being unplayably tippy, a small **roll stabilization** torque gently nudges the truck back upright when its roll angle is below `stabilizationMaxAngle` (35°). Beyond that angle the truck is considered to be toppling and stabilization stops — no saving it.

## Movement System
- **Motor**: Forward force is applied via `AddForce` with `VelocityChange` mode, clamped to `maxForwardSpeed`.
- **Braking**: Pressing S first brakes (if moving forward), then reverses up to `maxReverseSpeed`.
- **Coasting drag**: When no throttle is pressed, the truck gently decelerates.
- **Direct movement assist**: A `MovePosition` call supplements physics forces to ensure the truck always feels responsive, even if physics alone would be sluggish.
- **Debug fallback**: A `Transform`-based movement path (`debugTransformMovementFallback`) ensures the truck is playable even if Rigidbody physics aren't behaving as expected.

## Steering
Steering uses `MoveRotation` for crisp yaw response plus `AddTorque` for a physics-blended feel. Steering effectiveness scales with speed — the truck won't turn when stationary.

## Public API
| Member | Description |
|---|---|
| `Rigidbody` | Exposes the truck's Rigidbody for external reads |
| `CurrentSpeed` | Velocity magnitude in m/s |
| `SignedRollAngle` | Current lean angle (+ = leaning right) |
| `SetControlsEnabled(bool)` | GameManager calls this to freeze input on win/loss |
| `ResetTruck(pos, rot)` | Resets position, velocity, and center of mass |

## Debug HUD
An `OnGUI` overlay in the bottom-left corner shows keyboard state, controls enabled, throttle/steer values, velocity, position, and kinematic status — useful for diagnosing input issues in builds.

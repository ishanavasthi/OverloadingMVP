# Mechanics Index

A central registry of all game mechanics in this project. Each mechanic follows the standard folder layout and is self-contained under its own directory.

---

## Registered Mechanics

| # | Mechanic Name | Author | Description | Status |
|---|---|---|---|---|
| 1 | [Overloading](#overloading) | Ishan Avasthi | Top-heavy truck balancing — manage throttle and steering to prevent the overloaded truck from tipping over | ✅ Complete |

---

## Overloading

**Core Idea:** Drive an overloaded truck to the finish line. The cargo raises the center of mass, making the truck dangerously unstable during turns and acceleration.

### How It Works

The mechanic is built on three compounding physics systems:

1. **Raised Center of Mass** — The Rigidbody's center of mass is offset upward (`Y = 1.1`), replicating a top-heavy load. Higher center of mass → less stability.
2. **Lateral Tip Force** — When steering at speed, a sideways force proportional to `speed × steerInput` pushes the truck outward, simulating centrifugal load shift.
3. **Steering Roll Torque** — An additional roll torque makes the truck lean away from the turn direction. Turning right → lean left.

A small **roll stabilization** torque gently corrects lean up to 35°. Beyond that angle, stabilization cuts off entirely — the truck is past the point of no return.

### Folder Structure

```
Assets/
├── Runtime/
│   └── Mechanic/
│       └── Overloading/
│           ├── Scripts/
│           │   ├── TruckController.cs      ← Core mechanic: physics, input, balance
│           │   ├── GameManager.cs           ← Game loop: win / lose / restart
│           │   ├── FinishLine.cs            ← Win trigger + checker visual
│           │   ├── HazardZone.cs            ← Lose-on-contact markers
│           │   └── CameraFollow.cs          ← Smooth chase camera
│           └── Script_Explainers/
│               ├── TruckController_Explainer.md
│               ├── GameManager_Explainer.md
│               ├── FinishLine_Explainer.md
│               ├── HazardZone_Explainer.md
│               └── CameraFollow_Explainer.md
├── Sample/
│   └── Overloading/
│       └── OverloadingTruck.zip             ← Playable sample build
└── Editor/
    ├── Phase1SceneBuilder.cs                ← One-click scene builder
    └── WebGLBuild.cs                        ← WebGL export for itch.io
```

### Key Tuning Parameters

| Parameter | Value | Effect |
|---|---|---|
| `centerOfMassOffset.y` | 1.1 | Higher = more unstable |
| `lateralTipForce` | 1800 | Sideways push on steering |
| `steeringRollTorque` | 1200 | Roll lean on steering |
| `rollStabilization` | 450 | Corrective upright torque |
| `stabilizationMaxAngle` | 35° | Stabilization cutoff |
| `toppleAngle` | 65° | Game-over lean threshold |
| `toppleDelay` | 0.8s | Grace period before loss |
| `mass` | 1200 kg | Truck Rigidbody mass |

### Dependencies

- Unity `6000.4.3f1`
- Universal Render Pipeline (URP)
- Input System package

---

## Adding a New Mechanic

To register a new mechanic in this index:

1. Follow the folder structure described in [Contributing](README.md#contributing).
2. Add a row to the **Registered Mechanics** table above.
3. Add a new section below with: core idea, how it works, folder structure, key tuning parameters, and dependencies.

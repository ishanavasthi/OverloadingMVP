# Overloading

Overloading is a small 3D Unity truck simulator prototype built for a game jam. The goal is intentionally focused: drive an overloaded, top-heavy truck to the finish line without tipping over, falling off the road, or crashing into hazards.

## Gameplay

- Drive forward carefully and manage the truck's balance.
- The truck is overloaded, so aggressive throttle and sharp steering can make it lean and topple.
- Reach the black-and-white finish line to complete the race.
- Colliding with hazards, falling, or toppling causes a game over.
- Press `R` to restart after winning or losing.

## Controls

| Key | Action |
| --- | --- |
| `W` | Drive forward |
| `S` | Brake / reverse |
| `A` | Steer left |
| `D` | Steer right |
| `R` | Restart |

## Unity Version

This project was developed with Unity `6000.4.3f1`.

## Running the Game

1. Open the project folder in Unity Hub.
2. Open `Assets/Scenes/SampleScene.unity`.
3. Press Play.

If the generated scene needs to be rebuilt, use the Unity menu:

```text
Overloading > Build Complete MVP Scene
```

## WebGL Build For itch.io

This project includes an editor build script for WebGL:

```text
Overloading > Build WebGL
```

The build output is written to:

```text
Builds/WebGL
```

To upload to itch.io:

1. Install `WebGL Build Support` for Unity `6000.4.3f1` from Unity Hub.
2. Reopen this project in Unity.
3. Run `Overloading > Build WebGL`.
4. Zip the contents of `Builds/WebGL`.
5. Upload the zip to itch.io.
6. On itch.io, set the project kind to `HTML`.
7. Check `This file will be played in the browser`.

## Project Structure

```text
Assets/
+-- Editor/
|   +-- Phase1SceneBuilder.cs
|   +-- WebGLBuild.cs
+-- Scenes/
|   +-- SampleScene.unity
+-- Scripts/
    +-- CameraFollow.cs
    +-- FinishLine.cs
    +-- GameManager.cs
    +-- HazardZone.cs
    +-- TruckController.cs
```

## Main Scripts

- `TruckController.cs` handles WASD input, truck movement, top-heavy balancing, debug movement fallback, and Rigidbody setup.
- `GameManager.cs` owns win/loss state, restart, topple detection, falling detection, finish distance backup, and modal feedback.
- `FinishLine.cs` handles finish trigger detection and builds the black-and-white checker finish visual.
- `HazardZone.cs` marks colliders that cause a game over.
- `CameraFollow.cs` provides a simple chase camera.
- `Phase1SceneBuilder.cs` creates and wires the complete MVP scene from the Unity editor.
- `WebGLBuild.cs` builds the browser-playable WebGL version for itch.io.

## Design Notes

The game is scoped as a compact jam MVP. It avoids larger systems like cargo inventories, multiple levels, upgrades, AI traffic, or complex suspension. The core challenge is readable and immediate: drive fast enough to finish, but gently enough to keep the overloaded truck upright.

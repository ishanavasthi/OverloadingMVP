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

## Mechanics

All game mechanics are cataloged in [**MECHANICS.md**](MECHANICS.md). Currently registered:

| Mechanic | Description |
|---|---|
| **Overloading** | Top-heavy truck balancing — raised center of mass, lateral tip forces, and steering roll torque create an unstable driving challenge |

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
├── Runtime/
│   └── Mechanic/
│       └── <Mechanic_Name>/
│           ├── Scripts/            ← Runtime gameplay scripts
│           └── Script_Explainers/  ← Markdown docs explaining each script
├── Sample/
│   └── <Mechanic_Name>/
│       └── <Mechanic_Zip>.zip     ← Playable sample / build artifact
│       └── <Videos_Zip>.zip     ← Demo Video & Scripts Explained
├── Editor/                         ← Editor-only tooling scripts
└── Scenes/                         ← Unity scene files
```

## Main Scripts

- `TruckController.cs` handles WASD input, truck movement, top-heavy balancing, debug movement fallback, and Rigidbody setup.
- `GameManager.cs` owns win/loss state, restart, topple detection, falling detection, finish distance backup, and modal feedback.
- `FinishLine.cs` handles finish trigger detection and builds the black-and-white checker finish visual.
- `HazardZone.cs` marks colliders that cause a game over.
- `CameraFollow.cs` provides a simple chase camera.
- `Phase1SceneBuilder.cs` creates and wires the complete MVP scene from the Unity editor.
- `WebGLBuild.cs` builds the browser-playable WebGL version for itch.io.

## Contributing

### Prerequisites

- Unity **6000.4.3f1** (install via Unity Hub)
- Git

### Getting Started

1. **Clone the repository**
   ```bash
   git clone https://github.com/ishanavasthi/OverloadingMVP.git
   cd OverloadingMVP
   ```

2. **Open in Unity Hub**
   - Click *Open* → select the cloned folder.
   - Unity will import assets and regenerate project files automatically.

3. **Open the scene**
   - Navigate to `Assets/Scenes/SampleScene.unity` and double-click it.

4. **Press Play** to verify everything works.

### Adding a New Mechanic

Follow this folder structure **strictly**:

```text
Assets/
├── Runtime/
│   └── Mechanic/
│       └── <Your_Mechanic_Name>/
│           ├── Scripts/            ← Your runtime .cs files go here
│           └── Script_Explainers/  ← One .md explainer per script
├── Sample/
│   └── <Your_Mechanic_Name>/
│       └── <Mechanic_Zip>.zip     ← Playable demo or build artifact
```

**Step-by-step:**

1. **Create your mechanic folder**
   - Under `Assets/Runtime/Mechanic/`, create a folder named after your mechanic (e.g., `Drifting`).
   - Inside it, create `Scripts/` and `Script_Explainers/` subfolders.

2. **Write your scripts**
   - Place all runtime `.cs` files in `Scripts/`.
   - Editor-only scripts (if any) go in `Assets/Editor/`.

3. **Write script explainers**
   - For each `.cs` file, create a matching `<ScriptName>_Explainer.md` in `Script_Explainers/`.
   - Document the purpose, how it works, key methods, serialized fields, and public API.

4. **Add a sample**
   - Under `Assets/Sample/<Your_Mechanic_Name>/`, place a `.zip` of a playable build or demo scene.

5. **Register your mechanic**
   - Open [MECHANICS.md](MECHANICS.md).
   - Add a row to the **Registered Mechanics** table.
   - Add a detailed section for your mechanic below the table (core idea, how it works, folder structure, tuning parameters, dependencies).

6. **Test**
   - Open `SampleScene.unity` and press Play.
   - Verify your mechanic works and doesn't break existing mechanics.
   - If you have a WebGL build, test with `Overloading > Build WebGL`.

7. **Commit and push**
   ```bash
   git add -A
   git commit -m "Add <Your_Mechanic_Name> mechanic"
   git push origin main
   ```

### Code Guidelines

- Use `[SerializeField]` for Inspector-exposed fields; keep them `private`.
- Use `[RequireComponent]` to declare hard component dependencies.
- Use `FindAnyObjectByType<T>()` as a fallback for unassigned references, not as the primary wiring strategy.
- Keep runtime scripts in `Runtime/Mechanic/<Name>/Scripts/` — never in `Assets/Editor/`.
- All editor-only code must go in `Assets/Editor/` or be wrapped in `#if UNITY_EDITOR`.

## Design Notes

The game is scoped as a compact jam MVP. It avoids larger systems like cargo inventories, multiple levels, upgrades, AI traffic, or complex suspension. The core challenge is readable and immediate: drive fast enough to finish, but gently enough to keep the overloaded truck upright.

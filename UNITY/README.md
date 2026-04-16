# War of Tanks — Unity

2D top-down RTS game built in Unity 2020.3.49f1 (Built-in Pipeline).

## Requirements

- Unity Hub
- Unity **2020.3.49f1**

## How to Open

1. Open Unity Hub.
2. Click **Add** and select the `UNITY/` folder.
3. Open the project with Unity 2020.3.49f1.
4. Open the main scene from `Assets/Scenes/`.
5. Press **Play** to run.

## Project Structure

```
UNITY/
└── Assets/
    ├── Scenes/         # Game scenes (FinalScene, Tilemaps)
    ├── Scripts/        # C# game scripts
    ├── Prefabs/        # Tank and environment prefabs
    ├── Sprites/        # Sprite assets (Square, Circle)
    └── Tilemaps/       # Tile assets and palettes
        ├── Palettes/   # Tile palette prefabs
        └── Tiles/      # Flat-color placeholder tile assets
```

## Key Systems

| System | Issue | Status |
|---|---|---|
| Unity Project Base Setup | [#4](https://github.com/oussema-fatnassi/WarOfTanks/issues/4) | ✅ Done |
| Tank Prefab - Movement, Cannon, HP, Respawn | [#8](https://github.com/oussema-fatnassi/WarOfTanks/issues/8) | In progress |
| Environment - Tilemaps Setup | [#9](https://github.com/oussema-fatnassi/WarOfTanks/issues/9) | ✅ Done |
| Player Controls - Selection & Commands | [#10](https://github.com/oussema-fatnassi/WarOfTanks/issues/10) | Not started |
| RTS Camera | [#11](https://github.com/oussema-fatnassi/WarOfTanks/issues/11) | ✅ Done |
| Generic State Machine System | [#12](https://github.com/oussema-fatnassi/WarOfTanks/issues/12) | ✅ Done |
| Navigation - A* Pathfinding | [#13](https://github.com/oussema-fatnassi/WarOfTanks/issues/13) | Not started |
| Navigation - Dijkstra | [#14](https://github.com/oussema-fatnassi/WarOfTanks/issues/14) | Not started |
| Navigation - Flow Field | [#15](https://github.com/oussema-fatnassi/WarOfTanks/issues/15) | Not started |
| Local Obstacle Avoidance | [#16](https://github.com/oussema-fatnassi/WarOfTanks/issues/16) | Not started |
| Control Zone - State Machine | [#17](https://github.com/oussema-fatnassi/WarOfTanks/issues/17) | Not started |
| Gameplay & Win Conditions | [#18](https://github.com/oussema-fatnassi/WarOfTanks/issues/18) | Not started |
| Detection System - Field of View | [#19](https://github.com/oussema-fatnassi/WarOfTanks/issues/19) | Not started |
| Fog of War (WebGL-Compatible) | [#20](https://github.com/oussema-fatnassi/WarOfTanks/issues/20) | Not started |
| AI - Generic Behaviour Tree System | [#21](https://github.com/oussema-fatnassi/WarOfTanks/issues/21) | Not started |
| AI - Tank Behaviour Trees (Specializations) | [#22](https://github.com/oussema-fatnassi/WarOfTanks/issues/22) | Not started |
| Commander AI | [#23](https://github.com/oussema-fatnassi/WarOfTanks/issues/23) | Not started |
| WebGL Build (GitHub Actions) | [#7](https://github.com/oussema-fatnassi/WarOfTanks/issues/7) | Not started |

## Architecture Notes

- UML class diagrams committed to `docs/uml/` — Unity, Backend, and AI layers ([#1](https://github.com/oussema-fatnassi/WarOfTanks/issues/1) ✅)
- ERD (MongoDB schema) committed to `docs/erd/` ([#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) ✅)
- State machine diagrams committed to `docs/state-machines/` — Zone Capture FSM and Game State FSM ([#3](https://github.com/oussema-fatnassi/WarOfTanks/issues/3) ✅)
- Technology justification committed to `docs/technical-note/` ([#35](https://github.com/oussema-fatnassi/WarOfTanks/issues/35) — Figma mockups remaining)
- The AI + tank system must remain a self-contained modular prefab (championship requirement).
- Naming conventions: see `docs/naming-conventions.md` in the root repo.

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
    ├── Scenes/
    │   └── FeaturesTesting/   # Per-issue test scenes
    ├── Scripts/
    │   ├── AI/
    │   │   ├── BehaviourTree/ # IBehaviourNode, NodeStatus, Selector, Sequence, Inverter, Repeater, ActionNode, ConditionNode, BehaviourTree
    │   │   └── TankAI.cs      # MonoBehaviour AI controller (pathfinding, block detection)
    │   ├── Commands/          # ICommand implementations (Move, Attack, AttackZone, Stop)
    │   ├── Inputs/            # PlayerInputHandler (Unity Input System)
    │   ├── Interfaces/        # ICommand, ICommandReceiver, ISelectable, ITankComponents, IDamageable
    │   ├── Managers/          # SelectionManager singleton
    │   ├── Navigation/        # NavigationStrategy, AStarStrategy, StraightLineStrategy
    │   ├── Tanks/             # Tank, TankController, TurretController, SelectionIndicator, TankConstants
    │   ├── Tools/             # SingletonBehaviour<T>
    │   └── UI/                # SelectionBox, HealthBarUI
    ├── Tests/
    │   └── BehaviourTreeTests.cs  # 14 unit tests (Unity Test Runner)
    ├── Prefabs/               # Tank, GameManager, UI, Zone prefabs
    ├── Sprites/               # Sprite assets
    └── Tilemaps/              # Tile assets and palettes
```

## Key Systems

| System | Issue | Status |
|---|---|---|
| Unity Project Base Setup | [#4](https://github.com/oussema-fatnassi/WarOfTanks/issues/4) | ✅ Done |
| Tank Prefab - Movement, Cannon, HP, Respawn | [#8](https://github.com/oussema-fatnassi/WarOfTanks/issues/8) | ✅ Done |
| Environment - Tilemaps Setup | [#9](https://github.com/oussema-fatnassi/WarOfTanks/issues/9) | ✅ Done |
| Player Controls - Selection & Commands | [#10](https://github.com/oussema-fatnassi/WarOfTanks/issues/10) | ✅ Done |
| RTS Camera | [#11](https://github.com/oussema-fatnassi/WarOfTanks/issues/11) | ✅ Done |
| Generic State Machine System | [#12](https://github.com/oussema-fatnassi/WarOfTanks/issues/12) | ✅ Done |
| Navigation - A* Pathfinding | [#13](https://github.com/oussema-fatnassi/WarOfTanks/issues/13) | ✅ Done |
| Navigation - Dijkstra | [#14](https://github.com/oussema-fatnassi/WarOfTanks/issues/14) | Not started |
| Navigation - Flow Field | [#15](https://github.com/oussema-fatnassi/WarOfTanks/issues/15) | Not started |
| Local Obstacle Avoidance | [#16](https://github.com/oussema-fatnassi/WarOfTanks/issues/16) | ✅ Done |
| Control Zone - State Machine | [#17](https://github.com/oussema-fatnassi/WarOfTanks/issues/17) | ✅ Done |
| Gameplay & Win Conditions | [#18](https://github.com/oussema-fatnassi/WarOfTanks/issues/18) | ✅ Done |
| Detection System - Field of View | [#19](https://github.com/oussema-fatnassi/WarOfTanks/issues/19) | Not started |
| Fog of War (WebGL-Compatible) | [#20](https://github.com/oussema-fatnassi/WarOfTanks/issues/20) | Not started |
| AI - Generic Behaviour Tree System | [#21](https://github.com/oussema-fatnassi/WarOfTanks/issues/21) | ✅ Done |
| AI - Tank Behaviour Trees (Specializations) | [#22](https://github.com/oussema-fatnassi/WarOfTanks/issues/22) | Not started |
| Commander AI | [#23](https://github.com/oussema-fatnassi/WarOfTanks/issues/23) | Not started |
| WebGL Build (GitHub Actions) | [#7](https://github.com/oussema-fatnassi/WarOfTanks/issues/7) | Not started |

## Architecture Notes

- UML class diagrams committed to `docs/uml/` — Unity, Backend, and AI layers ([#1](https://github.com/oussema-fatnassi/WarOfTanks/issues/1) ✅)
- ERD (MongoDB schema) committed to `docs/erd/` ([#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) ✅)
- State machine diagrams committed to `docs/state-machines/` — Zone Capture FSM and Game State FSM ([#3](https://github.com/oussema-fatnassi/WarOfTanks/issues/3) ✅)
- Technology justification committed to `docs/technical-note/` ([#35](https://github.com/oussema-fatnassi/WarOfTanks/issues/35) — Figma mockups remaining)
- Navigation uses a custom Physics2D LayerMask-based grid (not Unity NavMesh) — Grid, PathNode, and INavigable are modular and algorithm-agnostic
- Behaviour Tree framework (`Assets/Scripts/AI/BehaviourTree/`) is pure C# with no MonoBehaviour dependencies — `TankAI` is the only Unity integration point
- The AI + tank system must remain a self-contained modular prefab (championship requirement)
- Naming conventions: see `docs/naming-conventions.md` in the root repo

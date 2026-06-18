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
    │   │   ├── CommanderAI.cs         # Team-level strategist — aggregates vision, evaluates scenarios, dispatches EStrategicOrder
    │   │   ├── TankAI.cs              # Orchestrator — BT tick, pathfinding, stall recalc, order reception (partial class)
    │   │   ├── TankAI.Actions.cs      # Private action methods + ExecuteStrategicOrder + ExecuteFullAggressionOrder
    │   │   ├── TankAI.CaptorTree.cs   # Captor role behaviour tree
    │   │   ├── TankAI.AttackerTree.cs # Attacker role behaviour tree
    │   │   ├── TankAI.DefenderTree.cs # Defender role behaviour tree
    │   │   ├── TankBlackboard.cs      # Shared data snapshot (HP, vision results, zone state)
    │   │   ├── VisionSystem.cs        # Detection system — 360° scan, Linecast LoS, _obstacleLayerMask
    │   │   └── DetectionResult.cs     # Per-target detection data (target, distance, angle, isInLineOfSight)
    │   ├── Commands/          # ICommand implementations (Move, Attack, AttackZone, Stop)
    │   ├── Enums/             # ETankTeam, ETankRole, EPathfinderType, EStrategicOrder
    │   ├── GameStates/        # GameStateMachine, PlayingState, PausedState, GameOverState
    │   ├── Inputs/            # PlayerInputHandler (Unity Input System)
    │   ├── Interfaces/        # ICommand, ICommandReceiver, ISelectable, ITankComponents, IDamageable, IVisionSystem
    │   ├── Managers/          # GameManager (tank registry, debug), SelectionManager, ScoreManager, MatchTimer, TeamManager
    │   ├── Navigation/        # NavigationStrategy, AStarStrategy, FlowFieldStrategy, StraightLineStrategy
    │   ├── Tanks/             # Tank, TankController, TurretController, PlayerAutoAim, SelectionIndicator, TankConstants
    │   ├── Tools/             # SingletonBehaviour<T>, DebugLogger
    │   └── UI/                # SelectionBox, HealthBarUI, ZoneUIController, GameHUD, GameOverScreen, MainMenuController
    ├── Tests/
    │   └── BehaviourTreeTests.cs  # 14 unit tests (Unity Test Runner)
    ├── Prefabs/
    │   └── Tanks/             # Tank.prefab (player), Enemy.prefab (AI — TankAI + VisionSystem)
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
| Navigation - Flow Field | [#15](https://github.com/oussema-fatnassi/WarOfTanks/issues/15) | ✅ Done |
| Local Obstacle Avoidance | [#16](https://github.com/oussema-fatnassi/WarOfTanks/issues/16) | ✅ Done |
| Control Zone - State Machine | [#17](https://github.com/oussema-fatnassi/WarOfTanks/issues/17) | ✅ Done |
| Gameplay & Win Conditions | [#18](https://github.com/oussema-fatnassi/WarOfTanks/issues/18) | ✅ Done |
| Detection System - Field of View | [#19](https://github.com/oussema-fatnassi/WarOfTanks/issues/19) | ✅ Done |
| Fog of War (WebGL-Compatible) | [#20](https://github.com/oussema-fatnassi/WarOfTanks/issues/20) | Not started |
| AI - Generic Behaviour Tree System | [#21](https://github.com/oussema-fatnassi/WarOfTanks/issues/21) | ✅ Done |
| AI - Tank Behaviour Trees (Specializations) | [#22](https://github.com/oussema-fatnassi/WarOfTanks/issues/22) | ✅ Done |
| Commander AI | [#23](https://github.com/oussema-fatnassi/WarOfTanks/issues/23) | ✅ Done |
| WebGL Build (GitHub Actions) | [#7](https://github.com/oussema-fatnassi/WarOfTanks/issues/7) | Not started |

## Architecture Notes

- UML class diagrams committed to `docs/uml/` — Unity, Backend, and AI layers ([#1](https://github.com/oussema-fatnassi/WarOfTanks/issues/1) ✅)
- ERD (MongoDB schema) committed to `docs/erd/` ([#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) ✅)
- State machine diagrams committed to `docs/state-machines/` — Zone Capture FSM and Game State FSM ([#3](https://github.com/oussema-fatnassi/WarOfTanks/issues/3) ✅)
- Technology justification committed to `docs/technical-note/` ([#35](https://github.com/oussema-fatnassi/WarOfTanks/issues/35) — Figma mockups remaining)
- Navigation uses a custom Physics2D LayerMask-based grid (not Unity NavMesh) — Grid, PathNode, and INavigable are modular and algorithm-agnostic
- BT framework (`Assets/Scripts/AI/BehaviourTree/`) is pure C# with no MonoBehaviour dependencies — `TankAI` is the only Unity integration point
- `TankAI` uses `partial class` split across 5 files — role-specific trees and action methods stay private without exposing TankAI internals
- `VisionSystem` implements 360° enemy detection via `Physics2D.Linecast` per-target line-of-sight; `IVisionSystem` interface decouples it from `TankBlackboard` and `PlayerAutoAim` consumers ([#19](https://github.com/oussema-fatnassi/WarOfTanks/issues/19) ✅)
- `CommanderAI` is a scene-level MonoBehaviour (not attached to a tank) that aggregates each tank's `EnemyResults` into a unified battlefield picture and dispatches `EStrategicOrder` directives every configurable interval (default 1s); `TankAI.ReceiveOrder()` + a root-level override `Selector` interleave commander orders on top of the role tree, with auto-clear on action success ([#23](https://github.com/oussema-fatnassi/WarOfTanks/issues/23) ✅)
- `TankAI` path recovery now uses position-based stall detection backed by `Tank.GetBlockedCells()` with a static-only fallback — same primitive as the player's `MoveCommand`, eliminates spawn-queue deadlocks
- The AI + tank system must remain a self-contained modular prefab (championship requirement)
- Naming conventions: see `docs/naming-conventions.md` in the root repo

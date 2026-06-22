# War of Tanks

[![Backend CI](https://github.com/oussema-fatnassi/WarOfTanks/actions/workflows/backend-ci.yml/badge.svg)](https://github.com/oussema-fatnassi/WarOfTanks/actions/workflows/backend-ci.yml)

> 2D top-down RTS game — La Plateforme school project

## Project Description

War of Tanks is a 2D top-down real-time strategy game built in Unity where a player commands a team of 3 tanks against an AI-controlled enemy team of 3 tanks. Both teams compete to capture and hold a central control zone, with the first team to reach the score threshold winning the match. Match history, player stats, and a leaderboard are tracked via a Go backend and displayed in a React frontend.

## Tech Stack

| Layer      | Technology                               |
| ---------- | ---------------------------------------- |
| Game       | Unity 2020.3.49f1 — 2D Built-in Pipeline |
| Backend    | Go + Gin                                 |
| Frontend   | React + TypeScript (Vite)                |
| Database   | MongoDB                                  |
| Deployment | Docker + Docker Compose + Render         |

## Team

- **Oussema** — Unity, Frontend
- **Oroitz** — Unity, C# Scripting
- **Kamelia** — Backend, DevOps

## Repository Structure

```
WarOfTanks/
├── UNITY/        # Unity 2020.3.49f1 project — see UNITY/README.md
├── BACKEND/      # Go + Gin REST API — see BACKEND/README.md
└── FRONTEND/     # React + TypeScript — see FRONTEND/README.md
```

## Quick Start

### Game (Unity)

See [UNITY/README.md](UNITY/README.md)

### Full Stack (Docker)

```bash
cp BACKEND/.env.example BACKEND/.env   # then edit secrets
docker-compose up --build
```

- Frontend: http://localhost:3000
- Backend: http://localhost:8080

### Frontend (dev mode)

```bash
cd FRONTEND
npm install
npm run dev
```

## Features Completed

### Sprint 1 — Setup & Architecture

- ✅ [#1](https://github.com/oussema-fatnassi/WarOfTanks/issues/1) UML Class Diagrams — all layers documented (Unity, Backend, AI)
- ✅ [#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) ERD - MongoDB Schema Design — committed to `docs/erd/`
- ✅ [#3](https://github.com/oussema-fatnassi/WarOfTanks/issues/3) State Machine Diagrams — Zone Capture FSM + Game State FSM committed to `docs/state-machines/`
- ✅ [#4](https://github.com/oussema-fatnassi/WarOfTanks/issues/4) Unity Project Base Setup — scenes, input system, packages configured
- ✅ [#5](https://github.com/oussema-fatnassi/WarOfTanks/issues/5) GitHub Actions - Backend CI — Go tests, race detector, coverage report, MongoDB service container
- ✅ [#6](https://github.com/oussema-fatnassi/WarOfTanks/issues/6) GitHub Actions - Code Quality — ESLint + Prettier for frontend, golangci-lint for backend
- ✅ [#7](https://github.com/oussema-fatnassi/WarOfTanks/issues/7) GitHub Actions - WebGL Build — `game-ci/unity-builder` workflow (`projectPath: UNITY`, Unity 2020.3.49f1, WebGL target), Library caching, artifact-shape verification, uploads `waroftanks-webgl-build` artifact; triggers on push to `main`, PRs touching `UNITY/**`, and manual dispatch; WebGL compression set to Disabled to match the working embed (merged to dev via PR #79)
- ✅ [#24](https://github.com/oussema-fatnassi/WarOfTanks/issues/24) Backend Project Setup — Go + Gin + MongoDB + Docker running
- ✅ [#54](https://github.com/oussema-fatnassi/WarOfTanks/issues/54) MongoDB Database Initialization — collections created, indexes applied, AI configs seeded (idempotent)

### Sprint 2 — Core Gameplay

- ✅ [#8](https://github.com/oussema-fatnassi/WarOfTanks/issues/8) Tank Prefab - Movement, Cannon, HP, Respawn — physics movement, independent turret, damage falloff, health bar, death/respawn cycle
- ✅ [#9](https://github.com/oussema-fatnassi/WarOfTanks/issues/9) Environment - Tilemaps Setup — 6 layers, collision matrix, symmetric map painted
- ✅ [#10](https://github.com/oussema-fatnassi/WarOfTanks/issues/10) Player Controls - Selection & Commands — single-click, shift-click, box drag-select; Move/Attack/AttackZone/Stop commands; Command Pattern + A\* navigation integration, blocked-cell avoidance, `IDamageable` interface
- ✅ [#11](https://github.com/oussema-fatnassi/WarOfTanks/issues/11) RTS Camera
- ✅ [#12](https://github.com/oussema-fatnassi/WarOfTanks/issues/12) Generic State Machine System
- ✅ [#29](https://github.com/oussema-fatnassi/WarOfTanks/issues/29) Frontend Project Setup — Vite + React + TypeScript scaffolded
- ✅ [#35](https://github.com/oussema-fatnassi/WarOfTanks/issues/35) Technical Note — UML, ERD, tech justification, diagrams (Figma mockups delivered in [#47](https://github.com/oussema-fatnassi/WarOfTanks/issues/47))

### Sprint 3 — Navigation & Backend Foundation

- ✅ [#17](https://github.com/oussema-fatnassi/WarOfTanks/issues/17) Control Zone - State Machine — 4-state FSM (Neutral/Capturing/Captured/Contested), circular gauge UI, team color feedback
- ✅ [#13](https://github.com/oussema-fatnassi/WarOfTanks/issues/13) Navigation - A\* Pathfinding — custom Physics2D grid, PathNode system, hazard cost, diagonal movement, INavigable interface
- ✅ [#16](https://github.com/oussema-fatnassi/WarOfTanks/issues/16) Local Obstacle Avoidance — CircleCast block detection, dynamic HashSet-based path recalculation, anti-oscillation rolling window
- ✅ [#25](https://github.com/oussema-fatnassi/WarOfTanks/issues/25) Player Model & Auth Routes (Register + Login)
- ✅ [#26](https://github.com/oussema-fatnassi/WarOfTanks/issues/26) JWT Auth Middleware & Refresh Token Route — access token (1h) + refresh token (7d), middleware protecting all authenticated routes
- ✅ [#51](https://github.com/oussema-fatnassi/WarOfTanks/issues/51) Frontend App Structure — React Router v6, Axios client with JWT interceptors, TypeScript types, protected routes
- ✅ [#54](https://github.com/oussema-fatnassi/WarOfTanks/issues/54) MongoDB Database Initialization — Collections, Indexes & Seed Data

### Sprint 4 — AI Systems & Backend Routes

- ✅ [#18](https://github.com/oussema-fatnassi/WarOfTanks/issues/18) Gameplay & Win Conditions — GameStateMachine (Playing/Paused/GameOver), configurable timer, zone-based scoring, GameHUD, GameOverScreen, MainMenuController
- ✅ [#21](https://github.com/oussema-fatnassi/WarOfTanks/issues/21) AI - Generic Behaviour Tree System — pure C# BT framework (IBehaviourNode, Selector, Sequence, Inverter, Repeater, ActionNode, ConditionNode, BehaviourTree root), dedicated asmdef, 14 unit tests, UML diagram committed
- ✅ [#15](https://github.com/oussema-fatnassi/WarOfTanks/issues/15) Navigation - Flow Field — BFS integration field + direction vector grid, `GetDirectionAtWorldPos` for multi-tank query, `FlowFieldStrategy` with world↔grid translation, cyan arrow debug gizmo, swappable via `EPathfinderType.FLOWFIELD`
- ✅ [#27](https://github.com/oussema-fatnassi/WarOfTanks/issues/27) Player & Match Routes — `GET /players` leaderboard (sorted by score, paginated), `GET /players/me`, `POST /matches` (MongoDB transaction: saves match + updates player stats atomically), `GET /matches` (own history, paginated), all routes protected by JWT middleware
- ✅ [#22](https://github.com/oussema-fatnassi/WarOfTanks/issues/22) AI - Tank Behaviour Trees (Specializations) — 3 AI roles (Captor, Attacker, Defender) each with a priority-ordered BT; TankBlackboard data snapshot; VisionSystem stub; GameManager tank registry; partial class TankAI split across 5 files; Enemy.prefab updated
- ✅ [#30](https://github.com/oussema-fatnassi/WarOfTanks/issues/30) Auth Pages (Register + Login) — two-column auth layout, login/register pages with form validation, AuthContext + AuthProvider + useAuth hook, Axios refresh interceptor guard, CORS middleware, email required at registration
- ✅ [#28](https://github.com/oussema-fatnassi/WarOfTanks/issues/28) Backend Unit Tests & CI Integration — comprehensive handler tests (auth, player, match) against real MongoDB; CI updated to MongoDB replica set for transaction support; `go test -race -coverprofile`; `TestSaveMatch` covers win/loss stats update atomically
- ✅ [#19](https://github.com/oussema-fatnassi/WarOfTanks/issues/19) Detection System - Field of View — VisionSystem with configurable range + angle, target layer filtering, line-of-sight raycasts, Gizmo debug arcs
- ✅ [#23](https://github.com/oussema-fatnassi/WarOfTanks/issues/23) Commander AI — strategic AI commander that coordinates AI tanks, assigns roles, and adapts tactics based on game state
- ✅ [#20](https://github.com/oussema-fatnassi/WarOfTanks/issues/20) Fog of War (WebGL-Compatible) — `WarOfTanks.Fog` namespace; `FogOfWarManager` polls friendly vision every ~0.1s and reveals enemies only on `isInLineOfSight` with a hysteresis grace period; `FogVisibility` fades sprite + UI (CanvasGroup) so health bars hide too; `FogOfWarOverlay` runtime `Texture2D` darkens out-of-vision terrain; fog gates targeting via `CanTarget`. No post-processing, no compute shaders (merged to dev via PR #77 — WebGL build verification pending)
- ✅ [#31](https://github.com/oussema-fatnassi/WarOfTanks/issues/31) Leaderboard, Stats & Match History Pages — Leaderboard table (ranked by totalScore, current player highlighted), personal stats cards with win/loss bar, match history with pagination, responsive mobile layout with hamburger menu, loading skeletons and error handling
- ✅ [#19](https://github.com/oussema-fatnassi/WarOfTanks/issues/19) Detection System - Field of View — `VisionSystem` MonoBehaviour with `Scan()` (360° radius, `Physics2D.Linecast` per-target line-of-sight, Cover + Obstacle layer blocking) and `GetClosestTarget()`; `DetectionResult` data class; `TankBlackboard` enemy filtering and health ratio; `IVisionSystem` interface; `PlayerAutoAim` component reusing vision for player turret; `TurretController` `RotateTo`/`IsAimedAt`/`Fire` finalised

### Sprint 5 — Integration & Delivery

- ✅ [#33](https://github.com/oussema-fatnassi/WarOfTanks/issues/33) Docker Compose - Full Stack — `docker-compose.yml` with 3 services (MongoDB with healthcheck, Go backend with healthcheck, React frontend via nginx), multi-stage Dockerfiles, persistent volume, SPA routing via nginx
- ✅ [#23](https://github.com/oussema-fatnassi/WarOfTanks/issues/23) Commander AI — scene-level `CommanderAI` MonoBehaviour aggregating per-tank vision into a unified battlefield picture, evaluation tick every 1s, 7-scenario decision cascade (outnumbered fallback, contested zone, neutral, winning lead, losing-urgent, losing-general, default) dispatching `EStrategicOrder` (NONE / CAPTUREZONE / DEFENDZONE / FULLAGGRESSION / FALLBACK); `TankAI.ReceiveOrder()` + root-level override `Selector` so role trees are temporarily overridden with auto-clear on success; bundled AI-pathfinding fix replacing the coroutine block handler with position-based stall detection + `Tank.GetBlockedCells()` + static-only fallback (same primitive as `MoveCommand`), fixes deadlocks at spawn
- ✅ [#80](https://github.com/oussema-fatnassi/WarOfTanks/issues/80) Final UI Overhaul, Configurable Match Settings & Explosion Damage — cohesive MainMenu → Gameplay → Pause → GameOver flow; shared `MenuUIFactory` + editor generators producing all UI prefabs (MainMenu, GameHUD, PauseMenu, GameOver, ZoneCaptureBar); `MatchSettings` (`PlayerPrefs`-backed) exposing tank health, fire rate, respawn delay, bullet damage, explosion radius, match duration, score limit, and master volume, wired into the matching gameplay components with prefab-matching defaults; bullet damage switched from distance-travelled falloff to impact-centred area-of-effect explosions (`Physics2D.OverlapCircleAll`, friendly-fire ignored, per-tank dedupe); `FogOfWarManager` re-bootstraps on scene reload (merged to dev via PR #82)
- ✅ [#83](https://github.com/oussema-fatnassi/WarOfTanks/issues/83) Final Visuals — Sprites, Procedural Background & Tank Art — replaces the flat colored prototype tilemaps with a sci-fi visual pass while keeping the gameplay data layer (tags/layers/colliders) intact; `BlueprintBackground` + `BlueprintGrid` shader draw the blueprint-grid background procedurally (no image); `TankAppearance` generates the tank body/cannon/glow in code with team tint and a WebGL-safe additive glow; `RandomFloorTile` picks a varied tile per cell via a stable spatial hash; `SciFiFloorTool` editor pipeline (slice → tile → apply) retiles all 5 terrain layers and `TransparencyBaker` bakes checkerboard backgrounds to alpha (merged to dev via PR #86)

- ✅ [#73](https://github.com/oussema-fatnassi/WarOfTanks/issues/73) Unity — Post Match Result to Backend on Game Over — `MatchResultPayload` data class, `MatchResultSender` MonoBehaviour (fire-and-forget coroutine via `UnityWebRequest`), `AuthToken` static token store, `GameManager.SendMatchResult()` wired into `ShowGameOver()`; Bearer token header, 201/401/error handling; Docker Compose updated to MongoDB replica set (`--replSet rs0` + auto-init healthcheck) for transaction support
- ✅ [#34](https://github.com/oussema-fatnassi/WarOfTanks/issues/34) Deploy Backend to Render — `render.yaml` Blueprint (Docker web service, `healthCheckPath /health`, auto-deploy from `main` on `checksPass`, secrets via `sync: false` / `generateValue: true`); root `GET /health` endpoint; CORS generalized to a comma-separated `ALLOWED_ORIGINS` allow-list with `FRONTEND_ORIGIN`/local fallbacks; `JWT_REFRESH_SECRET` now required and `PORT` defaulted; frontend `vercel.json` (SPA rewrites) + `DEPLOYMENT.md` guide (merged to dev via PR #88 — Render/Atlas dashboard setup pending)
- ✅ [#47](https://github.com/oussema-fatnassi/WarOfTanks/issues/47) UI Mockups - Figma (Zoning, Wireframe, Hi-fi, Prototype) — Figma design deliverables completed
- ✅ [#32](https://github.com/oussema-fatnassi/WarOfTanks/issues/32) WebGL Game Embed — Unity build embedded on the `/play` route (auth-protected); Vercel `prebuild` pulls the latest `waroftanks-webgl-build` release artifact into `public/UnityBuild/` (build stays git-ignored); React passes the production API URL + JWT into the iframe via a same-origin `postMessage` bridge so the game authenticates and posts match results against the deployed backend

## In Progress / Remaining

### Sprint 3 — Navigation & Backend Foundation

| #                                                               | Title                 | Owner  | Priority | Status      |
| --------------------------------------------------------------- | --------------------- | ------ | -------- | ----------- |
| [#14](https://github.com/oussema-fatnassi/WarOfTanks/issues/14) | Navigation - Dijkstra | Oroitz | Medium   | Not started |

### Sprint 5 — Integration & Delivery

| #                                                               | Title                                    | Owner   | Priority | Status      |
| --------------------------------------------------------------- | ---------------------------------------- | ------- | -------- | ----------- |
| [#36](https://github.com/oussema-fatnassi/WarOfTanks/issues/36) | Presentation Slides                      | All     | High     | Not started |
| [#66](https://github.com/oussema-fatnassi/WarOfTanks/issues/66) | Refactor: GameManager SOLID improvements | Oussema | Low      | Not started |

## Known Bugs & Issues

- [#76](https://github.com/oussema-fatnassi/WarOfTanks/issues/76) Add password validation conditions at registration (open).

## Architecture

The project is split into three independently deployable layers:

- **Game (Unity)** — 2D top-down gameplay with RTS controls, A\* pathfinding, behaviour tree AI, control zone state machine, and WebGL build target. The AI + tank system is designed as a self-contained modular prefab.
- **Backend (Go + Gin)** — REST API handling player registration/login (JWT auth, access token 1h / refresh token 7d), match result recording, and leaderboard queries. Data persisted in MongoDB.
- **Frontend (React + TypeScript)** — Web interface with auth pages, leaderboard, match history, and an embedded WebGL build of the game.

Architecture diagrams are live in `docs/`:

- UML Class Diagrams — `docs/uml/` ([#1](https://github.com/oussema-fatnassi/WarOfTanks/issues/1) ✅)
- ERD (MongoDB Schema) — `docs/erd/` ([#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) ✅)
- State Machine Diagrams — `docs/state-machines/` ([#3](https://github.com/oussema-fatnassi/WarOfTanks/issues/3) ✅)
- Technology Justification — `docs/technical-note/` ([#35](https://github.com/oussema-fatnassi/WarOfTanks/issues/35) ✅)
- UI Mockups (Figma) — Zoning, wireframe, hi-fi, prototype ([#47](https://github.com/oussema-fatnassi/WarOfTanks/issues/47) ✅)

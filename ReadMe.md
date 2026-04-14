# War of Tanks

> 2D top-down RTS game — La Plateforme school project

## Project Description

War of Tanks is a 2D top-down real-time strategy game built in Unity where a player commands a team of 3 tanks against an AI-controlled enemy team of 3 tanks. Both teams compete to capture and hold a central control zone, with the first team to reach the score threshold winning the match. Match history, player stats, and a leaderboard are tracked via a Go backend and displayed in a React frontend.

## Tech Stack

| Layer | Technology |
|---|---|
| Game | Unity 2020.3.49f1 — 2D Built-in Pipeline |
| Backend | Go + Gin |
| Frontend | React + TypeScript (Vite) |
| Database | MongoDB |
| Deployment | Docker + Docker Compose + Render |

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

### Backend
```bash
cd BACKEND
docker-compose up --build
```

### Frontend
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
- ✅ [#6](https://github.com/oussema-fatnassi/WarOfTanks/issues/6) GitHub Actions - Code Quality — ESLint + Prettier for frontend, golangci-lint for backend
- ✅ [#24](https://github.com/oussema-fatnassi/WarOfTanks/issues/24) Backend Project Setup — Go + Gin + MongoDB + Docker running

### Sprint 2 — Core Gameplay
- ✅ [#9](https://github.com/oussema-fatnassi/WarOfTanks/issues/9) Environment - Tilemaps Setup — 6 layers, collision matrix, symmetric map painted
- ✅ [#35](https://github.com/oussema-fatnassi/WarOfTanks/issues/35) Technical Note — Figma mockups remaining (see #47)
- ✅ [#11](https://github.com/oussema-fatnassi/WarOfTanks/issues/11) RTS Camera
- ✅ [#12](https://github.com/oussema-fatnassi/WarOfTanks/issues/12) Generic State Machine System

## In Progress / Remaining

### Sprint 1 — Setup & Architecture (due March 24)

| # | Title | Owner | Priority | Status |
|---|---|---|---|---|
| [#5](https://github.com/oussema-fatnassi/WarOfTanks/issues/5) | GitHub Actions - Backend CI | Kamelia | Critical | Not started |

### Sprint 2 — Core Gameplay (Apr 13 – Apr 19)

| # | Title | Owner | Priority | Status |
|---|---|---|---|---|
| [#8](https://github.com/oussema-fatnassi/WarOfTanks/issues/8) | Tank Prefab - Movement, Cannon, HP, Respawn | Oroitz | Critical | In progress |
| [#10](https://github.com/oussema-fatnassi/WarOfTanks/issues/10) | Player Controls - Selection & Commands | Oroitz | Critical | Not started |
| [#29](https://github.com/oussema-fatnassi/WarOfTanks/issues/29) | Frontend Project Setup (Vite + React + TypeScript) | Oussema | Critical | In progress |

### Sprint 3 — Navigation & Backend Foundation (Apr 20 – Apr 26)

| # | Title | Owner | Priority | Status |
|---|---|---|---|---|
| [#13](https://github.com/oussema-fatnassi/WarOfTanks/issues/13) | Navigation - A* Pathfinding | Oussema | Critical | Not started |
| [#14](https://github.com/oussema-fatnassi/WarOfTanks/issues/14) | Navigation - Dijkstra | Oroitz | Medium | Not started |
| [#16](https://github.com/oussema-fatnassi/WarOfTanks/issues/16) | Local Obstacle Avoidance | Oroitz | High | Not started |
| [#17](https://github.com/oussema-fatnassi/WarOfTanks/issues/17) | Control Zone - State Machine | Oussema | Critical | Not started |
| [#18](https://github.com/oussema-fatnassi/WarOfTanks/issues/18) | Gameplay & Win Conditions - Teams, Score, Timer, Menu | Oroitz | High | Not started |
| [#25](https://github.com/oussema-fatnassi/WarOfTanks/issues/25) | Player Model & Auth Routes (Register + Login) | Kamelia | Critical | Not started |
| [#26](https://github.com/oussema-fatnassi/WarOfTanks/issues/26) | JWT Auth Middleware & Refresh Token Route | Kamelia | Critical | Not started |

### Sprint 4 — AI Systems & Backend Routes (Apr 27 – May 3)

| # | Title | Owner | Priority | Status |
|---|---|---|---|---|
| [#15](https://github.com/oussema-fatnassi/WarOfTanks/issues/15) | Navigation - Flow Field | Kamelia | Medium | Not started |
| [#19](https://github.com/oussema-fatnassi/WarOfTanks/issues/19) | Detection System - Field of View | Oroitz | High | Not started |
| [#20](https://github.com/oussema-fatnassi/WarOfTanks/issues/20) | Fog of War (WebGL-Compatible) | Oroitz | High | Not started |
| [#21](https://github.com/oussema-fatnassi/WarOfTanks/issues/21) | AI - Generic Behaviour Tree System | Oussema | Critical | Not started |
| [#22](https://github.com/oussema-fatnassi/WarOfTanks/issues/22) | AI - Tank Behaviour Trees (Specializations) | Oroitz | Critical | Not started |
| [#27](https://github.com/oussema-fatnassi/WarOfTanks/issues/27) | Player & Match Routes | Kamelia | High | Not started |
| [#28](https://github.com/oussema-fatnassi/WarOfTanks/issues/28) | Backend Unit Tests & CI Integration | Kamelia | High | Not started |
| [#30](https://github.com/oussema-fatnassi/WarOfTanks/issues/30) | Auth Pages (Register + Login) | Oussema | High | Not started |

### Sprint 5 — Integration & Delivery (May 4 – May 10)

| # | Title | Owner | Priority | Status |
|---|---|---|---|---|
| [#7](https://github.com/oussema-fatnassi/WarOfTanks/issues/7) | GitHub Actions - WebGL Build | Oussema | Medium | Not started |
| [#23](https://github.com/oussema-fatnassi/WarOfTanks/issues/23) | Commander AI | Oussema | High | Not started |
| [#31](https://github.com/oussema-fatnassi/WarOfTanks/issues/31) | Leaderboard, Stats & Match History Pages | Oussema | High | Not started |
| [#32](https://github.com/oussema-fatnassi/WarOfTanks/issues/32) | WebGL Game Embed | Oussema | High | Not started |
| [#33](https://github.com/oussema-fatnassi/WarOfTanks/issues/33) | Docker Compose - Full Stack | Kamelia | High | Not started |
| [#34](https://github.com/oussema-fatnassi/WarOfTanks/issues/34) | Deploy Backend to Render | Kamelia | Medium | Not started |
| [#36](https://github.com/oussema-fatnassi/WarOfTanks/issues/36) | Presentation Slides | All | High | Not started |
| [#47](https://github.com/oussema-fatnassi/WarOfTanks/issues/47) | UI Mockups - Figma (Zoning, Wireframe, Hi-fi, Prototype) | Oussema | High | Not started |

## Known Bugs & Issues

No bugs reported yet.

## Architecture

The project is split into three independently deployable layers:

- **Game (Unity)** — 2D top-down gameplay with RTS controls, A* pathfinding, behaviour tree AI, control zone state machine, and WebGL build target. The AI + tank system is designed as a self-contained modular prefab.
- **Backend (Go + Gin)** — REST API handling player registration/login (JWT auth, access token 1h / refresh token 7d), match result recording, and leaderboard queries. Data persisted in MongoDB.
- **Frontend (React + TypeScript)** — Web interface with auth pages, leaderboard, match history, and an embedded WebGL build of the game.

Architecture diagrams are live in `docs/`:
- UML Class Diagrams — `docs/uml/` ([#1](https://github.com/oussema-fatnassi/WarOfTanks/issues/1) ✅)
- ERD (MongoDB Schema) — `docs/erd/` ([#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) ✅)
- State Machine Diagrams — `docs/state-machines/` ([#3](https://github.com/oussema-fatnassi/WarOfTanks/issues/3) ✅)
- Technology Justification — `docs/technical-note/` ([#35](https://github.com/oussema-fatnassi/WarOfTanks/issues/35) — Figma mockups remaining in [#47](https://github.com/oussema-fatnassi/WarOfTanks/issues/47))

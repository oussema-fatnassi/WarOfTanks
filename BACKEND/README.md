# War of Tanks — Backend

REST API built with Go + Gin, backed by MongoDB, containerized with Docker.

## Requirements

- Docker & Docker Compose

## How to Run

```bash
cd BACKEND
docker-compose up --build
```

The API will be available at `http://localhost:8080`.

## Environment Variables

| Variable | Description |
|---|---|
| `MONGO_URI` | MongoDB connection string |
| `JWT_SECRET` | Secret key for signing JWT tokens |
| `PORT` | Server port (default: 8080) |

Create a `.env` file in `BACKEND/` with these values before running.

## API Endpoints

> Routes marked ✅ are live. Others are in progress — see issues below.

| Method | Route | Description | Status |
|---|---|---|---|
| POST | `/api/v1/auth/register` | Register a new player | ✅ Live |
| POST | `/api/v1/auth/login` | Login and receive JWT tokens | ✅ Live |
| POST | `/api/v1/auth/refresh` | Refresh access token | 🔲 #26 |
| GET | `/api/v1/players/:id` | Get player profile | 🔲 #27 |
| GET | `/api/v1/matches` | Get match history | 🔲 #27 |
| POST | `/api/v1/matches` | Record a match result | 🔲 #27 |
| GET | `/api/v1/leaderboard` | Get leaderboard | 🔲 #27 |

## JWT Strategy

- Access token: **1 hour**
- Refresh token: **7 days**
- See `docs/jwt-guide.md` for full implementation details.

## Key Issues

| # | Title | Status |
|---|---|---|
| [#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) | ERD - MongoDB Schema Design | ✅ Done |
| [#24](https://github.com/oussema-fatnassi/WarOfTanks/issues/24) | Backend Project Setup (Go + Gin + MongoDB + Docker) | ✅ Done |
| [#54](https://github.com/oussama-fatnassi/WarOfTanks/issues/54) | MongoDB Database Initialization — Collections, Indexes & Seed Data | ✅ Done |
| [#25](https://github.com/oussema-fatnassi/WarOfTanks/issues/25) | Player Model & Auth Routes (Register + Login) | ✅ Done |
| [#26](https://github.com/oussema-fatnassi/WarOfTanks/issues/26) | JWT Auth Middleware & Refresh Token Route | Not started |
| [#27](https://github.com/oussema-fatnassi/WarOfTanks/issues/27) | Player & Match Routes | Not started |
| [#28](https://github.com/oussema-fatnassi/WarOfTanks/issues/28) | Backend Unit Tests & CI Integration | Not started |
| [#5](https://github.com/oussema-fatnassi/WarOfTanks/issues/5) | GitHub Actions - Backend CI | ✅ Done |

## Architecture Notes

- MongoDB schema (ERD) committed to `docs/erd/` ([#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) ✅)
- Naming conventions: see `docs/naming-conventions.md` in the root repo.

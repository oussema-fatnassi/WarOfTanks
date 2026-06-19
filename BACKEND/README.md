# War of Tanks — Backend

REST API built with Go + Gin, backed by MongoDB, containerized with Docker.

## Requirements

- Docker & Docker Compose

## How to Run

### With Docker Compose (recommended)

```bash
# From the project root
cp BACKEND/.env.example BACKEND/.env   # then edit secrets
docker-compose up --build
```

### Standalone

```bash
cd BACKEND
go run ./cmd/main.go
```

The API will be available at `http://localhost:8080`.

## Environment Variables

| Variable             | Description                                          |
| -------------------- | ---------------------------------------------------- |
| `MONGODB_URI`        | MongoDB connection string (`mongodb://mongo:27017` in Docker) |
| `MONGODB_DB_NAME`    | Database name (default: `waroftanks`)                |
| `JWT_SECRET`         | Secret key for signing access tokens                 |
| `JWT_REFRESH_SECRET` | Secret key for signing refresh tokens                |
| `PORT`               | Server port (default: `8080`)                        |
| `FRONTEND_ORIGIN`    | Frontend URL allowed by CORS (`http://localhost:3000` in Docker) |

Create a `.env` file in `BACKEND/` with these values before running.

## API Endpoints

> Routes marked ✅ are live. Others are in progress — see issues below.

| Method | Route                   | Description                                          | Status  |
| ------ | ----------------------- | ---------------------------------------------------- | ------- |
| POST   | `/api/v1/auth/register` | Register a new player                                | ✅ Live |
| POST   | `/api/v1/auth/login`    | Login and receive JWT tokens                         | ✅ Live |
| POST   | `/api/v1/auth/refresh`  | Refresh access token via HttpOnly cookie             | ✅ Live |
| POST   | `/api/v1/auth/logout`   | Clear refresh token cookie                           | ✅ Live |
| GET    | `/api/v1/players`       | Leaderboard — all players sorted by score, paginated | ✅ Live |
| GET    | `/api/v1/players/me`    | Authenticated player's own profile                   | ✅ Live |
| POST   | `/api/v1/matches`       | Record a match result (atomic transaction)           | ✅ Live |
| GET    | `/api/v1/matches`       | Authenticated player's match history, paginated      | ✅ Live |

## Running Tests

Tests require a real MongoDB instance (no mocking). For local development:

```bash
# With a local MongoDB running on localhost:27017
go test ./... -v

# With a replica set (needed for SaveMatch transaction tests)
MONGODB_TEST_URI="mongodb://localhost:27017/?replicaSet=rs0" go test ./... -v -race
```

Tests that require a replica set skip automatically when one is not available.

The CI pipeline starts MongoDB 7 with a single-node replica set before running `go test ./... -v -race -coverprofile=coverage.out`.

## JWT Strategy

- Access token: **1 hour**
- Refresh token: **7 days**
- See `docs/jwt-guide.md` for full implementation details.

## Key Issues

| #                                                               | Title                                                              | Status      |
| --------------------------------------------------------------- | ------------------------------------------------------------------ | ----------- |
| [#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2)   | ERD - MongoDB Schema Design                                        | ✅ Done     |
| [#24](https://github.com/oussema-fatnassi/WarOfTanks/issues/24) | Backend Project Setup (Go + Gin + MongoDB + Docker)                | ✅ Done     |
| [#54](https://github.com/oussama-fatnassi/WarOfTanks/issues/54) | MongoDB Database Initialization — Collections, Indexes & Seed Data | ✅ Done     |
| [#25](https://github.com/oussema-fatnassi/WarOfTanks/issues/25) | Player Model & Auth Routes (Register + Login)                      | ✅ Done     |
| [#26](https://github.com/oussema-fatnassi/WarOfTanks/issues/26) | JWT Auth Middleware & Refresh Token Route                          | ✅ Done     |
| [#27](https://github.com/oussema-fatnassi/WarOfTanks/issues/27) | Player & Match Routes                                              | ✅ Done     |
| [#28](https://github.com/oussema-fatnassi/WarOfTanks/issues/28) | Backend Unit Tests & CI Integration                                | ✅ Done     |
| [#5](https://github.com/oussema-fatnassi/WarOfTanks/issues/5)   | GitHub Actions - Backend CI                                        | ✅ Done     |
| [#33](https://github.com/oussema-fatnassi/WarOfTanks/issues/33) | Docker Compose - Full Stack                                        | ✅ Done     |

## Architecture Notes

- MongoDB schema (ERD) committed to `docs/erd/` ([#2](https://github.com/oussema-fatnassi/WarOfTanks/issues/2) ✅)
- Naming conventions: see `docs/naming-conventions.md` in the root repo.

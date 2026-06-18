# War of Tanks — Frontend

Web interface built with React + TypeScript (Vite). Handles player auth, leaderboard, match history, and embeds the Unity WebGL build.

## Requirements

- Node.js **22+**
- npm

## How to Run

```bash
cd FRONTEND
npm install
npm run dev
```

The app will be available at `http://localhost:5173`.

## Build for Production

```bash
npm run build
```

## Code Quality

```bash
npm run lint          # ESLint — zero warnings enforced
npm run format:check  # Prettier — formatting check
npm run format        # Prettier — auto-format all files
```

Both lint and format checks run automatically on every PR to `dev` via GitHub Actions.

## Project Structure

```
FRONTEND/
├── public/           # Static assets (favicon, icons)
├── src/
│   ├── api/          # Axios client with JWT interceptors
│   ├── assets/       # Images and SVGs
│   ├── auth/         # tokenStore — module-level token ref for interceptor
│   ├── components/   # Reusable UI components (Navbar, ProtectedRoute, StatCard, WinLossBar, ErrorBanner, PageHeader, SkeletonRows)
│   ├── context/      # AuthContext + AuthProvider
│   ├── hooks/        # Custom React hooks (useAuth)
│   ├── pages/        # One file per route
│   ├── types/        # Shared TypeScript interfaces (Player, Match, AuthTokens)
│   ├── App.tsx       # Root component and routing
│   └── main.tsx      # Entry point
├── .env.example      # Environment variable template
├── eslint.config.js  # ESLint config (TypeScript + React)
├── vite.config.ts    # Vite config
└── tsconfig.app.json # TypeScript config (strict mode enabled)
```

## Pages

| Page          | Route          | Description                                 |
| ------------- | -------------- | ------------------------------------------- |
| Login         | `/login`       | Authenticate and receive JWT tokens         |
| Register      | `/register`    | Create a new account                        |
| Game          | `/play`        | Embedded Unity WebGL build                  |
| Leaderboard   | `/leaderboard` | Top players by wins/score                   |
| Match History | `/matches`     | Past match results for the logged-in player |
| Stats         | `/stats`       | Player statistics                           |

## Key Issues

| #                                                               | Title                                                        | Status      |
| --------------------------------------------------------------- | ------------------------------------------------------------ | ----------- |
| [#29](https://github.com/oussema-fatnassi/WarOfTanks/issues/29) | Frontend Project Setup (Vite + React + TypeScript)           | ✅ Done     |
| [#51](https://github.com/oussema-fatnassi/WarOfTanks/issues/51) | Frontend App Structure — Routing, Axios Client, Types, Pages | ✅ Done     |
| [#30](https://github.com/oussema-fatnassi/WarOfTanks/issues/30) | Auth Pages (Register + Login)                                | ✅ Done     |
| [#31](https://github.com/oussema-fatnassi/WarOfTanks/issues/31) | Leaderboard, Stats & Match History Pages                     | ✅ Done     |
| [#32](https://github.com/oussema-fatnassi/WarOfTanks/issues/32) | WebGL Game Embed                                             | Not started |
| [#6](https://github.com/oussema-fatnassi/WarOfTanks/issues/6)   | GitHub Actions - Code Quality (ESLint + Prettier)            | ✅ Done     |

## Architecture Notes

- Auth uses JWT — access token stored in memory, refresh token in an httpOnly cookie.
- The Unity WebGL build is embedded on the `/play` route via the Unity loader. Build assets go in `public/unity-build/`.
- JWT is passed from React into the Unity runtime via the Unity JSLib bridge for authenticated API calls.
- Naming conventions: see `docs/naming-conventions.md` in the root repo.

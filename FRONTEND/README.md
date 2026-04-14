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
│   ├── assets/       # Images and SVGs
│   ├── pages/        # One file per route
│   ├── components/   # Reusable UI components
│   ├── services/     # API calls to Go backend
│   ├── hooks/        # Custom React hooks (e.g. useAuth)
│   ├── App.tsx       # Root component and routing
│   └── main.tsx      # Entry point
├── .prettierrc       # Prettier config (Tailwind plugin included)
├── eslint.config.js  # ESLint config (TypeScript + React)
├── vite.config.ts    # Vite config with Tailwind plugin
└── tsconfig.app.json # TypeScript config
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

| #                                                               | Title                                              | Status      |
| --------------------------------------------------------------- | -------------------------------------------------- | ----------- |
| [#29](https://github.com/oussema-fatnassi/WarOfTanks/issues/29) | Frontend Project Setup (Vite + React + TypeScript) | In progress |
| [#30](https://github.com/oussema-fatnassi/WarOfTanks/issues/30) | Auth Pages (Register + Login)                      | Not started |
| [#31](https://github.com/oussema-fatnassi/WarOfTanks/issues/31) | Leaderboard, Stats & Match History Pages           | Not started |
| [#32](https://github.com/oussema-fatnassi/WarOfTanks/issues/32) | WebGL Game Embed                                   | Not started |
| [#6](https://github.com/oussema-fatnassi/WarOfTanks/issues/6)   | GitHub Actions - Code Quality (ESLint + Prettier)  | ✅ Done     |

## Architecture Notes

- Auth uses JWT — access token stored in memory, refresh token in an httpOnly cookie.
- The Unity WebGL build is embedded on the `/play` route via the Unity loader. Build assets go in `public/unity-build/`.
- JWT is passed from React into the Unity runtime via the Unity JSLib bridge for authenticated API calls.
- Naming conventions: see `docs/naming-conventions.md` in the root repo.

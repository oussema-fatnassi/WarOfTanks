# War of Tanks — Frontend

Web interface built with React + TypeScript (Vite). Displays player auth, leaderboard, match history, and embeds the Unity WebGL build.

## Requirements

- Node.js 18+
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

## Pages

| Page | Route | Description |
|---|---|---|
| Register | `/register` | Create a new account |
| Login | `/login` | Authenticate and receive tokens |
| Game | `/play` | Embedded Unity WebGL build |
| Leaderboard | `/leaderboard` | Top players by wins/score |
| Match History | `/matches` | Past match results for the logged-in player |
| Stats | `/stats` | Player statistics |

## Key Issues

| # | Title | Status |
|---|---|---|
| [#29](https://github.com/oussema-fatnassi/WarOfTanks/issues/29) | Frontend Project Setup (Vite + React + TypeScript) | Not started |
| [#30](https://github.com/oussema-fatnassi/WarOfTanks/issues/30) | Auth Pages (Register + Login) | Not started |
| [#31](https://github.com/oussema-fatnassi/WarOfTanks/issues/31) | Leaderboard, Stats & Match History Pages | Not started |
| [#32](https://github.com/oussema-fatnassi/WarOfTanks/issues/32) | WebGL Game Embed | Not started |

## Architecture Notes

- Auth uses JWT — access token stored in memory, refresh token in an httpOnly cookie.
- The Unity WebGL build is embedded via an `<iframe>` or Unity loader on the `/play` page.
- Naming conventions: see `docs/naming-conventions.md` in the root repo.

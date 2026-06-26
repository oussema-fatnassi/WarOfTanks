# Frontend Unit Testing Strategy

## Objective

The frontend unit test suite gives fast feedback on the React application without
starting the full Docker stack or Unity build. It protects the behavior that can
regress inside the browser code itself: form validation, route protection,
rendered statistics, table content, formatting helpers, and JWT client logic.

The Playwright E2E suite remains necessary for full-stack confidence. Unit tests
do not replace E2E tests; they reduce the number of regressions that reach that
slower layer.

## Tooling

The project uses Vitest with React Testing Library and jsdom:

- `vitest` runs tests with Vite-compatible TypeScript and React transforms.
- `@testing-library/react` tests components through user-visible output instead
  of implementation details.
- `@testing-library/user-event` simulates realistic typing and clicking.
- `@testing-library/jest-dom` adds readable DOM assertions.
- `jsdom` provides the browser-like environment used by React component tests.
- `@vitest/coverage-v8` generates text, HTML, and LCOV coverage reports.

## What Is Covered

The first unit-test layer covers:

- Login form submit behavior and invalid-login messaging.
- Register form client-side validation, duplicate username handling, and payload
  normalization.
- Protected route redirect behavior.
- Match history score rendering, `mm:ss` duration formatting, result labels, and
  victory/defeat filters.
- Stats page totals, win rate, latest match score, result, and duration.
- Leaderboard row rendering, API order preservation, and current-player
  highlighting.
- Token storage helpers.
- Axios JWT behavior: bearer token attachment and single-flight refresh when
  concurrent protected requests receive `401`.
- Pure formatting helpers.

## Commands

Run from `FRONTEND/`:

```bash
npm run test:unit
npm run test:unit:watch
npm run test:unit:coverage
```

Coverage output is written to `FRONTEND/coverage/` and ignored by Git.

## CI Integration

The frontend code-quality workflow runs:

1. `npm run lint`
2. `npm run format:check`
3. `npm run test:unit:coverage`

The generated coverage folder is uploaded as the `frontend-unit-coverage`
artifact. This makes the PR evidence reusable for the dossier without committing
generated reports to the repository.

## Dossier Notes

For the dossier, this section can be summarized as follows:

> The React frontend is tested with Vitest and React Testing Library. This choice
> keeps the tests close to Vite, TypeScript, and the user-facing DOM while
> avoiding the cost of launching the complete application stack for every small
> UI rule. The unit suite validates form rules, protected navigation, rendering
> of player data, formatting helpers, and the JWT refresh interceptor. Playwright
> remains the E2E layer for validating the complete deployed behavior.

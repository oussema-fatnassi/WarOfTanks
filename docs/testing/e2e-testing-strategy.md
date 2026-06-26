# War of Tanks - End-to-End Testing Strategy

## 1. Purpose of This Document

This document explains why end-to-end tests should be added to **War of Tanks**, what tool should be used, and why **Playwright** is the preferred choice for this project.

The goal is not only to describe a technical setup. This document also provides a justification that can be reused later in the project dossier to explain the testing strategy, the limits of the current implementation, and the next quality improvements planned for the application.

## 2. Current Testing Situation

The project already contains backend tests for the Go/Gin API. These tests are useful because they validate isolated backend behavior such as authentication, player routes, match routes, JWT handling, and MongoDB persistence.

However, the project does not yet contain a complete end-to-end test suite. This means there is no automated test that verifies the full user journey across all layers of the application:

- the React frontend,
- the Go/Gin backend,
- the MongoDB database,
- the JWT access-token and refresh-token flow,
- the protected frontend routes,
- the Unity WebGL page embedded in the web application.

This is an important missing layer because War of Tanks is no longer only a Unity game. It is now a complete web application that combines authentication, player data, match history, leaderboard information, deployment, and a WebGL build hosted inside React.

## 3. Why End-to-End Tests Are Needed

End-to-end tests are necessary because they validate the project from the user's point of view. A unit test can confirm that a function works, and an API test can confirm that an endpoint returns the correct response, but neither of them proves that the complete application works when all parts are connected.

For War of Tanks, the most important risks are integration risks:

- a player can register successfully in the backend but the frontend form may fail to handle the response;
- a player can log in successfully but the access token may not be stored or sent correctly by the frontend;
- the refresh token cookie may work in the backend but fail after a browser reload because of CORS or cookie configuration;
- a protected page may render correctly in isolation but fail when the user session expires;
- the Unity WebGL iframe may load but not receive the API URL or access token from the React application;
- match data may be saved by the API but not appear correctly in the history, statistics, or leaderboard pages.

These problems are not purely frontend or backend problems. They appear between layers. This is why E2E tests are needed: they verify that the real workflow works as a complete system.

## 4. Project Areas That Should Be Covered

The E2E test suite should focus first on the web application and its integration with the backend. It should not try to automate all Unity gameplay immediately, because full gameplay automation inside WebGL can be fragile and expensive.

The first E2E scope should cover these flows:

1. Anonymous user redirection.
   - Visiting `/` without a session should redirect to `/login`.
   - Protected pages should not be accessible without authentication.

2. Registration.
   - A new player should be able to create an account from the React form.
   - The backend should create the player in MongoDB.
   - The user should become authenticated after registration or be able to log in directly afterward, depending on the final intended behavior.

3. Login.
   - A registered player should be able to log in.
   - The frontend should receive an access token.
   - Authenticated requests should include the bearer token.

4. Session restoration.
   - After login, refreshing the browser page should restore the session using the refresh-token cookie.
   - The frontend should call the refresh route and recover the player profile.

5. Protected application pages.
   - `/leaderboard` should display player ranking data.
   - `/stats` should display the authenticated player's statistics.
   - `/history` should display the authenticated player's match history.

6. Logout.
   - The logout action should clear the frontend session.
   - Protected pages should redirect back to `/login` after logout.

7. Unity WebGL integration boundary.
   - `/play` should load the Unity WebGL iframe.
   - The React page should send the expected web-client configuration message to the iframe.
   - The configuration should include the API base URL and the current access token.

8. Match result integration.
   - A test should verify that a match result can be posted to the backend.
   - After the result is saved, history, statistics, and leaderboard data should reflect the update.

## 5. Recommended Tool: Playwright

The recommended E2E tool for War of Tanks is **Playwright**.

Playwright is designed for browser automation and end-to-end testing. It allows tests to interact with the application like a real user: navigating pages, filling forms, clicking buttons, waiting for route changes, inspecting network requests, and checking visible UI state.

It is particularly adapted to this project because War of Tanks uses:

- React and Vite for the frontend,
- protected routes with React Router,
- Axios for authenticated HTTP requests,
- a Go/Gin API,
- MongoDB through Docker Compose,
- JWT access tokens and refresh-token cookies,
- a Unity WebGL iframe embedded in the React page.

Playwright can cover the browser side and still help prepare backend state through its API testing tools.

## 6. Why Playwright Fits This Project

### 6.1. Vite and Local Server Support

The frontend is a Vite application. Playwright supports starting a local web server before running tests through its `webServer` configuration. This is useful because the tests can automatically start the frontend and wait until it is ready before executing browser actions.

The official Playwright documentation also recommends using a `baseURL`, which fits the project because the tests can navigate with relative paths such as `/login`, `/leaderboard`, or `/play`.

Reference: [Playwright web server documentation](https://playwright.dev/docs/test-webserver).

### 6.2. Full Browser Testing

Playwright can run tests in real browser engines. This matters because the project is a web application and includes a Unity WebGL build. The behavior of cookies, iframes, fullscreen, local storage, and WebGL embedding must be validated in a real browser environment, not only in a simulated DOM.

This is important for routes like `/play`, where the React page hosts Unity inside an iframe and sends configuration through `postMessage`.

### 6.3. API Setup and Data Preparation

E2E tests often need controlled data. For example, a test may need a known player, a known match history, or a known leaderboard state.

Playwright includes API testing capabilities, which can be used to call backend endpoints directly during setup or assertions. For War of Tanks, this is useful because tests can prepare or verify data without clicking through unnecessary UI every time.

Reference: [Playwright API testing documentation](https://playwright.dev/docs/api-testing).

### 6.4. Debugging with Traces

Authentication and session bugs can be difficult to debug because they involve several layers: browser storage, cookies, frontend state, backend responses, CORS, and redirects.

Playwright provides a Trace Viewer that records useful debugging information for failed tests, including actions, screenshots, DOM snapshots, console logs, and network activity. This is valuable for CI because it allows a failed E2E test to be investigated after the run.

Reference: [Playwright Trace Viewer documentation](https://playwright.dev/docs/trace-viewer).

### 6.5. CI Compatibility

The project already uses automation through GitHub Actions and Docker-based services. Playwright fits this direction because it can run headless in CI and can generate reports or traces when tests fail.

For this project, the future CI setup should start the backend and MongoDB, start or serve the frontend, run Playwright, and upload the report or trace artifacts if a test fails.

## 7. Why Not Cypress

Cypress is a strong and popular E2E testing framework, and it would be a valid option for many frontend applications. It provides a good developer experience and is especially comfortable for testing standard browser interfaces.

However, Playwright is a better fit for War of Tanks for several reasons.

### 7.1. Better Fit for Multi-Layer Integration

War of Tanks is not only a frontend application. The E2E tests must validate integration between:

- React routes,
- JWT access tokens,
- refresh-token cookies,
- the Go/Gin API,
- MongoDB data,
- the Unity iframe integration.

Playwright's combination of browser tests and API request tools makes it easier to prepare backend state and validate API effects from the same test suite.

### 7.2. Stronger Match for Iframes and WebGL Boundaries

The `/play` page embeds Unity WebGL inside an iframe. The first E2E tests should not try to play a full match automatically, but they should validate that:

- the iframe is loaded,
- the page sends the expected configuration,
- the access token is available to the game bridge.

Playwright gives direct browser automation primitives that are well suited to this kind of integration boundary.

### 7.3. Browser Coverage

Cypress supports major browsers such as Chrome, Firefox, and Edge, and documents WebKit support separately. Playwright is built around cross-browser testing across Chromium, Firefox, and WebKit, which is useful for a web application that includes iframe and WebGL behavior.

Reference: [Cypress browser launching documentation](https://docs.cypress.io/app/references/launching-browsers).

### 7.4. Debugging Artifacts for CI

Cypress also has debugging tools, but Playwright's trace system is especially useful for this project because failures may involve frontend navigation, cookies, network requests, and iframe behavior at the same time.

For a school project and dossier, this also gives a clear quality argument: failed E2E tests can produce evidence that is understandable and reviewable.

## 8. What Should Not Be Tested First

The first E2E test suite should not try to automate the entire Unity gameplay loop. Full gameplay automation inside WebGL can be unstable because it depends on canvas rendering, timing, input simulation, scene loading, and game state.

Instead, the first version should test the boundary between the web app and the Unity build:

- the `/play` page is protected,
- the Unity iframe loads,
- the React app sends configuration to the iframe,
- the authenticated API token is available to the WebGL bridge,
- match result posting can be verified through the backend and web pages.

Gameplay logic itself should remain covered by Unity-side tests or targeted runtime tests where possible. E2E tests should validate the user journey and integration points, not replace all lower-level tests.

## 9. Proposed Test Architecture

The recommended structure is:

```text
FRONTEND/
  e2e/
    auth.spec.ts
    protected-routes.spec.ts
    leaderboard.spec.ts
    history.spec.ts
    play-page.spec.ts
  playwright.config.ts
```

The frontend `package.json` should eventually include scripts such as:

```json
{
  "test:e2e": "playwright test",
  "test:e2e:ui": "playwright test --ui",
  "test:e2e:report": "playwright show-report"
}
```

The test environment should run against a controlled backend and database. The preferred setup is:

1. Start MongoDB and the backend with Docker Compose or a test-specific Compose file.
2. Use a dedicated test database name to avoid modifying development data.
3. Start the Vite frontend with `VITE_API_URL` pointing to the test backend.
4. Run Playwright against the frontend.
5. Clean test data between tests or use unique test users per run.

## 10. Data Isolation Strategy

E2E tests must not depend on random existing data. They should be deterministic.

The recommended strategy is to use generated test users, for example:

```text
e2e_user_<timestamp>
```

For match history and leaderboard tests, the suite should either:

- create data through public API calls,
- use backend test helpers,
- or use a dedicated test-only seed/reset mechanism guarded by an environment flag.

The database used for E2E should be separated from the normal development database when possible.

## 11. First Test Cases to Implement

The first implementation should stay focused and prove the value of E2E tests without becoming too large.

Recommended first tests:

1. `auth.spec.ts`
   - register a new user,
   - log in,
   - verify redirect to a protected page,
   - log out,
   - verify protected route redirection.

2. `session-refresh.spec.ts`
   - log in,
   - reload the browser,
   - verify the user is still authenticated through the refresh-token cookie.

3. `protected-routes.spec.ts`
   - try to access `/leaderboard`, `/stats`, `/history`, and `/play` without a session,
   - verify redirection to `/login`.

4. `play-page.spec.ts`
   - log in,
   - open `/play`,
   - verify the Unity iframe exists,
   - verify that the page can send the expected web-client configuration.

5. `match-data.spec.ts`
   - create or post a match result,
   - verify that the result appears in history,
   - verify that stats and leaderboard values are updated.

## 12. Limits of the First Version

The first E2E suite should be considered an integration safety net, not a full gameplay validation system.

Its limits should be explicit:

- it will not fully simulate tactical gameplay inside Unity;
- it will not replace backend unit/integration tests;
- it will not replace manual validation of the Unity WebGL build;
- it will focus on web flows, authentication, persistence, and the Unity bridge.

This limitation is acceptable because the project risk is currently highest at the integration layer between React, the API, MongoDB, JWT, and the Unity WebGL embed.


## 13. Conclusion

Adding E2E tests is an important next step for War of Tanks because the project has evolved from a standalone Unity game into a full-stack web application.

Playwright is the most appropriate tool because it can test the real browser experience while still supporting API-level preparation and debugging artifacts. The first test suite should focus on critical user journeys and integration points rather than trying to automate every Unity gameplay interaction.

This approach gives the project a stronger quality baseline and provides concrete evidence for the dossier that the main application flows are verified across frontend, backend, database, authentication, and WebGL integration.


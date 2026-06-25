import { defineConfig, devices } from '@playwright/test'

const frontendPort = process.env.E2E_FRONTEND_PORT ?? '5173'
const baseURL = process.env.E2E_BASE_URL ?? `http://localhost:${frontendPort}`
const apiURL = process.env.E2E_API_URL ?? 'http://localhost:8080'

export default defineConfig({
  testDir: './e2e',
  fullyParallel: false,
  workers: 1,
  timeout: 30_000,
  expect: {
    timeout: 7_000,
  },
  reporter: process.env.CI ? [['html'], ['list']] : [['html'], ['line']],
  use: {
    baseURL,
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  webServer: {
    command: `npm run dev -- --host localhost --port ${frontendPort}`,
    url: baseURL,
    reuseExistingServer: !process.env.CI,
    env: {
      VITE_API_URL: apiURL,
    },
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
  ],
})

import { expect, type APIRequestContext, type Page } from '@playwright/test'

export interface E2EUser {
  username: string
  email: string
  password: string
}

export interface MatchResult {
  winnerTeam: 1 | 2
  playerScore: number
  aiScore: number
  duration: number
}

export const apiURL = process.env.E2E_API_URL ?? 'http://localhost:8080'

export const createE2EUser = (): E2EUser => {
  const suffix = `${Date.now().toString(36)}${Math.random()
    .toString(36)
    .slice(2, 7)}`
  const username = `e2e_${suffix}`.slice(0, 20)

  return {
    username,
    email: `${username}@example.test`,
    password: 'Password123!',
  }
}

export const registerWithApi = async (
  request: APIRequestContext,
  user: E2EUser,
) => {
  const response = await request.post(`${apiURL}/api/v1/auth/register`, {
    data: {
      username: user.username,
      email: user.email,
      password: user.password,
    },
  })

  expect(response.status(), await response.text()).toBe(201)
}

export const loginWithApi = async (
  request: APIRequestContext,
  user: E2EUser,
) => {
  const response = await request.post(`${apiURL}/api/v1/auth/login`, {
    data: {
      username: user.username,
      password: user.password,
    },
  })

  expect(response.status(), await response.text()).toBe(200)

  const body = (await response.json()) as { accessToken: string }
  expect(body.accessToken).toBeTruthy()

  return body.accessToken
}

export const saveMatchWithApi = async (
  request: APIRequestContext,
  accessToken: string,
  result: MatchResult,
) => {
  const response = await request.post(`${apiURL}/api/v1/matches`, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
    data: result,
  })

  expect(response.status(), await response.text()).toBe(201)
}

export const loginWithUi = async (page: Page, user: E2EUser) => {
  await page.goto('/login')
  await page.locator('input[name="username"]').fill(user.username)
  await page.locator('input[name="password"]').fill(user.password)
  await page.getByRole('button', { name: /login/i }).click()
  await expect(page).toHaveURL(/\/leaderboard$/)
}

export const registerWithUi = async (page: Page, user: E2EUser) => {
  await page.goto('/register')
  await page.locator('input[name="username"]').fill(user.username)
  await page.locator('input[name="email"]').fill(user.email)
  await page.locator('input[name="password"]').fill(user.password)
  await page.locator('input[name="confirmPassword"]').fill(user.password)
  await page.getByRole('button', { name: /create account/i }).click()
}

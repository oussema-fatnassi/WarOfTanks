import { expect, test } from '@playwright/test'
import {
  createE2EUser,
  loginWithUi,
  registerWithApi,
  registerWithUi,
} from './support/auth'

test.describe('authentication', () => {
  test('shows an error for invalid login credentials', async ({
    page,
    request,
  }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)

    await page.goto('/login')
    await page.locator('input[name="username"]').fill(user.username)
    await page.locator('input[name="password"]').fill('wrong-password')
    await page.getByRole('button', { name: /login/i }).click()

    await expect(page).toHaveURL(/\/login$/)
    await expect(page.getByText('Invalid username or password')).toBeVisible()
  })

  test('validates registration fields before creating an account', async ({
    page,
  }) => {
    await page.goto('/register')

    await page.locator('input[name="username"]').fill('ab')
    await page.locator('input[name="email"]').fill('invalid-email')
    await page.locator('input[name="password"]').fill('short')
    await page.locator('input[name="confirmPassword"]').fill('different')
    await page.getByRole('button', { name: /create account/i }).click()

    await expect(
      page.getByText('Use 3-20 letters, numbers, or underscores'),
    ).toBeVisible()
    await expect(page.getByText('Enter a valid email address')).toBeVisible()
    await expect(
      page.getByText('Password must be at least 8 characters'),
    ).toBeVisible()
    await expect(page.getByText('Passwords do not match')).toBeVisible()
    await expect(page).toHaveURL(/\/register$/)
  })

  test('shows an error for a duplicate username', async ({ page, request }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)

    await page.goto('/register')
    await page.locator('input[name="username"]').fill(user.username)
    await page.locator('input[name="email"]').fill(`other-${user.email}`)
    await page.locator('input[name="password"]').fill(user.password)
    await page.locator('input[name="confirmPassword"]').fill(user.password)
    await page.getByRole('button', { name: /create account/i }).click()

    await expect(page.getByText('Username already taken')).toBeVisible()
    await expect(page).toHaveURL(/\/register$/)
  })

  test('registers a player through the UI, then logs in and logs out', async ({
    page,
  }) => {
    const user = createE2EUser()

    await registerWithUi(page, user)

    await expect(page).toHaveURL(/\/login$/)
    await expect(
      page.getByText('Account created. You can sign in now.'),
    ).toBeVisible()

    await loginWithUi(page, user)

    await expect(
      page.getByRole('heading', { name: 'Leaderboard' }),
    ).toBeVisible()
    await expect(page.getByText(user.username).first()).toBeVisible()

    await page.getByRole('button', { name: 'Logout' }).click()

    await expect(page).toHaveURL(/\/login$/)
    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible()
  })

  test('restores the authenticated session after a browser reload', async ({
    page,
    request,
  }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)
    await loginWithUi(page, user)

    await page.reload()

    await expect(page).toHaveURL(/\/leaderboard$/)
    await expect(page.getByText(user.username).first()).toBeVisible()
  })
})

import { expect, test } from '@playwright/test'

const protectedRoutes = ['/leaderboard', '/stats', '/history', '/play']

test.describe('protected routes', () => {
  test('redirects the root route to login when no session exists', async ({
    page,
  }) => {
    await page.goto('/')

    await expect(page).toHaveURL(/\/login$/)
    await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible()
  })

  for (const route of protectedRoutes) {
    test(`redirects ${route} to login without a session`, async ({ page }) => {
      await page.goto(route)

      await expect(page).toHaveURL(/\/login$/)
      await expect(page.getByRole('heading', { name: 'Sign in' })).toBeVisible()
    })
  }
})

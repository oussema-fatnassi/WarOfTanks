import { expect, test } from '@playwright/test'
import {
  createE2EUser,
  loginWithUi,
  registerWithApi,
  registerWithUi,
} from './support/auth'

test.describe('authentication', () => {
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

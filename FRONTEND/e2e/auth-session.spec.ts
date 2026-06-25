import { expect, test } from '@playwright/test'
import {
  apiURL,
  createE2EUser,
  loginWithApi,
  loginWithUi,
  registerWithApi,
  saveMatchWithApi,
} from './support/auth'

test.describe('auth session API', () => {
  test('rejects refresh when no refresh cookie is present', async ({
    request,
  }) => {
    const response = await request.post(`${apiURL}/api/v1/auth/refresh`)

    expect(response.status(), await response.text()).toBe(401)
  })

  test('refreshes access from the login cookie and clears it on logout', async ({
    request,
  }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)

    const accessToken = await loginWithApi(request, user)

    const refresh = await request.post(`${apiURL}/api/v1/auth/refresh`)
    expect(refresh.status(), await refresh.text()).toBe(200)

    const refreshBody = (await refresh.json()) as { accessToken?: string }
    expect(refreshBody.accessToken).toBeTruthy()

    const logout = await request.post(`${apiURL}/api/v1/auth/logout`, {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    })
    expect(logout.status(), await logout.text()).toBe(200)

    const refreshAfterLogout = await request.post(
      `${apiURL}/api/v1/auth/refresh`,
    )
    expect(refreshAfterLogout.status(), await refreshAfterLogout.text()).toBe(
      401,
    )
  })

  test('refreshes and retries a protected browser request after a 401', async ({
    page,
    request,
  }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)
    const accessToken = await loginWithApi(request, user)
    await saveMatchWithApi(request, accessToken, {
      winnerTeam: 1,
      playerScore: Date.now(),
      aiScore: 1,
      duration: 60,
    })

    let playersRequests = 0
    let forcedUnauthorized = false
    await page.route(`${apiURL}/api/v1/players`, async (route) => {
      playersRequests += 1

      if (playersRequests === 1) {
        forcedUnauthorized = true
        await route.fulfill({
          status: 401,
          contentType: 'application/json',
          body: JSON.stringify({ error: 'invalid or expired token' }),
        })
        return
      }

      await route.continue()
    })

    await loginWithUi(page, user)

    await expect.poll(() => forcedUnauthorized).toBe(true)
    await expect.poll(() => playersRequests).toBeGreaterThanOrEqual(2)
    await expect(
      page.getByRole('row', { name: new RegExp(user.username) }),
    ).toBeVisible()
  })
})

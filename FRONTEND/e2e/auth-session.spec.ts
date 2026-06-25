import { expect, test } from '@playwright/test'
import {
  apiURL,
  createE2EUser,
  loginWithApi,
  registerWithApi,
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
})

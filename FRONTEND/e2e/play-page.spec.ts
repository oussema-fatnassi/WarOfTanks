import { expect, test } from '@playwright/test'
import { createE2EUser, loginWithUi, registerWithApi } from './support/auth'

test.describe('Unity WebGL page', () => {
  test('loads the protected play page with the Unity iframe', async ({
    page,
    request,
  }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)
    await loginWithUi(page, user)

    await page.goto('/play')

    await expect(page.locator('iframe[title="War of Tanks"]')).toBeVisible()
    await expect(page.locator('iframe[title="War of Tanks"]')).toHaveAttribute(
      'src',
      '/UnityBuild/index.html',
    )
    await expect(page.getByText(user.username).first()).toBeVisible()
  })
})

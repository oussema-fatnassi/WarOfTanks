import { expect, test } from '@playwright/test'
import { createE2EUser, loginWithUi, registerWithApi } from './support/auth'

test.describe('Unity WebGL page', () => {
  test('loads the protected play page and configures the Unity iframe', async ({
    page,
    request,
  }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)
    await loginWithUi(page, user)

    await page.route('**/UnityBuild/index.html', async (route) => {
      await route.fulfill({
        status: 200,
        contentType: 'text/html',
        body: `
          <!doctype html>
          <html>
            <body>
              <span id="status">waiting</span>
              <script>
                window.addEventListener('message', (event) => {
                  if (
                    event.origin !== window.location.origin ||
                    event.data?.type !== 'wot:web-client-config'
                  ) return;

                  document.getElementById('status').textContent =
                    'configured ' +
                    event.data.apiBaseUrl +
                    ' token=' +
                    Boolean(event.data.accessToken);
                });

                window.parent.postMessage({ type: 'wot:unity-ready' }, window.location.origin);
              </script>
            </body>
          </html>
        `,
      })
    })

    await page.goto('/play')

    await expect(page.locator('iframe[title="War of Tanks"]')).toBeVisible()
    await expect(page.locator('iframe[title="War of Tanks"]')).toHaveAttribute(
      'src',
      '/UnityBuild/index.html',
    )
    await expect(page.getByText(user.username).first()).toBeVisible()
    await expect(
      page.frameLocator('iframe[title="War of Tanks"]').locator('#status'),
    ).toHaveText('configured http://localhost:8080 token=true')
  })
})

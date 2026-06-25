import { expect, test } from '@playwright/test'
import {
  createE2EUser,
  loginWithApi,
  loginWithUi,
  registerWithApi,
  saveMatchWithApi,
} from './support/auth'

test.describe('match results', () => {
  test('saved matches are reflected in history, stats, and leaderboard', async ({
    page,
    request,
  }) => {
    const user = createE2EUser()

    await registerWithApi(request, user)
    const accessToken = await loginWithApi(request, user)

    await saveMatchWithApi(request, accessToken, {
      winnerTeam: 1,
      playerScore: 42,
      aiScore: 17,
      duration: 210,
    })
    await saveMatchWithApi(request, accessToken, {
      winnerTeam: 2,
      playerScore: 12,
      aiScore: 24,
      duration: 95,
    })

    await loginWithUi(page, user)

    await page.goto('/history')
    await expect(
      page.getByRole('heading', { name: 'Match history' }),
    ).toBeVisible()
    await expect(page.getByText('2 matches')).toBeVisible()
    await expect(
      page.getByRole('row', { name: /42.*17.*Victory.*03:30/ }),
    ).toBeVisible()
    await expect(
      page.getByRole('row', { name: /12.*24.*Defeat.*01:35/ }),
    ).toBeVisible()

    await page.getByRole('button', { name: 'Victories' }).click()
    await expect(page.getByText('1 matches')).toBeVisible()
    await expect(
      page.getByRole('row', { name: /42.*17.*Victory.*03:30/ }),
    ).toBeVisible()
    await expect(
      page.getByRole('row', { name: /12.*24.*Defeat.*01:35/ }),
    ).toBeHidden()

    await page.getByRole('button', { name: 'Defeats' }).click()
    await expect(page.getByText('1 matches')).toBeVisible()
    await expect(
      page.getByRole('row', { name: /12.*24.*Defeat.*01:35/ }),
    ).toBeVisible()

    await page.goto('/stats')
    await expect(
      page.getByRole('heading', { name: 'Your stats' }),
    ).toBeVisible()
    await expect(statCard(page, 'Total Score')).toContainText('54')
    await expect(statCard(page, 'Matches Played')).toContainText('2')
    await expect(statCard(page, 'Wins · Losses')).toContainText('1 · 1')
    await expect(statCard(page, 'Win Rate')).toContainText('50%')
    await expect(page.getByText('Last match')).toBeVisible()
    await expect(page.getByText('12:24')).toBeVisible()
    await expect(page.getByText('01:35')).toBeVisible()
    await expect(page.getByText('Defeat')).toBeVisible()

    await page.goto('/leaderboard')
    await expect(
      page.getByRole('heading', { name: 'Leaderboard' }),
    ).toBeVisible()
    await expect(
      page.getByRole('row', {
        name: new RegExp(`${user.username}.*54.*1.*1.*2.*50%`),
      }),
    ).toBeVisible()
  })
})

const statCard = (page: import('@playwright/test').Page, label: string) =>
  page
    .getByText(label)
    .locator('xpath=ancestor::div[contains(@class, "rounded-card")][1]')

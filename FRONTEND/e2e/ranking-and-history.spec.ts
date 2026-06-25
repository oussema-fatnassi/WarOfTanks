import { expect, test } from '@playwright/test'
import {
  createE2EUser,
  loginWithApi,
  loginWithUi,
  registerWithApi,
  saveMatchWithApi,
  type E2EUser,
  type MatchResult,
} from './support/auth'

test.describe('ranking and history depth', () => {
  test('loads older match history entries with pagination', async ({
    page,
    request,
  }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)
    const accessToken = await loginWithApi(request, user)

    for (let i = 0; i < 12; i += 1) {
      await saveMatchWithApi(request, accessToken, {
        winnerTeam: 1,
        playerScore: 100 + i,
        aiScore: 10 + i,
        duration: 60 + i,
      })
    }

    await loginWithUi(page, user)
    await page.goto('/history')

    await expect(
      page.getByRole('heading', { name: 'Match history' }),
    ).toBeVisible()
    await expect(page.getByText('10 matches')).toBeVisible()
    await expect(page.getByRole('button', { name: 'Load more' })).toBeVisible()
    await expect(
      page.getByRole('row', { name: /100.*10.*Victory.*01:00/ }),
    ).toBeHidden()

    await page.getByRole('button', { name: 'Load more' }).click()

    await expect(page.getByText('12 matches')).toBeVisible()
    await expect(
      page.getByRole('row', { name: /100.*10.*Victory.*01:00/ }),
    ).toBeVisible()
    await expect(page.getByRole('button', { name: 'Load more' })).toBeHidden()
  })

  test('orders leaderboard rows by total score and highlights the current player', async ({
    page,
    request,
  }) => {
    const highScorePlayer = createE2EUser()
    const currentPlayer = createE2EUser()
    const lowerScorePlayer = createE2EUser()

    await createScoredPlayer(request, highScorePlayer, {
      winnerTeam: 1,
      playerScore: 900_000_000,
      aiScore: 1,
      duration: 60,
    })
    await createScoredPlayer(request, currentPlayer, {
      winnerTeam: 1,
      playerScore: 800_000_000,
      aiScore: 2,
      duration: 60,
    })
    await createScoredPlayer(request, lowerScorePlayer, {
      winnerTeam: 1,
      playerScore: 700_000_000,
      aiScore: 3,
      duration: 60,
    })

    await loginWithUi(page, currentPlayer)
    await page.goto('/leaderboard')

    await expect(
      page.getByRole('heading', { name: 'Leaderboard' }),
    ).toBeVisible()

    const rows = page.locator('tbody tr')
    await expect(
      rows.filter({ hasText: highScorePlayer.username }),
    ).toBeVisible()
    await expect(rows.filter({ hasText: currentPlayer.username })).toBeVisible()
    await expect(
      rows.filter({ hasText: lowerScorePlayer.username }),
    ).toBeVisible()

    const rowTexts = await rows.allTextContents()
    const highScoreIndex = rowTexts.findIndex((text) =>
      text.includes(highScorePlayer.username),
    )
    const currentPlayerIndex = rowTexts.findIndex((text) =>
      text.includes(currentPlayer.username),
    )
    const lowerScoreIndex = rowTexts.findIndex((text) =>
      text.includes(lowerScorePlayer.username),
    )

    expect(highScoreIndex).toBeGreaterThanOrEqual(0)
    expect(currentPlayerIndex).toBeGreaterThanOrEqual(0)
    expect(lowerScoreIndex).toBeGreaterThanOrEqual(0)
    expect(highScoreIndex).toBeLessThan(currentPlayerIndex)
    expect(currentPlayerIndex).toBeLessThan(lowerScoreIndex)
    await expect(
      rows.filter({ hasText: currentPlayer.username }),
    ).toContainText('You')
  })
})

const createScoredPlayer = async (
  request: Parameters<typeof registerWithApi>[0],
  user: E2EUser,
  match: MatchResult,
) => {
  await registerWithApi(request, user)
  const accessToken = await loginWithApi(request, user)
  await saveMatchWithApi(request, accessToken, match)
}

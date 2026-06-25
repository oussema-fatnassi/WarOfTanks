import { expect, test, type APIRequestContext } from '@playwright/test'
import {
  apiURL,
  createE2EUser,
  loginWithApi,
  registerWithApi,
  saveMatchWithApi,
} from './support/auth'

interface ApiMatch {
  playerScore: number
  aiScore: number
  winnerTeam: number
  duration: number
  playerSnapshot: {
    username: string
  }
}

interface ApiPlayer {
  stats: {
    wins: number
    losses: number
    totalMatches: number
    totalScore: number
  }
}

test.describe('match API contract', () => {
  test('rejects match reads and writes without a valid access token', async ({
    request,
  }) => {
    const unauthenticatedRead = await request.get(`${apiURL}/api/v1/matches`)
    expect(unauthenticatedRead.status()).toBe(401)

    const unauthenticatedWrite = await request.post(
      `${apiURL}/api/v1/matches`,
      {
        data: {
          winnerTeam: 1,
          playerScore: 10,
          aiScore: 5,
          duration: 90,
        },
      },
    )
    expect(unauthenticatedWrite.status()).toBe(401)

    const malformedTokenWrite = await request.post(`${apiURL}/api/v1/matches`, {
      headers: {
        Authorization: 'Bearer not-a-real-token',
      },
      data: {
        winnerTeam: 1,
        playerScore: 10,
        aiScore: 5,
        duration: 90,
      },
    })
    expect(malformedTokenWrite.status()).toBe(401)
  })

  test('rejects a match payload without a winner team', async ({ request }) => {
    const user = createE2EUser()
    await registerWithApi(request, user)
    const accessToken = await loginWithApi(request, user)

    const response = await request.post(`${apiURL}/api/v1/matches`, {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
      data: {
        playerScore: 10,
        aiScore: 5,
        duration: 90,
      },
    })

    expect(response.status(), await response.text()).toBe(400)

    const player = await getCurrentPlayer(request, accessToken)
    expect(player.stats.totalMatches).toBe(0)
    expect(player.stats.totalScore).toBe(0)
  })

  test('returns only the authenticated player match history', async ({
    request,
  }) => {
    const playerOne = createE2EUser()
    const playerTwo = createE2EUser()

    await registerWithApi(request, playerOne)
    await registerWithApi(request, playerTwo)

    const playerOneToken = await loginWithApi(request, playerOne)
    const playerTwoToken = await loginWithApi(request, playerTwo)

    await saveMatchWithApi(request, playerOneToken, {
      winnerTeam: 1,
      playerScore: 31,
      aiScore: 18,
      duration: 122,
    })
    await saveMatchWithApi(request, playerTwoToken, {
      winnerTeam: 1,
      playerScore: 99,
      aiScore: 1,
      duration: 60,
    })

    const response = await request.get(`${apiURL}/api/v1/matches`, {
      headers: {
        Authorization: `Bearer ${playerOneToken}`,
      },
    })

    expect(response.status(), await response.text()).toBe(200)

    const matches = (await response.json()) as ApiMatch[]
    expect(matches).toHaveLength(1)
    expect(matches[0]).toMatchObject({
      playerScore: 31,
      aiScore: 18,
      winnerTeam: 1,
      duration: 122,
      playerSnapshot: {
        username: playerOne.username,
      },
    })
  })
})

const getCurrentPlayer = async (
  request: APIRequestContext,
  accessToken: string,
) => {
  const response = await request.get(`${apiURL}/api/v1/players/me`, {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  })

  expect(response.status(), await response.text()).toBe(200)

  return (await response.json()) as ApiPlayer
}

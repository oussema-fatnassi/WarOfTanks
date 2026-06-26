import { screen, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { client } from '../api/client'
import { renderWithAuth } from '../test/renderWithAuth'
import type { Player } from '../types'
import LeaderboardPage from './LeaderboardPage'

vi.mock('../api/client', () => ({
  client: {
    get: vi.fn(),
  },
}))

const mockedGet = vi.mocked(client.get)

const player = (
  id: string,
  username: string,
  totalScore: number,
  wins = 1,
): Player => ({
  id,
  username,
  email: `${username}@example.com`,
  stats: {
    wins,
    losses: 1,
    totalMatches: wins + 1,
    totalScore,
  },
})

describe('LeaderboardPage', () => {
  beforeEach(() => {
    mockedGet.mockReset()
  })

  it('renders leaderboard rows in API order and highlights the current player', async () => {
    const high = player('player-1', 'HighScore', 900)
    const current = player('player-2', 'CurrentPlayer', 800)
    const low = player('player-3', 'LowScore', 700)
    mockedGet.mockResolvedValue({ data: [high, current, low] })

    renderWithAuth(<LeaderboardPage />, { auth: { player: current } })

    expect(await screen.findByText('HighScore')).toBeVisible()
    const rows = screen.getAllByRole('row').slice(1)

    expect(within(rows[0]).getByText('HighScore')).toBeVisible()
    expect(within(rows[1]).getByText('CurrentPlayer')).toBeVisible()
    expect(within(rows[1]).getByText('You')).toBeVisible()
    expect(within(rows[2]).getByText('LowScore')).toBeVisible()
    expect(mockedGet).toHaveBeenCalledWith('/api/v1/players')
  })
})

import { screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { client } from '../api/client'
import { renderWithAuth } from '../test/renderWithAuth'
import type { Match, Player } from '../types'
import StatsPage from './StatsPage'

vi.mock('../api/client', () => ({
  client: {
    get: vi.fn(),
  },
}))

const mockedGet = vi.mocked(client.get)

const currentPlayer: Player = {
  id: 'player-1',
  username: 'Commander',
  email: 'commander@example.com',
  stats: {
    wins: 3,
    losses: 1,
    totalMatches: 4,
    totalScore: 1200,
  },
}

const lastMatch: Match = {
  id: 'match-1',
  playerId: 'player-1',
  playerSnapshot: { username: 'Commander' },
  winnerTeam: 1,
  playerScore: 300,
  aiScore: 120,
  duration: 185,
  createdAt: '2026-05-12T21:14:00.000Z',
}

describe('StatsPage', () => {
  beforeEach(() => {
    mockedGet.mockReset()
  })

  it('renders player totals and the latest match summary', async () => {
    mockedGet.mockImplementation((url) => {
      if (url === '/api/v1/players/me') {
        return Promise.resolve({ data: currentPlayer })
      }

      return Promise.resolve({ data: [lastMatch] })
    })

    renderWithAuth(<StatsPage />, { auth: { player: currentPlayer } })

    expect(await screen.findByText('1200')).toBeVisible()
    expect(screen.getByText('4')).toBeVisible()
    expect(screen.getByText('3 · 1')).toBeVisible()
    expect(screen.getByText('75%')).toBeVisible()
    expect(await screen.findByText('03:05')).toBeVisible()
    expect(screen.getAllByText('300')).toHaveLength(2)
    expect(screen.getAllByText('120')).toHaveLength(2)
    expect(screen.getByText('Victory')).toBeVisible()
    expect(mockedGet).toHaveBeenCalledWith('/api/v1/players/me')
    expect(mockedGet).toHaveBeenCalledWith('/api/v1/matches', {
      params: { limit: 1 },
    })
  })
})

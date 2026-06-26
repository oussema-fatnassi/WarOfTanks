import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { client } from '../api/client'
import { renderWithAuth } from '../test/renderWithAuth'
import type { Match } from '../types'
import HistoryPage from './HistoryPage'

vi.mock('../api/client', () => ({
  client: {
    get: vi.fn(),
  },
}))

const mockedGet = vi.mocked(client.get)

const match = (
  id: string,
  winnerTeam: number,
  playerScore: number,
  aiScore: number,
  duration: number,
): Match => ({
  id,
  playerId: 'player-1',
  playerSnapshot: { username: 'Commander' },
  winnerTeam,
  playerScore,
  aiScore,
  duration,
  createdAt: '2026-05-12T21:14:00.000Z',
})

describe('HistoryPage', () => {
  beforeEach(() => {
    mockedGet.mockReset()
  })

  it('renders scores, result labels, duration formatting, and filters', async () => {
    const user = userEvent.setup()
    mockedGet.mockResolvedValue({
      data: [
        match('match-1', 1, 250, 100, 125),
        match('match-2', 2, 80, 120, 59),
      ],
    })

    renderWithAuth(<HistoryPage />)

    expect(await screen.findByText('250')).toBeVisible()
    expect(screen.getByText('02:05')).toBeVisible()
    expect(screen.getByText('Victory')).toBeVisible()
    expect(screen.getByText('Defeat')).toBeVisible()

    await user.click(screen.getByRole('button', { name: 'Victories' }))
    expect(screen.getByText('Victory')).toBeVisible()
    expect(screen.queryByText('Defeat')).not.toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Defeats' }))
    expect(screen.queryByText('Victory')).not.toBeInTheDocument()
    expect(screen.getByText('Defeat')).toBeVisible()
    expect(mockedGet).toHaveBeenCalledWith('/api/v1/matches', {
      params: { limit: 10, offset: 0 },
    })
  })
})

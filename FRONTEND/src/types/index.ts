interface PlayerStats {
  wins: number
  losses: number
  totalMatches: number
  totalScore: number
}

interface Player {
  id: string
  username: string
  email: string
  stats: PlayerStats
  createdAt?: string
  updatedAt?: string
}

interface Match {
  id: string
  playerId: string
  playerSnapshot: { username: string }
  winnerTeam: number
  playerScore: number
  aiScore: number
  duration: number
  createdAt: string
}

interface AuthTokens {
  accessToken: string
}

export type { Player, PlayerStats, Match, AuthTokens }

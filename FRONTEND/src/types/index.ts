interface Player {
    id: string
    username: string
    totalMatches: number
    wins: number
    losses: number
    totalScore: number
}

interface Match {
    id: string
    playerId: string
    teamAScore: number
    teamBScore: number
    playerWon: boolean
    durationSecs: number
    playedAt: string
}

interface AuthTokens {
    accessToken: string
}

export type { Player, Match, AuthTokens }
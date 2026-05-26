import { setAccessToken as storeSetAccessToken } from '../auth/tokenStore'
import { useState, type ReactNode } from 'react'
import { AuthContext } from './AuthContext'
import { client } from '../api/client'
import type { Player } from '../types'

interface LoginResponsePlayer {
  id: string
  username: string
  email?: string
  totalMatches?: number
  wins?: number
  losses?: number
  totalScore?: number
  stats?: {
    totalMatches?: number
    wins?: number
    losses?: number
    totalScore?: number
  }
}

interface LoginResponse {
  accessToken: string
  player: LoginResponsePlayer
}

const normalizePlayer = (player: LoginResponsePlayer): Player => ({
  id: player.id,
  username: player.username,
  email: player.email ?? '',
  totalMatches: player.stats?.totalMatches ?? player.totalMatches ?? 0,
  wins: player.stats?.wins ?? player.wins ?? 0,
  losses: player.stats?.losses ?? player.losses ?? 0,
  totalScore: player.stats?.totalScore ?? player.totalScore ?? 0,
})

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [player, setPlayer] = useState<Player | null>(null)
  const [accessToken, setAccessToken] = useState<string | null>(null)

  const handleSetAccessToken = (token: string | null) => {
    storeSetAccessToken(token)
    setAccessToken(token)
  }

  const login = async (username: string, password: string) => {
    const response = await client.post<LoginResponse>('/api/v1/auth/login', {
      username,
      password,
    })

    setPlayer(normalizePlayer(response.data.player))
    handleSetAccessToken(response.data.accessToken)
  }

  const logout = async () => {
    try {
      await client.post('/api/v1/auth/logout')
    } finally {
      setPlayer(null)
      handleSetAccessToken(null)
    }
  }

  const setTokenOnly = (token: string | null) => {
    if (!token) {
      setPlayer(null)
    }
    handleSetAccessToken(token)
  }

  return (
    <AuthContext.Provider
      value={{
        player,
        accessToken,
        setAccessToken: setTokenOnly,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

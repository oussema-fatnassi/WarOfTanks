import { setAccessToken as storeSetAccessToken } from '../auth/tokenStore'
import { useEffect, useState, type ReactNode } from 'react'
import { AuthContext } from './AuthContext'
import { client } from '../api/client'
import type { Player } from '../types'

const PLAYER_CACHE_KEY = 'wot_player'

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

interface RefreshResponse {
  accessToken: string
  player?: LoginResponsePlayer
}

const normalizePlayer = (player: LoginResponsePlayer): Player => ({
  id: player.id,
  username: player.username,
  email: player.email ?? '',
  stats: {
    totalMatches: player.stats?.totalMatches ?? player.totalMatches ?? 0,
    wins: player.stats?.wins ?? player.wins ?? 0,
    losses: player.stats?.losses ?? player.losses ?? 0,
    totalScore: player.stats?.totalScore ?? player.totalScore ?? 0,
  },
})

const readCachedPlayer = (): Player | null => {
  try {
    const raw = localStorage.getItem(PLAYER_CACHE_KEY)
    return raw ? (JSON.parse(raw) as Player) : null
  } catch {
    return null
  }
}

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [player, setPlayer] = useState<Player | null>(null)
  const [accessToken, setAccessToken] = useState<string | null>(null)
  const [initializing, setInitializing] = useState(true)

  const handleSetAccessToken = (token: string | null) => {
    storeSetAccessToken(token)
    setAccessToken(token)
  }

  const persistPlayer = (next: Player | null) => {
    setPlayer(next)
    if (next) localStorage.setItem(PLAYER_CACHE_KEY, JSON.stringify(next))
    else localStorage.removeItem(PLAYER_CACHE_KEY)
  }

  // Restore the session on boot using the refresh-token cookie.
  useEffect(() => {
    let active = true
    const restore = async () => {
      try {
        const { data } = await client.post<RefreshResponse>('/api/v1/auth/refresh')
        if (!active) return
        handleSetAccessToken(data.accessToken)

        const fromRefresh = data.player ? normalizePlayer(data.player) : null
        const fromMe =
          fromRefresh ??
          (await client
            .get<LoginResponsePlayer>('/api/v1/players/me')
            .then(res => normalizePlayer(res.data))
            .catch(() => null))
        if (active) persistPlayer(fromMe ?? readCachedPlayer())
      } catch {
        // No valid refresh session — remain logged out.
      } finally {
        if (active) setInitializing(false)
      }
    }
    restore()
    return () => {
      active = false
    }
  }, [])

  const login = async (username: string, password: string) => {
    const response = await client.post<LoginResponse>('/api/v1/auth/login', {
      username,
      password,
    })

    persistPlayer(normalizePlayer(response.data.player))
    handleSetAccessToken(response.data.accessToken)
  }

  const logout = async () => {
    try {
      await client.post('/api/v1/auth/logout')
    } finally {
      persistPlayer(null)
      handleSetAccessToken(null)
    }
  }

  const setTokenOnly = (token: string | null) => {
    if (!token) {
      persistPlayer(null)
    }
    handleSetAccessToken(token)
  }

  return (
    <AuthContext.Provider
      value={{
        player,
        accessToken,
        initializing,
        setAccessToken: setTokenOnly,
        login,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

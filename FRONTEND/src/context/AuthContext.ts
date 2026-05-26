import { createContext } from 'react'
import type { Player } from '../types'

interface AuthContextType {
  player: Player | null
  accessToken: string | null
  setAccessToken: (token: string | null) => void
  login: (username: string, password: string) => Promise<void>
  logout: () => Promise<void>
}

export const AuthContext = createContext<AuthContextType | undefined>(undefined)

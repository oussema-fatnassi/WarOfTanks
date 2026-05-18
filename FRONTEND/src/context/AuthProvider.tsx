import { setAccessToken as storeSetAccessToken } from '../auth/tokenStore'
import { useState, type ReactNode } from 'react'
import { AuthContext } from './AuthContext'

export const AuthProvider = ({ children }: { children: ReactNode }) => {
  const [accessToken, setAccessToken] = useState<string | null>(null)
  const handleSetAccessToken = (token: string | null) => {
    storeSetAccessToken(token)
    setAccessToken(token)
  }

  const logout = () => {
    handleSetAccessToken(null)
  }

  return (
    <AuthContext.Provider
      value={{ accessToken, setAccessToken: handleSetAccessToken, logout }}
    >
      {children}
    </AuthContext.Provider>
  )
}

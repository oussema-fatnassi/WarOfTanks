import { render, type RenderOptions } from '@testing-library/react'
import type { ComponentProps, ReactElement } from 'react'
import { MemoryRouter } from 'react-router-dom'
import { vi } from 'vitest'
import { AuthContext } from '../context/AuthContext'

type AuthValue = ComponentProps<typeof AuthContext.Provider>['value']

const defaultAuthValue: AuthValue = {
  player: null,
  accessToken: null,
  initializing: false,
  setAccessToken: vi.fn(),
  login: vi.fn(),
  logout: vi.fn(),
}

interface RenderWithAuthOptions extends Omit<RenderOptions, 'wrapper'> {
  auth?: Partial<AuthValue>
  route?: string
}

export const renderWithAuth = (
  ui: ReactElement,
  { auth, route = '/', ...options }: RenderWithAuthOptions = {},
) =>
  render(
    <AuthContext.Provider value={{ ...defaultAuthValue, ...auth }}>
      <MemoryRouter initialEntries={[route]}>{ui}</MemoryRouter>
    </AuthContext.Provider>,
    options,
  )

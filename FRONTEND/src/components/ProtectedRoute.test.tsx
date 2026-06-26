import { screen } from '@testing-library/react'
import { Route, Routes } from 'react-router-dom'
import { describe, expect, it } from 'vitest'
import { renderWithAuth } from '../test/renderWithAuth'
import ProtectedRoute from './ProtectedRoute'

const renderProtectedRoute = (accessToken: string | null) =>
  renderWithAuth(
    <Routes>
      <Route element={<ProtectedRoute />}>
        <Route path="/leaderboard" element={<h1>Leaderboard</h1>} />
      </Route>
      <Route path="/login" element={<h1>Sign in</h1>} />
    </Routes>,
    { auth: { accessToken }, route: '/leaderboard' },
  )

describe('ProtectedRoute', () => {
  it('redirects anonymous users to login', () => {
    renderProtectedRoute(null)
    expect(screen.getByRole('heading', { name: 'Sign in' })).toBeVisible()
  })

  it('renders the protected content when an access token exists', () => {
    renderProtectedRoute('access-token')
    expect(screen.getByRole('heading', { name: 'Leaderboard' })).toBeVisible()
  })
})

import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { renderWithAuth } from '../test/renderWithAuth'
import LoginPage from './LoginPage'

describe('LoginPage', () => {
  it('submits trimmed credentials to the auth context', async () => {
    const user = userEvent.setup()
    const login = vi.fn().mockResolvedValue(undefined)

    renderWithAuth(<LoginPage />, { auth: { login } })

    await user.type(screen.getByLabelText(/username/i), '  commander  ')
    await user.type(screen.getByLabelText(/password/i), 'secret-pass')
    await user.click(screen.getByRole('button', { name: /login/i }))

    expect(login).toHaveBeenCalledWith('commander', 'secret-pass')
  })

  it('shows an error when login fails', async () => {
    const user = userEvent.setup()
    const login = vi.fn().mockRejectedValue(new Error('invalid credentials'))

    renderWithAuth(<LoginPage />, { auth: { login } })

    await user.type(screen.getByLabelText(/username/i), 'commander')
    await user.type(screen.getByLabelText(/password/i), 'wrong-pass')
    await user.click(screen.getByRole('button', { name: /login/i }))

    expect(
      await screen.findByText('Invalid username or password'),
    ).toBeVisible()
  })
})

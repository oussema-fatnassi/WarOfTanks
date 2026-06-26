import { screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { client } from '../api/client'
import { renderWithAuth } from '../test/renderWithAuth'
import RegisterPage from './RegisterPage'

vi.mock('../api/client', () => ({
  client: {
    post: vi.fn(),
  },
}))

const mockedPost = vi.mocked(client.post)

describe('RegisterPage', () => {
  beforeEach(() => {
    mockedPost.mockReset()
  })

  it('validates fields before creating an account', async () => {
    const user = userEvent.setup()

    renderWithAuth(<RegisterPage />)

    await user.type(screen.getByLabelText(/username/i), 'ab')
    await user.type(screen.getByLabelText(/^email$/i), 'invalid-email')
    await user.type(screen.getByLabelText(/^password/i), 'short')
    await user.type(screen.getByLabelText(/confirm password/i), 'different')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    expect(
      screen.getByText('Use 3-20 letters, numbers, or underscores'),
    ).toBeVisible()
    expect(screen.getByText('Enter a valid email address')).toBeVisible()
    expect(
      screen.getByText('Password must be at least 8 characters'),
    ).toBeVisible()
    expect(screen.getByText('Passwords do not match')).toBeVisible()
    expect(mockedPost).not.toHaveBeenCalled()
  })

  it('sends the expected registration payload', async () => {
    const user = userEvent.setup()
    mockedPost.mockResolvedValue({ data: {} })

    renderWithAuth(<RegisterPage />)

    await user.type(screen.getByLabelText(/username/i), '  tank_user  ')
    await user.type(screen.getByLabelText(/^email$/i), '  tank@example.com  ')
    await user.type(screen.getByLabelText(/^password/i), 'strong-pass')
    await user.type(screen.getByLabelText(/confirm password/i), 'strong-pass')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    expect(mockedPost).toHaveBeenCalledWith('/api/v1/auth/register', {
      username: 'tank_user',
      email: 'tank@example.com',
      password: 'strong-pass',
    })
  })

  it('shows a field error when the username already exists', async () => {
    const user = userEvent.setup()
    mockedPost.mockRejectedValue({
      isAxiosError: true,
      response: { status: 409 },
    })

    renderWithAuth(<RegisterPage />)

    await user.type(screen.getByLabelText(/username/i), 'tank_user')
    await user.type(screen.getByLabelText(/^email$/i), 'tank@example.com')
    await user.type(screen.getByLabelText(/^password/i), 'strong-pass')
    await user.type(screen.getByLabelText(/confirm password/i), 'strong-pass')
    await user.click(screen.getByRole('button', { name: /create account/i }))

    expect(await screen.findByText('Username already taken')).toBeVisible()
  })
})

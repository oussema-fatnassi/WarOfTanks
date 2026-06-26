import { afterEach, describe, expect, it } from 'vitest'
import { getAccessToken, setAccessToken } from './tokenStore'

describe('tokenStore', () => {
  afterEach(() => {
    setAccessToken(null)
  })

  it('keeps the access token in module memory', () => {
    setAccessToken('access-token')
    expect(getAccessToken()).toBe('access-token')
  })

  it('clears the access token', () => {
    setAccessToken('access-token')
    setAccessToken(null)
    expect(getAccessToken()).toBeNull()
  })
})

import type { AxiosAdapter, InternalAxiosRequestConfig } from 'axios'
import { beforeEach, describe, expect, it, vi } from 'vitest'

const responseFor = (config: InternalAxiosRequestConfig, data = {}) => ({
  data,
  status: 200,
  statusText: 'OK',
  headers: {},
  config,
})

const unauthorizedFor = (config: InternalAxiosRequestConfig) => ({
  isAxiosError: true,
  config,
  response: {
    data: {},
    status: 401,
    statusText: 'Unauthorized',
    headers: {},
    config,
  },
})

describe('api client', () => {
  beforeEach(() => {
    vi.resetModules()
  })

  it('attaches the in-memory access token to protected requests', async () => {
    const { client } = await import('./client')
    const { setAccessToken } = await import('../auth/tokenStore')
    const adapter = vi.fn<AxiosAdapter>((config) =>
      Promise.resolve(responseFor(config)),
    )
    client.defaults.adapter = adapter

    setAccessToken('access-token')
    await client.get('/api/v1/players')

    expect(adapter).toHaveBeenCalledTimes(1)
    expect(adapter.mock.calls[0][0].headers.Authorization).toBe(
      'Bearer access-token',
    )
  })

  it('uses one refresh request for concurrent 401 responses and retries both calls', async () => {
    const { client } = await import('./client')
    const { setAccessToken } = await import('../auth/tokenStore')
    let staleAttempts = 0
    let refreshCalls = 0
    let resolveRefresh: (() => void) | undefined

    const adapter = vi.fn<AxiosAdapter>(async (config) => {
      const authHeader = config.headers.Authorization

      if (config.url === '/api/v1/auth/refresh') {
        refreshCalls += 1
        await new Promise<void>((resolve) => {
          resolveRefresh = resolve
        })
        return responseFor(config, { accessToken: 'fresh-token' })
      }

      if (authHeader === 'Bearer stale-token') {
        staleAttempts += 1
        return Promise.reject(unauthorizedFor(config))
      }

      return responseFor(config, { ok: true })
    })
    client.defaults.adapter = adapter

    setAccessToken('stale-token')
    const requests = Promise.all([
      client.get('/api/v1/players'),
      client.get('/api/v1/matches'),
    ])

    await vi.waitFor(() => {
      expect(staleAttempts).toBe(2)
      expect(refreshCalls).toBe(1)
    })
    resolveRefresh?.()

    const responses = await requests
    expect(responses).toHaveLength(2)
    expect(refreshCalls).toBe(1)
    expect(
      adapter.mock.calls.filter(
        ([config]) => config.url !== '/api/v1/auth/refresh',
      ),
    ).toHaveLength(4)
    expect(adapter.mock.calls.at(-1)?.[0].headers.Authorization).toBe(
      'Bearer fresh-token',
    )
  })
})

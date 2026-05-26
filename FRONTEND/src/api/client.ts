import axios from 'axios'
import { getAccessToken, setAccessToken } from '../auth/tokenStore'

const baseURL = import.meta.env.VITE_API_URL

export const client = axios.create({
  baseURL,
  withCredentials: true,
})

let isRefreshing = false
let pendingQueue: Array<(token: string | null) => void> = []

function processPendingQueue(token: string | null) {
  pendingQueue.forEach((callback) => callback(token))
  pendingQueue = []
}

client.interceptors.request.use((config) => {
  const token = getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

client.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config
    const requestUrl = originalRequest?.url ?? ''
    const canRefresh =
      !requestUrl.includes('/auth/login') &&
      !requestUrl.includes('/auth/register') &&
      !requestUrl.includes('/auth/refresh')

    if (
      error.response?.status === 401 &&
      canRefresh &&
      !originalRequest._retry
    ) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          pendingQueue.push((token) => {
            if (!token) {
              reject(error)
              return
            }

            originalRequest.headers.Authorization = `Bearer ${token}`
            resolve(client(originalRequest))
          })
        })
      }

      originalRequest._retry = true
      isRefreshing = true

      try {
        const response = await client.post('/api/v1/auth/refresh')
        const newAccessToken = response.data.accessToken

        setAccessToken(newAccessToken)
        processPendingQueue(newAccessToken)
        originalRequest.headers.Authorization = `Bearer ${newAccessToken}`

        return client(originalRequest)
      } catch (refreshError) {
        processPendingQueue(null)
        return Promise.reject(refreshError)
      } finally {
        isRefreshing = false
      }
    }

    return Promise.reject(error)
  },
)

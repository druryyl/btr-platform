import axios, { type AxiosError, type AxiosInstance } from 'axios'
import router from '@/router'
import { notifyUnauthorized } from '@/services/authEvents'
import { clearStoredAuth, getStoredToken } from '@/services/authStorage'

const baseURL = import.meta.env.VITE_API_BASE_URL as string

if (!baseURL) {
  console.warn('VITE_API_BASE_URL is not configured.')
}

export const httpClient: AxiosInstance = axios.create({
  baseURL,
  headers: {
    'Content-Type': 'application/json',
  },
})

httpClient.interceptors.request.use((config) => {
  const token = getStoredToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

httpClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError<{ Message?: string }>) => {
    if (error.response?.status === 401) {
      clearStoredAuth()
      notifyUnauthorized()
      const currentPath = router.currentRoute.value.fullPath
      if (currentPath !== '/login') {
        router.push({
          path: '/login',
          query: { redirect: currentPath },
        })
      }
    }

    return Promise.reject(error)
  },
)

export function getApiErrorMessage(error: unknown, fallback = 'An unexpected error occurred.'): string {
  if (axios.isAxiosError(error)) {
    const message = error.response?.data?.Message
    if (typeof message === 'string' && message.length > 0) {
      return message
    }

    if (error.response?.status === 401) {
      return 'Invalid credentials.'
    }
  }

  return fallback
}

import { defineStore } from 'pinia'
import { computed, ref } from 'vue'
import { login as loginApi } from '@/api/authApi'
import { getApiErrorMessage } from '@/api/httpClient'
import type { LoginUserInfo } from '@/models/auth'
import {
  clearStoredAuth,
  isStoredAuthValid,
  loadStoredAuth,
  saveStoredAuth,
} from '@/services/authStorage'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string | null>(null)
  const expiresAt = ref<string | null>(null)
  const user = ref<LoginUserInfo | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  const isAuthenticated = computed(() => {
    if (!token.value || !expiresAt.value) {
      return false
    }

    return new Date(expiresAt.value).getTime() > Date.now()
  })

  function hydrateFromStorage(): void {
    if (!isStoredAuthValid()) {
      clearStoredAuth()
      token.value = null
      expiresAt.value = null
      user.value = null
      return
    }

    const stored = loadStoredAuth()
    if (!stored) {
      return
    }

    token.value = stored.token
    expiresAt.value = stored.expiresAt
    user.value = stored.user
  }

  async function login(userId: string, password: string): Promise<boolean> {
    loading.value = true
    error.value = null

    try {
      const response = await loginApi({ UserId: userId.trim(), Password: password })

      token.value = response.Token
      expiresAt.value = response.ExpiresAt
      user.value = response.User

      saveStoredAuth({
        token: response.Token,
        expiresAt: response.ExpiresAt,
        user: response.User,
      })

      return true
    } catch (err) {
      error.value = getApiErrorMessage(err, 'Login failed.')
      return false
    } finally {
      loading.value = false
    }
  }

  function logout(): void {
    token.value = null
    expiresAt.value = null
    user.value = null
    error.value = null
    clearStoredAuth()
  }

  hydrateFromStorage()

  return {
    token,
    expiresAt,
    user,
    loading,
    error,
    isAuthenticated,
    login,
    logout,
    hydrateFromStorage,
  }
})

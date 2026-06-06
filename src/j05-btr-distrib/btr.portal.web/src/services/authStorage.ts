import type { LoginUserInfo, StoredAuth } from '@/models/auth'

const TOKEN_KEY = 'btr_portal_token'
const USER_KEY = 'btr_portal_user'
const EXPIRES_KEY = 'btr_portal_expires_at'

export function loadStoredAuth(): StoredAuth | null {
  const token = localStorage.getItem(TOKEN_KEY)
  const expiresAt = localStorage.getItem(EXPIRES_KEY)
  const userRaw = localStorage.getItem(USER_KEY)

  if (!token || !expiresAt || !userRaw) {
    return null
  }

  try {
    const user = JSON.parse(userRaw) as LoginUserInfo
    return { token, expiresAt, user }
  } catch {
    clearStoredAuth()
    return null
  }
}

export function saveStoredAuth(auth: StoredAuth): void {
  localStorage.setItem(TOKEN_KEY, auth.token)
  localStorage.setItem(EXPIRES_KEY, auth.expiresAt)
  localStorage.setItem(USER_KEY, JSON.stringify(auth.user))
}

export function clearStoredAuth(): void {
  localStorage.removeItem(TOKEN_KEY)
  localStorage.removeItem(EXPIRES_KEY)
  localStorage.removeItem(USER_KEY)
}

export function getStoredToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function isStoredAuthValid(): boolean {
  const auth = loadStoredAuth()
  if (!auth) {
    return false
  }

  return new Date(auth.expiresAt).getTime() > Date.now()
}

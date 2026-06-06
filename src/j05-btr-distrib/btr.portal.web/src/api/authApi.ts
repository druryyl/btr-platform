import { httpClient } from '@/api/httpClient'
import { isApiSuccess, type ApiResponse } from '@/models/api'
import type { LoginRequest, LoginResponse } from '@/models/auth'

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const { data } = await httpClient.post<ApiResponse<LoginResponse>>('/api/auth/login', request)

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Login failed.')
  }

  return data.Data
}

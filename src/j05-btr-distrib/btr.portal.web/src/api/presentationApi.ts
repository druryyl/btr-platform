import { httpClient } from '@/api/httpClient'
import { isApiSuccess, type ApiResponse } from '@/models/api'

export interface PresentationConfig {
  Enabled: boolean
  BusinessDate: string
}

export async function fetchPresentationConfig(): Promise<{
  enabled: boolean
  businessDate: string
}> {
  const { data } = await httpClient.get<ApiResponse<PresentationConfig>>('/api/config/presentation')

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load presentation configuration.')
  }

  return {
    enabled: data.Data.Enabled,
    businessDate: data.Data.BusinessDate,
  }
}

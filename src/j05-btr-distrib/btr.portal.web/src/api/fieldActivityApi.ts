import { httpClient } from '@/api/httpClient'
import { isApiSuccess, type ApiResponse } from '@/models/api'
import type {
  FieldActivityOverviewResponse,
  FieldActivityResponse,
  FieldActivitySalesmenResponse,
} from '@/models/fieldActivity'

export async function getFieldActivity(
  salesPersonId: string,
  visitDate: string,
): Promise<FieldActivityResponse> {
  const { data } = await httpClient.get<ApiResponse<FieldActivityResponse>>(
    '/api/dashboard/field-activity',
    {
      params: {
        salesPersonId,
        visitDate,
      },
    },
  )

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load field activity.')
  }

  return data.Data
}

export async function listFieldActivitySalesmen(): Promise<FieldActivitySalesmenResponse> {
  const { data } = await httpClient.get<ApiResponse<FieldActivitySalesmenResponse>>(
    '/api/dashboard/field-activity/salesmen',
  )

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load salesmen list.')
  }

  return data.Data
}

export async function getFieldActivityOverview(
  visitDate: string,
): Promise<FieldActivityOverviewResponse> {
  const { data } = await httpClient.get<ApiResponse<FieldActivityOverviewResponse>>(
    '/api/dashboard/field-activity/overview',
    {
      params: { visitDate },
    },
  )

  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load field activity overview.')
  }

  return data.Data
}

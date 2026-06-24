import { httpClient } from '@/api/httpClient'
import { isApiSuccess, type ApiResponse } from '@/models/api'
import type {
  EntityAnalyticsSearchResponse,
  EntityAnalyticsTypesResponse,
  EntityCompareQuery,
  EntityCompareResponse,
  EntityPerformanceProfileResponse,
  EntitySearchResult,
} from '@/models/entityAnalytics'

export async function fetchEntityAnalyticsTypes(): Promise<EntityAnalyticsTypesResponse> {
  const { data } = await httpClient.get<ApiResponse<EntityAnalyticsTypesResponse>>(
    '/api/entity-analytics/types',
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load entity analytics types')
  }
  return data.Data
}

export async function fetchEntityProfile(
  entityType: string,
  entityId: string,
): Promise<EntityPerformanceProfileResponse> {
  const { data } = await httpClient.get<ApiResponse<EntityPerformanceProfileResponse>>(
    `/api/entity-analytics/${encodeURIComponent(entityType)}/${encodeURIComponent(entityId)}`,
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load entity performance profile')
  }
  return data.Data
}

export async function fetchEntityCompare(
  query: EntityCompareQuery,
): Promise<EntityCompareResponse> {
  const { data } = await httpClient.get<ApiResponse<EntityCompareResponse>>(
    '/api/entity-analytics/compare',
    {
      params: {
        entityType: query.entityType,
        entityIds: query.entityIds.join(','),
        kpiIds: query.kpiIds?.length ? query.kpiIds.join(',') : undefined,
        periodYear: query.periodYear,
        periodMonth: query.periodMonth,
      },
    },
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load entity comparison')
  }
  return data.Data
}

export async function searchEntities(
  entityType: string,
  q: string,
  top = 10,
): Promise<EntitySearchResult[]> {
  const { data } = await httpClient.get<ApiResponse<EntityAnalyticsSearchResponse>>(
    '/api/entity-analytics/search',
    {
      params: { entityType, q, top },
    },
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to search entities')
  }
  return data.Data.Results ?? []
}

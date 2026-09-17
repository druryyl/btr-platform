import { httpClient } from '@/api/httpClient'
import { isApiSuccess, type ApiResponse } from '@/models/api'
import type {
  EntityAnalyticsSearchResponse,
  EntityAnalyticsTypesResponse,
  EntityCompareQuery,
  EntityCompareResponse,
  EntityDataHealthResponse,
  EntityPerformanceProfileResponse,
  EntitySearchResult,
  InvestigationLensesResponse,
  MapPresetsResponse,
  PeerDistributionResponse,
  PeerGroupRulesResponse,
  PopulationMapResponse,
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

export async function fetchMapPresets(entityType: string): Promise<MapPresetsResponse> {
  const { data } = await httpClient.get<ApiResponse<MapPresetsResponse>>(
    '/api/entity-analytics/presets',
    { params: { entityType } },
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load map presets')
  }
  return data.Data
}

export async function fetchInvestigationLenses(
  entityType: string,
): Promise<InvestigationLensesResponse> {
  const { data } = await httpClient.get<ApiResponse<InvestigationLensesResponse>>(
    '/api/entity-analytics/lenses',
    { params: { entityType } },
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load investigation lenses')
  }
  return data.Data
}

export async function fetchEntityDataHealth(
  entityType: string,
): Promise<EntityDataHealthResponse> {
  const { data } = await httpClient.get<ApiResponse<EntityDataHealthResponse>>(
    '/api/entity-analytics/data-health',
    { params: { entityType } },
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load data health')
  }
  return data.Data
}

/** Default axios timeout for population map (prevents infinite loading shimmer). */
export const POPULATION_MAP_TIMEOUT_MS = 30_000

export async function fetchPopulationMap(params: {
  entityType: string
  presetId?: string
  dimensionFilter?: string
  attentionOnly?: boolean
  signal?: AbortSignal
  timeoutMs?: number
}): Promise<PopulationMapResponse> {
  const { data } = await httpClient.get<ApiResponse<PopulationMapResponse>>(
    '/api/entity-analytics/population',
    {
      params: {
        entityType: params.entityType,
        presetId: params.presetId,
        dimensionFilter: params.dimensionFilter || undefined,
        attentionOnly: params.attentionOnly ?? undefined,
      },
      signal: params.signal,
      timeout: params.timeoutMs ?? POPULATION_MAP_TIMEOUT_MS,
    },
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load population map')
  }
  return data.Data
}

export async function fetchPeerGroupRules(entityType: string): Promise<PeerGroupRulesResponse> {
  const { data } = await httpClient.get<ApiResponse<PeerGroupRulesResponse>>(
    '/api/entity-analytics/peer-group-rules',
    { params: { entityType } },
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load peer group rules')
  }
  return data.Data
}

export async function fetchPeerDistribution(params: {
  entityType: string
  entityId: string
  kpiId: string
  dimensionFilter?: string
  peerGroupRuleId?: string
}): Promise<PeerDistributionResponse> {
  const { data } = await httpClient.get<ApiResponse<PeerDistributionResponse>>(
    '/api/entity-analytics/peer-distribution',
    {
      params: {
        entityType: params.entityType,
        entityId: params.entityId,
        kpiId: params.kpiId,
        dimensionFilter: params.dimensionFilter || undefined,
        peerGroupRuleId: params.peerGroupRuleId || undefined,
      },
    },
  )
  if (!isApiSuccess(data) || !data.Data) {
    throw new Error(data.Message ?? 'Failed to load peer distribution')
  }
  return data.Data
}

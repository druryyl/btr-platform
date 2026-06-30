import type { LocationQuery, LocationQueryValue } from 'vue-router'

export interface WorkspaceUrlState {
  entityType: string
  presetId?: string | null
  entityIds?: string[]
  dimensionFilter?: string | null
  attentionOnly?: boolean
}

function readQueryValue(value: LocationQueryValue | LocationQueryValue[] | undefined): string | null {
  if (Array.isArray(value)) return value[0] ?? null
  return value ?? null
}

export function parseWorkspaceUrlState(
  entityType: string,
  query: LocationQuery,
): WorkspaceUrlState {
  const entitiesRaw = readQueryValue(query.entities)
  const entityIds = entitiesRaw
    ? entitiesRaw.split(',').map((id) => id.trim()).filter(Boolean)
    : []

  const attentionRaw = readQueryValue(query.attentionOnly)
  return {
    entityType,
    presetId: readQueryValue(query.preset),
    entityIds,
    dimensionFilter: readQueryValue(query.filter),
    attentionOnly: attentionRaw === '1' || attentionRaw === 'true',
  }
}

export function buildWorkspaceQuery(state: WorkspaceUrlState): Record<string, string> {
  const query: Record<string, string> = {}
  if (state.presetId) query.preset = state.presetId
  if (state.entityIds?.length) query.entities = state.entityIds.join(',')
  if (state.dimensionFilter) query.filter = state.dimensionFilter
  if (state.attentionOnly) query.attentionOnly = '1'
  return query
}

export function buildWorkspaceRouteName(): string {
  return 'entity-analytics-workspace'
}

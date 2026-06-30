import type { RouteLocationRaw } from 'vue-router'
import { buildWorkspaceQuery } from '@/services/investigationWorkspaceUrl'

export function buildWorkspaceRoute(
  entityType: string,
  options?: {
    presetId?: string | null
    entityIds?: string[]
    dimensionFilter?: string | null
    attentionOnly?: boolean
  },
): RouteLocationRaw {
  return {
    name: 'entity-analytics-workspace',
    params: { entityType },
    query: buildWorkspaceQuery({
      entityType,
      presetId: options?.presetId,
      entityIds: options?.entityIds,
      dimensionFilter: options?.dimensionFilter,
      attentionOnly: options?.attentionOnly,
    }),
  }
}

export const ENTITY_TYPE_WORKSPACE_LABELS: Record<string, string> = {
  Customer: 'Customers',
  Salesman: 'Salesmen',
  Supplier: 'Suppliers',
  Item: 'Items',
}

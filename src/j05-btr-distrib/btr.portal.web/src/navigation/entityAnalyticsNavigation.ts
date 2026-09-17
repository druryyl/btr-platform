import type { RouteLocationRaw } from 'vue-router'

export interface EntityAnalyticsNavConfig {
  entityType: string
  singularLabel: string
  pluralLabel: string
  compareRouteName: string
}

export const ENTITY_ANALYTICS_NAV: Record<string, EntityAnalyticsNavConfig> = {
  Customer: {
    entityType: 'Customer',
    singularLabel: 'Customer',
    pluralLabel: 'Customers',
    compareRouteName: 'customer-compare',
  },
  Salesman: {
    entityType: 'Salesman',
    singularLabel: 'Salesman',
    pluralLabel: 'Salesmen',
    compareRouteName: 'salesman-compare',
  },
  Supplier: {
    entityType: 'Supplier',
    singularLabel: 'Principal',
    pluralLabel: 'Principals',
    compareRouteName: 'supplier-compare',
  },
  Item: {
    entityType: 'Item',
    singularLabel: 'Item',
    pluralLabel: 'Items',
    compareRouteName: 'item-compare',
  },
}

export function getEntityAnalyticsNav(entityType: string | null | undefined): EntityAnalyticsNavConfig | undefined {
  if (!entityType) return undefined
  return ENTITY_ANALYTICS_NAV[entityType]
}

export function getEntityDisplayLabel(entityType: string | null | undefined): string {
  const config = getEntityAnalyticsNav(entityType)
  return config?.singularLabel ?? (entityType ?? '')
}

export function buildCompareRoute(
  entityType: string,
  entityId?: string | null,
): RouteLocationRaw {
  const config = getEntityAnalyticsNav(entityType)
  if (!config) {
    return { name: 'entity-analytics-home' }
  }

  const id = entityId?.trim()
  return {
    name: config.compareRouteName,
    query: id ? { entities: id } : {},
  }
}

export const PROFILE_ROW_CLICK_HINT = 'Click a row to open Performance Profile'

import type { RouteLocationRaw } from 'vue-router'

export interface EntityAnalyticsNavConfig {
  entityType: string
  pluralLabel: string
  compareRouteName: string
}

export const ENTITY_ANALYTICS_NAV: Record<string, EntityAnalyticsNavConfig> = {
  Customer: {
    entityType: 'Customer',
    pluralLabel: 'Customers',
    compareRouteName: 'customer-compare',
  },
  Salesman: {
    entityType: 'Salesman',
    pluralLabel: 'Salesmen',
    compareRouteName: 'salesman-compare',
  },
  Supplier: {
    entityType: 'Supplier',
    pluralLabel: 'Suppliers',
    compareRouteName: 'supplier-compare',
  },
  Item: {
    entityType: 'Item',
    pluralLabel: 'Items',
    compareRouteName: 'item-compare',
  },
}

export function getEntityAnalyticsNav(entityType: string | null | undefined): EntityAnalyticsNavConfig | undefined {
  if (!entityType) return undefined
  return ENTITY_ANALYTICS_NAV[entityType]
}

export function buildCompareRoute(
  entityType: string,
  entityCode?: string | null,
): RouteLocationRaw {
  const config = getEntityAnalyticsNav(entityType)
  if (!config) {
    return { name: 'entity-analytics-home' }
  }

  const code = entityCode?.trim()
  return {
    name: config.compareRouteName,
    query: code ? { entities: code } : {},
  }
}

export const PROFILE_ROW_CLICK_HINT = 'Click a row to open Performance Profile'

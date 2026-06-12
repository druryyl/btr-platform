import type {
  AchievementBand,
  DashboardAlertCenterAlertRow,
  DashboardAlertCenterCategoryGroup,
  DashboardAlertCenterCategorySummary,
  DashboardAlertCenterNavigationLinks,
} from '@/models/dashboard'

export const ALERT_CENTER_CATEGORY_ORDER = [
  'Sales',
  'Customer',
  'Collection',
  'Inventory',
  'Purchasing',
  'Location',
] as const

export type AlertCenterCategory = (typeof ALERT_CENTER_CATEGORY_ORDER)[number]

export const ALERT_CENTER_CATEGORY_ICONS: Record<AlertCenterCategory, string> = {
  Sales: 'pi pi-chart-line',
  Customer: 'pi pi-users',
  Collection: 'pi pi-wallet',
  Inventory: 'pi pi-box',
  Purchasing: 'pi pi-shopping-cart',
  Location: 'pi pi-map-marker',
}

export const CRITICAL_ALERTS_LIMIT = 5

function categoryOrderIndex(category: string): number {
  const index = ALERT_CENTER_CATEGORY_ORDER.indexOf(category as AlertCenterCategory)
  return index >= 0 ? index : ALERT_CENTER_CATEGORY_ORDER.length
}

function achievementBandRank(band: AchievementBand | null): number {
  if (band === 'Critical') return 0
  if (band === 'Warning') return 1
  return 2
}

export function flattenAlertRows(
  groups: DashboardAlertCenterCategoryGroup[],
): DashboardAlertCenterAlertRow[] {
  return groups.flatMap((group) => group.Alerts)
}

export function compareAlertsByPriority(
  a: DashboardAlertCenterAlertRow,
  b: DashboardAlertCenterAlertRow,
): number {
  const bandDiff = achievementBandRank(a.AchievementBand) - achievementBandRank(b.AchievementBand)
  if (bandDiff !== 0) return bandDiff

  const sortDiff = a.SortOrder - b.SortOrder
  if (sortDiff !== 0) return sortDiff

  const amountA = a.ValueAmount ?? Number.NEGATIVE_INFINITY
  const amountB = b.ValueAmount ?? Number.NEGATIVE_INFINITY
  if (amountB !== amountA) return amountB - amountA

  return (a.EntityName ?? '').localeCompare(b.EntityName ?? '', undefined, {
    sensitivity: 'base',
  })
}

export function prioritizeAlerts(
  alerts: DashboardAlertCenterAlertRow[],
): DashboardAlertCenterAlertRow[] {
  return [...alerts].sort(compareAlertsByPriority)
}

export function getTopCriticalAlerts(
  groups: DashboardAlertCenterCategoryGroup[],
  limit = CRITICAL_ALERTS_LIMIT,
): DashboardAlertCenterAlertRow[] {
  return prioritizeAlerts(flattenAlertRows(groups)).slice(0, limit)
}

export function sortCategorySummaries(
  summaries: DashboardAlertCenterCategorySummary[],
): DashboardAlertCenterCategorySummary[] {
  return [...summaries].sort((a, b) => {
    if (b.TotalCount !== a.TotalCount) return b.TotalCount - a.TotalCount
    return categoryOrderIndex(a.Category) - categoryOrderIndex(b.Category)
  })
}

export function findHighestCountCategory(
  summaries: DashboardAlertCenterCategorySummary[],
): string | null {
  const withAlerts = summaries.filter((summary) => summary.TotalCount > 0)
  if (withAlerts.length === 0) return null
  return sortCategorySummaries(withAlerts)[0]?.Category ?? null
}

export function getCategoryDashboardRoute(
  category: string,
  navigation: DashboardAlertCenterNavigationLinks | null,
): string | null {
  if (!navigation) return null

  switch (category) {
    case 'Sales':
      return navigation.SalesDashboardRoute
    case 'Customer':
      return navigation.CustomerDashboardRoute
    case 'Collection':
      return navigation.CollectionDashboardRoute
    case 'Inventory':
      return navigation.InventoryRiskDashboardRoute
    case 'Purchasing':
      return navigation.PurchasingDashboardRoute
    case 'Location':
      return navigation.LocationDashboardRoute
    default:
      return null
  }
}

export function categoryPanelId(category: string): string {
  return `alert-category-${category.toLowerCase().replace(/\s+/g, '-')}`
}

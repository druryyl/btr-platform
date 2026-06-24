import type { RouteLocationNormalizedLoaded } from 'vue-router'
import type { InvestigationBreadcrumbContext } from '@/models/investigation'

function readQueryString(route: RouteLocationNormalizedLoaded, key: string): string {
  const value = route.query[key]
  if (typeof value === 'string' && value.trim()) {
    return value.trim()
  }
  return ''
}

export interface InvestigationQueryApplyResult {
  breadcrumb: InvestigationBreadcrumbContext
  freeText: string
  customerId: string
  customerCode: string
  salesmanId: string
  brgId: string
  warehouseId: string
  supplierId: string
  periodMode: string
  postingFilter: string
  allOpenBalances: boolean
}

export function applyInvestigationQuery(
  route: RouteLocationNormalizedLoaded,
): InvestigationQueryApplyResult {
  const breadcrumb: InvestigationBreadcrumbContext = {
    signalKey: readQueryString(route, 'signalKey'),
    signalLabel: readQueryString(route, 'signalLabel'),
    source: readQueryString(route, 'source') || undefined,
    entityType: readQueryString(route, 'entityType') || undefined,
    entityName: readQueryString(route, 'q') || undefined,
    dashboardRoute:
      typeof history.state?.investigationDashboardRoute === 'string'
        ? history.state.investigationDashboardRoute
        : undefined,
    desktopNextStep:
      typeof history.state?.investigationDesktopNextStep === 'string'
        ? history.state.investigationDesktopNextStep
        : undefined,
    investigationSteps: Array.isArray(history.state?.investigationSteps)
      ? history.state.investigationSteps
      : undefined,
  }

  const periodMode = readQueryString(route, 'periodMode')

  return {
    breadcrumb,
    freeText: readQueryString(route, 'q'),
    customerId: readQueryString(route, 'customerId'),
    customerCode: readQueryString(route, 'customerCode'),
    salesmanId: readQueryString(route, 'salesmanId'),
    brgId: readQueryString(route, 'brgId'),
    warehouseId: readQueryString(route, 'warehouseId'),
    supplierId: readQueryString(route, 'supplierId'),
    periodMode,
    postingFilter: readQueryString(route, 'posting'),
    allOpenBalances: periodMode === 'allOpenBalances',
  }
}

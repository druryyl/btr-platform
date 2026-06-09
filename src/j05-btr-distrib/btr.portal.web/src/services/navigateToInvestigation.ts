import type { HistoryState, Router } from 'vue-router'
import type { InvestigationMetadata } from '@/models/investigation'
import { buildInvestigationQuery } from '@/services/buildInvestigationQuery'

export function navigateToInvestigation(
  router: Router,
  investigation: InvestigationMetadata,
  sourceDashboard: string,
): void {
  if (!investigation.ReportRoute) {
    return
  }

  const query = buildInvestigationQuery(investigation, sourceDashboard)

  void router.push({
    path: investigation.ReportRoute,
    query,
    state: {
      investigationDashboardRoute: investigation.DashboardRoute ?? undefined,
      investigationDesktopNextStep: investigation.DesktopNextStep ?? undefined,
      investigationSteps: investigation.InvestigationSteps ?? undefined,
    } as HistoryState,
  })
}

export function navigateToDashboard(
  router: Router,
  dashboardRoute: string,
): void {
  void router.push(dashboardRoute)
}

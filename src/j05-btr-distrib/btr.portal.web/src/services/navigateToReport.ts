import type { Router } from 'vue-router'
import type { InvestigationMetadata } from '@/models/investigation'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

/** @deprecated Use navigateToInvestigation with Investigation metadata */
export function navigateToReport(
  router: Router,
  route: string,
  customerName: string,
  sourceDashboard = '',
): void {
  const investigation: InvestigationMetadata = {
    SignalKey: '',
    SignalLabel: '',
    EntityType: '',
    EntityId: '',
    EntityName: customerName,
    ReportRoute: route,
    SuggestedQuery: { FreeText: customerName },
  }

  navigateToInvestigation(router, investigation, sourceDashboard)
}

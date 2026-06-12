import type { DashboardAlertCenterAlertRow } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'

export function formatAlertValue(row: DashboardAlertCenterAlertRow): string {
  if (row.ValueText) {
    return row.ValueText
  }

  if (row.ValueAmount != null) {
    return formatCurrency(row.ValueAmount)
  }

  return '—'
}

export function canInvestigateAlert(row: DashboardAlertCenterAlertRow): boolean {
  return (
    row.Investigation?.ReportRoute != null
    && row.EntityType !== 'Wilayah'
    && row.EntityType !== 'Company'
  )
}

export function canViewDashboardAlert(row: DashboardAlertCenterAlertRow): boolean {
  return Boolean(row.DashboardRoute)
}

import type { InvestigationMetadata, InvestigationQueryParams } from '@/models/investigation'

export function buildInvestigationQuery(
  investigation: InvestigationMetadata,
  sourceDashboard: string,
): InvestigationQueryParams {
  const query: InvestigationQueryParams = {}
  const suggested = investigation.SuggestedQuery

  if (suggested?.FreeText?.trim()) {
    query.q = suggested.FreeText.trim()
  }

  if (suggested?.CustomerId?.trim()) {
    query.customerId = suggested.CustomerId.trim()
  }

  if (suggested?.CustomerCode?.trim()) {
    query.customerCode = suggested.CustomerCode.trim()
  } else if (
    investigation.EntityType === 'Customer' &&
    investigation.EntityId?.trim() &&
    investigation.ReportRoute?.includes('/reports/customers')
  ) {
    query.customerCode = investigation.EntityId.trim()
  }

  if (suggested?.SalesmanId?.trim()) {
    query.salesmanId = suggested.SalesmanId.trim()
  }

  if (suggested?.BrgId?.trim()) {
    query.brgId = suggested.BrgId.trim()
  }

  if (suggested?.WarehouseId?.trim()) {
    query.warehouseId = suggested.WarehouseId.trim()
  }

  if (suggested?.SupplierId?.trim()) {
    query.supplierId = suggested.SupplierId.trim()
  }

  if (suggested?.PeriodMode) {
    query.periodMode = suggested.PeriodMode
  }

  if (suggested?.PostingFilter) {
    query.posting = suggested.PostingFilter
  }

  if (investigation.SignalKey) {
    query.signalKey = investigation.SignalKey
  }

  if (investigation.SignalLabel) {
    query.signalLabel = investigation.SignalLabel
  }

  if (sourceDashboard) {
    query.source = sourceDashboard
  }

  if (investigation.EntityType) {
    query.entityType = investigation.EntityType
  }

  return query
}

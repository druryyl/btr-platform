export const FI02_INVOICE_ATTRIBUTED_SALESMAN_LABEL = 'Invoice-Attributed Salesman'

export const FI02_TOP_OVERDUE_SALESMEN_HEADING = 'Top Overdue Salesmen (Invoice-Attributed)'

export const FI02_TOP_OVERDUE_SALESMEN_TITLE = 'Top 10 Invoice-Attributed Overdue Salesmen'

export const FI02_TOP_OVERDUE_SALESMEN_EMPTY = 'No invoice-attributed overdue salesman ranking data.'

export const FI02_INVOICE_ATTRIBUTED_SALESMAN_NOTE =
  'This ranking is invoice-attributed overdue exposure, not an owned or assigned customer book. It does not assign Customer ownership to a Salesman.'

export const FI02_ATTRIBUTION_DISCLOSURES = [
  FI02_INVOICE_ATTRIBUTED_SALESMAN_NOTE,
  'No Principal overdue or collection KPI is shown.',
] as const

export function isForbiddenAccountOwnerLabel(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized === 'owner'
    || normalized === 'accountowner'
    || normalized === 'assignedsalesman'
    || normalized === 'topoverduesalesmen'
    || normalized === 'topoverduesalesman'
}

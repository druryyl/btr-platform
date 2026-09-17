export const FI04_INVOICE_ATTRIBUTED_SALESMAN_LABEL = 'Invoice-Attributed Salesman'

export const FI04_INVOICE_ATTRIBUTED_SALESMAN_NOTE =
  'This column is the invoice-attributed Salesman on the open Faktur, not the Customer account owner. It does not assign Customer ownership to a Salesman.'

export const FI04_ATTRIBUTION_DISCLOSURES = [
  FI04_INVOICE_ATTRIBUTED_SALESMAN_NOTE,
  'No Principal financial column is shown.',
] as const

export function isForbiddenAccountOwnerLabel(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized === 'owner'
    || normalized === 'accountowner'
    || normalized === 'assignedsalesman'
}

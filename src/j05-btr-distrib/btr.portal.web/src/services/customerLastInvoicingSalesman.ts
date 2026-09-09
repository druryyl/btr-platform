export const LAST_INVOICING_SALESMAN_RELATIONSHIP_CODE = 'AssignedSalesman'

export const LAST_INVOICING_SALESMAN_LABEL = 'Last Invoicing Salesman'

export const LAST_INVOICING_SALESMAN_NOTE =
  'This is the last invoicing Salesman, not the Customer owner.'

export function isLastInvoicingSalesmanLabel(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized === 'salesman' || normalized === 'lastinvoicingsalesman'
}

export function isLastInvoicingSalesmanRelationship(relationshipCode: string | null | undefined): boolean {
  return relationshipCode === LAST_INVOICING_SALESMAN_RELATIONSHIP_CODE
}

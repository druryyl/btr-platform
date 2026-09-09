import {
  LAST_INVOICING_SALESMAN_LABEL,
  LAST_INVOICING_SALESMAN_NOTE,
} from '@/services/customerLastInvoicingSalesman'

export const CU01_LAST_INVOICING_SALESMAN_LABEL = LAST_INVOICING_SALESMAN_LABEL

export const CU01_LAST_INVOICING_SALESMAN_NOTE = LAST_INVOICING_SALESMAN_NOTE

export const CU01_ATTRIBUTION_DISCLOSURES = [
  'Customer credit, piutang, and lifecycle measures remain Customer-level. They are not allocated to a Salesman or a Principal.',
  'The latest-Faktur Salesman is the last invoicing Salesman, a recency indicator, not the Customer owner.',
  'This page does not label that Salesman as Assigned Salesman or Owner.',
] as const

export function isForbiddenCustomerOwnerLabel(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized === 'assignedsalesman' || normalized === 'owner'
}

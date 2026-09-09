import {
  LAST_INVOICING_SALESMAN_LABEL,
  LAST_INVOICING_SALESMAN_NOTE,
} from '@/services/customerLastInvoicingSalesman'

export const CU01_LAST_INVOICING_SALESMAN_LABEL = LAST_INVOICING_SALESMAN_LABEL

export const CU01_LAST_INVOICING_SALESMAN_NOTE = LAST_INVOICING_SALESMAN_NOTE

export const CU01_PRINCIPAL_MIX_KPI_ID = 'PRN-SALES-001'

export const CU01_PRINCIPAL_MIX_NOTE =
  'Principal mix reads the Customer–Principal relationship projection only. It does not recompute relationships from raw transactions.'

export const CU01_ATTRIBUTION_DISCLOSURES = [
  'Customer credit, piutang, and lifecycle measures remain Customer-level. They are not allocated to a Salesman or a Principal.',
  'Customer total sales, credit, and piutang remain Customer-level and are not allocated to Principals.',
  'The latest-Faktur Salesman is the last invoicing Salesman, a recency indicator, not the Customer owner.',
  'This page does not label that Salesman as Assigned Salesman or Owner.',
  CU01_PRINCIPAL_MIX_NOTE,
  'The mix lists Principals present on that Customer\'s projection only. No pre-purchase assigned Principal is shown.',
] as const

export function isForbiddenCustomerOwnerLabel(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized === 'assignedsalesman' || normalized === 'owner'
}

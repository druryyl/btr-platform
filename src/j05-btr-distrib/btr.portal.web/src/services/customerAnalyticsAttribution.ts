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

export const CU02_LAST_INVOICING_SALESMAN_LABEL = LAST_INVOICING_SALESMAN_LABEL

export const CU02_LAST_INVOICING_SALESMAN_NOTE =
  'This is the last invoicing Salesman on the current-month invoice, a recency indicator, not the Customer owner.'

export const CU02_LOW_RECOVERY_EXPLANATION =
  'Last invoicing Salesman has low recovery vs billing and the customer is overdue. This is invoice attribution, not Customer ownership.'

const CU02_LEGACY_ASSIGNED_SALESMAN_EXPLANATION =
  'Assigned salesman has low recovery vs billing and customer is overdue.'

export const CU02_ATTRIBUTION_DISCLOSURES = [
  'The Salesman shown is the last invoicing Salesman on the current-month invoice, a recency indicator, not the Customer owner.',
  'This page does not present that Salesman as Assigned Salesman or Owner.',
  'Customer risk, credit, piutang, and decline measures remain Customer-level. Principal-specific decline is not shown.',
] as const

export const CU04_LAST_INVOICING_SALESMAN_LABEL = LAST_INVOICING_SALESMAN_LABEL

export const CU04_LAST_INVOICING_SALESMAN_FILTER_ALL = 'All Last Invoicing Salesmen'

export const CU04_LAST_INVOICING_SALESMAN_NOTE =
  'This is the last invoicing Salesman, a commercial attribution on the latest invoice, not the Customer owner.'

export const CU04_ATTRIBUTION_DISCLOSURES = [
  'The Salesman filter and displayed Salesman are the last invoicing Salesman, a commercial attribution on the latest invoice, not the Customer owner.',
  'This page does not describe that Salesman as Assigned Salesman or Owner.',
  'Customer portfolio measures remain Customer-level. Principal portfolio mix is not shown.',
] as const

export const CU05_ACTION_ROUTE_LABEL = 'Action Route'

export const CU05_ACTION_ROUTE_NOTE =
  'This is the portfolio action function for the recommended action, not the Customer owner.'

export const CU05_LAST_INVOICING_SALESMAN_LABEL = LAST_INVOICING_SALESMAN_LABEL

export const CU05_LAST_INVOICING_SALESMAN_NOTE =
  'This is the last invoicing Salesman, a commercial attribution on the latest invoice, not the Customer owner.'

export const CU05_ATTRIBUTION_DISCLOSURES = [
  'The Salesman column is the last invoicing Salesman, a commercial attribution on the latest invoice, not the Customer owner.',
  'This page does not label that Salesman as Owner of the Customer.',
  'The action route is the portfolio action function, not Customer ownership.',
  'Customer totals remain Customer-level. Pair evidence is not shown.',
] as const

export function isForbiddenCustomerOwnerLabel(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized === 'assignedsalesman' || normalized === 'owner'
}

export function correctCu02AttributionText(text: string | null | undefined): string {
  if (!text) return text ?? ''
  if (text.trim().toLowerCase() === CU02_LEGACY_ASSIGNED_SALESMAN_EXPLANATION.toLowerCase()) {
    return CU02_LOW_RECOVERY_EXPLANATION
  }
  return text
}

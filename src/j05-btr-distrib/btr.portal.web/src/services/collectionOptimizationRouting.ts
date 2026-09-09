export const CU03_ACTION_ROUTE_LABEL = 'Action Route'

export const CU03_ACTION_ROUTE_NOTE =
  'This is the operational route for today\'s collection action, not the Customer owner.'

export const CU03_ROUTED_SALESMAN_LABEL = 'Routed Salesman'

export const CU03_ROUTED_SALESMAN_NOTE =
  'This Salesman is an operational route for today\'s collection work, not the Customer owner.'

export const CU03_SALESMAN_WORKLOAD_TYPE = 'Salesman'

export const CU03_SALESMAN_WORKLOAD_LABEL = CU03_ROUTED_SALESMAN_LABEL

export const CU03_ATTRIBUTION_DISCLOSURES = [
  'Collection queues remain Customer-level. They are not allocated to a Salesman or a Principal.',
  'Salesman routing is operational routing of today\'s collection work, not Customer ownership.',
  'This page does not describe the routed Salesman or action route as the Customer owner or Assigned Salesman.',
  'No Principal collection impact is shown.',
] as const

export function isForbiddenCustomerOwnershipLabel(label: string | null | undefined): boolean {
  const normalized = (label ?? '').toLowerCase().replace(/[^a-z0-9]/g, '')
  return normalized === 'owner'
    || normalized === 'accountowner'
    || normalized === 'assignedsalesman'
    || normalized === 'customerowner'
}

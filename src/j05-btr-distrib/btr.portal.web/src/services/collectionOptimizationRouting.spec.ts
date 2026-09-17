import { describe, expect, it } from 'vitest'
import {
  CU03_ACTION_ROUTE_LABEL,
  CU03_ACTION_ROUTE_NOTE,
  CU03_ATTRIBUTION_DISCLOSURES,
  CU03_ROUTED_SALESMAN_LABEL,
  CU03_ROUTED_SALESMAN_NOTE,
  CU03_SALESMAN_WORKLOAD_LABEL,
  CU03_SALESMAN_WORKLOAD_TYPE,
  isForbiddenCustomerOwnershipLabel,
} from '@/services/collectionOptimizationRouting'

describe('CU03 operational routing labels', () => {
  it('describes action routing as operational, not Customer ownership', () => {
    expect(CU03_ACTION_ROUTE_LABEL).toBe('Action Route')
    expect(isForbiddenCustomerOwnershipLabel(CU03_ACTION_ROUTE_LABEL)).toBe(false)
    expect(isForbiddenCustomerOwnershipLabel('Owner')).toBe(true)
    expect(isForbiddenCustomerOwnershipLabel('Assigned Salesman')).toBe(true)
    expect(isForbiddenCustomerOwnershipLabel('Account Owner')).toBe(true)
    expect(CU03_ACTION_ROUTE_NOTE.toLowerCase()).toContain('operational route')
    expect(CU03_ACTION_ROUTE_NOTE.toLowerCase()).toContain('not the customer owner')
  })

  it('describes Salesman routing as operational, not Customer ownership', () => {
    expect(CU03_ROUTED_SALESMAN_LABEL).toBe('Routed Salesman')
    expect(CU03_SALESMAN_WORKLOAD_LABEL).toBe(CU03_ROUTED_SALESMAN_LABEL)
    expect(CU03_SALESMAN_WORKLOAD_TYPE).toBe('Salesman')
    expect(isForbiddenCustomerOwnershipLabel(CU03_ROUTED_SALESMAN_LABEL)).toBe(false)
    expect(CU03_ROUTED_SALESMAN_NOTE.toLowerCase()).toContain('operational route')
    expect(CU03_ROUTED_SALESMAN_NOTE.toLowerCase()).toContain('not the customer owner')
  })

  it('keeps collection queues Customer-level and does not add Principal collection impact', () => {
    const text = CU03_ATTRIBUTION_DISCLOSURES.join(' ')
    expect(text.toLowerCase()).toContain('collection queues remain customer-level')
    expect(text.toLowerCase()).toContain('not customer ownership')
    expect(text.toLowerCase()).toContain('no principal collection impact is shown')
    expect(text.toLowerCase()).not.toContain('prn-')
    expect(CU03_ATTRIBUTION_DISCLOSURES.some((item) => /^owner$/i.test(item.trim()))).toBe(false)
  })
})

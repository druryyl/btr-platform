import { describe, expect, it } from 'vitest'
import {
  CU01_ATTRIBUTION_DISCLOSURES,
  CU01_LAST_INVOICING_SALESMAN_LABEL,
  CU01_LAST_INVOICING_SALESMAN_NOTE,
  isForbiddenCustomerOwnerLabel,
} from '@/services/customerAnalyticsAttribution'

describe('CU01 ownership labels', () => {
  it('labels the latest-Faktur Salesman as last invoicing recency, not Assigned Salesman or Owner', () => {
    expect(CU01_LAST_INVOICING_SALESMAN_LABEL).toBe('Last Invoicing Salesman')
    expect(CU01_LAST_INVOICING_SALESMAN_LABEL).not.toMatch(/assigned salesman/i)
    expect(isForbiddenCustomerOwnerLabel(CU01_LAST_INVOICING_SALESMAN_LABEL)).toBe(false)
    expect(isForbiddenCustomerOwnerLabel('Assigned Salesman')).toBe(true)
    expect(isForbiddenCustomerOwnerLabel('Owner')).toBe(true)
    expect(CU01_LAST_INVOICING_SALESMAN_NOTE.toLowerCase()).toContain('last invoicing salesman')
    expect(CU01_LAST_INVOICING_SALESMAN_NOTE.toLowerCase()).toContain('not the customer owner')
  })

  it('keeps credit, piutang, and sales at Customer level and shows Principal mix from the projection only', () => {
    const text = CU01_ATTRIBUTION_DISCLOSURES.join(' ')
    expect(text.toLowerCase()).toContain('customer-level')
    expect(text.toLowerCase()).toContain('credit')
    expect(text.toLowerCase()).toContain('piutang')
    expect(text.toLowerCase()).toContain('lifecycle')
    expect(text.toLowerCase()).toContain('not allocated to principals')
    expect(text.toLowerCase()).toContain('principal mix')
    expect(text.toLowerCase()).toContain('relationship projection only')
    expect(text.toLowerCase()).toContain('does not recompute relationships from raw transactions')
    expect(text.toLowerCase()).toContain('no pre-purchase assigned principal')
    expect(CU01_ATTRIBUTION_DISCLOSURES.some((item) => /assigned salesman/i.test(item) && !/does not label/i.test(item))).toBe(false)
  })
})

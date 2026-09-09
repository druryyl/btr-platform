import { describe, expect, it } from 'vitest'
import {
  CU01_ATTRIBUTION_DISCLOSURES,
  CU01_LAST_INVOICING_SALESMAN_LABEL,
  CU01_LAST_INVOICING_SALESMAN_NOTE,
  CU02_ATTRIBUTION_DISCLOSURES,
  CU02_LAST_INVOICING_SALESMAN_LABEL,
  CU02_LAST_INVOICING_SALESMAN_NOTE,
  CU02_LOW_RECOVERY_EXPLANATION,
  correctCu02AttributionText,
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

describe('CU02 ownership labels', () => {
  it('labels the latest current-month invoice Salesman as last invoicing recency, not the Customer owner', () => {
    expect(CU02_LAST_INVOICING_SALESMAN_LABEL).toBe('Last Invoicing Salesman')
    expect(CU02_LAST_INVOICING_SALESMAN_LABEL).not.toMatch(/assigned salesman/i)
    expect(isForbiddenCustomerOwnerLabel(CU02_LAST_INVOICING_SALESMAN_LABEL)).toBe(false)
    expect(CU02_LAST_INVOICING_SALESMAN_NOTE.toLowerCase()).toContain('last invoicing salesman')
    expect(CU02_LAST_INVOICING_SALESMAN_NOTE.toLowerCase()).toContain('not the customer owner')
    expect(CU02_LAST_INVOICING_SALESMAN_NOTE.toLowerCase()).toContain('recency indicator')
  })

  it('keeps risk measures Customer-level and does not present one Salesman as owner', () => {
    const text = CU02_ATTRIBUTION_DISCLOSURES.join(' ')
    expect(text.toLowerCase()).toContain('not the customer owner')
    expect(text.toLowerCase()).toContain('does not present')
    expect(text.toLowerCase()).toContain('customer-level')
    expect(text.toLowerCase()).toContain('principal-specific decline is not shown')
    expect(CU02_ATTRIBUTION_DISCLOSURES.some((item) => /assigned salesman/i.test(item) && !/does not present/i.test(item))).toBe(false)
    expect(CU02_LOW_RECOVERY_EXPLANATION.toLowerCase()).toContain('last invoicing salesman')
    expect(CU02_LOW_RECOVERY_EXPLANATION.toLowerCase()).toContain('invoice attribution')
    expect(CU02_LOW_RECOVERY_EXPLANATION.toLowerCase()).not.toContain('assigned salesman')
    expect(correctCu02AttributionText('Assigned salesman has low recovery vs billing and customer is overdue.')).toBe(
      CU02_LOW_RECOVERY_EXPLANATION,
    )
    expect(correctCu02AttributionText('Chronic overdue exposure with no recent payment.')).toBe(
      'Chronic overdue exposure with no recent payment.',
    )
  })
})

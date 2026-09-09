import { describe, expect, it } from 'vitest'
import {
  FI04_ATTRIBUTION_DISCLOSURES,
  FI04_INVOICE_ATTRIBUTED_SALESMAN_LABEL,
  FI04_INVOICE_ATTRIBUTED_SALESMAN_NOTE,
  isForbiddenAccountOwnerLabel,
} from '@/services/piutangInvoiceAttribution'

describe('FI04 invoice-attributed salesman labels', () => {
  it('describes the Salesman column as invoice-attributed, not account owner', () => {
    expect(FI04_INVOICE_ATTRIBUTED_SALESMAN_LABEL).toBe('Invoice-Attributed Salesman')
    expect(FI04_INVOICE_ATTRIBUTED_SALESMAN_LABEL.toLowerCase()).toContain('invoice-attributed')
    expect(FI04_INVOICE_ATTRIBUTED_SALESMAN_NOTE.toLowerCase()).toContain('invoice-attributed')
    expect(FI04_INVOICE_ATTRIBUTED_SALESMAN_NOTE.toLowerCase()).toContain('not the customer account owner')
    expect(isForbiddenAccountOwnerLabel(FI04_INVOICE_ATTRIBUTED_SALESMAN_LABEL)).toBe(false)
    expect(isForbiddenAccountOwnerLabel('Owner')).toBe(true)
    expect(isForbiddenAccountOwnerLabel('Account Owner')).toBe(true)
    expect(isForbiddenAccountOwnerLabel('Assigned Salesman')).toBe(true)
  })

  it('does not add a Principal financial column', () => {
    const text = FI04_ATTRIBUTION_DISCLOSURES.join(' ')
    expect(text.toLowerCase()).toContain('no principal financial column')
    expect(text.toLowerCase()).not.toContain('prn-')
  })
})

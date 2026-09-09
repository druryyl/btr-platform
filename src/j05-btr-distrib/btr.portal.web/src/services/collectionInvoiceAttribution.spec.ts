import { describe, expect, it } from 'vitest'
import {
  FI02_ATTRIBUTION_DISCLOSURES,
  FI02_INVOICE_ATTRIBUTED_SALESMAN_LABEL,
  FI02_INVOICE_ATTRIBUTED_SALESMAN_NOTE,
  FI02_TOP_OVERDUE_SALESMEN_HEADING,
  FI02_TOP_OVERDUE_SALESMEN_TITLE,
  isForbiddenAccountOwnerLabel,
} from '@/services/collectionInvoiceAttribution'

describe('FI02 invoice-attributed salesman labels', () => {
  it('describes the Top Overdue Salesmen ranking as invoice-attributed, not account owner', () => {
    expect(FI02_TOP_OVERDUE_SALESMEN_HEADING.toLowerCase()).toContain('invoice-attributed')
    expect(FI02_TOP_OVERDUE_SALESMEN_TITLE.toLowerCase()).toContain('invoice-attributed')
    expect(FI02_INVOICE_ATTRIBUTED_SALESMAN_LABEL).toBe('Invoice-Attributed Salesman')
    expect(FI02_INVOICE_ATTRIBUTED_SALESMAN_NOTE.toLowerCase()).toContain('invoice-attributed')
    expect(FI02_INVOICE_ATTRIBUTED_SALESMAN_NOTE.toLowerCase()).toContain('not an owned or assigned customer book')
    expect(isForbiddenAccountOwnerLabel(FI02_INVOICE_ATTRIBUTED_SALESMAN_LABEL)).toBe(false)
    expect(isForbiddenAccountOwnerLabel('Owner')).toBe(true)
    expect(isForbiddenAccountOwnerLabel('Top Overdue Salesmen')).toBe(true)
  })

  it('does not add a Principal overdue or collection KPI', () => {
    const text = FI02_ATTRIBUTION_DISCLOSURES.join(' ')
    expect(text.toLowerCase()).toContain('no principal overdue or collection kpi')
    expect(text.toLowerCase()).not.toContain('prn-')
  })
})

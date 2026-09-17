import { describe, expect, it } from 'vitest'
import {
  PU01_MTD_PURCHASE_IN_LABEL,
  PU01_PERCENT_OF_PURCHASE_IN_LABEL,
  PU01_PURCHASE_IN_DISCLOSURES,
  PU01_PURCHASE_IN_LABEL,
  PU01_TOP_PRINCIPALS_TITLE,
  PU01_WEEKLY_PURCHASE_IN_TREND_TITLE,
  PU02_EVIDENCE_NOTE,
  PU02_PURCHASE_IN_DISCLOSURES,
  PU02_PURCHASE_IN_LABEL,
  isPurchaseGrowthRenamedToSalesGrowth,
  isSalesOutPresentedAsPurchase,
} from '@/services/purchasingPurchaseInLabels'

describe('PU01 and PU02 Purchase-In labels', () => {
  it('identifies PU01 purchase measures as Purchase-In and does not present PRN-SALES-001 as purchase value', () => {
    const labels = [
      PU01_PURCHASE_IN_LABEL,
      PU01_MTD_PURCHASE_IN_LABEL,
      PU01_PERCENT_OF_PURCHASE_IN_LABEL,
      PU01_WEEKLY_PURCHASE_IN_TREND_TITLE,
      PU01_TOP_PRINCIPALS_TITLE,
    ]

    for (const label of labels) {
      expect(label.toLowerCase()).toContain('purchase-in')
      expect(isSalesOutPresentedAsPurchase(label)).toBe(false)
      expect(isPurchaseGrowthRenamedToSalesGrowth(label)).toBe(false)
    }

    expect(isSalesOutPresentedAsPurchase('PRN-SALES-001')).toBe(true)
    expect(isSalesOutPresentedAsPurchase('Principal Sales-Out')).toBe(true)
    expect(isPurchaseGrowthRenamedToSalesGrowth('Sales Growth')).toBe(true)
    expect(isPurchaseGrowthRenamedToSalesGrowth('Purchase growth')).toBe(false)
  })

  it('states that PU01 purchase amounts are not Principal Sales-Out and does not rename purchase growth to sales growth', () => {
    const text = PU01_PURCHASE_IN_DISCLOSURES.join(' ')
    expect(text.toLowerCase()).toContain('purchase-in')
    expect(text.toLowerCase()).toContain('not principal sales-out')
    expect(text.toLowerCase()).toContain('prn-sales-001')
    expect(text.toLowerCase()).toContain('does not display prn-sales-001 as purchase value')
    expect(text.toLowerCase()).toContain('purchase growth is not sales growth')
  })

  it('keeps PU02 as purchase-invoice evidence labeled Purchase-In', () => {
    expect(PU02_PURCHASE_IN_LABEL).toBe('Purchase-In')
    expect(PU02_EVIDENCE_NOTE.toLowerCase()).toContain('purchase-invoice evidence')
    expect(PU02_EVIDENCE_NOTE.toLowerCase()).toContain('not principal sales-out')

    const text = PU02_PURCHASE_IN_DISCLOSURES.join(' ')
    expect(text.toLowerCase()).toContain('purchase-invoice evidence')
    expect(text.toLowerCase()).toContain('purchase-in')
    expect(text.toLowerCase()).toContain('not principal sales-out')
    expect(text.toLowerCase()).toContain('does not display prn-sales-001')
  })
})

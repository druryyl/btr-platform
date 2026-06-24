import { describe, expect, it } from 'vitest'
import type { DashboardCustomerRiskForecastAttentionItem } from '@/models/dashboard'
import {
  categoryBadgeSeverity,
  countCustomerRiskForecastAttentionBySignalFamily,
  filterCustomerRiskForecastAttentionItems,
  resolveSignalFamilyKey,
} from '@/services/customerRiskForecastSignals'

function row(signalKey: string): DashboardCustomerRiskForecastAttentionItem {
  return {
    SortOrder: 1,
    CustomerCode: 'C1',
    CustomerName: 'Test',
    SignalKey: signalKey,
    SignalLabel: signalKey,
    Severity: 'Moderate',
    Amount: null,
    HorizonText: '30 days',
    RuleId: 'CRF-P01',
    Explanation: 'Test',
    ReportRoute: '/reports/piutang',
  }
}

describe('resolveSignalFamilyKey', () => {
  it('maps payment delay signals', () => {
    expect(resolveSignalFamilyKey('LikelyLatePayer')).toBe('PaymentDelay')
  })

  it('maps credit limit signals', () => {
    expect(resolveSignalFamilyKey('ApproachingPlafond')).toBe('CreditLimit')
  })

  it('returns null for unknown signals', () => {
    expect(resolveSignalFamilyKey('Unknown')).toBeNull()
  })
})

describe('categoryBadgeSeverity', () => {
  it('maps category keys and labels', () => {
    expect(categoryBadgeSeverity('Healthy')).toBe('success')
    expect(categoryBadgeSeverity('Watch')).toBe('info')
    expect(categoryBadgeSeverity('Attention')).toBe('warn')
    expect(categoryBadgeSeverity('HighRisk')).toBe('danger')
    expect(categoryBadgeSeverity('High Risk')).toBe('danger')
    expect(categoryBadgeSeverity('Critical')).toBe('danger')
  })
})

describe('filterCustomerRiskForecastAttentionItems', () => {
  const items = [row('LikelyLatePayer'), row('ApproachingPlafond'), row('LikelyLatePayer')]

  it('returns empty array for empty input', () => {
    expect(filterCustomerRiskForecastAttentionItems([], '')).toEqual([])
  })

  it('returns all items when filter is empty', () => {
    expect(filterCustomerRiskForecastAttentionItems(items, '')).toHaveLength(3)
  })

  it('filters by signal family', () => {
    expect(filterCustomerRiskForecastAttentionItems(items, 'PaymentDelay')).toHaveLength(2)
    expect(filterCustomerRiskForecastAttentionItems(items, 'CreditLimit')).toHaveLength(1)
  })
})

describe('countCustomerRiskForecastAttentionBySignalFamily', () => {
  it('counts rows per signal family', () => {
    const counts = countCustomerRiskForecastAttentionBySignalFamily([
      row('LikelyLatePayer'),
      row('LikelyLatePayer'),
      row('SevereDecline'),
    ])

    expect(counts.PaymentDelay).toBe(2)
    expect(counts.PurchaseDecline).toBe(1)
    expect(counts.CreditLimit).toBe(0)
  })
})

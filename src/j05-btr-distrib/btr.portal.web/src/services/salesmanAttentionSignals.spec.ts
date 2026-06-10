import { describe, expect, it } from 'vitest'
import type { DashboardSalesmanAttentionItem } from '@/models/dashboard'
import {
  countSalesmanAttentionBySignal,
  filterSalesmanAttentionItems,
} from '@/services/salesmanAttentionSignals'

function row(signalKey: string): DashboardSalesmanAttentionItem {
  return {
    SalesPersonId: '1',
    SalesPersonCode: 'S1',
    SalesPersonName: 'Test',
    SignalKey: signalKey,
    SignalLabel: signalKey,
    ValueAmount: null,
    ValueText: null,
    WilayahName: '',
    ReportRoute: '',
    RequiresAttention: true,
  }
}

describe('filterSalesmanAttentionItems', () => {
  const items = [row('BelowTarget'), row('NoTarget'), row('BelowTarget')]

  it('returns empty array for empty input', () => {
    expect(filterSalesmanAttentionItems([], '')).toEqual([])
  })

  it('returns all items when filter is empty', () => {
    expect(filterSalesmanAttentionItems(items, '')).toHaveLength(3)
  })

  it('filters by signal key', () => {
    expect(filterSalesmanAttentionItems(items, 'BelowTarget')).toHaveLength(2)
    expect(filterSalesmanAttentionItems(items, 'NoTarget')).toHaveLength(1)
  })
})

describe('countSalesmanAttentionBySignal', () => {
  it('counts rows per approved signal', () => {
    const counts = countSalesmanAttentionBySignal([
      row('BelowTarget'),
      row('BelowTarget'),
      row('HighOverdueExposure'),
    ])

    expect(counts.BelowTarget).toBe(2)
    expect(counts.HighOverdueExposure).toBe(1)
    expect(counts.NoTarget).toBe(0)
    expect(counts.DormantCustomerPortfolio).toBe(0)
  })
})

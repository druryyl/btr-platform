import { describe, expect, it } from 'vitest'
import type { DashboardSalesmanAttentionItem } from '@/models/dashboard'
import {
  countSalesmanAttentionBySignal,
  filterActiveSalesmen,
  filterSalesmanAttentionItems,
} from '@/services/salesmanAttentionSignals'

function row(signalKey: string, isActive = true): DashboardSalesmanAttentionItem {
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
    IsActive: isActive,
  }
}

describe('filterSalesmanAttentionItems', () => {
  const items = [row('BelowTarget'), row('MissingTargetSetup'), row('BelowTarget')]

  it('returns empty array for empty input', () => {
    expect(filterSalesmanAttentionItems([], '')).toEqual([])
  })

  it('returns all items when filter is empty', () => {
    expect(filterSalesmanAttentionItems(items, '')).toHaveLength(3)
  })

  it('filters by signal key', () => {
    expect(filterSalesmanAttentionItems(items, 'BelowTarget')).toHaveLength(2)
    expect(filterSalesmanAttentionItems(items, 'MissingTargetSetup')).toHaveLength(1)
  })
})

describe('filterActiveSalesmen', () => {
  it('hides inactive rows when toggle is off', () => {
    const rows = [row('BelowTarget', true), row('BelowTarget', false)]
    expect(filterActiveSalesmen(rows, false)).toHaveLength(1)
  })

  it('shows all rows when toggle is on', () => {
    const rows = [row('BelowTarget', true), row('BelowTarget', false)]
    expect(filterActiveSalesmen(rows, true)).toHaveLength(2)
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
    expect(counts.MissingTargetSetup).toBe(0)
    expect(counts.DormantCustomerPortfolio).toBe(0)
  })
})

import { describe, expect, it } from 'vitest'
import type { DashboardPurchasingAttentionItem } from '@/models/dashboard'
import {
  countPurchasingAttentionBySignal,
  filterPurchasingAttentionItems,
} from '@/services/purchasingAttentionSignals'

function row(signalKey: string): DashboardPurchasingAttentionItem {
  return {
    EntityType: 'Principal',
    EntityName: 'Test Principal',
    SignalKey: signalKey,
    SignalLabel: signalKey,
    ValueAmount: null,
    ValueText: null,
    ReportRoute: '/reports/purchasing',
  }
}

describe('filterPurchasingAttentionItems', () => {
  const items = [row('QualifiedBacklog'), row('CompoundDependency'), row('QualifiedBacklog')]

  it('returns empty array for empty input', () => {
    expect(filterPurchasingAttentionItems([], '')).toEqual([])
  })

  it('returns all items when filter is empty', () => {
    expect(filterPurchasingAttentionItems(items, '')).toHaveLength(3)
  })

  it('filters by signal key', () => {
    expect(filterPurchasingAttentionItems(items, 'QualifiedBacklog')).toHaveLength(2)
    expect(filterPurchasingAttentionItems(items, 'CompoundDependency')).toHaveLength(1)
  })
})

describe('countPurchasingAttentionBySignal', () => {
  it('counts rows per approved signal', () => {
    const counts = countPurchasingAttentionBySignal([
      row('QualifiedBacklog'),
      row('QualifiedBacklog'),
      row('PurchasingInactivity'),
    ])

    expect(counts.QualifiedBacklog).toBe(2)
    expect(counts.PurchasingInactivity).toBe(1)
    expect(counts.CompoundDependency).toBe(0)
    expect(counts.UnknownPrincipal).toBe(0)
  })
})

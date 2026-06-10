import { describe, expect, it } from 'vitest'
import type { DashboardCollectionAttentionItem } from '@/models/dashboard'
import {
  countCollectionAttentionBySignal,
  filterCollectionAttentionItems,
} from '@/services/collectionAttentionSignals'

function row(signalKey: string): DashboardCollectionAttentionItem {
  return {
    EntityType: 'Customer',
    EntityCode: 'C1',
    EntityName: 'Test',
    SignalKey: signalKey,
    SignalLabel: signalKey,
    ValueAmount: null,
    ValueText: null,
    WilayahName: '',
    ReportRoute: '/reports/piutang',
  }
}

describe('filterCollectionAttentionItems', () => {
  const items = [row('ChronicOverdue'), row('LegacyDebt'), row('ChronicOverdue')]

  it('returns empty array for empty input', () => {
    expect(filterCollectionAttentionItems([], '')).toEqual([])
  })

  it('returns all items when filter is empty', () => {
    expect(filterCollectionAttentionItems(items, '')).toHaveLength(3)
  })

  it('filters by signal key', () => {
    expect(filterCollectionAttentionItems(items, 'ChronicOverdue')).toHaveLength(2)
    expect(filterCollectionAttentionItems(items, 'LegacyDebt')).toHaveLength(1)
  })
})

describe('countCollectionAttentionBySignal', () => {
  it('counts rows per approved signal', () => {
    const counts = countCollectionAttentionBySignal([
      row('ChronicOverdue'),
      row('ChronicOverdue'),
      row('LowRecoveryVsBilling'),
    ])

    expect(counts.ChronicOverdue).toBe(2)
    expect(counts.LowRecoveryVsBilling).toBe(1)
    expect(counts.LegacyDebt).toBe(0)
    expect(counts.WilayahHotspot).toBe(0)
  })
})

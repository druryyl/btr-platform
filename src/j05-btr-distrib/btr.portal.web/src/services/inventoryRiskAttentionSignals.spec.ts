import { describe, expect, it } from 'vitest'
import type { DashboardInventoryRiskAttentionItem } from '@/models/dashboard'
import {
  countInventoryRiskAttentionBySignal,
  filterInventoryRiskAttentionItems,
} from '@/services/inventoryRiskAttentionSignals'

function row(signalKey: string): DashboardInventoryRiskAttentionItem {
  return {
    BrgCode: 'B1',
    BrgName: 'Test Item',
    KategoriName: 'Cat',
    SupplierName: 'Sup',
    Qty: 1,
    InventoryValue: 100,
    DaysSinceLastFaktur: 90,
    SignalKey: signalKey,
    SignalLabel: signalKey,
    ReportRoute: '',
  }
}

describe('filterInventoryRiskAttentionItems', () => {
  const items = [row('DeadStock'), row('SlowMoving'), row('DeadStock')]

  it('returns empty array for empty input', () => {
    expect(filterInventoryRiskAttentionItems([], '')).toEqual([])
  })

  it('returns all items when filter is empty', () => {
    expect(filterInventoryRiskAttentionItems(items, '')).toHaveLength(3)
  })

  it('filters by signal key', () => {
    expect(filterInventoryRiskAttentionItems(items, 'DeadStock')).toHaveLength(2)
    expect(filterInventoryRiskAttentionItems(items, 'SlowMoving')).toHaveLength(1)
  })
})

describe('countInventoryRiskAttentionBySignal', () => {
  it('counts rows per approved signal', () => {
    const counts = countInventoryRiskAttentionBySignal([
      row('DeadStock'),
      row('DeadStock'),
      row('SlowMoving'),
    ])

    expect(counts.DeadStock).toBe(2)
    expect(counts.SlowMoving).toBe(1)
  })
})

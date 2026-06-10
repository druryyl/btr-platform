import { describe, expect, it } from 'vitest'
import type { DashboardCustomerAttentionItem } from '@/models/dashboard'
import {
  countCustomerAttentionBySignal,
  filterCustomerAttentionItems,
} from '@/services/customerAttentionSignals'

function row(signalKey: string): DashboardCustomerAttentionItem {
  return {
    CustomerCode: 'C1',
    CustomerName: 'Test',
    SignalKey: signalKey,
    SignalLabel: signalKey,
    ValueAmount: null,
    ValueText: null,
    WilayahName: '',
    ReportRoute: '',
    RequiresAttention: true,
  }
}

describe('filterCustomerAttentionItems', () => {
  const items = [row('Overdue'), row('Dormant'), row('Overdue')]

  it('returns empty array for empty input', () => {
    expect(filterCustomerAttentionItems([], '')).toEqual([])
  })

  it('returns all items when filter is empty', () => {
    expect(filterCustomerAttentionItems(items, '')).toHaveLength(3)
  })

  it('filters by signal key', () => {
    expect(filterCustomerAttentionItems(items, 'Overdue')).toHaveLength(2)
    expect(filterCustomerAttentionItems(items, 'Dormant')).toHaveLength(1)
  })
})

describe('countCustomerAttentionBySignal', () => {
  it('counts rows per approved signal', () => {
    const counts = countCustomerAttentionBySignal([
      row('Overdue'),
      row('Overdue'),
      row('PlafondBreach'),
    ])

    expect(counts.Overdue).toBe(2)
    expect(counts.PlafondBreach).toBe(1)
    expect(counts.Dormant).toBe(0)
    expect(counts.SuspendedWithSales).toBe(0)
  })
})

import type { DashboardCollectionAttentionItem } from '@/models/dashboard'

export const COLLECTION_ATTENTION_SIGNAL_ALL = ''

export const COLLECTION_ATTENTION_SIGNAL_KEYS = [
  'ChronicOverdue',
  'PlafondBreachOverdue',
  'LegacyDebt',
  'Overdue',
  'HighOverdueWorkload',
  'LowRecoveryVsBilling',
  'WilayahHotspot',
] as const

export type CollectionAttentionSignalKey = (typeof COLLECTION_ATTENTION_SIGNAL_KEYS)[number]

export const COLLECTION_ATTENTION_SIGNAL_LABELS: Record<CollectionAttentionSignalKey, string> = {
  ChronicOverdue: 'Chronic Overdue',
  PlafondBreachOverdue: 'Plafond Breach + Overdue',
  LegacyDebt: 'Legacy Debt',
  Overdue: 'Overdue',
  HighOverdueWorkload: 'High Overdue Workload',
  LowRecoveryVsBilling: 'Low Recovery vs Billing',
  WilayahHotspot: 'Wilayah Hotspot',
}

export function filterCollectionAttentionItems(
  items: DashboardCollectionAttentionItem[],
  signalKey: string,
): DashboardCollectionAttentionItem[] {
  if (!signalKey) {
    return items
  }

  return items.filter((item) => item.SignalKey === signalKey)
}

export function countCollectionAttentionBySignal(
  items: DashboardCollectionAttentionItem[],
): Record<CollectionAttentionSignalKey, number> {
  const counts: Record<CollectionAttentionSignalKey, number> = {
    ChronicOverdue: 0,
    PlafondBreachOverdue: 0,
    LegacyDebt: 0,
    Overdue: 0,
    HighOverdueWorkload: 0,
    LowRecoveryVsBilling: 0,
    WilayahHotspot: 0,
  }

  for (const item of items) {
    if (item.SignalKey in counts) {
      counts[item.SignalKey as CollectionAttentionSignalKey]++
    }
  }

  return counts
}

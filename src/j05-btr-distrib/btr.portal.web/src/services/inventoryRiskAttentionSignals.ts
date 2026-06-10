import type { DashboardInventoryRiskAttentionItem } from '@/models/dashboard'

export const INVENTORY_RISK_ATTENTION_SIGNAL_ALL = ''

export const INVENTORY_RISK_ATTENTION_SIGNAL_KEYS = ['DeadStock', 'SlowMoving'] as const

export type InventoryRiskAttentionSignalKey = (typeof INVENTORY_RISK_ATTENTION_SIGNAL_KEYS)[number]

export const INVENTORY_RISK_ATTENTION_SIGNAL_LABELS: Record<InventoryRiskAttentionSignalKey, string> = {
  DeadStock: 'Dead Stock',
  SlowMoving: 'Slow Moving',
}

export function filterInventoryRiskAttentionItems(
  items: DashboardInventoryRiskAttentionItem[],
  signalKey: string,
): DashboardInventoryRiskAttentionItem[] {
  if (!signalKey) {
    return items
  }

  return items.filter((item) => item.SignalKey === signalKey)
}

export function countInventoryRiskAttentionBySignal(
  items: DashboardInventoryRiskAttentionItem[],
): Record<InventoryRiskAttentionSignalKey, number> {
  const counts: Record<InventoryRiskAttentionSignalKey, number> = {
    DeadStock: 0,
    SlowMoving: 0,
  }

  for (const item of items) {
    if (item.SignalKey in counts) {
      counts[item.SignalKey as InventoryRiskAttentionSignalKey]++
    }
  }

  return counts
}

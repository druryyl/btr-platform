import type { DashboardPurchasingAttentionItem } from '@/models/dashboard'

export const PURCHASING_ATTENTION_SIGNAL_ALL = ''

export const PURCHASING_ATTENTION_SIGNAL_KEYS = [
  'QualifiedBacklog',
  'CompoundDependency',
  'PrincipalInventoryNoPurchase',
  'UnknownPrincipal',
  'PrincipalAtRiskExposure',
  'PrincipalSpendConcentration',
  'PrincipalInventoryConcentration',
  'PurchasingInactivity',
] as const

export type PurchasingAttentionSignalKey = (typeof PURCHASING_ATTENTION_SIGNAL_KEYS)[number]

export const PURCHASING_ATTENTION_SIGNAL_LABELS: Record<PurchasingAttentionSignalKey, string> = {
  QualifiedBacklog: 'Qualified Backlog',
  CompoundDependency: 'Compound Dependency',
  PrincipalInventoryNoPurchase: 'Inventory, No Purchase',
  UnknownPrincipal: 'Unknown Principal',
  PrincipalAtRiskExposure: 'At-Risk Exposure',
  PrincipalSpendConcentration: 'Spend Concentration',
  PrincipalInventoryConcentration: 'Inventory Concentration',
  PurchasingInactivity: 'Purchasing Inactivity',
}

export function filterPurchasingAttentionItems(
  items: DashboardPurchasingAttentionItem[],
  signalKey: string,
): DashboardPurchasingAttentionItem[] {
  if (!signalKey) {
    return items
  }

  return items.filter((item) => item.SignalKey === signalKey)
}

export function countPurchasingAttentionBySignal(
  items: DashboardPurchasingAttentionItem[],
): Record<PurchasingAttentionSignalKey, number> {
  const counts: Record<PurchasingAttentionSignalKey, number> = {
    QualifiedBacklog: 0,
    CompoundDependency: 0,
    PrincipalInventoryNoPurchase: 0,
    UnknownPrincipal: 0,
    PrincipalAtRiskExposure: 0,
    PrincipalSpendConcentration: 0,
    PrincipalInventoryConcentration: 0,
    PurchasingInactivity: 0,
  }

  for (const item of items) {
    if (item.SignalKey in counts) {
      counts[item.SignalKey as PurchasingAttentionSignalKey]++
    }
  }

  return counts
}

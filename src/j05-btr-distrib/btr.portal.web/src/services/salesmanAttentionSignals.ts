import type { DashboardSalesmanAttentionItem } from '@/models/dashboard'

export const SALESMAN_ATTENTION_SIGNAL_ALL = ''

export const SALESMAN_ATTENTION_SIGNAL_KEYS = [
  'BelowTarget',
  'NoTarget',
  'HighOverdueExposure',
  'HighPiutangExposure',
  'CustomerConcentration',
  'DormantCustomerPortfolio',
] as const

export type SalesmanAttentionSignalKey = (typeof SALESMAN_ATTENTION_SIGNAL_KEYS)[number]

export const SALESMAN_ATTENTION_SIGNAL_LABELS: Record<SalesmanAttentionSignalKey, string> = {
  BelowTarget: 'Below Target',
  NoTarget: 'No Target',
  HighOverdueExposure: 'High Overdue Exposure',
  HighPiutangExposure: 'High Piutang Exposure',
  CustomerConcentration: 'Customer Concentration',
  DormantCustomerPortfolio: 'Dormant Customer Portfolio',
}

export function filterSalesmanAttentionItems(
  items: DashboardSalesmanAttentionItem[],
  signalKey: string,
): DashboardSalesmanAttentionItem[] {
  if (!signalKey) {
    return items
  }

  return items.filter((item) => item.SignalKey === signalKey)
}

export function countSalesmanAttentionBySignal(
  items: DashboardSalesmanAttentionItem[],
): Record<SalesmanAttentionSignalKey, number> {
  const counts: Record<SalesmanAttentionSignalKey, number> = {
    BelowTarget: 0,
    NoTarget: 0,
    HighOverdueExposure: 0,
    HighPiutangExposure: 0,
    CustomerConcentration: 0,
    DormantCustomerPortfolio: 0,
  }

  for (const item of items) {
    if (item.SignalKey in counts) {
      counts[item.SignalKey as SalesmanAttentionSignalKey]++
    }
  }

  return counts
}

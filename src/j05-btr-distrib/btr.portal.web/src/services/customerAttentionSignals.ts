import type { DashboardCustomerAttentionItem } from '@/models/dashboard'

export const CUSTOMER_ATTENTION_SIGNAL_ALL = ''

export const CUSTOMER_ATTENTION_SIGNAL_KEYS = [
  'Overdue',
  'Dormant',
  'PlafondBreach',
  'SuspendedWithSales',
] as const

export type CustomerAttentionSignalKey = (typeof CUSTOMER_ATTENTION_SIGNAL_KEYS)[number]

export const CUSTOMER_ATTENTION_SIGNAL_LABELS: Record<CustomerAttentionSignalKey, string> = {
  Overdue: 'Overdue',
  Dormant: 'Dormant',
  PlafondBreach: 'Plafond Breach',
  SuspendedWithSales: 'Suspended + Sales',
}

export function filterCustomerAttentionItems(
  items: DashboardCustomerAttentionItem[],
  signalKey: string,
): DashboardCustomerAttentionItem[] {
  if (!signalKey) {
    return items
  }

  return items.filter((item) => item.SignalKey === signalKey)
}

export function countCustomerAttentionBySignal(
  items: DashboardCustomerAttentionItem[],
): Record<CustomerAttentionSignalKey, number> {
  const counts: Record<CustomerAttentionSignalKey, number> = {
    Overdue: 0,
    Dormant: 0,
    PlafondBreach: 0,
    SuspendedWithSales: 0,
  }

  for (const item of items) {
    if (item.SignalKey in counts) {
      counts[item.SignalKey as CustomerAttentionSignalKey]++
    }
  }

  return counts
}

import type { DashboardCustomerRiskForecastAttentionItem } from '@/models/dashboard'

export const CUSTOMER_RISK_FORECAST_SIGNAL_ALL = ''

export const CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_KEYS = [
  'PaymentDelay',
  'CreditLimit',
  'Inactivity',
  'PurchaseDecline',
  'CollectionRisk',
] as const

export type CustomerRiskForecastSignalFamilyKey =
  (typeof CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_KEYS)[number]

export const CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_LABELS: Record<
  CustomerRiskForecastSignalFamilyKey,
  string
> = {
  PaymentDelay: 'Payment Delay',
  CreditLimit: 'Credit Limit',
  Inactivity: 'Inactivity',
  PurchaseDecline: 'Purchase Decline',
  CollectionRisk: 'Collection Risk',
}

export const CUSTOMER_RISK_FORECAST_CATEGORY_KEYS = [
  'Healthy',
  'Watch',
  'Attention',
  'HighRisk',
  'Critical',
] as const

export type CustomerRiskForecastCategoryKey =
  (typeof CUSTOMER_RISK_FORECAST_CATEGORY_KEYS)[number]

export const CUSTOMER_RISK_FORECAST_CATEGORY_LABELS: Record<
  CustomerRiskForecastCategoryKey,
  string
> = {
  Healthy: 'Healthy',
  Watch: 'Watch',
  Attention: 'Attention',
  HighRisk: 'High Risk',
  Critical: 'Critical',
}

export const CUSTOMER_RISK_FORECAST_CATEGORY_CHART_COLORS: Record<
  CustomerRiskForecastCategoryKey,
  string
> = {
  Healthy: '#22c55e',
  Watch: '#0ea5e9',
  Attention: '#f59e0b',
  HighRisk: '#ef4444',
  Critical: '#991b1b',
}

export const CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_CHART_COLORS: Record<
  CustomerRiskForecastSignalFamilyKey,
  string
> = {
  PaymentDelay: '#6366f1',
  CreditLimit: '#8b5cf6',
  Inactivity: '#64748b',
  PurchaseDecline: '#f97316',
  CollectionRisk: '#ef4444',
}

const PAYMENT_DELAY_SIGNALS = new Set([
  'LikelyLatePayer',
  'EscalatingOverdue',
  'NoRecentPayment',
  'DueSoonSlowPayer',
])

const CREDIT_LIMIT_SIGNALS = new Set([
  'ProjectedPlafondBreach',
  'ApproachingPlafond',
  'BreachedWorsening',
])

const INACTIVITY_SIGNALS = new Set([
  'ApproachingDormant',
  'ImminentDormant',
  'LegacyDebtForward',
])

const PURCHASE_DECLINE_SIGNALS = new Set([
  'ModerateDecline',
  'SevereDecline',
  'StoppedAfterHistory',
])

const COLLECTION_RISK_SIGNALS = new Set([
  'DueExposureConcentration',
  'ChronicTrajectory',
  'LowRecoveryCustomer',
  'HighCollectionRisk',
])

export type CustomerRiskCategoryBadgeSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary'

export function categoryBadgeSeverity(category: string): CustomerRiskCategoryBadgeSeverity {
  const normalized = category.replace(/\s+/g, '')

  if (normalized === 'Healthy') return 'success'
  if (normalized === 'Watch') return 'info'
  if (normalized === 'Attention') return 'warn'
  if (normalized === 'HighRisk' || normalized === 'Critical') return 'danger'

  return 'secondary'
}

export function resolveSignalFamilyKey(
  signalKey: string,
): CustomerRiskForecastSignalFamilyKey | null {
  if (PAYMENT_DELAY_SIGNALS.has(signalKey)) return 'PaymentDelay'
  if (CREDIT_LIMIT_SIGNALS.has(signalKey)) return 'CreditLimit'
  if (INACTIVITY_SIGNALS.has(signalKey)) return 'Inactivity'
  if (PURCHASE_DECLINE_SIGNALS.has(signalKey)) return 'PurchaseDecline'
  if (COLLECTION_RISK_SIGNALS.has(signalKey)) return 'CollectionRisk'

  return null
}

export function filterCustomerRiskForecastAttentionItems(
  items: DashboardCustomerRiskForecastAttentionItem[],
  signalFamilyKey: string,
): DashboardCustomerRiskForecastAttentionItem[] {
  if (!signalFamilyKey) {
    return items
  }

  return items.filter((item) => resolveSignalFamilyKey(item.SignalKey) === signalFamilyKey)
}

export function countCustomerRiskForecastAttentionBySignalFamily(
  items: DashboardCustomerRiskForecastAttentionItem[],
): Record<CustomerRiskForecastSignalFamilyKey, number> {
  const counts: Record<CustomerRiskForecastSignalFamilyKey, number> = {
    PaymentDelay: 0,
    CreditLimit: 0,
    Inactivity: 0,
    PurchaseDecline: 0,
    CollectionRisk: 0,
  }

  for (const item of items) {
    const family = resolveSignalFamilyKey(item.SignalKey)
    if (family) {
      counts[family]++
    }
  }

  return counts
}

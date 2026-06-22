import type {
  DashboardCustomerPortfolioCustomerRow,
  DashboardCustomerPortfolioPriorityRow,
} from '@/models/dashboard'

export const CUSTOMER_PORTFOLIO_VIEW_ALL = 'all'
export const CUSTOMER_PORTFOLIO_VIEW_ATTENTION = 'attention'

export const CUSTOMER_PORTFOLIO_CONCENTRATION_OMZET = 'Omzet'
export const CUSTOMER_PORTFOLIO_CONCENTRATION_PIUTANG = 'Piutang'

export const CUSTOMER_PORTFOLIO_LIFECYCLE_KEYS = [
  'NeverPurchased',
  'Dormant',
  'New',
  'Declining',
  'Growing',
  'Mature',
] as const

export const CUSTOMER_PORTFOLIO_TIER_KEYS = [
  'Strategic',
  'HighValue',
  'MediumValue',
  'LowValue',
] as const

export const CUSTOMER_PORTFOLIO_ACTION_KEYS = [
  'Grow',
  'Retain',
  'Protect',
  'Collect',
  'ReviewCredit',
  'Recover',
  'Monitor',
  'ExitReview',
] as const

export type CustomerPortfolioLifecycleKey = (typeof CUSTOMER_PORTFOLIO_LIFECYCLE_KEYS)[number]
export type CustomerPortfolioTierKey = (typeof CUSTOMER_PORTFOLIO_TIER_KEYS)[number]
export type CustomerPortfolioActionKey = (typeof CUSTOMER_PORTFOLIO_ACTION_KEYS)[number]

export const CUSTOMER_PORTFOLIO_LIFECYCLE_LABELS: Record<CustomerPortfolioLifecycleKey, string> = {
  NeverPurchased: 'Never Purchased',
  Dormant: 'Dormant',
  New: 'New',
  Declining: 'Declining',
  Growing: 'Growing',
  Mature: 'Mature',
}

export const CUSTOMER_PORTFOLIO_TIER_LABELS: Record<CustomerPortfolioTierKey, string> = {
  Strategic: 'Strategic',
  HighValue: 'High Value',
  MediumValue: 'Medium Value',
  LowValue: 'Low Value',
}

export const CUSTOMER_PORTFOLIO_ACTION_LABELS: Record<CustomerPortfolioActionKey, string> = {
  Grow: 'Grow',
  Retain: 'Retain',
  Protect: 'Protect',
  Collect: 'Collect',
  ReviewCredit: 'Review Credit',
  Recover: 'Recover',
  Monitor: 'Monitor',
  ExitReview: 'Exit Review',
}

export const CUSTOMER_PORTFOLIO_ACTION_OWNER_LABELS: Record<string, string> = {
  Sales: 'Sales',
  Finance: 'Finance',
  Management: 'Management',
  'Management + Sales': 'Management + Sales',
}

export const CUSTOMER_PORTFOLIO_LIFECYCLE_CHART_COLORS: Record<CustomerPortfolioLifecycleKey, string> = {
  NeverPurchased: '#64748b',
  Dormant: '#94a3b8',
  New: '#22c55e',
  Declining: '#ef4444',
  Growing: '#0ea5e9',
  Mature: '#6366f1',
}

export const CUSTOMER_PORTFOLIO_TIER_CHART_COLORS: Record<CustomerPortfolioTierKey, string> = {
  Strategic: '#7c3aed',
  HighValue: '#2563eb',
  MediumValue: '#0ea5e9',
  LowValue: '#94a3b8',
}

export type CustomerPortfolioBadgeSeverity = 'success' | 'info' | 'warn' | 'danger' | 'secondary'

export function actionBadgeSeverity(actionKey: string): CustomerPortfolioBadgeSeverity {
  switch (actionKey) {
    case 'Collect':
    case 'ExitReview':
      return 'danger'
    case 'ReviewCredit':
    case 'Protect':
      return 'warn'
    case 'Retain':
    case 'Recover':
      return 'info'
    case 'Grow':
      return 'success'
    default:
      return 'secondary'
  }
}

export function actionOwnerBadgeSeverity(owner: string): CustomerPortfolioBadgeSeverity {
  if (owner === 'Sales') return 'info'
  if (owner === 'Finance') return 'warn'
  if (owner === 'Management' || owner === 'Management + Sales') return 'danger'
  return 'secondary'
}

export function buildM30CollectLink(customerKey: string | null | undefined): string {
  if (!customerKey?.trim()) {
    return '/dashboard/collection-optimization'
  }

  return `/dashboard/collection-optimization?customerKey=${encodeURIComponent(customerKey.trim())}`
}

export function buildCustomerReportLink(customerCode: string | null | undefined): string {
  if (!customerCode?.trim()) {
    return '/reports/customers'
  }

  return `/reports/customers?customerCode=${encodeURIComponent(customerCode.trim())}`
}

type PortfolioFilterableRow = Pick<
  DashboardCustomerPortfolioCustomerRow,
  | 'IsAttention'
  | 'WilayahName'
  | 'Klasifikasi'
  | 'PortfolioTier'
  | 'LifecycleStage'
  | 'PrimaryActionKey'
  | 'SalesPersonName'
>

export interface CustomerPortfolioFilterState {
  view: typeof CUSTOMER_PORTFOLIO_VIEW_ALL | typeof CUSTOMER_PORTFOLIO_VIEW_ATTENTION
  wilayah: string
  klasifikasi: string
  tier: string
  lifecycle: string
  action: string
  salesman: string
}

export function createDefaultPortfolioFilters(): CustomerPortfolioFilterState {
  return {
    view: CUSTOMER_PORTFOLIO_VIEW_ATTENTION,
    wilayah: '',
    klasifikasi: '',
    tier: '',
    lifecycle: '',
    action: '',
    salesman: '',
  }
}

export function filterPortfolioCustomers<T extends PortfolioFilterableRow>(
  rows: T[],
  filters: CustomerPortfolioFilterState,
): T[] {
  return rows.filter((row) => {
    if (filters.view === CUSTOMER_PORTFOLIO_VIEW_ATTENTION && !row.IsAttention) {
      return false
    }

    if (filters.wilayah && row.WilayahName !== filters.wilayah) {
      return false
    }

    if (filters.klasifikasi && row.Klasifikasi !== filters.klasifikasi) {
      return false
    }

    if (filters.tier && row.PortfolioTier !== filters.tier) {
      return false
    }

    if (filters.lifecycle && row.LifecycleStage !== filters.lifecycle) {
      return false
    }

    if (filters.action && row.PrimaryActionKey !== filters.action) {
      return false
    }

    if (filters.salesman && row.SalesPersonName !== filters.salesman) {
      return false
    }

    return true
  })
}

export function groupPriorityRowsByAction(
  rows: DashboardCustomerPortfolioPriorityRow[],
): Record<string, DashboardCustomerPortfolioPriorityRow[]> {
  const grouped: Record<string, DashboardCustomerPortfolioPriorityRow[]> = {}

  for (const row of rows) {
    const key = row.PrimaryActionKey || 'Monitor'
    grouped[key] = grouped[key] ?? []
    grouped[key].push(row)
  }

  return grouped
}

export function collectDistinctFilterValues<T extends PortfolioFilterableRow>(
  rows: T[],
): {
  wilayah: string[]
  klasifikasi: string[]
  tier: string[]
  lifecycle: string[]
  action: string[]
  salesman: string[]
} {
  const wilayah = new Set<string>()
  const klasifikasi = new Set<string>()
  const tier = new Set<string>()
  const lifecycle = new Set<string>()
  const action = new Set<string>()
  const salesman = new Set<string>()

  for (const row of rows) {
    if (row.WilayahName) wilayah.add(row.WilayahName)
    if (row.Klasifikasi) klasifikasi.add(row.Klasifikasi)
    if (row.PortfolioTier) tier.add(row.PortfolioTier)
    if (row.LifecycleStage) lifecycle.add(row.LifecycleStage)
    if (row.PrimaryActionKey) action.add(row.PrimaryActionKey)
    if (row.SalesPersonName) salesman.add(row.SalesPersonName)
  }

  const sortValues = (values: Set<string>) =>
    [...values].sort((a, b) => a.localeCompare(b, 'id-ID'))

  return {
    wilayah: sortValues(wilayah),
    klasifikasi: sortValues(klasifikasi),
    tier: sortValues(tier),
    lifecycle: sortValues(lifecycle),
    action: sortValues(action),
    salesman: sortValues(salesman),
  }
}

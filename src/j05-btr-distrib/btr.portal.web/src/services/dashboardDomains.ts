export type DashboardDomain =
  | 'sales'
  | 'finance'
  | 'collection'
  | 'inventory'
  | 'purchasing'
  | 'customer'
  | 'salesman'
  | 'alert'
  | 'portfolio'

export interface DashboardDomainMeta {
  label: string
  icon: string
}

export const DASHBOARD_DOMAIN_META: Record<DashboardDomain, DashboardDomainMeta> = {
  sales: { label: 'Sales', icon: 'pi pi-chart-line' },
  finance: { label: 'Finance', icon: 'pi pi-wallet' },
  collection: { label: 'Collection', icon: 'pi pi-money-bill' },
  inventory: { label: 'Inventory', icon: 'pi pi-box' },
  purchasing: { label: 'Purchasing', icon: 'pi pi-shopping-cart' },
  customer: { label: 'Customer', icon: 'pi pi-users' },
  salesman: { label: 'Salesman', icon: 'pi pi-id-card' },
  alert: { label: 'Alert', icon: 'pi pi-exclamation-triangle' },
  portfolio: { label: 'Customer Portfolio', icon: 'pi pi-heart' },
}

/** Maps executive/API domain names to dashboard domain keys. */
export function resolveDashboardDomain(domain: string): DashboardDomain {
  const normalized = domain.trim().toLowerCase()

  switch (normalized) {
    case 'sales':
      return 'sales'
    case 'piutang':
    case 'finance':
      return 'finance'
    case 'collection':
      return 'collection'
    case 'inventory':
      return 'inventory'
    case 'purchasing':
      return 'purchasing'
    case 'customer':
    case 'customers':
      return 'customer'
    case 'salesman':
    case 'salesmen':
      return 'salesman'
    case 'portfolio':
    case 'customer portfolio':
      return 'portfolio'
    default:
      return 'alert'
  }
}

import { PortalMenuCodes } from './portalMenuCodes'
import type { PortalMenuGroup, PortalMenuGroupId, PortalMenuItem } from './portalMenu.types'

type PortalMenuItemInput = Omit<PortalMenuItem, 'isDashboard'> & { isDashboard?: boolean }

function item(entry: PortalMenuItemInput): PortalMenuItem {
  return {
    ...entry,
    isDashboard: entry.isDashboard ?? true,
  }
}

const portalMenuItems: PortalMenuItem[] = [
  item({
    code: PortalMenuCodes.EX01,
    label: 'Executive',
    icon: 'pi pi-th-large',
    routeName: 'dashboard',
    route: '/dashboard',
    order: 1,
    groupId: 'executive',
  }),
  item({
    code: PortalMenuCodes.EX02,
    label: 'Alert Center',
    icon: 'pi pi-bell',
    routeName: 'alert-center',
    route: '/alerts',
    order: 2,
    groupId: 'executive',
  }),
  item({
    code: PortalMenuCodes.EX03,
    label: 'Entity Analytics',
    icon: 'pi pi-id-card',
    routeName: 'entity-analytics-home',
    route: '/analytics',
    order: 3,
    groupId: 'executive',
    isDashboard: false,
  }),
  item({
    code: PortalMenuCodes.SA01,
    label: 'Sales',
    icon: 'pi pi-chart-line',
    routeName: 'sales-dashboard',
    route: '/dashboard/sales',
    order: 1,
    groupId: 'sales',
  }),
  item({
    code: PortalMenuCodes.SA02,
    label: 'Sales Forecast',
    icon: 'pi pi-chart-bar',
    routeName: 'sales-forecast-dashboard',
    route: '/dashboard/sales-forecast',
    order: 2,
    groupId: 'sales',
  }),
  item({
    code: PortalMenuCodes.SA03,
    label: 'Sales Report',
    icon: 'pi pi-list',
    routeName: 'sales-report',
    route: '/reports/sales',
    order: 3,
    groupId: 'sales',
    isDashboard: false,
  }),
  item({
    code: PortalMenuCodes.CU01,
    label: 'Customers',
    icon: 'pi pi-users',
    routeName: 'customers-dashboard',
    route: '/dashboard/customers',
    order: 1,
    groupId: 'customers',
  }),
  item({
    code: PortalMenuCodes.CU02,
    label: 'Customer Risk Forecast',
    icon: 'pi pi-shield',
    routeName: 'customer-risk-forecast-dashboard',
    route: '/dashboard/customer-risk-forecast',
    order: 2,
    groupId: 'customers',
  }),
  item({
    code: PortalMenuCodes.CU03,
    label: 'Collection Optimization',
    icon: 'pi pi-sliders-h',
    routeName: 'collection-optimization-dashboard',
    route: '/dashboard/collection-optimization',
    order: 3,
    groupId: 'customers',
  }),
  item({
    code: PortalMenuCodes.CU04,
    label: 'Customer Portfolio',
    icon: 'pi pi-briefcase',
    routeName: 'customer-portfolio-dashboard',
    route: '/dashboard/customer-portfolio',
    order: 4,
    groupId: 'customers',
  }),
  item({
    code: PortalMenuCodes.CU05,
    label: 'Customer Report',
    icon: 'pi pi-users',
    routeName: 'customer-report',
    route: '/reports/customers',
    order: 5,
    groupId: 'customers',
    isDashboard: false,
  }),
  item({
    code: PortalMenuCodes.FI01,
    label: 'Piutang',
    icon: 'pi pi-wallet',
    routeName: 'piutang-dashboard',
    route: '/dashboard/piutang',
    order: 1,
    groupId: 'finance',
  }),
  item({
    code: PortalMenuCodes.FI02,
    label: 'Collection',
    icon: 'pi pi-money-bill',
    routeName: 'collection-dashboard',
    route: '/dashboard/collection',
    order: 2,
    groupId: 'finance',
  }),
  item({
    code: PortalMenuCodes.FI03,
    label: 'Cash Flow Forecast',
    icon: 'pi pi-chart-line',
    routeName: 'cash-flow-forecast-dashboard',
    route: '/dashboard/cash-flow-forecast',
    order: 3,
    groupId: 'finance',
  }),
  item({
    code: PortalMenuCodes.FI04,
    label: 'Piutang Report',
    icon: 'pi pi-wallet',
    routeName: 'piutang-report',
    route: '/reports/piutang',
    order: 4,
    groupId: 'finance',
    isDashboard: false,
  }),
  item({
    code: PortalMenuCodes.SF01,
    label: 'Salesmen',
    icon: 'pi pi-id-card',
    routeName: 'salesmen-dashboard',
    route: '/dashboard/salesmen',
    order: 1,
    groupId: 'sales-force',
  }),
  item({
    code: PortalMenuCodes.SF02,
    label: 'Field Activity',
    icon: 'pi pi-map',
    routeName: 'field-activity-dashboard',
    route: '/dashboard/field-activity',
    order: 2,
    groupId: 'sales-force',
  }),
  item({
    code: PortalMenuCodes.IN01,
    label: 'Inventory',
    icon: 'pi pi-box',
    routeName: 'inventory-dashboard',
    route: '/dashboard/inventory',
    order: 1,
    groupId: 'inventory',
  }),
  item({
    code: PortalMenuCodes.IN02,
    label: 'Inventory Risk',
    icon: 'pi pi-exclamation-triangle',
    routeName: 'inventory-risk-dashboard',
    route: '/dashboard/inventory-risk',
    order: 2,
    groupId: 'inventory',
  }),
  item({
    code: PortalMenuCodes.IN03,
    label: 'Inventory Forecast',
    icon: 'pi pi-chart-line',
    routeName: 'inventory-forecast-dashboard',
    route: '/dashboard/inventory-forecast',
    order: 3,
    groupId: 'inventory',
  }),
  item({
    code: PortalMenuCodes.IN04,
    label: 'Inventory Optimization',
    icon: 'pi pi-sliders-h',
    routeName: 'inventory-optimization-dashboard',
    route: '/dashboard/inventory-optimization',
    order: 4,
    groupId: 'inventory',
  }),
  item({
    code: PortalMenuCodes.IN05,
    label: 'Inventory Report',
    icon: 'pi pi-box',
    routeName: 'inventory-report',
    route: '/reports/inventory',
    order: 5,
    groupId: 'inventory',
    isDashboard: false,
  }),
  item({
    code: PortalMenuCodes.PU01,
    label: 'Purchasing',
    icon: 'pi pi-shopping-cart',
    routeName: 'purchasing-dashboard',
    route: '/dashboard/purchasing',
    order: 1,
    groupId: 'purchasing',
  }),
  item({
    code: PortalMenuCodes.PU02,
    label: 'Purchasing Report',
    icon: 'pi pi-shopping-cart',
    routeName: 'purchasing-report',
    route: '/reports/purchasing',
    order: 2,
    groupId: 'purchasing',
    isDashboard: false,
  }),
  item({
    code: PortalMenuCodes.OP01,
    label: 'Locations',
    icon: 'pi pi-map-marker',
    routeName: 'locations-dashboard',
    route: '/dashboard/locations',
    order: 1,
    groupId: 'operations',
  }),
]

const groupDefinitions: Array<{ id: PortalMenuGroupId; label: string; order: number }> = [
  { id: 'executive', label: 'Executive', order: 1 },
  { id: 'sales', label: 'Sales', order: 2 },
  { id: 'customers', label: 'Customers', order: 3 },
  { id: 'finance', label: 'Finance', order: 4 },
  { id: 'sales-force', label: 'Sales Force', order: 5 },
  { id: 'inventory', label: 'Inventory', order: 6 },
  { id: 'purchasing', label: 'Purchasing', order: 7 },
  { id: 'operations', label: 'Operations', order: 8 },
]

export const portalMenuGroups: PortalMenuGroup[] = groupDefinitions.map((group) => ({
  ...group,
  items: portalMenuItems
    .filter((menuItem) => menuItem.groupId === group.id)
    .sort((a, b) => a.order - b.order),
}))

export const allPortalMenuItems: PortalMenuItem[] = [...portalMenuItems].sort((a, b) => {
  const groupA = groupDefinitions.find((g) => g.id === a.groupId)?.order ?? 99
  const groupB = groupDefinitions.find((g) => g.id === b.groupId)?.order ?? 99
  if (groupA !== groupB) return groupA - groupB
  return a.order - b.order
})

export function getDomainDashboardLinks(): PortalMenuItem[] {
  return allPortalMenuItems.filter(
    (menuItem) => menuItem.isDashboard && menuItem.code !== PortalMenuCodes.EX02,
  )
}

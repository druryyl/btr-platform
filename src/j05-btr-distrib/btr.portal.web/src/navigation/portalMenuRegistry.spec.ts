import { describe, expect, it } from 'vitest'
import { PortalMenuCodes } from '@/navigation/portalMenuCodes'
import { allPortalMenuItems, getDomainDashboardLinks, portalMenuGroups } from '@/navigation/portalMenuRegistry'
import { formatMenuLabel, findMenuItemByRoute } from '@/navigation/portalMenuHelpers'

const expectedRouteNames = [
  'dashboard',
  'alert-center',
  'sales-dashboard',
  'sales-forecast-dashboard',
  'sales-report',
  'customers-dashboard',
  'customer-risk-forecast-dashboard',
  'collection-optimization-dashboard',
  'customer-portfolio-dashboard',
  'customer-report',
  'piutang-dashboard',
  'collection-dashboard',
  'cash-flow-forecast-dashboard',
  'piutang-report',
  'salesmen-dashboard',
  'field-activity-dashboard',
  'inventory-dashboard',
  'inventory-risk-dashboard',
  'inventory-forecast-dashboard',
  'inventory-optimization-dashboard',
  'inventory-report',
  'purchasing-dashboard',
  'purchasing-report',
  'locations-dashboard',
] as const

describe('portalMenuRegistry', () => {
  it('contains exactly 24 menu items', () => {
    expect(allPortalMenuItems).toHaveLength(24)
  })

  it('defines 8 domain groups in management scan order', () => {
    expect(portalMenuGroups).toHaveLength(8)
    expect(portalMenuGroups.map((g) => g.label)).toEqual([
      'Executive',
      'Sales',
      'Customers',
      'Finance',
      'Sales Force',
      'Inventory',
      'Purchasing',
      'Operations',
    ])
  })

  it('uses unique codes, routes, and route names', () => {
    const codes = allPortalMenuItems.map((item) => item.code)
    const routes = allPortalMenuItems.map((item) => item.route)
    const routeNames = allPortalMenuItems.map((item) => item.routeName)

    expect(new Set(codes).size).toBe(24)
    expect(new Set(routes).size).toBe(24)
    expect(new Set(routeNames).size).toBe(24)
  })

  it('maps collection optimization to customers group as CU03', () => {
    const item = findMenuItemByRoute('/dashboard/collection-optimization')
    expect(item?.code).toBe(PortalMenuCodes.CU03)
    expect(item?.groupId).toBe('customers')
  })

  it('nests reports within domain groups', () => {
    const salesGroup = portalMenuGroups.find((g) => g.id === 'sales')
    expect(salesGroup?.items.map((i) => i.code)).toEqual(['SA01', 'SA02', 'SA03'])
  })

  it('provides 18 domain dashboard links excluding alert center', () => {
    expect(getDomainDashboardLinks()).toHaveLength(18)
    expect(getDomainDashboardLinks().some((item) => item.code === PortalMenuCodes.EX02)).toBe(false)
  })

  it('uses route names that match the portal router configuration', () => {
    const routeNames = new Set(allPortalMenuItems.map((item) => item.routeName))
    for (const routeName of expectedRouteNames) {
      expect(routeNames.has(routeName)).toBe(true)
    }
    expect(routeNames.size).toBe(expectedRouteNames.length)
  })
})

describe('portalMenuHelpers', () => {
  it('formats menu labels as CODE · Label', () => {
    expect(formatMenuLabel('EX01', 'Executive')).toBe('EX01 · Executive')
  })
})

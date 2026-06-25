import { describe, expect, it } from 'vitest'
import { buildCompareRoute, getEntityAnalyticsNav } from '@/navigation/entityAnalyticsNavigation'

describe('entityAnalyticsNavigation', () => {
  it('maps supported entity types to compare routes', () => {
    expect(getEntityAnalyticsNav('Customer')?.compareRouteName).toBe('customer-compare')
    expect(getEntityAnalyticsNav('Salesman')?.compareRouteName).toBe('salesman-compare')
    expect(getEntityAnalyticsNav('Supplier')?.compareRouteName).toBe('supplier-compare')
    expect(getEntityAnalyticsNav('Item')?.compareRouteName).toBe('item-compare')
  })

  it('builds compare route with optional entity preselect', () => {
    expect(buildCompareRoute('Customer', 'C001')).toEqual({
      name: 'customer-compare',
      query: { entities: 'C001' },
    })
    expect(buildCompareRoute('Item')).toEqual({
      name: 'item-compare',
      query: {},
    })
  })
})

import { describe, expect, it } from 'vitest'
import {
  buildCompareRoute,
  getEntityAnalyticsNav,
  getEntityDisplayLabel,
} from '@/navigation/entityAnalyticsNavigation'

describe('entityAnalyticsNavigation', () => {
  it('maps supported entity types to compare routes', () => {
    expect(getEntityAnalyticsNav('Customer')?.compareRouteName).toBe('customer-compare')
    expect(getEntityAnalyticsNav('Salesman')?.compareRouteName).toBe('salesman-compare')
    expect(getEntityAnalyticsNav('Supplier')?.compareRouteName).toBe('supplier-compare')
    expect(getEntityAnalyticsNav('Item')?.compareRouteName).toBe('item-compare')
  })

  it('presents Supplier as Principal without changing technical identifiers', () => {
    const supplier = getEntityAnalyticsNav('Supplier')
    expect(supplier?.entityType).toBe('Supplier')
    expect(supplier?.compareRouteName).toBe('supplier-compare')
    expect(supplier?.singularLabel).toBe('Principal')
    expect(supplier?.pluralLabel).toBe('Principals')
  })

  it('resolves display labels, defaulting to the raw entity type', () => {
    expect(getEntityDisplayLabel('Supplier')).toBe('Principal')
    expect(getEntityDisplayLabel('Customer')).toBe('Customer')
    expect(getEntityDisplayLabel('Salesman')).toBe('Salesman')
    expect(getEntityDisplayLabel('Item')).toBe('Item')
    expect(getEntityDisplayLabel('Unknown')).toBe('Unknown')
    expect(getEntityDisplayLabel(null)).toBe('')
  })

  it('builds compare route with optional entity id preselect', () => {
    expect(buildCompareRoute('Customer', 'CUST-42')).toEqual({
      name: 'customer-compare',
      query: { entities: 'CUST-42' },
    })
    expect(buildCompareRoute('Item')).toEqual({
      name: 'item-compare',
      query: {},
    })
  })
})

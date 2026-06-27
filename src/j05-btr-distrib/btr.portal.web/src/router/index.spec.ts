import { describe, expect, it } from 'vitest'
import { createRouter, createMemoryHistory } from 'vue-router'

function createTestRouter(base = '/') {
  return createRouter({
    history: createMemoryHistory(base),
    routes: [
      {
        path: '/',
        children: [
          { path: '', redirect: { name: 'dashboard' } },
          {
            path: 'dashboard',
            children: [
              { path: '', name: 'dashboard', component: { template: '<div />' } },
              { path: 'sales', name: 'sales-dashboard', component: { template: '<div />' } },
              {
                path: 'sales-forecast',
                name: 'sales-forecast-dashboard',
                component: { template: '<div />' },
              },
              {
                path: 'cash-flow-forecast',
                name: 'cash-flow-forecast-dashboard',
                component: { template: '<div />' },
              },
              {
                path: 'customer-risk-forecast',
                name: 'customer-risk-forecast-dashboard',
                component: { template: '<div />' },
              },
              {
                path: 'collection-optimization',
                name: 'collection-optimization-dashboard',
                component: { template: '<div />' },
              },
              {
                path: 'customer-portfolio',
                name: 'customer-portfolio-dashboard',
                component: { template: '<div />' },
              },
              {
                path: 'inventory-forecast',
                name: 'inventory-forecast-dashboard',
                component: { template: '<div />' },
              },
              {
                path: 'inventory-optimization',
                name: 'inventory-optimization-dashboard',
                component: { template: '<div />' },
              },
              {
                path: 'field-activity',
                name: 'field-activity-overview',
                component: { template: '<div />' },
              },
              {
                path: 'field-activity/detail',
                name: 'field-activity-detail',
                component: { template: '<div />' },
              },
            ],
          },
          { path: 'alerts', name: 'alert-center', component: { template: '<div />' } },
          { path: 'reports/sales', name: 'sales-report', component: { template: '<div />' } },
          { path: 'analytics', name: 'entity-analytics-home', component: { template: '<div />' } },
          {
            path: 'analytics/customers/compare',
            name: 'customer-compare',
            component: { template: '<div />' },
          },
          {
            path: 'analytics/salesmen/compare',
            name: 'salesman-compare',
            component: { template: '<div />' },
          },
          {
            path: 'analytics/suppliers/compare',
            name: 'supplier-compare',
            component: { template: '<div />' },
          },
          {
            path: 'analytics/items/compare',
            name: 'item-compare',
            component: { template: '<div />' },
          },
          {
            path: 'analytics/customers/:customerId',
            name: 'customer-performance-profile',
            redirect: (to) => ({
              name: 'entity-performance-profile',
              params: { entityType: 'Customer', entityId: to.params.customerId },
            }),
          },
          {
            path: 'analytics/salesmen/:salesPersonId',
            name: 'salesman-performance-profile',
            component: { template: '<div />' },
          },
          {
            path: 'analytics/suppliers/:supplierId',
            name: 'supplier-performance-profile',
            component: { template: '<div />' },
          },
          {
            path: 'analytics/items/:brgId',
            name: 'item-performance-profile',
            component: { template: '<div />' },
          },
          {
            path: 'analytics/:entityType/:entityId',
            name: 'entity-performance-profile',
            component: { template: '<div />' },
          },
        ],
      },
    ],
  })
}

describe('dashboard route matching', () => {
  it('resolves /dashboard/sales to sales-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/sales')
    expect(resolved.name).toBe('sales-dashboard')
  })

  it('resolves /dashboard/sales-forecast to sales-forecast-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/sales-forecast')
    expect(resolved.name).toBe('sales-forecast-dashboard')
  })

  it('resolves /dashboard/cash-flow-forecast to cash-flow-forecast-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/cash-flow-forecast')
    expect(resolved.name).toBe('cash-flow-forecast-dashboard')
  })

  it('resolves /dashboard/customer-risk-forecast to customer-risk-forecast-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/customer-risk-forecast')
    expect(resolved.name).toBe('customer-risk-forecast-dashboard')
  })

  it('resolves /dashboard/collection-optimization to collection-optimization-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/collection-optimization')
    expect(resolved.name).toBe('collection-optimization-dashboard')
  })

  it('resolves /dashboard/customer-portfolio to customer-portfolio-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/customer-portfolio')
    expect(resolved.name).toBe('customer-portfolio-dashboard')
  })

  it('resolves /dashboard/inventory-forecast to inventory-forecast-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/inventory-forecast')
    expect(resolved.name).toBe('inventory-forecast-dashboard')
  })

  it('resolves /dashboard/inventory-optimization to inventory-optimization-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/inventory-optimization')
    expect(resolved.name).toBe('inventory-optimization-dashboard')
  })

  it('resolves /dashboard to dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard')
    expect(resolved.name).toBe('dashboard')
  })

  it('resolves /dashboard/field-activity to field-activity-overview', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/field-activity')
    expect(resolved.name).toBe('field-activity-overview')
  })

  it('resolves /dashboard/field-activity/detail to field-activity-detail', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/field-activity/detail')
    expect(resolved.name).toBe('field-activity-detail')
  })

  it('with /portal/ base, push /dashboard/sales stays under portal', async () => {
    const router = createTestRouter('/portal/')
    await router.push('/dashboard/sales')
    expect(router.currentRoute.value.name).toBe('sales-dashboard')
    expect(router.currentRoute.value.fullPath).toBe('/dashboard/sales')
  })

  it('with /portal/ base, resolve href for dashboard child routes', () => {
    const router = createTestRouter('/portal/')
    expect(router.resolve('/dashboard/sales').href).toBe('/portal/dashboard/sales')
    expect(router.resolve('/dashboard/customer-portfolio').href).toBe(
      '/portal/dashboard/customer-portfolio',
    )
    expect(router.resolve('/dashboard').href).toBe('/portal/dashboard')
    expect(router.resolve('/alerts').href).toBe('/portal/alerts')
    expect(router.resolve('/reports/sales').href).toBe('/portal/reports/sales')
  })

  it('resolves /analytics to entity-analytics-home', () => {
    const router = createTestRouter()
    expect(router.resolve('/analytics').name).toBe('entity-analytics-home')
  })

  it('resolves /analytics/customers/compare to customer-compare', () => {
    const router = createTestRouter()
    expect(router.resolve('/analytics/customers/compare').name).toBe('customer-compare')
  })

  it('redirects /analytics/customers/C001 to entity-performance-profile', async () => {
    const router = createTestRouter()
    await router.push('/analytics/customers/C001')
    expect(router.currentRoute.value.name).toBe('entity-performance-profile')
    expect(router.currentRoute.value.params.entityType).toBe('Customer')
    expect(router.currentRoute.value.params.entityId).toBe('C001')
  })

  it('resolves /analytics/salesmen/compare to salesman-compare', () => {
    const router = createTestRouter()
    expect(router.resolve('/analytics/salesmen/compare').name).toBe('salesman-compare')
  })

  it('resolves /analytics/salesmen/SP100 to salesman-performance-profile', async () => {
    const router = createTestRouter()
    await router.push('/analytics/salesmen/SP100')
    expect(router.currentRoute.value.name).toBe('salesman-performance-profile')
    expect(router.currentRoute.value.params.salesPersonId).toBe('SP100')
  })

  it('resolves /analytics/suppliers/compare to supplier-compare', () => {
    const router = createTestRouter()
    expect(router.resolve('/analytics/suppliers/compare').name).toBe('supplier-compare')
  })

  it('resolves /analytics/suppliers/SUPA to supplier-performance-profile', async () => {
    const router = createTestRouter()
    await router.push('/analytics/suppliers/SUPA')
    expect(router.currentRoute.value.name).toBe('supplier-performance-profile')
    expect(router.currentRoute.value.params.supplierId).toBe('SUPA')
  })

  it('resolves /analytics/items/compare to item-compare', () => {
    const router = createTestRouter()
    expect(router.resolve('/analytics/items/compare').name).toBe('item-compare')
  })

  it('resolves /analytics/items/BRG001 to item-performance-profile', async () => {
    const router = createTestRouter()
    await router.push('/analytics/items/BRG001')
    expect(router.currentRoute.value.name).toBe('item-performance-profile')
    expect(router.currentRoute.value.params.brgId).toBe('BRG001')
  })
})

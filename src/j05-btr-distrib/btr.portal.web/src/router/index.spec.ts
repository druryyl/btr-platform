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
                path: 'inventory-forecast',
                name: 'inventory-forecast-dashboard',
                component: { template: '<div />' },
              },
              {
                path: 'inventory-optimization',
                name: 'inventory-optimization-dashboard',
                component: { template: '<div />' },
              },
              { path: 'field-activity', name: 'field-activity-dashboard', component: { template: '<div />' } },
            ],
          },
          { path: 'alerts', name: 'alert-center', component: { template: '<div />' } },
          { path: 'reports/sales', name: 'sales-report', component: { template: '<div />' } },
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

  it('resolves /dashboard/field-activity to field-activity-dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard/field-activity')
    expect(resolved.name).toBe('field-activity-dashboard')
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
    expect(router.resolve('/dashboard').href).toBe('/portal/dashboard')
    expect(router.resolve('/alerts').href).toBe('/portal/alerts')
    expect(router.resolve('/reports/sales').href).toBe('/portal/reports/sales')
  })
})

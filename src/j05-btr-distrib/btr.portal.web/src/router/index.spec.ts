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

  it('resolves /dashboard to dashboard', () => {
    const router = createTestRouter()
    const resolved = router.resolve('/dashboard')
    expect(resolved.name).toBe('dashboard')
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

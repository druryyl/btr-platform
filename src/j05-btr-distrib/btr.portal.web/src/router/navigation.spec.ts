import { describe, expect, it, beforeEach } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { createRouter, createMemoryHistory } from 'vue-router'

const routes = [
  {
    path: '/login',
    name: 'login',
    component: { template: '<div>Login</div>' },
    meta: { public: true },
  },
  {
    path: '/',
    component: { template: '<router-view />' },
    meta: { requiresAuth: true },
    children: [
      { path: '', redirect: { name: 'dashboard' } },
      {
        path: 'dashboard',
        children: [
          { path: '', name: 'dashboard', component: { template: '<div>Executive</div>' } },
          { path: 'sales', name: 'sales-dashboard', component: { template: '<div>Sales</div>' } },
          { path: 'piutang', name: 'piutang-dashboard', component: { template: '<div>Piutang</div>' } },
        ],
      },
      { path: 'alerts', name: 'alert-center', component: { template: '<div>Alerts</div>' } },
      { path: 'reports/sales', name: 'sales-report', component: { template: '<div>Report</div>' } },
    ],
  },
]

describe('dashboard child navigation', () => {
  let router: ReturnType<typeof createRouter>

  beforeEach(async () => {
    setActivePinia(createPinia())
    router = createRouter({
      history: createMemoryHistory('/portal/'),
      routes,
    })

    await router.push('/dashboard')
  })

  it('navigates from executive to sales dashboard', async () => {
    await router.push('/dashboard/sales')
    expect(router.currentRoute.value.name).toBe('sales-dashboard')
  })

  it('navigates from sales back to executive dashboard', async () => {
    await router.push('/dashboard/sales')
    await router.push('/dashboard')
    expect(router.currentRoute.value.name).toBe('dashboard')
  })

  it('navigates from executive to alerts', async () => {
    await router.push('/alerts')
    expect(router.currentRoute.value.name).toBe('alert-center')
  })
})

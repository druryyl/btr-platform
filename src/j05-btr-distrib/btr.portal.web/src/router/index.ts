import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/authStore'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/auth/LoginView.vue'),
      meta: { public: true },
    },
    {
      path: '/',
      component: () => import('@/layouts/MainLayout.vue'),
      meta: { requiresAuth: true },
      children: [
        {
          path: '',
          redirect: '/dashboard',
        },
        {
          path: 'dashboard',
          name: 'dashboard',
          component: () => import('@/views/dashboard/DashboardHomeView.vue'),
        },
        {
          path: 'dashboard/sales',
          name: 'sales-dashboard',
          component: () => import('@/views/dashboard/SalesDashboardView.vue'),
        },
        {
          path: 'dashboard/piutang',
          name: 'piutang-dashboard',
          component: () => import('@/views/dashboard/PiutangDashboardView.vue'),
        },
        {
          path: 'dashboard/inventory',
          name: 'inventory-dashboard',
          component: () => import('@/views/dashboard/InventoryDashboardView.vue'),
        },
        {
          path: 'reports/sales',
          name: 'sales-report',
          component: () => import('@/views/reports/SalesReportView.vue'),
        },
        {
          path: 'reports/piutang',
          name: 'piutang-report',
          component: () => import('@/views/reports/PiutangReportView.vue'),
        },
        {
          path: 'reports/inventory',
          name: 'inventory-report',
          component: () => import('@/views/reports/InventoryReportView.vue'),
        },
        {
          path: 'reports/purchasing',
          name: 'purchasing-report',
          component: () => import('@/views/reports/PurchasingReportView.vue'),
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/dashboard',
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return {
      path: '/login',
      query: { redirect: to.fullPath },
    }
  }

  if (to.path === '/login' && auth.isAuthenticated) {
    return { path: '/dashboard' }
  }

  return true
})

export default router

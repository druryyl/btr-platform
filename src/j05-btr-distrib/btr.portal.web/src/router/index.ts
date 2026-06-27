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
          redirect: { name: 'dashboard' },
        },
        {
          path: 'dashboard',
          children: [
            {
              path: '',
              name: 'dashboard',
              component: () => import('@/views/dashboard/DashboardHomeView.vue'),
            },
            {
              path: 'sales',
              name: 'sales-dashboard',
              component: () => import('@/views/dashboard/SalesDashboardView.vue'),
            },
            {
              path: 'sales-forecast',
              name: 'sales-forecast-dashboard',
              component: () => import('@/views/dashboard/SalesForecastDashboardView.vue'),
            },
            {
              path: 'piutang',
              name: 'piutang-dashboard',
              component: () => import('@/views/dashboard/PiutangDashboardView.vue'),
            },
            {
              path: 'customers',
              name: 'customers-dashboard',
              component: () => import('@/views/dashboard/CustomerDashboardView.vue'),
            },
            {
              path: 'customer-risk-forecast',
              name: 'customer-risk-forecast-dashboard',
              component: () => import('@/views/dashboard/CustomerRiskForecastDashboardView.vue'),
            },
            {
              path: 'collection-optimization',
              name: 'collection-optimization-dashboard',
              component: () => import('@/views/dashboard/CollectionOptimizationDashboardView.vue'),
            },
            {
              path: 'customer-portfolio',
              name: 'customer-portfolio-dashboard',
              component: () => import('@/views/dashboard/CustomerPortfolioDashboardView.vue'),
            },
            {
              path: 'salesmen',
              name: 'salesmen-dashboard',
              component: () => import('@/views/dashboard/SalesmanDashboardView.vue'),
            },
            {
              path: 'field-activity',
              name: 'field-activity-overview',
              component: () => import('@/views/dashboard/FieldActivityOverviewView.vue'),
            },
            {
              path: 'field-activity/detail',
              name: 'field-activity-detail',
              component: () => import('@/views/dashboard/FieldActivityDashboardView.vue'),
            },
            {
              path: 'collection',
              name: 'collection-dashboard',
              component: () => import('@/views/dashboard/CollectionDashboardView.vue'),
            },
            {
              path: 'cash-flow-forecast',
              name: 'cash-flow-forecast-dashboard',
              component: () => import('@/views/dashboard/CashFlowForecastDashboardView.vue'),
            },
            {
              path: 'inventory',
              name: 'inventory-dashboard',
              component: () => import('@/views/dashboard/InventoryDashboardView.vue'),
            },
            {
              path: 'inventory-risk',
              name: 'inventory-risk-dashboard',
              component: () => import('@/views/dashboard/InventoryRiskDashboardView.vue'),
            },
            {
              path: 'inventory-forecast',
              name: 'inventory-forecast-dashboard',
              component: () => import('@/views/dashboard/InventoryForecastDashboardView.vue'),
            },
            {
              path: 'inventory-optimization',
              name: 'inventory-optimization-dashboard',
              component: () => import('@/views/dashboard/InventoryOptimizationDashboardView.vue'),
            },
            {
              path: 'purchasing',
              name: 'purchasing-dashboard',
              component: () => import('@/views/dashboard/PurchasingDashboardView.vue'),
            },
            {
              path: 'locations',
              name: 'locations-dashboard',
              component: () => import('@/views/dashboard/LocationDashboardView.vue'),
            },
          ],
        },
        {
          path: 'alerts',
          name: 'alert-center',
          component: () => import('@/views/alerts/AlertCenterView.vue'),
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
        {
          path: 'reports/customers',
          name: 'customer-report',
          component: () => import('@/views/reports/CustomerReportView.vue'),
        },
        {
          path: 'analytics',
          name: 'entity-analytics-home',
          component: () => import('@/views/analytics/EntityAnalyticsHomeView.vue'),
        },
        {
          path: 'analytics/customers/compare',
          name: 'customer-compare',
          component: () => import('@/views/analytics/CustomerCompareView.vue'),
        },
        {
          path: 'analytics/salesmen/compare',
          name: 'salesman-compare',
          component: () => import('@/views/analytics/SalesmanCompareView.vue'),
        },
        {
          path: 'analytics/suppliers/compare',
          name: 'supplier-compare',
          component: () => import('@/views/analytics/SupplierCompareView.vue'),
        },
        {
          path: 'analytics/items/compare',
          name: 'item-compare',
          component: () => import('@/views/analytics/ItemCompareView.vue'),
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
          component: () => import('@/views/analytics/SalesmanProfileView.vue'),
        },
        {
          path: 'analytics/suppliers/:supplierId',
          name: 'supplier-performance-profile',
          component: () => import('@/views/analytics/SupplierProfileView.vue'),
        },
        {
          path: 'analytics/items/:brgId',
          name: 'item-performance-profile',
          component: () => import('@/views/analytics/ItemProfileView.vue'),
        },
        {
          path: 'analytics/:entityType/:entityId',
          name: 'entity-performance-profile',
          component: () => import('@/views/analytics/EntityPerformanceProfileView.vue'),
        },
      ],
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: { name: 'dashboard' },
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
    return { name: 'dashboard' }
  }

  return true
})

export default router

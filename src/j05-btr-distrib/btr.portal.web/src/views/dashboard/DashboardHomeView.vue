<script setup lang="ts">
import { onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import Button from 'primevue/button'
import Message from 'primevue/message'
import KpiCard from '@/components/KpiCard.vue'
import { formatCurrency, formatDateTime, formatNumber } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

onMounted(() => {
  void dashboard.loadDashboard()
})
</script>

<template>
  <div class="dashboard-home">
    <div class="dashboard-home__header">
      <div>
        <h1>Dashboard</h1>
        <p>Operational summary from BTR reporting sources.</p>
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="dashboard.loading"
        @click="dashboard.loadDashboard()"
      />
    </div>

    <Message v-if="dashboard.error" severity="error" :closable="false">
      {{ dashboard.error }}
    </Message>

    <div class="dashboard-home__grid">
      <KpiCard title="Sales" icon="pi pi-chart-line" :loading="dashboard.loading">
        <div class="metric">
          <span class="metric__label">Invoiced Omzet (Faktur)</span>
          <span class="metric__value">
            {{ dashboard.overview?.Sales ? formatCurrency(dashboard.overview.Sales.TotalOmzet) : '-' }}
          </span>
        </div>
        <div class="metric">
          <span class="metric__label">Total Faktur</span>
          <span class="metric__value">
            {{ dashboard.overview?.Sales ? formatNumber(dashboard.overview.Sales.TotalFaktur) : '-' }}
          </span>
        </div>
        <div class="metric">
          <span class="metric__label">Total Customer</span>
          <span class="metric__value">
            {{ dashboard.overview?.Sales ? formatNumber(dashboard.overview.Sales.TotalCustomer) : '-' }}
          </span>
        </div>
        <RouterLink to="/dashboard/sales" class="kpi-card__link">
          View sales analytics →
        </RouterLink>
        <div v-if="dashboard.overview?.Sales" class="metric__meta">
          Updated {{ formatDateTime(dashboard.overview.Sales.GeneratedAt) }}
        </div>
      </KpiCard>

      <KpiCard title="Piutang" icon="pi pi-wallet" :loading="dashboard.loading">
        <div class="metric">
          <span class="metric__label">Total Piutang</span>
          <span class="metric__value">
            {{ dashboard.overview?.Piutang ? formatCurrency(dashboard.overview.Piutang.TotalPiutang) : '-' }}
          </span>
        </div>
        <div class="metric">
          <span class="metric__label">Total Customer</span>
          <span class="metric__value">
            {{ dashboard.overview?.Piutang ? formatNumber(dashboard.overview.Piutang.TotalCustomer) : '-' }}
          </span>
        </div>
        <RouterLink to="/dashboard/piutang" class="kpi-card__link">
          View piutang analytics →
        </RouterLink>
        <div v-if="dashboard.overview?.Piutang" class="metric__meta">
          Updated {{ formatDateTime(dashboard.overview.Piutang.GeneratedAt) }}
        </div>
      </KpiCard>

      <KpiCard title="Purchasing" icon="pi pi-shopping-cart" :loading="dashboard.loading">
        <div class="metric">
          <span class="metric__label">Grand Total Purchase</span>
          <span class="metric__value">
            {{
              dashboard.overview?.Purchasing
                ? formatCurrency(dashboard.overview.Purchasing.GrandTotalPurchase)
                : '-'
            }}
          </span>
        </div>
        <div class="metric">
          <span class="metric__label">Total Invoice</span>
          <span class="metric__value">
            {{
              dashboard.overview?.Purchasing
                ? formatNumber(dashboard.overview.Purchasing.TotalInvoice)
                : '-'
            }}
          </span>
        </div>
        <RouterLink to="/dashboard/purchasing" class="kpi-card__link">
          View purchasing analytics →
        </RouterLink>
        <div v-if="dashboard.overview?.Purchasing" class="metric__meta">
          Updated {{ formatDateTime(dashboard.overview.Purchasing.GeneratedAt) }}
        </div>
      </KpiCard>

      <KpiCard title="Inventory" icon="pi pi-box" :loading="dashboard.loading">
        <div class="metric">
          <span class="metric__label">Total Inventory Value</span>
          <span class="metric__value">
            {{
              dashboard.overview?.Inventory
                ? formatCurrency(dashboard.overview.Inventory.TotalInventoryValue)
                : '-'
            }}
          </span>
        </div>
        <div class="metric">
          <span class="metric__label">Total Item</span>
          <span class="metric__value">
            {{ dashboard.overview?.Inventory ? formatNumber(dashboard.overview.Inventory.TotalItem) : '-' }}
          </span>
        </div>
        <RouterLink to="/dashboard/inventory" class="kpi-card__link">
          View inventory analytics →
        </RouterLink>
        <div v-if="dashboard.overview?.Inventory" class="metric__meta">
          Updated {{ formatDateTime(dashboard.overview.Inventory.GeneratedAt) }}
        </div>
      </KpiCard>
    </div>
  </div>
</template>

<style scoped>
.dashboard-home__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.dashboard-home__header h1 {
  margin: 0;
  font-size: 1.75rem;
}

.dashboard-home__header p {
  margin: 0.375rem 0 0;
  color: var(--p-text-muted-color);
}

.dashboard-home__grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
}

.metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.metric__label {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.metric__value {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.metric__meta {
  margin-top: 0.5rem;
  font-size: 0.8rem;
  color: var(--p-text-muted-color);
}

.kpi-card__link {
  display: inline-block;
  margin-top: 0.5rem;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
}

.kpi-card__link:hover {
  text-decoration: underline;
}

@media (max-width: 1200px) {
  .dashboard-home__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 768px) {
  .dashboard-home__grid {
    grid-template-columns: 1fr;
  }
}
</style>

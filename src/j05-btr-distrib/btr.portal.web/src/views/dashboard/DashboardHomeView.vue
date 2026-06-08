<script setup lang="ts">
import { onMounted } from 'vue'
import Button from 'primevue/button'
import Message from 'primevue/message'
import ExecutiveAttentionCard from '@/components/dashboard/ExecutiveAttentionCard.vue'
import ExecutiveDomainSummaryRow from '@/components/dashboard/ExecutiveDomainSummaryRow.vue'
import ExecutiveExposureSection from '@/components/dashboard/ExecutiveExposureSection.vue'
import { formatCurrency, formatDateTime, formatPercent } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

onMounted(() => {
  void dashboard.loadExecutive()
})
</script>

<template>
  <div class="dashboard-home">
    <div class="dashboard-home__header">
      <div>
        <h1>Management Attention Center</h1>
        <p>What requires management attention today?</p>
        <p v-if="dashboard.executive?.LastRefreshed" class="dashboard-home__refreshed">
          Last Refreshed: {{ formatDateTime(dashboard.executive.LastRefreshed) }}
        </p>
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="dashboard.loading"
        @click="dashboard.loadExecutive()"
      />
    </div>

    <Message
      v-if="dashboard.executive && !dashboard.executive.IsDataFresh"
      severity="warn"
      :closable="false"
      class="dashboard-home__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <Message
      v-if="dashboard.executive && dashboard.executive.OverallHealthStatus === 'degraded'"
      severity="error"
      :closable="false"
      class="dashboard-home__banner"
    >
      Dashboard snapshot refresh is degraded. Some analytics may be outdated.
    </Message>

    <Message
      v-if="dashboard.executive && dashboard.executive.OverallHealthStatus === 'refreshing'"
      severity="info"
      :closable="false"
      class="dashboard-home__banner"
    >
      Dashboard snapshots are currently refreshing.
    </Message>

    <Message v-if="dashboard.error" severity="error" :closable="false">
      {{ dashboard.error }}
    </Message>

    <section class="dashboard-home__section">
      <h2 class="dashboard-home__section-title">Attention Cards</h2>
      <div class="dashboard-home__grid">
        <ExecutiveAttentionCard
          title="Sales"
          icon="pi pi-chart-line"
          route="/dashboard/sales"
          :loading="dashboard.loading"
          :requires-attention="dashboard.executive?.Sales.RequiresAttention"
          :achievement-band="dashboard.executive?.Sales.AchievementBand"
          :unavailable="dashboard.executive != null && !dashboard.executive.Sales.IsAvailable"
        >
          <div class="metric">
            <span class="metric__label">Achievement %</span>
            <span class="metric__value">
              {{ formatPercent(dashboard.executive?.Sales.AchievementPercent) }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Total Achievement</span>
            <span class="metric__value">
              {{
                dashboard.executive?.Sales.IsAvailable
                  ? formatCurrency(dashboard.executive.Sales.TotalAchievement)
                  : '—'
              }}
            </span>
          </div>
        </ExecutiveAttentionCard>

        <ExecutiveAttentionCard
          title="Piutang"
          icon="pi pi-wallet"
          route="/dashboard/piutang"
          :loading="dashboard.loading"
          :requires-attention="dashboard.executive?.Piutang.RequiresAttention"
          :unavailable="dashboard.executive != null && !dashboard.executive.Piutang.IsAvailable"
        >
          <div class="metric">
            <span class="metric__label">Total Piutang</span>
            <span class="metric__value">
              {{
                dashboard.executive?.Piutang.IsAvailable
                  ? formatCurrency(dashboard.executive.Piutang.TotalPiutang)
                  : '—'
              }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Overdue Customer</span>
            <span class="metric__value">
              {{ dashboard.executive?.Piutang.IsAvailable ? dashboard.executive.Piutang.OverdueCustomer : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">&gt; 90 Day Amount</span>
            <span class="metric__value">
              {{
                dashboard.executive?.Piutang.IsAvailable
                  ? `${formatCurrency(dashboard.executive.Piutang.AgingOver90Amount)} (${formatPercent(dashboard.executive.Piutang.AgingOver90Percent)})`
                  : '—'
              }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Customer %</span>
            <span class="metric__value">
              {{ formatPercent(dashboard.executive?.Piutang.TopCustomerPercent) }}
            </span>
          </div>
        </ExecutiveAttentionCard>

        <ExecutiveAttentionCard
          title="Purchasing"
          icon="pi pi-shopping-cart"
          route="/dashboard/purchasing"
          :loading="dashboard.loading"
          :requires-attention="dashboard.executive?.Purchasing.RequiresAttention"
          :unavailable="dashboard.executive != null && !dashboard.executive.Purchasing.IsAvailable"
        >
          <div class="metric">
            <span class="metric__label">Pending Posting</span>
            <span class="metric__value metric__value--compact">
              {{
                dashboard.executive?.Purchasing.IsAvailable
                  ? `Pending Posting · ${dashboard.executive.Purchasing.PendingPostingInvoiceCount} invoices · ${formatCurrency(dashboard.executive.Purchasing.PendingPostingValue)}`
                  : '—'
              }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Principal %</span>
            <span class="metric__value">
              {{ formatPercent(dashboard.executive?.Purchasing.TopPrincipalPercent) }}
            </span>
          </div>
        </ExecutiveAttentionCard>

        <ExecutiveAttentionCard
          title="Inventory"
          icon="pi pi-box"
          route="/dashboard/inventory"
          :loading="dashboard.loading"
          :requires-attention="dashboard.executive?.Inventory.RequiresAttention"
          :unavailable="dashboard.executive != null && !dashboard.executive.Inventory.IsAvailable"
        >
          <div class="metric">
            <span class="metric__label">Total Inventory Value</span>
            <span class="metric__value">
              {{
                dashboard.executive?.Inventory.IsAvailable
                  ? formatCurrency(dashboard.executive.Inventory.TotalInventoryValue)
                  : '—'
              }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Category %</span>
            <span class="metric__value">
              {{ formatPercent(dashboard.executive?.Inventory.TopCategoryPercent) }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Supplier %</span>
            <span class="metric__value">
              {{ formatPercent(dashboard.executive?.Inventory.TopSupplierPercent) }}
            </span>
          </div>
        </ExecutiveAttentionCard>
      </div>
    </section>

    <section class="dashboard-home__section">
      <h2 class="dashboard-home__section-title">Critical Exposure Lists</h2>
      <div class="dashboard-home__exposures">
        <ExecutiveExposureSection
          title="Top 5 Customers"
          name-header="Customer"
          :items="dashboard.executive?.CriticalExposures.TopCustomers ?? []"
          :loading="dashboard.loading"
        />
        <ExecutiveExposureSection
          title="Top 5 Categories"
          name-header="Category"
          :items="dashboard.executive?.CriticalExposures.TopCategories ?? []"
          :loading="dashboard.loading"
        />
        <ExecutiveExposureSection
          title="Top 5 Suppliers"
          name-header="Supplier"
          :items="dashboard.executive?.CriticalExposures.TopSuppliers ?? []"
          :loading="dashboard.loading"
        />
        <ExecutiveExposureSection
          title="Top 5 Principals"
          name-header="Principal"
          :items="dashboard.executive?.CriticalExposures.TopPrincipals ?? []"
          :loading="dashboard.loading"
        />
      </div>
    </section>

    <section class="dashboard-home__section">
      <h2 class="dashboard-home__section-title">Domain Summaries</h2>
      <div class="dashboard-home__summaries">
        <ExecutiveDomainSummaryRow
          v-for="summary in dashboard.executive?.DomainSummaries ?? []"
          :key="summary.Domain"
          :domain="summary.Domain"
          :summary-text="summary.SummaryText"
          :route="summary.DetailDashboardRoute"
          :available="summary.IsAvailable"
        />
      </div>
    </section>
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

.dashboard-home__refreshed {
  margin-top: 0.5rem !important;
  font-size: 0.875rem !important;
  color: var(--p-text-color) !important;
}

.dashboard-home__banner {
  margin-bottom: 1rem;
}

.dashboard-home__section {
  margin-bottom: 2rem;
}

.dashboard-home__section-title {
  margin: 0 0 1rem;
  font-size: 1.1rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--p-text-muted-color);
}

.dashboard-home__grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
}

.dashboard-home__exposures {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

.dashboard-home__summaries {
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius);
  padding: 0 1rem;
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
  font-size: 1.1rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.metric__value--compact {
  font-size: 0.95rem;
  font-weight: 600;
  line-height: 1.4;
}

@media (max-width: 1200px) {
  .dashboard-home__grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .dashboard-home__exposures {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .dashboard-home__grid {
    grid-template-columns: 1fr;
  }
}
</style>

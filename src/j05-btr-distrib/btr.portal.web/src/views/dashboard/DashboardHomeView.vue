<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Button from 'primevue/button'
import Message from 'primevue/message'
import PlatformSnapshotHealthBanners from '@/components/platform/PlatformSnapshotHealthBanners.vue'
import ExecutiveAttentionCard from '@/components/dashboard/ExecutiveAttentionCard.vue'
import ExecutivePortfolioSummarySection from '@/components/dashboard/ExecutivePortfolioSummarySection.vue'
import ExecutiveDomainSummaryRow from '@/components/dashboard/ExecutiveDomainSummaryRow.vue'
import ExecutiveExposureSection from '@/components/dashboard/ExecutiveExposureSection.vue'
import DashboardSectionHeader from '@/components/dashboard/primitives/DashboardSectionHeader.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import { formatCurrency, formatDateTime } from '@/services/formatters'
import {
  formatDashboardCurrency,
  formatDashboardEmpty,
  formatDashboardPercent,
} from '@/services/dashboardEmptyStates'
import { shouldShowInfrastructureError } from '@/services/platformDiagnostics'
import { useDashboardStore } from '@/stores/dashboardStore'
import { usePresentationStore } from '@/stores/presentationStore'

const dashboard = useDashboardStore()
const presentation = usePresentationStore()
const router = useRouter()

const visibleError = computed(() =>
  shouldShowInfrastructureError(dashboard.error, presentation.hidePlatformDiagnostics)
    ? dashboard.error
    : null,
)

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
        <p
          v-if="dashboard.executive?.LastRefreshed && !presentation.hidePlatformDiagnostics"
          class="dashboard-home__refreshed"
        >
          Last Refreshed: {{ formatDateTime(dashboard.executive.LastRefreshed) }}
        </p>
      </div>
      <div class="dashboard-home__actions">
        <Button
          label="Open Alert Center"
          icon="pi pi-bell"
          @click="router.push('/alerts')"
        />
        <Button
          label="Refresh"
          icon="pi pi-refresh"
          outlined
          :loading="dashboard.loading"
          @click="dashboard.loadExecutive()"
        />
      </div>
    </div>

    <PlatformSnapshotHealthBanners
      v-if="dashboard.executive"
      :is-data-fresh="dashboard.executive.IsDataFresh"
      :overall-health-status="dashboard.executive.OverallHealthStatus"
    />

    <Message v-if="visibleError" severity="error" :closable="false">
      {{ visibleError }}
    </Message>

    <section class="dashboard-home__section">
      <DashboardSectionHeader
        title="Attention Cards"
        icon="pi pi-eye"
        domain="alert"
      />
      <div class="dashboard-home__grid">
        <ExecutiveAttentionCard
          title="Sales"
          icon="pi pi-chart-line"
          domain="sales"
          route="/dashboard/sales"
          hero
          :loading="dashboard.loading"
          :requires-attention="dashboard.executive?.Sales.RequiresAttention"
          :achievement-band="dashboard.executive?.Sales.AchievementBand"
          :unavailable="dashboard.executive != null && !dashboard.executive.Sales.IsAvailable"
        >
          <DashboardMetric
            label="Achievement %"
            :value="formatDashboardPercent(dashboard.executive?.Sales.AchievementPercent, 'no-target')"
            variant="primary"
            :empty="dashboard.executive?.Sales.AchievementPercent == null"
            :progress="dashboard.executive?.Sales.AchievementPercent ?? null"
          />
          <DashboardMetric
            label="Total Achievement"
            :value="
              formatDashboardCurrency(
                dashboard.executive?.Sales.IsAvailable
                  ? dashboard.executive?.Sales.TotalAchievement
                  : null,
                formatCurrency,
              )
            "
            variant="secondary"
            :empty="!dashboard.executive?.Sales.IsAvailable"
          />
        </ExecutiveAttentionCard>

        <ExecutiveAttentionCard
          title="Piutang"
          icon="pi pi-wallet"
          domain="finance"
          route="/dashboard/piutang"
          :loading="dashboard.loading"
          :requires-attention="dashboard.executive?.Piutang.RequiresAttention"
          :unavailable="dashboard.executive != null && !dashboard.executive.Piutang.IsAvailable"
        >
          <DashboardMetric
            label="Total Piutang"
            :value="
              formatDashboardCurrency(
                dashboard.executive?.Piutang.IsAvailable
                  ? dashboard.executive?.Piutang.TotalPiutang
                  : null,
                formatCurrency,
              )
            "
            variant="primary"
            :empty="!dashboard.executive?.Piutang.IsAvailable"
          />
          <DashboardMetric
            label="Overdue Customer"
            :value="
              dashboard.executive?.Piutang.IsAvailable
                ? String(dashboard.executive.Piutang.OverdueCustomer)
                : formatDashboardEmpty('no-data')
            "
            variant="secondary"
            :empty="!dashboard.executive?.Piutang.IsAvailable"
          />
          <DashboardMetric
            label="> 90 Day Amount"
            :value="
              dashboard.executive?.Piutang.IsAvailable
                ? `${formatCurrency(dashboard.executive.Piutang.AgingOver90Amount)} (${formatDashboardPercent(dashboard.executive.Piutang.AgingOver90Percent)})`
                : formatDashboardEmpty('no-data')
            "
            variant="secondary"
            :empty="!dashboard.executive?.Piutang.IsAvailable"
            :progress="dashboard.executive?.Piutang.AgingOver90Percent ?? null"
            progress-status="critical"
          />
          <DashboardMetric
            label="Top Customer %"
            :value="formatDashboardPercent(dashboard.executive?.Piutang.TopCustomerPercent)"
            variant="secondary"
            :empty="dashboard.executive?.Piutang.TopCustomerPercent == null"
            :progress="dashboard.executive?.Piutang.TopCustomerPercent ?? null"
          />
        </ExecutiveAttentionCard>

        <ExecutiveAttentionCard
          title="Purchasing"
          icon="pi pi-shopping-cart"
          domain="purchasing"
          route="/dashboard/purchasing"
          :loading="dashboard.loading"
          :requires-attention="dashboard.executive?.Purchasing.RequiresAttention"
          :unavailable="dashboard.executive != null && !dashboard.executive.Purchasing.IsAvailable"
        >
          <DashboardMetric
            label="Pending Posting"
            :value="
              dashboard.executive?.Purchasing.IsAvailable
                ? `${dashboard.executive.Purchasing.PendingPostingInvoiceCount} invoices · ${formatCurrency(dashboard.executive.Purchasing.PendingPostingValue)}`
                : formatDashboardEmpty('no-data')
            "
            variant="primary"
            :empty="!dashboard.executive?.Purchasing.IsAvailable"
          />
          <DashboardMetric
            label="Top Principal %"
            :value="formatDashboardPercent(dashboard.executive?.Purchasing.TopPrincipalPercent)"
            variant="secondary"
            :empty="dashboard.executive?.Purchasing.TopPrincipalPercent == null"
            :progress="dashboard.executive?.Purchasing.TopPrincipalPercent ?? null"
          />
        </ExecutiveAttentionCard>

        <ExecutiveAttentionCard
          title="Inventory"
          icon="pi pi-box"
          domain="inventory"
          route="/dashboard/inventory"
          :loading="dashboard.loading"
          :requires-attention="dashboard.executive?.Inventory.RequiresAttention"
          :unavailable="dashboard.executive != null && !dashboard.executive.Inventory.IsAvailable"
        >
          <DashboardMetric
            label="Total Inventory Value"
            :value="
              formatDashboardCurrency(
                dashboard.executive?.Inventory.IsAvailable
                  ? dashboard.executive?.Inventory.TotalInventoryValue
                  : null,
                formatCurrency,
              )
            "
            variant="primary"
            :empty="!dashboard.executive?.Inventory.IsAvailable"
          />
          <DashboardMetric
            label="Top Category %"
            :value="formatDashboardPercent(dashboard.executive?.Inventory.TopCategoryPercent)"
            variant="secondary"
            :empty="dashboard.executive?.Inventory.TopCategoryPercent == null"
            :progress="dashboard.executive?.Inventory.TopCategoryPercent ?? null"
          />
          <DashboardMetric
            label="Top Supplier %"
            :value="formatDashboardPercent(dashboard.executive?.Inventory.TopSupplierPercent)"
            variant="secondary"
            :empty="dashboard.executive?.Inventory.TopSupplierPercent == null"
            :progress="dashboard.executive?.Inventory.TopSupplierPercent ?? null"
          />
        </ExecutiveAttentionCard>
      </div>
    </section>

    <ExecutivePortfolioSummarySection
      :portfolio="dashboard.executive?.Portfolio"
      :loading="dashboard.loading"
    />

    <section class="dashboard-home__section">
      <DashboardSectionHeader
        title="Critical Exposure Lists"
        icon="pi pi-exclamation-triangle"
        domain="alert"
      />
      <div class="dashboard-home__exposures">
        <ExecutiveExposureSection
          title="Top 5 Customers"
          name-header="Customer"
          domain="customer"
          :items="dashboard.executive?.CriticalExposures.TopCustomers ?? []"
          :loading="dashboard.loading"
        />
        <ExecutiveExposureSection
          title="Top 5 Categories"
          name-header="Category"
          domain="inventory"
          :items="dashboard.executive?.CriticalExposures.TopCategories ?? []"
          :loading="dashboard.loading"
        />
        <ExecutiveExposureSection
          title="Top 5 Suppliers"
          name-header="Supplier"
          domain="purchasing"
          :items="dashboard.executive?.CriticalExposures.TopSuppliers ?? []"
          :loading="dashboard.loading"
        />
        <ExecutiveExposureSection
          title="Top 5 Principals"
          name-header="Principal"
          domain="purchasing"
          :items="dashboard.executive?.CriticalExposures.TopPrincipals ?? []"
          :loading="dashboard.loading"
        />
      </div>
    </section>

    <section class="dashboard-home__section">
      <DashboardSectionHeader
        title="Domain Summaries"
        icon="pi pi-compass"
        domain="sales"
      />
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

.dashboard-home__actions {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  flex-shrink: 0;
}

.dashboard-home__header h1 {
  margin: 0;
  font-size: 1.75rem;
  font-weight: 800;
  letter-spacing: -0.02em;
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

.dashboard-home__section {
  margin-bottom: 2rem;
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
  border-radius: var(--dashboard-radius);
  box-shadow: var(--dashboard-shadow-idle);
  padding: 0 1rem;
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

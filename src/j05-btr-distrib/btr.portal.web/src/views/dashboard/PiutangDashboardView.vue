<script setup lang="ts">
import { computed, onMounted } from 'vue'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import AgingPieChart from '@/components/dashboard/AgingPieChart.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatNumber } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

const rankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'CustomerName', header: 'Customer Name' },
  { field: 'OutstandingBalance', header: 'Outstanding Balance' },
]

const rankingRows = computed(
  () => (dashboard.piutang?.TopCustomers ?? []) as Record<string, unknown>[],
)

onMounted(() => {
  void dashboard.loadPiutang()
})
</script>

<template>
  <DashboardDetailLayout
    title="Piutang Dashboard"
    subtitle="Outstanding balance snapshot — open receivables only."
    :loading="dashboard.loading"
    :error="dashboard.error"
    @refresh="dashboard.loadPiutang()"
  >
    <div class="piutang-dashboard__kpi-row">
      <div class="metric">
        <span class="metric__label">Total Piutang</span>
        <span class="metric__value">
          {{ dashboard.piutang ? formatCurrency(dashboard.piutang.TotalPiutang) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Total Customer</span>
        <span class="metric__value">
          {{ dashboard.piutang ? formatNumber(dashboard.piutang.TotalCustomer) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Overdue Customer</span>
        <span class="metric__value">
          {{ dashboard.piutang ? formatNumber(dashboard.piutang.OverdueCustomer) : '—' }}
        </span>
      </div>
    </div>

    <AgingPieChart
      class="piutang-dashboard__section"
      :buckets="dashboard.piutang?.AgingBuckets ?? []"
      :loading="dashboard.loading"
    />

    <Top10RankingTable
      class="piutang-dashboard__section"
      title="Top 10 Outstanding Customers"
      :columns="rankingColumns"
      :rows="rankingRows"
      :loading="dashboard.loading"
      value-field="OutstandingBalance"
      empty-message="No outstanding customer data."
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.piutang-dashboard__kpi-row {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius);
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

.piutang-dashboard__section {
  margin-top: 1rem;
}

@media (max-width: 900px) {
  .piutang-dashboard__kpi-row {
    grid-template-columns: 1fr;
  }
}
</style>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import PostingStatusPieChart from '@/components/dashboard/PostingStatusPieChart.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import WeeklyTrendChart from '@/components/dashboard/WeeklyTrendChart.vue'
import { formatCurrency, formatNumber } from '@/services/formatters'
import type { DashboardSalesWeekTrendItem } from '@/models/dashboard'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

const rankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'PurchaseAmount', header: 'Purchase Amount' },
]

const rankingRows = computed(
  () => (dashboard.purchasing?.TopPrincipalRanking ?? []) as Record<string, unknown>[],
)

const weeklyTrendForChart = computed((): DashboardSalesWeekTrendItem[] =>
  (dashboard.purchasing?.WeeklyTrend ?? []).map((week) => ({
    WeekStart: week.WeekStart,
    WeekEnd: week.WeekEnd,
    WeekLabel: week.WeekLabel,
    RecognizedAmount: week.PurchaseAmount,
  })),
)

onMounted(() => {
  void dashboard.loadPurchasing()
})
</script>

<template>
  <DashboardDetailLayout
    title="Purchasing Dashboard"
    subtitle="Current Month Purchasing Activity (Void invoices excluded)"
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.purchasing?.GeneratedAt ?? null"
    @refresh="dashboard.loadPurchasing()"
  >
    <div class="purchasing-dashboard__kpi-row">
      <div class="metric">
        <span class="metric__label">Grand Total Purchase</span>
        <span class="metric__value">
          {{ dashboard.purchasing ? formatCurrency(dashboard.purchasing.GrandTotalPurchase) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Total Invoice</span>
        <span class="metric__value">
          {{ dashboard.purchasing ? formatNumber(dashboard.purchasing.TotalInvoice) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Pending Posting Invoice Count</span>
        <span class="metric__value">
          {{
            dashboard.purchasing
              ? formatNumber(dashboard.purchasing.PendingPostingInvoiceCount)
              : '—'
          }}
        </span>
      </div>
    </div>

    <WeeklyTrendChart
      class="purchasing-dashboard__section"
      :weekly-trend="weeklyTrendForChart"
      :loading="dashboard.loading"
      title="Weekly Purchase Trend"
      empty-message="No weekly purchase data for the current period."
    />

    <PostingStatusPieChart
      class="purchasing-dashboard__section"
      :items="dashboard.purchasing?.PostingStatusBreakdown ?? []"
      :loading="dashboard.loading"
    />

    <Top10RankingTable
      class="purchasing-dashboard__section"
      title="Top 10 Principal"
      :columns="rankingColumns"
      :rows="rankingRows"
      :loading="dashboard.loading"
      value-field="PurchaseAmount"
      empty-message="No principal ranking data for the current period."
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.purchasing-dashboard__kpi-row {
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

.purchasing-dashboard__section {
  margin-top: 1rem;
}

@media (max-width: 900px) {
  .purchasing-dashboard__kpi-row {
    grid-template-columns: 1fr;
  }
}
</style>

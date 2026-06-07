<script setup lang="ts">
import { computed, onMounted } from 'vue'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import TargetVsAchievementChart from '@/components/dashboard/TargetVsAchievementChart.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import WeeklyTrendChart from '@/components/dashboard/WeeklyTrendChart.vue'
import { formatCurrency, formatPercent } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

const rankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'SalesPersonName', header: 'Salesman' },
  { field: 'CompletedOmzet', header: 'Invoiced Omzet' },
]

const rankingRows = computed(
  () => (dashboard.sales?.TopSalesmanRanking ?? []) as Record<string, unknown>[],
)

onMounted(() => {
  void dashboard.loadSales()
})
</script>

<template>
  <DashboardDetailLayout
    title="Sales Dashboard"
    subtitle="Current month performance — invoiced sales (Faktur)."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.sales?.GeneratedAt ?? null"
    @refresh="dashboard.loadSales()"
  >
    <div class="sales-dashboard__kpi-row">
      <div class="metric">
        <span class="metric__label">Total Target</span>
        <span class="metric__value">
          {{ dashboard.sales ? formatCurrency(dashboard.sales.TotalTarget) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Total Achievement</span>
        <span class="metric__value">
          {{ dashboard.sales ? formatCurrency(dashboard.sales.TotalAchievement) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Achievement %</span>
        <span class="metric__value">
          {{ dashboard.sales ? formatPercent(dashboard.sales.AchievementPercent) : '—' }}
        </span>
      </div>
    </div>

    <TargetVsAchievementChart
      class="sales-dashboard__section"
      :data="dashboard.sales?.TargetVsAchievement ?? null"
      :loading="dashboard.loading"
    />

    <WeeklyTrendChart
      class="sales-dashboard__section"
      :weekly-trend="dashboard.sales?.WeeklyTrend ?? []"
      :loading="dashboard.loading"
    />

    <Top10RankingTable
      class="sales-dashboard__section"
      title="Top 10 Salesman"
      :columns="rankingColumns"
      :rows="rankingRows"
      :loading="dashboard.loading"
      value-field="CompletedOmzet"
      empty-message="No salesman ranking data for the current period."
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.sales-dashboard__kpi-row {
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

.sales-dashboard__section {
  margin-top: 1rem;
}

@media (max-width: 900px) {
  .sales-dashboard__kpi-row {
    grid-template-columns: 1fr;
  }
}
</style>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import TargetVsAchievementChart from '@/components/dashboard/TargetVsAchievementChart.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import WeeklyTrendChart from '@/components/dashboard/WeeklyTrendChart.vue'
import { formatCurrency, formatPercent } from '@/services/formatters'
import type { DashboardSalesRankingItem } from '@/models/dashboard'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()
const sourceLabel = resolveInvestigationSourceLabel('/dashboard/sales')

const rankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'SalesPersonName', header: 'Salesman' },
  { field: 'CompletedOmzet', header: 'Invoiced Omzet' },
]

const rankingRows = computed(
  () => (dashboard.sales?.TopSalesmanRanking ?? []) as Record<string, unknown>[],
)

function onRankingClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardSalesRankingItem
  if (!item.Investigation) return
  navigateToInvestigation(router, item.Investigation, sourceLabel)
}

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
    <div class="sales-dashboard__kpi-row" data-domain="sales">
      <DashboardMetric
        label="Total Target"
        :value="dashboard.sales ? formatCurrency(dashboard.sales.TotalTarget) : '—'"
        :empty="!dashboard.sales"
      />
      <DashboardMetric
        label="Total Achievement"
        :value="dashboard.sales ? formatCurrency(dashboard.sales.TotalAchievement) : '—'"
        :empty="!dashboard.sales"
      />
      <DashboardMetric
        label="Achievement %"
        :value="dashboard.sales ? formatPercent(dashboard.sales.AchievementPercent) : '—'"
        :empty="!dashboard.sales"
        :progress="dashboard.sales?.AchievementPercent ?? null"
      />
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
      clickable
      empty-message="No salesman ranking data for the current period."
      @row-click="onRankingClick"
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
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
  transition: box-shadow var(--dashboard-transition);
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

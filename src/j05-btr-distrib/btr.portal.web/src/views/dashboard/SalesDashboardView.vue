<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import TargetVsAchievementChart from '@/components/dashboard/TargetVsAchievementChart.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import WeeklyTrendChart from '@/components/dashboard/WeeklyTrendChart.vue'
import { formatCurrency, formatCurrencyCompact, formatPercent } from '@/services/formatters'
import type {
  DashboardSalesPrincipalContributionItem,
  DashboardSalesRankingItem,
} from '@/models/dashboard'
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

const principalColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'SupplierId', header: 'SupplierId' },
  { field: 'PrincipalSalesOutAmount', header: 'Principal Sales-Out' },
  { field: 'PrincipalTargetAmount', header: 'Principal Target' },
]

const principalRows = computed(
  () => (dashboard.sales?.PrincipalContribution?.Ranking ?? []) as Record<string, unknown>[],
)

const principalPeriodLabel = computed(() => {
  const contribution = dashboard.sales?.PrincipalContribution
  if (!contribution?.IsAvailable || !contribution.PeriodYear || !contribution.PeriodMonth) {
    return 'Current period'
  }

  const month = new Date(contribution.PeriodYear, contribution.PeriodMonth - 1, 1)
  return new Intl.DateTimeFormat('id-ID', { month: 'long', year: 'numeric' }).format(month)
})

function onRankingClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardSalesRankingItem
  if (!item.Investigation) return
  navigateToInvestigation(router, item.Investigation, sourceLabel)
}

function onPrincipalClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardSalesPrincipalContributionItem
  if (!item.SupplierId) return
  void router.push({
    name: 'principal-performance-dashboard',
    query: { supplierId: item.SupplierId },
  })
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
        :value="dashboard.sales ? formatCurrencyCompact(dashboard.sales.TotalTarget) : '—'"
        :title="dashboard.sales ? formatCurrency(dashboard.sales.TotalTarget) : undefined"
        :empty="!dashboard.sales"
      />
      <DashboardMetric
        label="Total Achievement"
        :value="dashboard.sales ? formatCurrencyCompact(dashboard.sales.TotalAchievement) : '—'"
        :title="dashboard.sales ? formatCurrency(dashboard.sales.TotalAchievement) : undefined"
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

    <section
      class="sales-dashboard__principal"
      data-kpi="PRN-SALES-001"
      aria-label="Principal contribution"
    >
      <div class="sales-dashboard__principal-kpis">
        <DashboardMetric
          label="Principal Sales-Out"
          :value="
            dashboard.sales?.PrincipalContribution?.IsAvailable
              ? formatCurrencyCompact(dashboard.sales.PrincipalContribution.PrincipalSalesOutAmount)
              : '—'
          "
          :title="
            dashboard.sales?.PrincipalContribution?.IsAvailable
              ? formatCurrency(dashboard.sales.PrincipalContribution.PrincipalSalesOutAmount)
              : undefined
          "
          :empty="!dashboard.sales?.PrincipalContribution?.IsAvailable"
        />
        <DashboardMetric
          label="Principal Target"
          :value="
            dashboard.sales?.PrincipalContribution?.PrincipalTargetAmount != null
              ? formatCurrencyCompact(dashboard.sales.PrincipalContribution.PrincipalTargetAmount)
              : '—'
          "
          :title="
            dashboard.sales?.PrincipalContribution?.PrincipalTargetAmount != null
              ? formatCurrency(dashboard.sales.PrincipalContribution.PrincipalTargetAmount)
              : undefined
          "
          :empty="dashboard.sales?.PrincipalContribution?.PrincipalTargetAmount == null"
        />
      </div>

      <p class="sales-dashboard__principal-note">
        {{ principalPeriodLabel }}. Ranking uses Principal Sales-Out only.
        {{ dashboard.sales?.PrincipalContribution?.CompanyHeaderNote }}
      </p>

      <section class="sales-dashboard__disclosure" aria-label="Principal Sales-Out disclosure">
        <h2>Principal Sales-Out disclosure</h2>
        <ul>
          <li
            v-for="statement in dashboard.sales?.PrincipalContribution?.Disclosures ?? []"
            :key="statement"
          >
            {{ statement }}
          </li>
        </ul>
      </section>

      <Top10RankingTable
        title="Principal contribution"
        :columns="principalColumns"
        :rows="principalRows"
        :loading="dashboard.loading"
        value-field="PrincipalSalesOutAmount"
        :currency-fields="['PrincipalTargetAmount']"
        clickable
        click-hint="Open Principal Performance"
        empty-message="No Principal Sales-Out ranking for the current period."
        @row-click="onPrincipalClick"
      />
    </section>
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

.sales-dashboard__section,
.sales-dashboard__principal {
  margin-top: 1rem;
}

.sales-dashboard__principal-kpis {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.sales-dashboard__principal-note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
}

.sales-dashboard__disclosure {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.sales-dashboard__disclosure h2 {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.sales-dashboard__disclosure ul {
  margin: 0;
  padding-left: 1.25rem;
}

.sales-dashboard__disclosure li + li {
  margin-top: 0.35rem;
}

@media (max-width: 900px) {
  .sales-dashboard__kpi-row,
  .sales-dashboard__principal-kpis {
    grid-template-columns: 1fr;
  }
}
</style>

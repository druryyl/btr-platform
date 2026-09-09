<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatCurrencyCompact, formatNumber } from '@/services/formatters'
import type { PrincipalPerformanceRankingItem } from '@/models/dashboard'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()

const rankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'SupplierId', header: 'SupplierId' },
  { field: 'PrincipalSalesOutAmount', header: 'Principal Sales-Out' },
]

const rankingRows = computed(
  () => (dashboard.principalPerformance?.Ranking ?? []) as Record<string, unknown>[],
)

const periodLabel = computed(() => {
  const page = dashboard.principalPerformance
  if (!page?.IsAvailable || !page.PeriodYear || !page.PeriodMonth) {
    return 'Current period'
  }

  const month = new Date(page.PeriodYear, page.PeriodMonth - 1, 1)
  return new Intl.DateTimeFormat('id-ID', { month: 'long', year: 'numeric' }).format(month)
})

function onRankingClick(row: Record<string, unknown>): void {
  const item = row as unknown as PrincipalPerformanceRankingItem
  if (!item.SupplierId) return
  void router.push({
    name: 'principal-performance-evidence',
    query: { supplierId: item.SupplierId },
  })
}

onMounted(() => {
  void dashboard.loadPrincipalPerformance()
})
</script>

<template>
  <DashboardDetailLayout
    title="Principal Performance"
    subtitle="Principal Sales-Out ranked by PRN-SALES-001."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.principalPerformance?.GeneratedAt ?? null"
    @refresh="dashboard.loadPrincipalPerformance()"
  >
    <div class="principal-performance__kpi-row" data-kpi="PRN-SALES-001">
      <DashboardMetric
        label="Principal Sales-Out"
        :value="
          dashboard.principalPerformance?.IsAvailable
            ? formatCurrencyCompact(dashboard.principalPerformance.PrincipalSalesOutAmount)
            : '—'
        "
        :title="
          dashboard.principalPerformance?.IsAvailable
            ? formatCurrency(dashboard.principalPerformance.PrincipalSalesOutAmount)
            : undefined
        "
        :empty="!dashboard.principalPerformance?.IsAvailable"
      />
      <DashboardMetric
        label="Unknown Principal exceptions"
        :value="
          dashboard.principalPerformance
            ? formatNumber(dashboard.principalPerformance.UnknownPrincipalExceptionCount)
            : '—'
        "
        :empty="!dashboard.principalPerformance"
      />
    </div>

    <p class="principal-performance__period">
      {{ periodLabel }}. Ranking uses Principal Sales-Out only.
    </p>

    <section class="principal-performance__disclosure" aria-label="Principal Sales-Out disclosure">
      <h2>Principal Sales-Out disclosure</h2>
      <ul>
        <li
          v-for="statement in dashboard.principalPerformance?.Disclosures ?? []"
          :key="statement"
        >
          {{ statement }}
        </li>
      </ul>
    </section>

    <Top10RankingTable
      class="principal-performance__section"
      title="Principal Sales-Out ranking"
      :columns="rankingColumns"
      :rows="rankingRows"
      :loading="dashboard.loading"
      value-field="PrincipalSalesOutAmount"
      clickable
      click-hint="Open Faktur Item evidence"
      empty-message="No Principal Sales-Out ranking for the current period."
      @row-click="onRankingClick"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.principal-performance__kpi-row {
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

.principal-performance__period {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
}

.principal-performance__disclosure {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.principal-performance__disclosure h2 {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.principal-performance__disclosure ul {
  margin: 0;
  padding-left: 1.25rem;
}

.principal-performance__disclosure li + li {
  margin-top: 0.35rem;
}

.principal-performance__section {
  margin-top: 1rem;
}

@media (max-width: 900px) {
  .principal-performance__kpi-row {
    grid-template-columns: 1fr;
  }
}
</style>

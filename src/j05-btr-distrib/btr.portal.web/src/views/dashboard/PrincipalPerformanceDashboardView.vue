<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatCurrencyCompact, formatNumber, formatPercent } from '@/services/formatters'
import type { PrincipalPerformanceRankingItem } from '@/models/dashboard'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const route = useRoute()
const router = useRouter()

const selectedSupplierId = computed(() => {
  const value = route.query.supplierId
  return typeof value === 'string' ? value.trim() : ''
})

const rankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'SupplierId', header: 'SupplierId' },
  { field: 'PrincipalSalesOutAmount', header: 'Principal Sales-Out' },
]

const targetRankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'PrincipalTargetAmount', header: 'Principal Target' },
  { field: 'AchievementAmount', header: 'Achievement Amount' },
  { field: 'AchievementPercentage', header: 'Achievement %' },
]

const returnRankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'GoodReturnAmount', header: 'Good Return Amount' },
  { field: 'BrokenReturnAmount', header: 'Broken Return Amount' },
  { field: 'TotalReturnAmount', header: 'Total Return Amount' },
  { field: 'ReturnPercentage', header: 'Return %' },
]

const rankingRows = computed(
  () => (dashboard.principalPerformance?.Ranking ?? []) as Record<string, unknown>[],
)

const targetAchievementIsAvailable = computed(
  () => dashboard.principalPerformance?.TargetAchievementIsAvailable === true,
)

const targetRankingRows = computed(() =>
  ((dashboard.principalPerformance?.Ranking ?? []) as PrincipalPerformanceRankingItem[]).map(
    (row) =>
      ({
        ...row,
        AchievementPercentage:
          row.AchievementPercentage != null ? row.AchievementPercentage * 100 : null,
      }) as Record<string, unknown>,
  ),
)

const achievementPercentDisplay = computed(() => {
  const ratio = dashboard.principalPerformance?.AchievementPercentage
  return formatPercent(ratio != null ? ratio * 100 : null)
})

const returnIsAvailable = computed(
  () => dashboard.principalPerformance?.ReturnIsAvailable === true,
)

const returnRankingRows = computed(() =>
  ((dashboard.principalPerformance?.Ranking ?? []) as PrincipalPerformanceRankingItem[]).map(
    (row) =>
      ({
        ...row,
        ReturnPercentage: row.ReturnPercentage != null ? row.ReturnPercentage * 100 : null,
      }) as Record<string, unknown>,
  ),
)

const returnPercentDisplay = computed(() => {
  const ratio = dashboard.principalPerformance?.ReturnPercentage
  return formatPercent(ratio != null ? ratio * 100 : null)
})

const missingTargetNote = computed(() => {
  const count = dashboard.principalPerformance?.MissingTargetExceptionCount ?? 0
  if (!targetAchievementIsAvailable.value && count === 0) return ''
  if (count === 0) return 'All sold Principals have a target record for the period.'
  return (
    `${formatNumber(count)} sold Principal${count === 1 ? '' : 's'} without a target record ` +
    'for the period. Sales-Out is retained.'
  )
})

const periodLabel = computed(() => {
  const page = dashboard.principalPerformance
  if (!page?.IsAvailable || !page.PeriodYear || !page.PeriodMonth) {
    return 'Current period'
  }

  const month = new Date(page.PeriodYear, page.PeriodMonth - 1, 1)
  return new Intl.DateTimeFormat('id-ID', { month: 'long', year: 'numeric' }).format(month)
})

const selectedPrincipal = computed(() => {
  if (!selectedSupplierId.value) return null
  return (
    dashboard.principalPerformance?.Ranking.find(
      (item) => item.SupplierId.localeCompare(selectedSupplierId.value, undefined, { sensitivity: 'accent' }) === 0,
    ) ?? null
  )
})

function onRankingClick(row: Record<string, unknown>): void {
  const item = row as unknown as PrincipalPerformanceRankingItem
  if (!item.SupplierId) return
  void router.push({
    name: 'principal-performance-evidence',
    query: { supplierId: item.SupplierId },
  })
}

function onReturnRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as PrincipalPerformanceRankingItem
  if (!item.SupplierId) return
  void router.push({
    name: 'principal-performance-return-evidence',
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

    <p v-if="selectedSupplierId" class="principal-performance__selected" data-selected-principal>
      Selected Principal:
      {{ selectedPrincipal?.PrincipalName || selectedSupplierId }}.
      Ranking remains Principal Sales-Out.
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

    <section
      class="principal-performance__target"
      data-kpi="PRN-TGT-001"
      aria-label="Principal target and achievement"
    >
      <div class="principal-performance__target-kpis">
        <DashboardMetric
          label="Principal Target"
          :value="
            targetAchievementIsAvailable &&
            dashboard.principalPerformance?.PrincipalTargetAmount != null
              ? formatCurrencyCompact(dashboard.principalPerformance.PrincipalTargetAmount)
              : '—'
          "
          :title="
            targetAchievementIsAvailable &&
            dashboard.principalPerformance?.PrincipalTargetAmount != null
              ? formatCurrency(dashboard.principalPerformance.PrincipalTargetAmount)
              : undefined
          "
          :empty="
            !targetAchievementIsAvailable ||
            dashboard.principalPerformance?.PrincipalTargetAmount == null
          "
        />
        <DashboardMetric
          label="Achievement Amount"
          :value="
            targetAchievementIsAvailable &&
            dashboard.principalPerformance?.AchievementAmount != null
              ? formatCurrencyCompact(dashboard.principalPerformance.AchievementAmount)
              : '—'
          "
          :title="
            targetAchievementIsAvailable &&
            dashboard.principalPerformance?.AchievementAmount != null
              ? formatCurrency(dashboard.principalPerformance.AchievementAmount)
              : undefined
          "
          :empty="
            !targetAchievementIsAvailable ||
            dashboard.principalPerformance?.AchievementAmount == null
          "
        />
        <DashboardMetric
          label="Achievement %"
          :value="targetAchievementIsAvailable ? achievementPercentDisplay : '—'"
          :empty="
            !targetAchievementIsAvailable ||
            dashboard.principalPerformance?.AchievementPercentage == null
          "
        />
      </div>

      <p class="principal-performance__target-note">
        Target versus achievement from stored Principal Target and stored achievement.
        Sales-Out shown above is unchanged.
      </p>
      <p v-if="missingTargetNote" class="principal-performance__target-note">
        {{ missingTargetNote }}
      </p>

      <Top10RankingTable
        title="Principal target and achievement"
        :columns="targetRankingColumns"
        :rows="targetRankingRows"
        :loading="dashboard.loading"
        value-field="PrincipalTargetAmount"
        :currency-fields="['AchievementAmount']"
        percent-field="AchievementPercentage"
        empty-message="No Principal target and achievement for the current period."
      />
    </section>

    <section
      class="principal-performance__returns"
      data-kpi="PRN-RET-003"
      aria-label="Principal returns"
    >
      <div class="principal-performance__returns-kpis">
        <DashboardMetric
          label="Good Return Amount"
          :value="
            returnIsAvailable && dashboard.principalPerformance?.GoodReturnAmount != null
              ? formatCurrencyCompact(dashboard.principalPerformance.GoodReturnAmount)
              : '—'
          "
          :title="
            returnIsAvailable && dashboard.principalPerformance?.GoodReturnAmount != null
              ? formatCurrency(dashboard.principalPerformance.GoodReturnAmount)
              : undefined
          "
          :empty="!returnIsAvailable || dashboard.principalPerformance?.GoodReturnAmount == null"
        />
        <DashboardMetric
          label="Broken Return Amount"
          :value="
            returnIsAvailable && dashboard.principalPerformance?.BrokenReturnAmount != null
              ? formatCurrencyCompact(dashboard.principalPerformance.BrokenReturnAmount)
              : '—'
          "
          :title="
            returnIsAvailable && dashboard.principalPerformance?.BrokenReturnAmount != null
              ? formatCurrency(dashboard.principalPerformance.BrokenReturnAmount)
              : undefined
          "
          :empty="!returnIsAvailable || dashboard.principalPerformance?.BrokenReturnAmount == null"
        />
        <DashboardMetric
          label="Total Return Amount"
          :value="
            returnIsAvailable && dashboard.principalPerformance?.TotalReturnAmount != null
              ? formatCurrencyCompact(dashboard.principalPerformance.TotalReturnAmount)
              : '—'
          "
          :title="
            returnIsAvailable && dashboard.principalPerformance?.TotalReturnAmount != null
              ? formatCurrency(dashboard.principalPerformance.TotalReturnAmount)
              : undefined
          "
          :empty="!returnIsAvailable || dashboard.principalPerformance?.TotalReturnAmount == null"
        />
        <DashboardMetric
          label="Return Percentage"
          :value="returnIsAvailable ? returnPercentDisplay : '—'"
          :empty="!returnIsAvailable || dashboard.principalPerformance?.ReturnPercentage == null"
        />
      </div>

      <p class="principal-performance__returns-note">
        Returns shown separately from stored Principal Target and stored achievement.
        Sales-Out shown above is unchanged. Return Percentage is a quality ratio,
        not a deduction from Sales-Out and not Net Sales.
      </p>

      <Top10RankingTable
        title="Principal returns"
        :columns="returnRankingColumns"
        :rows="returnRankingRows"
        :loading="dashboard.loading"
        value-field="TotalReturnAmount"
        :currency-fields="['GoodReturnAmount', 'BrokenReturnAmount']"
        percent-field="ReturnPercentage"
        clickable
        click-hint="Open Return Item evidence"
        empty-message="No Principal returns for the current period."
        @row-click="onReturnRowClick"
      />
    </section>
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

.principal-performance__period,
.principal-performance__selected {
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

.principal-performance__target {
  margin-top: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.principal-performance__target-kpis {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.principal-performance__target-note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
}

.principal-performance__returns {
  margin-top: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.principal-performance__returns-kpis {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.principal-performance__returns-note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
}

@media (max-width: 900px) {
  .principal-performance__kpi-row {
    grid-template-columns: 1fr;
  }

  .principal-performance__target-kpis {
    grid-template-columns: 1fr;
  }

  .principal-performance__returns-kpis {
    grid-template-columns: 1fr;
  }
}
</style>

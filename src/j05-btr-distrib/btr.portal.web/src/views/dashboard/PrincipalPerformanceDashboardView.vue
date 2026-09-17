<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Select from 'primevue/select'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import PrincipalDataCompletenessIndicator from '@/components/dashboard/PrincipalDataCompletenessIndicator.vue'
import PrincipalPortfolioOverview from '@/components/dashboard/PrincipalPortfolioOverview.vue'
import PrincipalOpportunityRiskBoard from '@/components/dashboard/PrincipalOpportunityRiskBoard.vue'
import PrincipalAchievementGapLeaderboard from '@/components/dashboard/PrincipalAchievementGapLeaderboard.vue'
import PrincipalCoverageReachAnalysis from '@/components/dashboard/PrincipalCoverageReachAnalysis.vue'
import PrincipalReturnRiskAnalysis from '@/components/dashboard/PrincipalReturnRiskAnalysis.vue'
import PrincipalSalesmanDependencyAnalysis from '@/components/dashboard/PrincipalSalesmanDependencyAnalysis.vue'
import PrincipalDetailTable from '@/components/dashboard/PrincipalDetailTable.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatCurrencyCompact, formatNumber } from '@/services/formatters'
import { principalDataCompleteness } from '@/services/principalDataCompleteness'
import { portfolioOverview } from '@/services/principalPortfolio'
import type { PrincipalPerformanceRankingItem, PrincipalSalesmanContributionItem } from '@/models/dashboard'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const route = useRoute()
const router = useRouter()

const selectedSupplierId = computed(() => {
  const value = route.query.supplierId
  return typeof value === 'string' ? value.trim() : ''
})

const defaultRankingKpiId = 'PRN-SALES-001'

const selectedRankingKpiId = ref<string>(defaultRankingKpiId)

const rankingOptions = computed(() => {
  const page = dashboard.principalPerformance
  const options: { label: string; value: string }[] = [
    { label: 'Principal Sales-Out', value: defaultRankingKpiId },
  ]
  for (const opt of page?.SupportingRankingOptions ?? []) {
    options.push({ label: opt.KpiName, value: opt.KpiId })
  }
  return options
})

const rankingValueField = computed(() => {
  switch (selectedRankingKpiId.value) {
    case 'PRN-RET-004':
      return 'ReturnPercentage'
    case 'PRN-TGT-003':
      return 'AchievementPercentage'
    case 'PRN-GRW-001':
      return 'MomGrowthPercentage'
    case 'PRN-GRW-002':
      return 'YoyGrowthPercentage'
    default:
      return 'PrincipalSalesOutAmount'
  }
})

const rankingValueHeader = computed(() => {
  switch (selectedRankingKpiId.value) {
    case 'PRN-RET-004':
      return 'Return %'
    case 'PRN-TGT-003':
      return 'Achievement %'
    case 'PRN-GRW-001':
      return 'MoM Growth %'
    case 'PRN-GRW-002':
      return 'YoY Growth %'
    default:
      return 'Principal Sales-Out'
  }
})

const isPercentageRanking = computed(
  () => selectedRankingKpiId.value !== defaultRankingKpiId,
)

const dynamicRankingColumns = computed(() => [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'SupplierId', header: 'SupplierId' },
  { field: 'RankingValue', header: rankingValueHeader.value },
])

const dynamicRankingRows = computed(() => {
  const source = (dashboard.principalPerformance?.Ranking ?? []) as PrincipalPerformanceRankingItem[]
  const valueField = rankingValueField.value
  const isPercent = isPercentageRanking.value

  const withValue = source
    .map((row) => {
      const raw = (row as unknown as Record<string, unknown>)[valueField]
      const numericValue = typeof raw === 'number' ? raw : null
      return { row, numericValue }
    })
    .filter((entry) => entry.numericValue != null)

  withValue.sort((a, b) => {
    const av = a.numericValue as number
    const bv = b.numericValue as number
    if (bv !== av) return bv - av
    return a.row.SupplierId.localeCompare(b.row.SupplierId, undefined, { sensitivity: 'accent' })
  })

  return withValue.map((entry, index) => {
    const base = entry.row as unknown as Record<string, unknown>
    const displayValue = isPercent && entry.numericValue != null
      ? (entry.numericValue as number) * 100
      : entry.numericValue
    return {
      ...base,
      Rank: index + 1,
      RankingValue: displayValue,
    } as Record<string, unknown>
  })
})

const rankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'SupplierId', header: 'SupplierId' },
  { field: 'PrincipalSalesOutAmount', header: 'Principal Sales-Out' },
]

const rankingRows = computed(
  () => (dashboard.principalPerformance?.Ranking ?? []) as Record<string, unknown>[],
)

const selectedPrincipal = computed(() => {
  if (!selectedSupplierId.value) return null
  return (
    dashboard.principalPerformance?.Ranking.find(
      (item) => item.SupplierId.localeCompare(selectedSupplierId.value, undefined, { sensitivity: 'accent' }) === 0,
    ) ?? null
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

const dataCompleteness = computed(() => {
  const page = dashboard.principalPerformance
  if (!page) return null
  return principalDataCompleteness(page)
})

const portfolioOverviewData = computed(() => {
  const page = dashboard.principalPerformance
  if (!page) return null
  return portfolioOverview(page)
})

const achievementGapRanking = computed(
  () => (dashboard.principalPerformance?.Ranking ?? []) as PrincipalPerformanceRankingItem[],
)

const salesmanContributions = computed(
  () =>
    (dashboard.principalPerformance?.SalesmanContributions ??
      []) as PrincipalSalesmanContributionItem[],
)

function onRankingClick(row: Record<string, unknown>): void {
  const item = row as unknown as PrincipalPerformanceRankingItem
  if (!item.SupplierId) return
  void router.replace({
    name: 'principal-performance-dashboard',
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
      {{ periodLabel }}. Default ranking uses Principal Sales-Out.
    </p>

    <PrincipalDataCompletenessIndicator :completeness="dataCompleteness" />

    <PrincipalPortfolioOverview :overview="portfolioOverviewData" />

    <PrincipalOpportunityRiskBoard
      :ranking="achievementGapRanking"
      :contributions="salesmanContributions"
      :loading="dashboard.loading"
    />

    <div class="principal-performance__pair">
      <PrincipalAchievementGapLeaderboard
        :ranking="achievementGapRanking"
        :loading="dashboard.loading"
      />

      <PrincipalCoverageReachAnalysis
        class="principal-performance__section"
        :ranking="achievementGapRanking"
        :loading="dashboard.loading"
      />
    </div>

    <div class="principal-performance__pair">
      <PrincipalReturnRiskAnalysis
        class="principal-performance__section"
        :ranking="achievementGapRanking"
        :loading="dashboard.loading"
      />

      <PrincipalSalesmanDependencyAnalysis
        class="principal-performance__section"
        :ranking="achievementGapRanking"
        :contributions="salesmanContributions"
        :loading="dashboard.loading"
      />
    </div>

    <div v-if="rankingOptions.length > 1" class="principal-performance__ranking-selector">
      <label for="ranking-kpi-select" class="principal-performance__ranking-label">
        Rank by:
      </label>
      <Select
        id="ranking-kpi-select"
        v-model="selectedRankingKpiId"
        :options="rankingOptions"
        option-label="label"
        option-value="value"
        data-testid="ranking-kpi-select"
        class="principal-performance__ranking-select"
      />
      <span v-if="selectedRankingKpiId !== defaultRankingKpiId" class="principal-performance__ranking-note">
        Supporting ranking only. Principal Sales-Out is unchanged.
      </span>
    </div>

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
      v-show="selectedRankingKpiId === defaultRankingKpiId"
      class="principal-performance__section"
      title="Principal Sales-Out ranking"
      :columns="rankingColumns"
      :rows="rankingRows"
      :loading="dashboard.loading"
      value-field="PrincipalSalesOutAmount"
      clickable
      click-hint="Select to view Salesman contribution"
      selected-field="SupplierId"
      :selected-value="selectedSupplierId"
      empty-message="No Principal Sales-Out ranking for the current period."
      @row-click="onRankingClick"
    />

    <Top10RankingTable
      v-if="selectedRankingKpiId !== defaultRankingKpiId"
      class="principal-performance__section"
      :title="`${rankingValueHeader} ranking`"
      :columns="dynamicRankingColumns"
      :rows="dynamicRankingRows"
      :loading="dashboard.loading"
      :value-field="isPercentageRanking ? 'RankingValue' : rankingValueField"
      :percent-field="isPercentageRanking ? 'RankingValue' : undefined"
      clickable
      click-hint="Select to view Salesman contribution"
      selected-field="SupplierId"
      :selected-value="selectedSupplierId"
      :empty-message="`No ${rankingValueHeader} ranking for the current period.`"
      @row-click="onRankingClick"
    />

    <PrincipalDetailTable
      class="principal-performance__section"
      :ranking="achievementGapRanking"
      :contributions="salesmanContributions"
      :loading="dashboard.loading"
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

.principal-performance__period,
.principal-performance__selected {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
}

.principal-performance__pair {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  align-items: start;
}

.principal-performance__ranking-selector {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 1rem;
  flex-wrap: wrap;
}

.principal-performance__ranking-label {
  font-weight: 600;
  color: var(--p-text-color);
}

.principal-performance__ranking-select {
  min-width: 260px;
}

.principal-performance__ranking-note {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
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

  .principal-performance__pair {
    grid-template-columns: 1fr;
  }
}
</style>

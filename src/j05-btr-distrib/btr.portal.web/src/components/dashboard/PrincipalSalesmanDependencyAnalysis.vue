<script setup lang="ts">
import { computed } from 'vue'
import Chart from 'primevue/chart'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import { createChartOptions } from '@/services/chartLayout'
import type {
  PrincipalPerformanceRankingItem,
  PrincipalSalesmanContributionItem,
} from '@/models/dashboard'
import type { ContributionDistributionEntry, PrincipalDependency } from '@/services/principalContribution'
import { principalDependencies } from '@/services/principalContribution'
import { NO_DATA_LABEL, formatWithNoData, orNoData } from '@/services/principalNoData'
import { formatNumber, formatPercent } from '@/services/formatters'

const SERIES_COLORS = [
  '#3b82f6',
  '#f59e0b',
  '#22c55e',
  '#8b5cf6',
  '#06b6d4',
  '#ef4444',
  '#84cc16',
  '#ec4899',
  '#0ea5e9',
  '#a855f7',
  '#14b8a6',
  '#f97316',
]

const props = defineProps<{
  ranking: PrincipalPerformanceRankingItem[]
  contributions: PrincipalSalesmanContributionItem[]
  loading: boolean
}>()

interface DependencyTableRow {
  supplierId: string
  principalName: string
  hasData: boolean
  topContributorName: string
  contributionPercent: string
  dependencyRatio: string
  dependencyRank: string
  contributingSalesmanCount: string
  contributionCoverage: string
  distribution: ContributionDistributionEntry[]
}

function toDependencyRow(dependency: PrincipalDependency): DependencyTableRow {
  return {
    supplierId: dependency.supplierId,
    principalName: dependency.principalName || dependency.supplierId,
    hasData: true,
    topContributorName: orNoData(dependency.topContributor?.salesPersonName),
    contributionPercent: formatWithNoData(
      dependency.topContributor?.share ?? null,
      formatPercent,
    ),
    dependencyRatio: formatWithNoData(dependency.dependencyRatio, formatPercent),
    dependencyRank: formatWithNoData(dependency.dependencyRank, (value) => `#${formatNumber(value)}`),
    contributingSalesmanCount: formatWithNoData(
      dependency.contributingSalesmanCount,
      formatNumber,
    ),
    contributionCoverage: formatWithNoData(dependency.contributionCoverage, formatPercent),
    distribution: dependency.distribution,
  }
}

function toNoDataRow(item: PrincipalPerformanceRankingItem): DependencyTableRow {
  return {
    supplierId: item.SupplierId,
    principalName: item.PrincipalName || item.SupplierId,
    hasData: false,
    topContributorName: NO_DATA_LABEL,
    contributionPercent: NO_DATA_LABEL,
    dependencyRatio: NO_DATA_LABEL,
    dependencyRank: NO_DATA_LABEL,
    contributingSalesmanCount: NO_DATA_LABEL,
    contributionCoverage: NO_DATA_LABEL,
    distribution: [],
  }
}

const dependencies = computed(() => principalDependencies(props.contributions, props.ranking))

const rows = computed<DependencyTableRow[]>(() => {
  const withData = new Set(dependencies.value.map((dependency) => dependency.supplierId))
  const dataRows = dependencies.value.map(toDependencyRow)
  const noDataRows = props.ranking
    .filter((item) => !withData.has(item.SupplierId))
    .map(toNoDataRow)
  return [...dataRows, ...noDataRows]
})

const chartRows = computed(() => rows.value.filter((row) => row.hasData))

const salesmanSeries = computed(() => {
  const totals = new Map<string, { salesPersonName: string; total: number }>()
  for (const row of chartRows.value) {
    for (const entry of row.distribution) {
      if (entry.share == null) continue
      const existing = totals.get(entry.salesPersonId)
      if (existing) {
        existing.total += entry.share
      } else {
        totals.set(entry.salesPersonId, {
          salesPersonName: entry.salesPersonName,
          total: entry.share,
        })
      }
    }
  }
  return [...totals.entries()]
    .map(([salesPersonId, value]) => ({
      salesPersonId,
      salesPersonName: value.salesPersonName,
      total: value.total,
    }))
    .sort((a, b) => {
      if (b.total !== a.total) return b.total - a.total
      return a.salesPersonName.localeCompare(b.salesPersonName, undefined, { sensitivity: 'accent' })
    })
})

const hasChartData = computed(() => salesmanSeries.value.length > 0)

const chartHeight = computed(() => Math.max(240, chartRows.value.length * 26))

const chartData = computed(() => ({
  labels: chartRows.value.map((row) => row.principalName),
  datasets: salesmanSeries.value.map((series, index) => ({
    label: series.salesPersonName,
    data: chartRows.value.map((row) => {
      const entry = row.distribution.find((item) => item.salesPersonId === series.salesPersonId)
      return entry?.share ?? 0
    }),
    backgroundColor: SERIES_COLORS[index % SERIES_COLORS.length],
    borderRadius: 2,
    stack: 'contribution',
  })),
}))

const chartOptions = computed(() =>
  createChartOptions({
    indexAxis: 'y' as const,
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { parsed: { x: number }; dataset: { label?: string } }) =>
            `${context.dataset.label ?? 'Contributor'}: ${formatPercent(context.parsed.x)} of this Principal's contribution`,
        },
      },
    },
    scales: {
      x: {
        stacked: true,
        min: 0,
        max: 100,
        ticks: {
          callback: (value: string | number) => `${Number(value).toFixed(0)}%`,
        },
      },
      y: {
        stacked: true,
      },
    },
  }),
)
</script>

<template>
  <section
    class="principal-salesman-dependency"
    aria-label="Salesman Dependency Analysis"
    data-testid="salesman-dependency-analysis"
  >
    <h2 class="principal-salesman-dependency__title">Salesman Dependency Analysis</h2>
    <p class="principal-salesman-dependency__note">
      One 100% stacked row per Principal, sorted by Dependency Ratio descending.
      Segments are the contributing salesmen. Contribution % is the Top
      Contributor's normalized share (denominator = SUM(ContributionAmount) for the
      Principal); Contribution Coverage = SUM(ContributionAmount) / Principal
      Sales-Out. Contribution is an analytical distribution, not a reconciliation
      of Principal Sales-Out (PRN-SALES-001). No Low/Medium/High dependency bands
      are applied, and Contributing Salesman Count is supplementary participation
      metadata only; it does not feed classification, ranking, alerts, or insights.
    </p>

    <div v-if="loading" class="principal-salesman-dependency__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <template v-else>
      <div
        v-if="hasChartData"
        class="principal-salesman-dependency__canvas portal-chart-canvas"
        :style="{ height: `${chartHeight}px` }"
        data-testid="salesman-dependency-chart"
      >
        <Chart type="bar" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="principal-salesman-dependency__empty">
        No salesman contribution data for the current period.
      </p>

      <p
        v-if="rows.length > 0"
        class="principal-salesman-dependency__population"
        data-testid="salesman-dependency-population"
      >
        {{ formatNumber(chartRows.length) }} of {{ formatNumber(rows.length) }} ranked
        Principals have a contribution snapshot; Principals without one render
        "{{ NO_DATA_LABEL }}" and are excluded from the stacked bars.
      </p>

      <DataTable
        :value="rows"
        striped-rows
        size="small"
        paginator
        :rows="10"
        class="principal-salesman-dependency__grid"
        data-testid="salesman-dependency-table"
      >
        <template #empty>
          <p class="principal-salesman-dependency__empty">
            No salesman contribution data for the current period.
          </p>
        </template>

        <Column header="Principal">
          <template #body="{ data }">
            {{ data.principalName }}
          </template>
        </Column>
        <Column header="Top Contributor">
          <template #body="{ data }">
            {{ data.topContributorName }}
          </template>
        </Column>
        <Column header="Contribution %" body-class="dash-numeric" header-class="dash-numeric">
          <template #body="{ data }">
            {{ data.contributionPercent }}
          </template>
        </Column>
        <Column header="Dependency Ratio" body-class="dash-numeric" header-class="dash-numeric">
          <template #body="{ data }">
            {{ data.dependencyRatio }}
          </template>
        </Column>
        <Column header="Dependency Rank" body-class="dash-numeric" header-class="dash-numeric">
          <template #body="{ data }">
            {{ data.dependencyRank }}
          </template>
        </Column>
        <Column
          header="Contributing Salesman Count"
          body-class="dash-numeric"
          header-class="dash-numeric"
        >
          <template #body="{ data }">
            {{ data.contributingSalesmanCount }}
          </template>
        </Column>
        <Column
          header="Contribution Coverage"
          body-class="dash-numeric"
          header-class="dash-numeric"
        >
          <template #body="{ data }">
            {{ data.contributionCoverage }}
          </template>
        </Column>
      </DataTable>
    </template>
  </section>
</template>

<style scoped>
.principal-salesman-dependency {
  margin-top: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.principal-salesman-dependency__title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.principal-salesman-dependency__note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.principal-salesman-dependency__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.principal-salesman-dependency__empty {
  margin: 0;
  padding: 1rem 0;
  color: var(--p-text-muted-color);
}

.principal-salesman-dependency__population {
  margin: 0.75rem 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.8125rem;
}

.principal-salesman-dependency__grid :deep(.p-datatable-thead > tr > th) {
  background: var(--dashboard-table-header-bg);
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--p-text-muted-color);
  border-bottom: 1px solid var(--p-surface-200);
}

.principal-salesman-dependency__grid :deep(.p-datatable-tbody > tr:hover) {
  background: var(--dashboard-table-row-hover);
}

.principal-salesman-dependency__grid :deep(.dash-numeric) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
</style>

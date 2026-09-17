<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import { compactAxisTitle, chartLegend, createChartOptions } from '@/services/chartLayout'
import type { PrincipalPerformanceRankingItem } from '@/models/dashboard'
import { opportunityEntries } from '@/services/principalOpportunityRisk'
import { formatWithNoData } from '@/services/principalNoData'
import { formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  ranking: PrincipalPerformanceRankingItem[]
  loading: boolean
}>()

interface CoveragePoint {
  supplierId: string
  principalName: string
  coveragePercent: number
  achievementPercent: number
}

function toPoint(item: PrincipalPerformanceRankingItem): CoveragePoint | null {
  if (item.CoveragePercentage == null || item.AchievementPercentage == null) {
    return null
  }
  return {
    supplierId: item.SupplierId,
    principalName: item.PrincipalName || item.SupplierId,
    coveragePercent: item.CoveragePercentage * 100,
    achievementPercent: item.AchievementPercentage * 100,
  }
}

const opportunitySupplierIds = computed(
  () => new Set(opportunityEntries(props.ranking).map((entry) => entry.supplierId)),
)

const points = computed<CoveragePoint[]>(() => {
  const result: CoveragePoint[] = []
  for (const item of props.ranking) {
    const point = toPoint(item)
    if (point != null) result.push(point)
  }
  return result
})

const opportunityPoints = computed(() =>
  points.value.filter((point) => opportunitySupplierIds.value.has(point.supplierId)),
)

const otherPoints = computed(() =>
  points.value.filter((point) => !opportunitySupplierIds.value.has(point.supplierId)),
)

const hasScatterData = computed(() => points.value.length > 0)

const rows = computed(() => props.ranking)

const chartData = computed(() => ({
  datasets: [
    {
      label: 'Other Principals',
      data: otherPoints.value.map((point) => ({
        x: point.coveragePercent,
        y: point.achievementPercent,
        principalName: point.principalName,
        supplierId: point.supplierId,
      })),
      backgroundColor: '#3b82f6',
      pointRadius: 5,
      pointHoverRadius: 7,
    },
    {
      label: 'Opportunity candidates',
      data: opportunityPoints.value.map((point) => ({
        x: point.coveragePercent,
        y: point.achievementPercent,
        principalName: point.principalName,
        supplierId: point.supplierId,
      })),
      backgroundColor: '#f59e0b',
      pointRadius: 7,
      pointHoverRadius: 9,
      pointStyle: 'triangle' as const,
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      legend: chartLegend.bottom(),
      tooltip: {
        callbacks: {
          label: (context: {
            raw: { x: number; y: number; principalName: string }
          }) => [
            context.raw.principalName,
            `Coverage ${formatPercent(context.raw.x)}`,
            `Achievement ${formatPercent(context.raw.y)}`,
          ],
        },
      },
    },
    scales: {
      x: {
        title: compactAxisTitle('Coverage %'),
        ticks: {
          callback: (value: string | number) => `${Number(value).toFixed(0)}%`,
        },
      },
      y: {
        title: compactAxisTitle('Achievement %'),
        ticks: {
          callback: (value: string | number) => `${Number(value).toFixed(0)}%`,
        },
      },
    },
  }),
)

function principalDisplay(item: PrincipalPerformanceRankingItem): string {
  return item.PrincipalName || item.SupplierId
}

function countDisplay(value: number | null | undefined): string {
  return formatWithNoData(value, formatNumber)
}

function percentDisplay(ratio: number | null | undefined): string {
  return formatWithNoData(ratio, (value) => formatPercent(value * 100))
}
</script>

<template>
  <Card class="principal-coverage-reach">
    <template #title>
      <div class="principal-coverage-reach__title">
        <i class="pi pi-chart-scatter" aria-hidden="true" />
        <span>Coverage &amp; Reach Analysis</span>
      </div>
      <p class="principal-coverage-reach__note">
        Coverage vs Achievement across ranked Principals (stored Coverage
        Percentage). Opportunity candidates are highlighted where Coverage
        percentile &ge; 70 and Achievement percentile &le; 40. Principals missing a
        metric render "No Data" and are excluded from the scatter and from
        percentile computation.
      </p>
    </template>

    <template #content>
      <div v-if="loading" class="principal-coverage-reach__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div
          v-if="hasScatterData"
          class="principal-coverage-reach__canvas portal-chart-canvas"
          data-testid="coverage-reach-scatter"
        >
          <Chart type="scatter" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="principal-coverage-reach__empty">
          No coverage and achievement data for the current period.
        </p>

        <p
          v-if="hasScatterData"
          class="principal-coverage-reach__population"
          data-testid="coverage-reach-population"
        >
          Opportunity candidates: {{ formatNumber(opportunityPoints.length) }} of
          {{ formatNumber(points.length) }} Principals with coverage and achievement
          data. Only ranked Principals carry coverage; non-ranked Principals are not
          shown.
        </p>

        <DataTable
          :value="rows"
          striped-rows
          size="small"
          paginator
          :rows="10"
          class="principal-coverage-reach__grid"
          data-testid="coverage-reach-table"
        >
          <template #empty>
            <p class="principal-coverage-reach__empty">
              No coverage and achievement data for the current period.
            </p>
          </template>

          <Column header="Principal">
            <template #body="{ data }">
              {{ principalDisplay(data) }}
            </template>
          </Column>
          <Column header="Active Customer" body-class="dash-numeric" header-class="dash-numeric">
            <template #body="{ data }">
              {{ countDisplay(data.ActiveCustomerCount) }}
            </template>
          </Column>
          <Column header="Total Customer" body-class="dash-numeric" header-class="dash-numeric">
            <template #body="{ data }">
              {{ countDisplay(data.TotalCustomerCount) }}
            </template>
          </Column>
          <Column header="Coverage %" body-class="dash-numeric" header-class="dash-numeric">
            <template #body="{ data }">
              {{ percentDisplay(data.CoveragePercentage) }}
            </template>
          </Column>
          <Column header="Achievement %" body-class="dash-numeric" header-class="dash-numeric">
            <template #body="{ data }">
              {{ percentDisplay(data.AchievementPercentage) }}
            </template>
          </Column>
        </DataTable>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.principal-coverage-reach {
  border-radius: var(--dashboard-radius);
  box-shadow: var(--dashboard-shadow-idle);
  transition: box-shadow var(--dashboard-transition);
}

.principal-coverage-reach__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.principal-coverage-reach__note {
  margin: 0.5rem 0 0;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
  font-weight: 400;
}

.principal-coverage-reach__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.principal-coverage-reach__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.principal-coverage-reach__population {
  margin: 0.75rem 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.8125rem;
}

.principal-coverage-reach__grid :deep(.p-datatable-thead > tr > th) {
  background: var(--dashboard-table-header-bg);
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--p-text-muted-color);
  border-bottom: 1px solid var(--p-surface-200);
}

.principal-coverage-reach__grid :deep(.p-datatable-tbody > tr:hover) {
  background: var(--dashboard-table-row-hover);
}

.principal-coverage-reach__grid :deep(.dash-numeric) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
</style>

<script setup lang="ts">
import { computed, ref } from 'vue'
import Chart from 'primevue/chart'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import SelectButton from 'primevue/selectbutton'
import ProgressSpinner from 'primevue/progressspinner'
import { createChartOptions } from '@/services/chartLayout'
import type { PrincipalPerformanceRankingItem } from '@/models/dashboard'
import { returnRiskEntries } from '@/services/principalOpportunityRisk'
import { formatWithNoData } from '@/services/principalNoData'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  ranking: PrincipalPerformanceRankingItem[]
  loading: boolean
}>()

type SortMetric = 'amount' | 'percent'

const sortOptions: { label: string; value: SortMetric }[] = [
  { label: 'Return Amount', value: 'amount' },
  { label: 'Return %', value: 'percent' },
]

const sortMetric = ref<SortMetric>('amount')

const riskSupplierIds = computed(
  () => new Set(returnRiskEntries(props.ranking).map((entry) => entry.supplierId)),
)

interface ReturnPoint {
  supplierId: string
  principalName: string
  returnAmount: number
  returnPercent: number
  isRisk: boolean
}

const points = computed<ReturnPoint[]>(() => {
  const result: ReturnPoint[] = []
  for (const item of props.ranking) {
    if (item.TotalReturnAmount == null || item.ReturnPercentage == null) continue
    result.push({
      supplierId: item.SupplierId,
      principalName: item.PrincipalName || item.SupplierId,
      returnAmount: item.TotalReturnAmount,
      returnPercent: item.ReturnPercentage * 100,
      isRisk: riskSupplierIds.value.has(item.SupplierId),
    })
  }
  return result
})

function sortValue(point: ReturnPoint): number {
  return sortMetric.value === 'amount' ? point.returnAmount : point.returnPercent
}

const sortedPoints = computed<ReturnPoint[]>(() =>
  [...points.value].sort((a, b) => {
    const difference = sortValue(b) - sortValue(a)
    if (difference !== 0) return difference
    if (a.principalName !== b.principalName) {
      return a.principalName.localeCompare(b.principalName, undefined, { sensitivity: 'accent' })
    }
    return a.supplierId.localeCompare(b.supplierId)
  }),
)

const populationSize = computed(() => points.value.length)
const riskCount = computed(() => points.value.filter((point) => point.isRisk).length)

const chartHeight = computed(() => Math.max(240, sortedPoints.value.length * 26))

const chartData = computed(() => ({
  labels: sortedPoints.value.map((point) => point.principalName),
  datasets: [
    {
      label: sortMetric.value === 'amount' ? 'Return Amount' : 'Return %',
      data: sortedPoints.value.map((point) =>
        sortMetric.value === 'amount' ? point.returnAmount : point.returnPercent,
      ),
      backgroundColor: sortedPoints.value.map((point) => (point.isRisk ? '#dc2626' : '#3b82f6')),
      borderRadius: 3,
    },
  ],
}))

const chartOptions = computed(() => {
  const isAmount = sortMetric.value === 'amount'
  return createChartOptions({
    indexAxis: 'y' as const,
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { dataIndex: number }) => {
            const point = sortedPoints.value[context.dataIndex]
            if (!point) return ''
            return [
              `Return Amount: ${formatCurrency(point.returnAmount)}`,
              `Return %: ${formatPercent(point.returnPercent)}`,
              point.isRisk
                ? 'Return risk: Return percentile >= 80'
                : 'Return percentile below 80',
            ]
          },
        },
      },
    },
    scales: {
      x: {
        ticks: {
          callback: (value: string | number) =>
            isAmount ? formatCurrency(Number(value)) : `${Number(value).toFixed(0)}%`,
        },
      },
    },
  })
})

function metricValue(item: PrincipalPerformanceRankingItem): number | null {
  if (sortMetric.value === 'amount') {
    return item.TotalReturnAmount ?? null
  }
  return item.ReturnPercentage ?? null
}

const tableRows = computed<PrincipalPerformanceRankingItem[]>(() => {
  const withValue = props.ranking.filter((item) => metricValue(item) != null)
  const withoutValue = props.ranking.filter((item) => metricValue(item) == null)

  withValue.sort((a, b) => {
    const difference = (metricValue(b) as number) - (metricValue(a) as number)
    if (difference !== 0) return difference
    return (a.PrincipalName || a.SupplierId).localeCompare(b.PrincipalName || b.SupplierId, undefined, {
      sensitivity: 'accent',
    })
  })

  return [...withValue, ...withoutValue]
})

function principalDisplay(item: PrincipalPerformanceRankingItem): string {
  return item.PrincipalName || item.SupplierId
}

function currencyDisplay(value: number | null | undefined): string {
  return formatWithNoData(value, (v) => formatCurrency(v))
}

function percentDisplay(ratio: number | null | undefined): string {
  return formatWithNoData(ratio, (v) => formatPercent(v * 100))
}
</script>

<template>
  <section
    class="principal-return-risk"
    aria-label="Return Risk Analysis"
    data-testid="return-risk-analysis"
  >
    <div class="principal-return-risk__header">
      <div class="principal-return-risk__heading">
        <h2 class="principal-return-risk__title">Return Risk Analysis</h2>
        <p class="principal-return-risk__note">
          Ranked by Total Return Amount or Return %. Principals at Return percentile
          &ge; 80 are highlighted as return risk. Return amounts stay independent of
          Principal Sales-Out; Return % is a quality ratio, not a deduction from
          Sales-Out and not Net Sales.
        </p>
      </div>
      <SelectButton
        v-model="sortMetric"
        :options="sortOptions"
        option-label="label"
        option-value="value"
        :allow-empty="false"
        aria-label="Sort return ranking by"
        data-testid="return-risk-sort"
      />
    </div>

    <div v-if="loading" class="principal-return-risk__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <template v-else>
      <div
        v-if="sortedPoints.length > 0"
        class="principal-return-risk__canvas portal-chart-canvas"
        :style="{ height: `${chartHeight}px` }"
        data-testid="return-risk-chart"
      >
        <Chart type="bar" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="principal-return-risk__empty">
        No return data for the current period.
      </p>

      <p
        v-if="sortedPoints.length > 0"
        class="principal-return-risk__population"
        data-testid="return-risk-population"
      >
        Return risk: {{ formatNumber(riskCount) }} of
        {{ formatNumber(populationSize) }} Principals with return data (Return
        percentile &ge; 80).
        <span class="principal-return-risk__legend">
          <span class="principal-return-risk__legend-dot" aria-hidden="true" />
          Return risk
        </span>
        Principals missing a return snapshot render "No Data" and are excluded from
        the ranking and from percentile computation.
      </p>

      <DataTable
        :value="tableRows"
        striped-rows
        size="small"
        paginator
        :rows="10"
        class="principal-return-risk__grid"
        data-testid="return-risk-table"
      >
        <template #empty>
          <p class="principal-return-risk__empty">
            No return data for the current period.
          </p>
        </template>

        <Column header="Principal">
          <template #body="{ data }">
            {{ principalDisplay(data) }}
          </template>
        </Column>
        <Column header="Return Amount" body-class="dash-numeric" header-class="dash-numeric">
          <template #body="{ data }">
            {{ currencyDisplay(data.TotalReturnAmount) }}
          </template>
        </Column>
        <Column header="Return %" body-class="dash-numeric" header-class="dash-numeric">
          <template #body="{ data }">
            {{ percentDisplay(data.ReturnPercentage) }}
          </template>
        </Column>
        <Column header="Good Return" body-class="dash-numeric" header-class="dash-numeric">
          <template #body="{ data }">
            {{ currencyDisplay(data.GoodReturnAmount) }}
          </template>
        </Column>
        <Column header="Broken Return" body-class="dash-numeric" header-class="dash-numeric">
          <template #body="{ data }">
            {{ currencyDisplay(data.BrokenReturnAmount) }}
          </template>
        </Column>
      </DataTable>
    </template>
  </section>
</template>

<style scoped>
.principal-return-risk {
  margin-top: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.principal-return-risk__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  flex-wrap: wrap;
}

.principal-return-risk__heading {
  flex: 1 1 auto;
  min-width: 0;
}

.principal-return-risk__title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.principal-return-risk__note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.principal-return-risk__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.principal-return-risk__empty {
  margin: 0;
  padding: 1rem 0;
  color: var(--p-text-muted-color);
}

.principal-return-risk__population {
  margin: 0.75rem 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.8125rem;
}

.principal-return-risk__legend {
  display: inline-flex;
  align-items: center;
  gap: 0.3rem;
  margin: 0 0.5rem;
  color: var(--p-text-color);
  font-weight: 600;
  white-space: nowrap;
}

.principal-return-risk__legend-dot {
  width: 0.6rem;
  height: 0.6rem;
  border-radius: 999px;
  background: #dc2626;
}

.principal-return-risk__grid :deep(.p-datatable-thead > tr > th) {
  background: var(--dashboard-table-header-bg);
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--p-text-muted-color);
  border-bottom: 1px solid var(--p-surface-200);
}

.principal-return-risk__grid :deep(.p-datatable-tbody > tr:hover) {
  background: var(--dashboard-table-row-hover);
}

.principal-return-risk__grid :deep(.dash-numeric) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}
</style>

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import SelectButton from 'primevue/selectbutton'
import { createChartOptions } from '@/services/chartLayout'
import type {
  FieldActivitySalesmanOverviewRow,
  FieldActivityWilayahBreakdownRow,
} from '@/models/fieldActivity'
import type { DashboardCollectionRankingRow } from '@/models/dashboard'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

const props = withDefaults(
  defineProps<{
    items: FieldActivityWilayahBreakdownRow[]
    salesmanRows?: FieldActivitySalesmanOverviewRow[]
    topOverdueWilayah?: DashboardCollectionRankingRow[]
    collectionAvailable?: boolean
    businessDate?: Date | null
    loading?: boolean
  }>(),
  {
    salesmanRows: () => [],
    topOverdueWilayah: () => [],
    collectionAvailable: true,
    businessDate: null,
    loading: false,
  },
)

type MeasureGroup = 'sales' | 'financial'
type SalesMeasure = 'actualVisits' | 'orders' | 'revenue' | 'orderConversion'
type FinancialMeasure = 'overdueExposure' | 'overdueConcentration'
type Measure = SalesMeasure | FinancialMeasure
type ValueKind = 'number' | 'currency' | 'percent'

const groupOptions: { label: string; value: MeasureGroup }[] = [
  { label: 'Sales Activity', value: 'sales' },
  { label: 'Financial Health', value: 'financial' },
]

const salesMeasureOptions: { label: string; value: SalesMeasure }[] = [
  { label: 'Actual Visits', value: 'actualVisits' },
  { label: 'Orders', value: 'orders' },
  { label: 'Revenue', value: 'revenue' },
  { label: 'Order Conversion', value: 'orderConversion' },
]

const financialMeasureOptions: { label: string; value: FinancialMeasure }[] = [
  { label: 'Overdue Exposure', value: 'overdueExposure' },
  { label: 'Overdue Concentration', value: 'overdueConcentration' },
]

const group = ref<MeasureGroup>('sales')
const measure = ref<Measure>('actualVisits')

watch(group, (next) => {
  measure.value = next === 'sales' ? 'actualVisits' : 'overdueExposure'
})

const measureOptions = computed(() =>
  group.value === 'sales' ? salesMeasureOptions : financialMeasureOptions,
)

interface TerritorySalesAggregate {
  orders: number
  revenue: number
  effectiveCalls: number
}

const salesAggregateByWilayah = computed(() => {
  const grouped = new Map<string, TerritorySalesAggregate>()

  for (const row of props.salesmanRows) {
    if (row.ActualVisits <= 0) continue
    const key = row.WilayahName?.trim() ? row.WilayahName : '(Unknown)'
    const aggregate = grouped.get(key) ?? { orders: 0, revenue: 0, effectiveCalls: 0 }
    aggregate.orders += row.OrdersCount
    aggregate.revenue += Number(row.OmzetAmount)
    aggregate.effectiveCalls += row.EffectiveCalls
    grouped.set(key, aggregate)
  }

  return grouped
})

interface TerritorySalesRow {
  wilayahName: string
  actualVisits: number
  orders: number
  revenue: number
  orderConversion: number | null
}

const salesRows = computed<TerritorySalesRow[]>(() =>
  props.items.map((item) => {
    const aggregate = salesAggregateByWilayah.value.get(item.WilayahName)
    return {
      wilayahName: item.WilayahName,
      actualVisits: item.ActualVisits,
      orders: aggregate?.orders ?? 0,
      revenue: aggregate?.revenue ?? 0,
      orderConversion:
        aggregate && item.ActualVisits > 0
          ? Math.round((aggregate.effectiveCalls / item.ActualVisits) * 1000) / 10
          : null,
    }
  }),
)

interface TerritoryChartRow {
  label: string
  value: number
  valueKind: ValueKind
  unit: string
}

const chartRows = computed<TerritoryChartRow[]>(() => {
  if (group.value === 'financial') {
    return props.topOverdueWilayah.map((row) => ({
      label: row.EntityName,
      value: measure.value === 'overdueConcentration' ? row.PercentOfTotal ?? 0 : row.Amount,
      valueKind: measure.value === 'overdueConcentration' ? 'percent' : 'currency',
      unit: '',
    }))
  }

  return salesRows.value.map((row) => {
    if (measure.value === 'orders') {
      return { label: row.wilayahName, value: row.orders, valueKind: 'number', unit: 'orders' }
    }
    if (measure.value === 'revenue') {
      return { label: row.wilayahName, value: row.revenue, valueKind: 'currency', unit: '' }
    }
    if (measure.value === 'orderConversion') {
      return {
        label: row.wilayahName,
        value: row.orderConversion ?? 0,
        valueKind: 'percent',
        unit: '',
      }
    }
    return { label: row.wilayahName, value: row.actualVisits, valueKind: 'number', unit: 'visits' }
  })
})

const chartHeight = computed(() => Math.max(224, chartRows.value.length * 26))
const hasData = computed(() => chartRows.value.some((row) => row.value > 0))

function formatValue(kind: ValueKind, value: number): string {
  if (kind === 'percent') return formatPercent(value)
  if (kind === 'currency') return formatCurrency(value)
  return formatNumber(value)
}

const chartData = computed(() => ({
  labels: chartRows.value.map((row) => row.label),
  datasets: [
    {
      data: chartRows.value.map((row) => row.value),
      backgroundColor: group.value === 'financial' ? '#ea580c' : '#0d9488',
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    indexAxis: 'y' as const,
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { parsed: { x: number }; dataIndex: number }) => {
            const row = chartRows.value[context.dataIndex]
            if (!row) return ` ${formatNumber(context.parsed.x)}`
            const formatted = formatValue(row.valueKind, context.parsed.x)
            return row.unit ? ` ${formatted} ${row.unit}` : ` ${formatted}`
          },
        },
      },
    },
  }),
)

const financialContextLabel = computed(() => {
  if (!props.businessDate) return 'Outstanding as of today'
  const formatted = new Intl.DateTimeFormat('id-ID', { dateStyle: 'medium' }).format(
    props.businessDate,
  )
  return `Outstanding as of ${formatted}`
})

const emptyMessage = computed(() => {
  if (group.value === 'financial') {
    return props.collectionAvailable
      ? 'No overdue territory data.'
      : 'Collection data is unavailable. Sales views are unaffected.'
  }
  return 'No wilayah visit data.'
})

interface OverdueDistributionRow {
  key: string
  name: string
  amount: number
  percentOfTotal: number | null
  barWidth: number
}

const overdueDistribution = computed<OverdueDistributionRow[]>(() => {
  const rows = props.topOverdueWilayah
  const maxAmount = rows.reduce((max, row) => Math.max(max, row.Amount), 0)

  return rows.map((row, index) => {
    const percent = row.PercentOfTotal
    const barWidth =
      percent != null
        ? Math.min(100, Math.max(0, percent))
        : maxAmount > 0
          ? (row.Amount / maxAmount) * 100
          : 0

    return {
      key: `${row.EntityCode}-${index}`,
      name: row.EntityName,
      amount: row.Amount,
      percentOfTotal: percent,
      barWidth,
    }
  })
})
</script>

<template>
  <Card class="field-activity-wilayah-chart portal-chart-card">
    <template #title>
      <div class="field-activity-wilayah-chart__header">
        <div class="field-activity-wilayah-chart__title">
          <i class="pi pi-map" aria-hidden="true" />
          <span>Territory Performance</span>
        </div>
        <div class="field-activity-wilayah-chart__selectors">
          <SelectButton
            v-model="group"
            :options="groupOptions"
            option-label="label"
            option-value="value"
          />
          <SelectButton
            v-model="measure"
            :options="measureOptions"
            option-label="label"
            option-value="value"
          />
        </div>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="field-activity-wilayah-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <template v-else>
        <p v-if="group === 'financial'" class="field-activity-wilayah-chart__context">
          {{ financialContextLabel }}
        </p>

        <div v-if="hasData" class="field-activity-wilayah-chart__scroll">
          <div
            class="field-activity-wilayah-chart__canvas portal-chart-canvas portal-chart-canvas--fluid"
            :style="{ height: `${chartHeight}px` }"
          >
            <Chart type="bar" :data="chartData" :options="chartOptions" />
          </div>
        </div>
        <p v-else class="field-activity-wilayah-chart__empty">{{ emptyMessage }}</p>

        <div v-if="group === 'financial'" class="field-activity-wilayah-chart__distribution">
          <h4 class="field-activity-wilayah-chart__distribution-title">Overdue Distribution</h4>
          <ul
            v-if="overdueDistribution.length"
            class="field-activity-wilayah-chart__distribution-list"
          >
            <li
              v-for="row in overdueDistribution"
              :key="row.key"
              class="field-activity-wilayah-chart__distribution-item"
            >
              <div class="field-activity-wilayah-chart__distribution-head">
                <span
                  class="field-activity-wilayah-chart__distribution-name"
                  :title="row.name"
                >
                  {{ row.name }}
                </span>
                <span class="field-activity-wilayah-chart__distribution-value">
                  {{ formatCurrency(row.amount) }}
                  <span class="field-activity-wilayah-chart__distribution-percent">
                    {{ formatPercent(row.percentOfTotal) }}
                  </span>
                </span>
              </div>
              <div class="field-activity-wilayah-chart__distribution-track">
                <div
                  class="field-activity-wilayah-chart__distribution-fill"
                  :style="{ width: `${row.barWidth}%` }"
                />
              </div>
            </li>
          </ul>
          <p v-else class="field-activity-wilayah-chart__empty">
            {{
              collectionAvailable
                ? 'No overdue territory data.'
                : 'Collection data is unavailable. Sales views are unaffected.'
            }}
          </p>
        </div>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.field-activity-wilayah-chart__header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.field-activity-wilayah-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.field-activity-wilayah-chart__selectors {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.5rem;
}

.field-activity-wilayah-chart__context {
  margin: 0 0 0.5rem;
  font-size: 0.6875rem;
  font-weight: 700;
  letter-spacing: 0.08em;
  text-transform: uppercase;
  color: var(--p-text-muted-color, #94a3b8);
}

.field-activity-wilayah-chart__scroll {
  max-height: 480px;
  overflow-y: auto;
}

.field-activity-wilayah-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.field-activity-wilayah-chart__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
}

.field-activity-wilayah-chart__distribution {
  margin-top: 1rem;
  padding-top: 0.875rem;
  border-top: 1px solid var(--p-content-border-color);
}

.field-activity-wilayah-chart__distribution-title {
  margin: 0 0 0.5rem;
  font-size: 0.9375rem;
}

.field-activity-wilayah-chart__distribution-list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
}

.field-activity-wilayah-chart__distribution-head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.5rem;
  font-size: 0.8125rem;
}

.field-activity-wilayah-chart__distribution-name {
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  color: var(--p-text-color);
}

.field-activity-wilayah-chart__distribution-value {
  flex: none;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-muted-color, #64748b);
}

.field-activity-wilayah-chart__distribution-percent {
  margin-left: 0.375rem;
  font-weight: 700;
  color: var(--domain-collection-color, #ea580c);
}

.field-activity-wilayah-chart__distribution-track {
  margin-top: 0.25rem;
  height: 0.375rem;
  border-radius: 999px;
  background: var(--kpi-status-unknown-bg, #f1f5f9);
  overflow: hidden;
}

.field-activity-wilayah-chart__distribution-fill {
  height: 100%;
  border-radius: 999px;
  background: var(--domain-collection-color, #ea580c);
  transition: width var(--dashboard-transition, 175ms ease);
}
</style>

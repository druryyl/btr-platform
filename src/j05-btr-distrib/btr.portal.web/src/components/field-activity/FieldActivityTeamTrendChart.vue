<script setup lang="ts">
import { computed, ref } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import SelectButton from 'primevue/selectbutton'
import ProgressSpinner from 'primevue/progressspinner'
import { chartLegend, createChartOptions } from '@/services/chartLayout'
import type { FieldActivityTrendPoint } from '@/models/fieldActivity'
import { formatCurrency, formatPercent } from '@/services/formatters'

const props = defineProps<{
  last7Days: FieldActivityTrendPoint[]
  last30Days: FieldActivityTrendPoint[]
  loading?: boolean
}>()

const horizonOptions = [
  { label: '7 days', value: '7' },
  { label: '30 days', value: '30' },
]

const horizon = ref<'7' | '30'>('7')

const points = computed(() => (horizon.value === '7' ? props.last7Days : props.last30Days))

const hasData = computed(() => points.value.length > 0)

const chartData = computed(() => ({
  labels: points.value.map((p) => p.TrendDate.slice(5)),
  datasets: [
    {
      label: 'Visit Execution %',
      data: points.value.map((p) => p.VisitExecutionPercent ?? 0),
      borderColor: '#2563eb',
      tension: 0.25,
      yAxisID: 'y',
    },
    {
      label: 'Effective Call Rate',
      data: points.value.map((p) => p.EffectiveCallRate ?? 0),
      borderColor: '#16a34a',
      tension: 0.25,
      yAxisID: 'y',
    },
    {
      label: 'Orders',
      data: points.value.map((p) => p.OrdersCount),
      borderColor: '#9333ea',
      tension: 0.25,
      yAxisID: 'y1',
    },
    {
      label: 'Order Value',
      data: points.value.map((p) => Number(p.OmzetAmount)),
      borderColor: '#ea580c',
      tension: 0.25,
      yAxisID: 'y1',
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      legend: chartLegend.bottom(),
    },
    scales: {
      y: {
        position: 'left' as const,
        ticks: {
          callback: (value: string | number) => formatPercent(Number(value)),
        },
      },
      y1: {
        position: 'right' as const,
        grid: { drawOnChartArea: false },
        ticks: {
          callback: (value: string | number) =>
            Number(value) > 1000 ? formatCurrency(Number(value)) : String(value),
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="field-activity-team-trend-chart portal-chart-card">
    <template #title>
      <div class="field-activity-team-trend-chart__header">
        <div class="field-activity-team-trend-chart__title">
          <i class="pi pi-chart-line" aria-hidden="true" />
          <span>Team Execution Trends</span>
        </div>
        <SelectButton
          v-model="horizon"
          :options="horizonOptions"
          option-label="label"
          option-value="value"
        />
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="field-activity-team-trend-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <div
        v-else-if="hasData"
        class="field-activity-team-trend-chart__canvas portal-chart-canvas"
      >
        <Chart type="line" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="field-activity-team-trend-chart__empty">No trend data available.</p>
    </template>
  </Card>
</template>

<style scoped>
.field-activity-team-trend-chart__header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.field-activity-team-trend-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.field-activity-team-trend-chart__canvas {
  height: 320px;
}

.field-activity-team-trend-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.field-activity-team-trend-chart__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
}
</style>

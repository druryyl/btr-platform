<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { chartLegend, compactAxisTitle, createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardInventoryForecastLevelItem } from '@/models/dashboard'

const props = defineProps<{
  projectedLevel: DashboardInventoryForecastLevelItem[]
  loading: boolean
}>()

const hasData = computed(() => (props.projectedLevel?.length ?? 0) > 0)

const chartData = computed(() => ({
  labels: props.projectedLevel.map((row) => `Day ${row.HorizonDay}`),
  datasets: [
    {
      type: 'line' as const,
      label: 'Projected inventory value',
      data: props.projectedLevel.map((row) => row.ProjectedInventoryValue),
      borderColor: '#6366f1',
      backgroundColor: 'rgba(99, 102, 241, 0.15)',
      tension: 0.2,
      fill: true,
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      legend: chartLegend.bottom(),
      tooltip: {
        callbacks: {
          label: (context: { dataset: { label?: string }; parsed: { y: number } }) =>
            ` ${context.dataset.label ?? ''}: ${formatCurrency(context.parsed.y)}`,
        },
      },
    },
    scales: {
      x: {
        title: compactAxisTitle('Planning horizon (days)'),
      },
      y: {
        ticks: {
          callback: (value: string | number) => formatCurrency(Number(value)),
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="inventory-forecast-level-chart portal-chart-card">
    <template #title>
      <div class="inventory-forecast-level-chart__title">
        <i class="pi pi-chart-line" aria-hidden="true" />
        <span>Forecast Inventory Level</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="inventory-forecast-level-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="inventory-forecast-level-chart__canvas portal-chart-canvas portal-chart-canvas--tall">
          <Chart type="line" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="inventory-forecast-level-chart__empty">
          No projected inventory level data available.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.inventory-forecast-level-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.inventory-forecast-level-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-forecast-level-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

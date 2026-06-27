<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { chartLegend, compactAxisTitle, createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import { formatNumber } from '@/services/formatters'
import type { DashboardInventoryForecastDailyConsumptionItem } from '@/models/dashboard'

const props = defineProps<{
  dailyConsumption: DashboardInventoryForecastDailyConsumptionItem[]
  loading: boolean
}>()

const adcReference = computed(
  () => props.dailyConsumption.find((day) => day.AdcReference > 0)?.AdcReference ?? 0,
)

const hasData = computed(
  () => props.dailyConsumption?.some((day) => day.UnitsSold > 0) ?? false,
)

const chartData = computed(() => ({
  labels: props.dailyConsumption.map((day) => String(day.DayIndex)),
  datasets: [
    {
      type: 'bar' as const,
      label: 'Daily units sold',
      data: props.dailyConsumption.map((day) => day.UnitsSold),
      backgroundColor: 'rgba(34, 197, 94, 0.85)',
      borderRadius: 2,
      order: 2,
    },
    {
      type: 'line' as const,
      label: '30-day ADC reference',
      data: props.dailyConsumption.map(() => adcReference.value),
      borderColor: '#0ea5e9',
      borderDash: [6, 4],
      borderWidth: 2,
      pointRadius: 0,
      fill: false,
      order: 1,
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
            ` ${context.dataset.label ?? ''}: ${formatNumber(context.parsed.y)}`,
        },
      },
    },
    scales: {
      x: {
        title: compactAxisTitle('Day (30-day lookback)'),
      },
      y: {
        ticks: {
          callback: (value: string | number) => formatNumber(Number(value)),
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="inventory-consumption-trend-chart portal-chart-card">
    <template #title>
      <div class="inventory-consumption-trend-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Consumption Trend</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="inventory-consumption-trend-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData || dailyConsumption.length > 0" class="inventory-consumption-trend-chart__canvas portal-chart-canvas portal-chart-canvas--tall">
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="inventory-consumption-trend-chart__empty">
          No daily consumption data for the lookback window.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.inventory-consumption-trend-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.inventory-consumption-trend-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-consumption-trend-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

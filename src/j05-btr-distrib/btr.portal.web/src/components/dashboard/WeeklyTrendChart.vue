<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardSalesWeekTrendItem } from '@/models/dashboard'

const props = defineProps<{
  weeklyTrend: DashboardSalesWeekTrendItem[]
  loading: boolean
}>()

const hasWeeklyData = computed(() =>
  props.weeklyTrend?.some((week) => week.RecognizedAmount > 0) ?? false,
)

const chartData = computed(() => ({
  labels: props.weeklyTrend.map((week) => week.WeekLabel),
  datasets: [
    {
      label: 'Omzet diakui',
      data: props.weeklyTrend.map((week) => week.RecognizedAmount),
      fill: false,
      borderColor: '#22c55e',
      backgroundColor: 'rgba(34, 197, 94, 0.15)',
      tension: 0.3,
      pointRadius: 4,
      pointHoverRadius: 6,
    },
  ],
}))

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: false,
    },
    tooltip: {
      callbacks: {
        label: (context: { parsed: { y: number } }) =>
          ` ${formatCurrency(context.parsed.y)}`,
      },
    },
  },
  scales: {
    x: {
      ticks: {
        maxRotation: 45,
        minRotation: 0,
      },
    },
    y: {
      ticks: {
        callback: (value: string | number) => formatCurrency(Number(value)),
      },
    },
  },
}))
</script>

<template>
  <Card class="weekly-trend-chart">
    <template #title>
      <div class="weekly-trend-chart__title">
        <i class="pi pi-chart-line" aria-hidden="true" />
        <span>Weekly Trend</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="weekly-trend-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasWeeklyData" class="weekly-trend-chart__canvas">
          <Chart type="line" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="weekly-trend-chart__empty">No weekly omzet data for the current period.</p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.weekly-trend-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.weekly-trend-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.weekly-trend-chart__canvas {
  height: 280px;
}

.weekly-trend-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

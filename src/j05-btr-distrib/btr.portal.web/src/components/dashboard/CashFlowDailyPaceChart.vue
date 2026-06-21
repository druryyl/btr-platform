<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardCashFlowDailyPaceItem } from '@/models/dashboard'

const props = defineProps<{
  dailyPace: DashboardCashFlowDailyPaceItem[]
  loading: boolean
}>()

const hasData = computed(
  () => props.dailyPace?.some((day) => day.IsElapsed && day.ActualCashAmount > 0) ?? false,
)

const projectedDaily = computed(
  () =>
    props.dailyPace.find((day) => day.ProjectedDailyCashAmount > 0)?.ProjectedDailyCashAmount ?? 0,
)

const chartData = computed(() => ({
  labels: props.dailyPace.map((day) => String(day.DayOfMonth)),
  datasets: [
    {
      type: 'bar' as const,
      label: 'Daily cash',
      data: props.dailyPace.map((day) => (day.IsElapsed ? day.ActualCashAmount : 0)),
      backgroundColor: props.dailyPace.map((day) =>
        day.IsElapsed ? 'rgba(34, 197, 94, 0.85)' : 'rgba(148, 163, 184, 0.35)',
      ),
      borderRadius: 2,
      order: 2,
    },
    {
      type: 'line' as const,
      label: 'MTD daily cash average',
      data: props.dailyPace.map(() => projectedDaily.value),
      borderColor: '#0ea5e9',
      borderDash: [6, 4],
      borderWidth: 2,
      pointRadius: 0,
      fill: false,
      order: 1,
    },
  ],
}))

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      display: true,
      position: 'bottom' as const,
    },
    tooltip: {
      callbacks: {
        label: (context: { dataset: { label?: string }; parsed: { y: number } }) =>
          ` ${context.dataset.label ?? ''}: ${formatCurrency(context.parsed.y)}`,
      },
    },
  },
  scales: {
    x: {
      title: {
        display: true,
        text: 'Day of month',
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
  <Card class="cash-flow-daily-pace-chart">
    <template #title>
      <div class="cash-flow-daily-pace-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Daily Collection Pace</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="cash-flow-daily-pace-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData || dailyPace.length > 0" class="cash-flow-daily-pace-chart__canvas">
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="cash-flow-daily-pace-chart__empty">
          No daily cash pace data for the current period.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.cash-flow-daily-pace-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.cash-flow-daily-pace-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.cash-flow-daily-pace-chart__canvas {
  height: 320px;
}

.cash-flow-daily-pace-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

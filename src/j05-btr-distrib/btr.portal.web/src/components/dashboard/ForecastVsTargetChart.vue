<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardSalesForecastVsTarget } from '@/models/dashboard'

const props = defineProps<{
  data: DashboardSalesForecastVsTarget | null
  loading: boolean
}>()

const hasData = computed(
  () =>
    (props.data?.TargetAmount ?? 0) > 0 ||
    (props.data?.CurrentAmount ?? 0) > 0 ||
    (props.data?.ForecastAmount ?? 0) > 0,
)

const chartData = computed(() => ({
  labels: ['Target', 'Current', 'Forecast'],
  datasets: [
    {
      label: 'Amount',
      data: [
        props.data?.TargetAmount ?? 0,
        props.data?.CurrentAmount ?? 0,
        props.data?.ForecastAmount ?? 0,
      ],
      backgroundColor: ['#6366f1', '#22c55e', '#0ea5e9'],
      borderRadius: 4,
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { parsed: { y: number } }) =>
            ` ${formatCurrency(context.parsed.y)}`,
        },
      },
    },
    scales: {
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
  <Card class="forecast-vs-target-chart portal-chart-card">
    <template #title>
      <div class="forecast-vs-target-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Forecast vs Target</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="forecast-vs-target-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="forecast-vs-target-chart__canvas portal-chart-canvas">
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="forecast-vs-target-chart__empty">
          No forecast comparison data for the current period.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.forecast-vs-target-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.forecast-vs-target-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.forecast-vs-target-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

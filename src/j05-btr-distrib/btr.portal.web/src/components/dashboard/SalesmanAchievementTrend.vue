<script setup lang="ts">
import { computed } from 'vue'
import Chart from 'primevue/chart'
import { createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import { formatPercent } from '@/services/formatters'
import type { SalesmanAchievementTrendPoint } from '@/models/dashboard'

const props = withDefaults(
  defineProps<{
    points: SalesmanAchievementTrendPoint[]
    loading: boolean
    emptyMessage?: string
  }>(),
  {
    emptyMessage: 'Trend available after month-end snapshots accumulate.',
  },
)

const hasTrendData = computed(() => (props.points?.length ?? 0) >= 2)

const chartData = computed(() => ({
  labels: props.points.map((point) => point.PeriodLabel),
  datasets: [
    {
      label: 'Achievement %',
      data: props.points.map((point) => point.AchievementPercent ?? null),
      fill: false,
      borderColor: '#3b82f6',
      backgroundColor: 'rgba(59, 130, 246, 0.15)',
      tension: 0.3,
      pointRadius: 4,
      pointHoverRadius: 6,
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { parsed: { y: number | null } }) =>
            context.parsed.y != null ? ` ${formatPercent(context.parsed.y)}` : ' —',
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
          callback: (value: string | number) => formatPercent(Number(value)),
        },
      },
    },
  }),
)
</script>

<template>
  <div class="salesman-achievement-trend">
    <div v-if="loading" class="salesman-achievement-trend__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <template v-else>
      <div v-if="hasTrendData" class="salesman-achievement-trend__canvas portal-chart-canvas">
        <Chart type="line" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="salesman-achievement-trend__empty">{{ emptyMessage }}</p>
    </template>
  </div>
</template>

<style scoped>
.salesman-achievement-trend__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.salesman-achievement-trend__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

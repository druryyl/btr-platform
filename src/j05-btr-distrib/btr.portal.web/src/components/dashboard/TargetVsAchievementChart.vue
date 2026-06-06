<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardSalesTargetVsAchievement } from '@/models/dashboard'

const props = defineProps<{
  data: DashboardSalesTargetVsAchievement | null
  loading: boolean
}>()

const hasData = computed(
  () =>
    (props.data?.TargetAmount ?? 0) > 0 || (props.data?.AchievementAmount ?? 0) > 0,
)

const chartData = computed(() => ({
  labels: ['Target', 'Achievement'],
  datasets: [
    {
      label: 'Amount',
      data: [props.data?.TargetAmount ?? 0, props.data?.AchievementAmount ?? 0],
      backgroundColor: ['#6366f1', '#22c55e'],
      borderRadius: 4,
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
    y: {
      ticks: {
        callback: (value: string | number) => formatCurrency(Number(value)),
      },
    },
  },
}))
</script>

<template>
  <Card class="target-vs-achievement-chart">
    <template #title>
      <div class="target-vs-achievement-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Target vs Achievement</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="target-vs-achievement-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="target-vs-achievement-chart__canvas">
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="target-vs-achievement-chart__empty">
          No target or achievement data for the current period.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.target-vs-achievement-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.target-vs-achievement-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.target-vs-achievement-chart__canvas {
  height: 280px;
}

.target-vs-achievement-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

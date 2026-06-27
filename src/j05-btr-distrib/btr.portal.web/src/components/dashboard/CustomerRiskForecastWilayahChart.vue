<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCustomerRiskForecastWilayahItem } from '@/models/dashboard'

const props = defineProps<{
  wilayah: DashboardCustomerRiskForecastWilayahItem[]
  loading: boolean
}>()

const sortedWilayah = computed(() =>
  [...(props.wilayah ?? [])].sort((a, b) => a.SortOrder - b.SortOrder),
)

const hasData = computed(() =>
  sortedWilayah.value.some((item) => item.ElevatedRiskCustomerCount > 0),
)

const chartHeight = computed(() => Math.max(224, sortedWilayah.value.length * 26))

const chartData = computed(() => ({
  labels: sortedWilayah.value.map((item) => item.WilayahName),
  datasets: [
    {
      data: sortedWilayah.value.map((item) => item.ElevatedRiskCustomerCount),
      backgroundColor: '#ef4444',
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    indexAxis: 'y' as const,
    plugins: {
      tooltip: {
        callbacks: {
          label: (context: { parsed: { x: number } }) =>
            ` ${context.parsed.x} elevated-risk customers`,
        },
      },
    },
    scales: {
      x: {
        beginAtZero: true,
        ticks: {
          precision: 0,
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="customer-risk-forecast-wilayah-chart portal-chart-card">
    <template #title>
      <div class="customer-risk-forecast-wilayah-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Top Wilayah by Elevated Risk</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-risk-forecast-wilayah-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div
          v-if="hasData"
          class="customer-risk-forecast-wilayah-chart__canvas portal-chart-canvas portal-chart-canvas--fluid"
          :style="{ height: `${chartHeight}px` }"
        >
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="customer-risk-forecast-wilayah-chart__empty">
          No wilayah concentration data available.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-risk-forecast-wilayah-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-risk-forecast-wilayah-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-risk-forecast-wilayah-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

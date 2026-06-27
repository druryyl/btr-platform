<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { chartLegend, createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCollectionOptimizationActionDistItem } from '@/models/dashboard'

const props = defineProps<{
  distribution: DashboardCollectionOptimizationActionDistItem[]
  loading: boolean
}>()

const chartData = computed(() => ({
  labels: (props.distribution ?? []).map((item) => item.ActionCategoryLabel),
  datasets: [
    {
      data: (props.distribution ?? []).map((item) => item.CustomerCount),
      backgroundColor: [
        '#991b1b',
        '#ef4444',
        '#f97316',
        '#0ea5e9',
        '#eab308',
        '#6366f1',
        '#64748b',
        '#22c55e',
        '#94a3b8',
      ],
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    plugins: { legend: chartLegend.right() },
  }),
)
</script>

<template>
  <Card class="collection-optimization-action-chart portal-chart-card">
    <template #title>Actions by Category</template>
    <template #content>
      <div v-if="loading" class="collection-optimization-action-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <div v-else-if="distribution.length > 0" class="collection-optimization-action-chart__canvas portal-chart-canvas portal-chart-canvas--short">
        <Chart type="doughnut" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="collection-optimization-action-chart__empty">No action distribution data.</p>
    </template>
  </Card>
</template>

<style scoped>
.collection-optimization-action-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.collection-optimization-action-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

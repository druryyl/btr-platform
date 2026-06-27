<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { createChartOptions } from '@/services/chartLayout'
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
      label: 'Impact (IDR)',
      data: (props.distribution ?? []).map((item) => item.ImpactTotal),
      backgroundColor: '#f97316',
      borderRadius: 4,
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    scales: { y: { beginAtZero: true } },
  }),
)
</script>

<template>
  <Card class="collection-optimization-impact-chart portal-chart-card">
    <template #title>Impact by Action Category</template>
    <template #content>
      <div v-if="loading" class="collection-optimization-impact-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <div v-else-if="distribution.length > 0" class="collection-optimization-impact-chart__canvas portal-chart-canvas portal-chart-canvas--short">
        <Chart type="bar" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="collection-optimization-impact-chart__empty">No impact data.</p>
    </template>
  </Card>
</template>

<style scoped>
.collection-optimization-impact-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.collection-optimization-impact-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

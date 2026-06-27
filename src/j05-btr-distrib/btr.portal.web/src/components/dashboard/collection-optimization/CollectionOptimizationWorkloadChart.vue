<script setup lang="ts">
import { computed, ref } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { createChartOptions } from '@/services/chartLayout'
import SelectButton from 'primevue/selectbutton'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCollectionOptimizationWorkloadItem } from '@/models/dashboard'

const props = defineProps<{
  workload: DashboardCollectionOptimizationWorkloadItem[]
  loading: boolean
}>()

const workloadType = ref('Wilayah')
const typeOptions = ['Wilayah', 'Salesman']

const filtered = computed(() =>
  (props.workload ?? []).filter((item) => item.WorkloadType === workloadType.value),
)

const chartData = computed(() => ({
  labels: filtered.value.map((item) => item.EntityLabel),
  datasets: [
    {
      label: 'Actions',
      data: filtered.value.map((item) => item.ActionCount),
      backgroundColor: '#6366f1',
      borderRadius: 4,
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    indexAxis: 'y' as const,
    scales: { x: { beginAtZero: true, ticks: { precision: 0 } } },
  }),
)
</script>

<template>
  <Card class="collection-optimization-workload-chart portal-chart-card">
    <template #title>
      <div class="collection-optimization-workload-chart__header">
        <span>Workload</span>
        <SelectButton v-model="workloadType" :options="typeOptions" size="small" />
      </div>
    </template>
    <template #content>
      <div v-if="loading" class="collection-optimization-workload-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <div v-else-if="filtered.length > 0" class="collection-optimization-workload-chart__canvas portal-chart-canvas portal-chart-canvas--short">
        <Chart type="bar" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="collection-optimization-workload-chart__empty">No workload data.</p>
    </template>
  </Card>
</template>

<style scoped>
.collection-optimization-workload-chart__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.collection-optimization-workload-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.collection-optimization-workload-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

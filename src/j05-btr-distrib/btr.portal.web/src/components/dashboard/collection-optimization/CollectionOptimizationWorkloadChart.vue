<script setup lang="ts">
import { computed, ref } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { createChartOptions } from '@/services/chartLayout'
import SelectButton from 'primevue/selectbutton'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCollectionOptimizationWorkloadItem } from '@/models/dashboard'
import {
  CU03_ROUTED_SALESMAN_NOTE,
  CU03_SALESMAN_WORKLOAD_LABEL,
  CU03_SALESMAN_WORKLOAD_TYPE,
} from '@/services/collectionOptimizationRouting'

const props = defineProps<{
  workload: DashboardCollectionOptimizationWorkloadItem[]
  loading: boolean
}>()

const workloadType = ref('Wilayah')
const typeOptions = [
  { label: 'Wilayah', value: 'Wilayah' },
  { label: CU03_SALESMAN_WORKLOAD_LABEL, value: CU03_SALESMAN_WORKLOAD_TYPE },
]

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
        <div>
          <span>Workload</span>
          <p
            v-if="workloadType === CU03_SALESMAN_WORKLOAD_TYPE"
            class="collection-optimization-workload-chart__note"
          >
            {{ CU03_ROUTED_SALESMAN_NOTE }}
          </p>
        </div>
        <SelectButton
          v-model="workloadType"
          :options="typeOptions"
          option-label="label"
          option-value="value"
          size="small"
        />
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
.collection-optimization-workload-chart__note {
  margin: 0.35rem 0 0;
  font-size: 0.875rem;
  font-weight: 400;
  line-height: 1.5;
  color: var(--p-text-muted-color);
}

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

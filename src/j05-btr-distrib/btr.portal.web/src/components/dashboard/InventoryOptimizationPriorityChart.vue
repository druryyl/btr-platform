<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardInventoryOptimizationPriorityDistItem } from '@/models/dashboard'

const props = defineProps<{
  distribution: DashboardInventoryOptimizationPriorityDistItem[]
  loading: boolean
}>()

const chartData = computed(() => ({
  labels: (props.distribution ?? []).map((item) => item.Category),
  datasets: [
    {
      label: 'Actions',
      data: (props.distribution ?? []).map((item) => item.ActionCount),
      backgroundColor: ['#ef4444', '#f97316', '#eab308', '#94a3b8'],
      borderRadius: 4,
    },
  ],
}))

const chartOptions = computed(() => ({
  indexAxis: 'y' as const,
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: {
    x: { beginAtZero: true, ticks: { precision: 0 } },
  },
}))
</script>

<template>
  <Card class="inventory-optimization-priority-chart">
    <template #title>Priority Score Distribution</template>
    <template #content>
      <div v-if="loading" class="inventory-optimization-priority-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <div v-else-if="distribution.length > 0" class="inventory-optimization-priority-chart__canvas">
        <Chart type="bar" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="inventory-optimization-priority-chart__empty">No priority distribution data.</p>
    </template>
  </Card>
</template>

<style scoped>
.inventory-optimization-priority-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-optimization-priority-chart__canvas {
  height: 220px;
}

.inventory-optimization-priority-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

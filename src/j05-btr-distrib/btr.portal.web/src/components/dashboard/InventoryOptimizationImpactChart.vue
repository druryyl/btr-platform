<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'

const props = defineProps<{
  purchaseImpactIdr: number
  deferrableSpendIdr: number
  recoverableCapitalIdr: number
  loading: boolean
}>()

const chartData = computed(() => ({
  labels: ['Purchase', 'Deferrable', 'Recoverable'],
  datasets: [
    {
      label: 'IDR',
      data: [props.purchaseImpactIdr, props.deferrableSpendIdr, props.recoverableCapitalIdr],
      backgroundColor: ['#3b82f6', '#f59e0b', '#22c55e'],
      borderRadius: 4,
    },
  ],
}))

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: { legend: { display: false } },
  scales: { y: { beginAtZero: true } },
}))
</script>

<template>
  <Card class="inventory-optimization-impact-chart">
    <template #title>Business Impact Summary</template>
    <template #content>
      <div v-if="loading" class="inventory-optimization-impact-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <div v-else class="inventory-optimization-impact-chart__canvas">
        <Chart type="bar" :data="chartData" :options="chartOptions" />
      </div>
    </template>
  </Card>
</template>

<style scoped>
.inventory-optimization-impact-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-optimization-impact-chart__canvas {
  height: 220px;
}
</style>

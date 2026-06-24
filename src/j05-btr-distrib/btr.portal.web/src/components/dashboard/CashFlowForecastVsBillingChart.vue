<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'

const props = defineProps<{
  billing: number
  cashMtd: number
  projectedCash: number
  loading: boolean
}>()

const hasData = computed(
  () => props.billing > 0 || props.cashMtd > 0 || props.projectedCash > 0,
)

const chartData = computed(() => ({
  labels: ['Billing', 'Cash MTD', 'Projected Cash'],
  datasets: [
    {
      label: 'Amount',
      data: [props.billing, props.cashMtd, props.projectedCash],
      backgroundColor: ['#6366f1', '#22c55e', '#0ea5e9'],
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
  <Card class="cash-flow-forecast-vs-billing-chart">
    <template #title>
      <div class="cash-flow-forecast-vs-billing-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Cash Forecast vs Billing</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="cash-flow-forecast-vs-billing-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="cash-flow-forecast-vs-billing-chart__canvas">
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="cash-flow-forecast-vs-billing-chart__empty">
          No billing or cash data for the current period.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.cash-flow-forecast-vs-billing-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.cash-flow-forecast-vs-billing-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.cash-flow-forecast-vs-billing-chart__canvas {
  height: 280px;
}

.cash-flow-forecast-vs-billing-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'

const props = defineProps<{
  elevatedRiskReceivable: number
  totalPiutang: number
  loading: boolean
}>()

const healthyReceivable = computed(() =>
  Math.max(0, props.totalPiutang - props.elevatedRiskReceivable),
)

const hasData = computed(
  () => props.elevatedRiskReceivable > 0 || props.totalPiutang > 0,
)

const chartData = computed(() => ({
  labels: ['Elevated Risk Receivable', 'Other Open Piutang'],
  datasets: [
    {
      label: 'Amount',
      data: [props.elevatedRiskReceivable, healthyReceivable.value],
      backgroundColor: ['#ef4444', '#22c55e'],
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
  <Card class="customer-risk-forecast-exposure-chart">
    <template #title>
      <div class="customer-risk-forecast-exposure-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Elevated Risk vs Total Piutang</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-risk-forecast-exposure-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="customer-risk-forecast-exposure-chart__canvas">
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="customer-risk-forecast-exposure-chart__empty">
          No piutang exposure data available.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-risk-forecast-exposure-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-risk-forecast-exposure-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-risk-forecast-exposure-chart__canvas {
  height: 280px;
}

.customer-risk-forecast-exposure-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

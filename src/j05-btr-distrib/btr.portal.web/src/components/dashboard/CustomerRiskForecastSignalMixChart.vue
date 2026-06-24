<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCustomerRiskForecastSignalMixItem } from '@/models/dashboard'
import {
  CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_CHART_COLORS,
  type CustomerRiskForecastSignalFamilyKey,
} from '@/services/customerRiskForecastSignals'

const props = defineProps<{
  signalMix: DashboardCustomerRiskForecastSignalMixItem[]
  loading: boolean
}>()

const sortedSignalMix = computed(() =>
  [...(props.signalMix ?? [])].sort((a, b) => a.SortOrder - b.SortOrder),
)

const hasData = computed(() =>
  sortedSignalMix.value.some((item) => item.CustomerCount > 0),
)

const chartData = computed(() => ({
  labels: sortedSignalMix.value.map((item) => item.SignalFamilyLabel),
  datasets: [
    {
      label: 'Customers',
      data: sortedSignalMix.value.map((item) => item.CustomerCount),
      backgroundColor: sortedSignalMix.value.map(
        (item) =>
          CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_CHART_COLORS[
            item.SignalFamilyKey as CustomerRiskForecastSignalFamilyKey
          ] ?? '#94a3b8',
      ),
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
          ` ${context.parsed.y} customers`,
      },
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: {
        precision: 0,
      },
    },
  },
}))
</script>

<template>
  <Card class="customer-risk-forecast-signal-mix-chart">
    <template #title>
      <div class="customer-risk-forecast-signal-mix-chart__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Signal Family Mix</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-risk-forecast-signal-mix-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="customer-risk-forecast-signal-mix-chart__canvas">
          <Chart type="bar" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="customer-risk-forecast-signal-mix-chart__empty">
          No signal family data available.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-risk-forecast-signal-mix-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-risk-forecast-signal-mix-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-risk-forecast-signal-mix-chart__canvas {
  height: 280px;
}

.customer-risk-forecast-signal-mix-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

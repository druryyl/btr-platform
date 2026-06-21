<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCustomerRiskForecastDistItem } from '@/models/dashboard'
import {
  CUSTOMER_RISK_FORECAST_CATEGORY_CHART_COLORS,
  type CustomerRiskForecastCategoryKey,
} from '@/services/customerRiskForecastSignals'

const props = defineProps<{
  distribution: DashboardCustomerRiskForecastDistItem[]
  loading: boolean
}>()

const sortedDistribution = computed(() =>
  [...(props.distribution ?? [])].sort((a, b) => a.SortOrder - b.SortOrder),
)

const hasData = computed(() =>
  sortedDistribution.value.some((item) => item.CustomerCount > 0),
)

const chartData = computed(() => {
  const nonZero = sortedDistribution.value.filter((item) => item.CustomerCount > 0)

  return {
    labels: nonZero.map((item) => item.CategoryLabel),
    datasets: [
      {
        data: nonZero.map((item) => item.CustomerCount),
        backgroundColor: nonZero.map(
          (item) =>
            CUSTOMER_RISK_FORECAST_CATEGORY_CHART_COLORS[
              item.Category as CustomerRiskForecastCategoryKey
            ] ?? '#94a3b8',
        ),
      },
    ],
  }
})

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'right' as const,
    },
    tooltip: {
      callbacks: {
        label: (context: { parsed: number; label: string }) =>
          ` ${context.label}: ${context.parsed} customers`,
      },
    },
  },
}))
</script>

<template>
  <Card class="customer-risk-forecast-category-chart">
    <template #title>
      <div class="customer-risk-forecast-category-chart__title">
        <i class="pi pi-chart-pie" aria-hidden="true" />
        <span>Risk Category Distribution</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-risk-forecast-category-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="customer-risk-forecast-category-chart__canvas">
          <Chart type="doughnut" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="customer-risk-forecast-category-chart__empty">
          No customer risk category data available.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-risk-forecast-category-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-risk-forecast-category-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-risk-forecast-category-chart__canvas {
  height: 280px;
}

.customer-risk-forecast-category-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

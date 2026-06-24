<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCustomerPortfolioLifecycleDistItem } from '@/models/dashboard'
import {
  CUSTOMER_PORTFOLIO_LIFECYCLE_CHART_COLORS,
  type CustomerPortfolioLifecycleKey,
} from '@/services/customerPortfolioSignals'

const props = defineProps<{
  distribution: DashboardCustomerPortfolioLifecycleDistItem[]
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
    labels: nonZero.map((item) => item.LifecycleLabel),
    datasets: [
      {
        data: nonZero.map((item) => item.CustomerCount),
        backgroundColor: nonZero.map(
          (item) =>
            CUSTOMER_PORTFOLIO_LIFECYCLE_CHART_COLORS[
              item.LifecycleStage as CustomerPortfolioLifecycleKey
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
    legend: { position: 'right' as const },
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
  <Card class="customer-portfolio-lifecycle-chart">
    <template #title>
      <div class="customer-portfolio-lifecycle-chart__title">
        <i class="pi pi-chart-pie" aria-hidden="true" />
        <span>Lifecycle Distribution</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-portfolio-lifecycle-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <template v-else>
        <div v-if="hasData" class="customer-portfolio-lifecycle-chart__canvas">
          <Chart type="doughnut" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="customer-portfolio-lifecycle-chart__empty">
          No lifecycle distribution data available.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-portfolio-lifecycle-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-portfolio-lifecycle-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-portfolio-lifecycle-chart__canvas {
  height: 280px;
}

.customer-portfolio-lifecycle-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

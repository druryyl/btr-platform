<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { chartLegend, createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCustomerPortfolioTierDistItem } from '@/models/dashboard'
import {
  CUSTOMER_PORTFOLIO_TIER_CHART_COLORS,
  type CustomerPortfolioTierKey,
} from '@/services/customerPortfolioSignals'

const props = defineProps<{
  distribution: DashboardCustomerPortfolioTierDistItem[]
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
    labels: nonZero.map((item) => item.TierLabel),
    datasets: [
      {
        data: nonZero.map((item) => item.CustomerCount),
        backgroundColor: nonZero.map(
          (item) =>
            CUSTOMER_PORTFOLIO_TIER_CHART_COLORS[item.PortfolioTier as CustomerPortfolioTierKey] ??
            '#94a3b8',
        ),
      },
    ],
  }
})

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      legend: chartLegend.right(),
      tooltip: {
        callbacks: {
          label: (context: { parsed: number; label: string }) =>
            ` ${context.label}: ${context.parsed} customers`,
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="customer-portfolio-tier-chart portal-chart-card">
    <template #title>
      <div class="customer-portfolio-tier-chart__title">
        <i class="pi pi-chart-pie" aria-hidden="true" />
        <span>Tier Distribution</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-portfolio-tier-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <template v-else>
        <div v-if="hasData" class="customer-portfolio-tier-chart__canvas portal-chart-canvas">
          <Chart type="doughnut" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="customer-portfolio-tier-chart__empty">
          No tier distribution data available.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-portfolio-tier-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-portfolio-tier-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-portfolio-tier-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

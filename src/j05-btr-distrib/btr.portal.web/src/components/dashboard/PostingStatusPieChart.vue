<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { chartLegend, createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardPurchasingPostingStatusItem } from '@/models/dashboard'

const props = defineProps<{
  items: DashboardPurchasingPostingStatusItem[]
  loading: boolean
}>()

const STATUS_COLORS: Record<string, string> = {
  BELUM: '#f97316',
  SUDAH: '#22c55e',
}

const sortedItems = computed(() =>
  [...props.items].sort((a, b) => a.SortOrder - b.SortOrder),
)

const hasData = computed(() =>
  sortedItems.value.some((item) => item.PurchaseAmount > 0),
)

const chartData = computed(() => {
  const nonZero = sortedItems.value.filter((item) => item.PurchaseAmount > 0)
  return {
    labels: nonZero.map((item) => item.StatusLabel),
    datasets: [
      {
        data: nonZero.map((item) => item.PurchaseAmount),
        backgroundColor: nonZero.map((item) => STATUS_COLORS[item.StatusKey] ?? '#94a3b8'),
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
            ` ${context.label}: ${formatCurrency(context.parsed)}`,
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="posting-status-pie-chart portal-chart-card">
    <template #title>
      <div class="posting-status-pie-chart__title">
        <i class="pi pi-chart-pie" aria-hidden="true" />
        <span>Posting Status Breakdown</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="posting-status-pie-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="posting-status-pie-chart__canvas portal-chart-canvas">
          <Chart type="pie" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="posting-status-pie-chart__empty">
          No posting status data for the current period.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.posting-status-pie-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.posting-status-pie-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.posting-status-pie-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

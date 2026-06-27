<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { chartLegend, compactAxisTitle, createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardCashFlowRecoveryTrendItem } from '@/models/dashboard'

const props = defineProps<{
  recoveryTrend: DashboardCashFlowRecoveryTrendItem[]
  loading: boolean
}>()

const hasData = computed(() => (props.recoveryTrend?.length ?? 0) > 0)

const chartData = computed(() => ({
  labels: props.recoveryTrend.map((row) => String(row.DayOfMonth)),
  datasets: [
    {
      type: 'line' as const,
      label: 'Cumulative collections',
      data: props.recoveryTrend.map((row) =>
        row.IsElapsed ? row.CumulativeCollections : null,
      ),
      borderColor: '#22c55e',
      backgroundColor: 'rgba(34, 197, 94, 0.1)',
      tension: 0.2,
      fill: false,
    },
    {
      type: 'line' as const,
      label: 'Cumulative billing',
      data: props.recoveryTrend.map((row) =>
        row.IsElapsed ? row.CumulativeBilling : null,
      ),
      borderColor: '#6366f1',
      backgroundColor: 'rgba(99, 102, 241, 0.1)',
      tension: 0.2,
      fill: false,
    },
  ],
}))

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      legend: chartLegend.bottom(),
      tooltip: {
        callbacks: {
          label: (context: { dataset: { label?: string }; parsed: { y: number | null } }) =>
            context.parsed.y == null
              ? ''
              : ` ${context.dataset.label ?? ''}: ${formatCurrency(context.parsed.y)}`,
        },
      },
    },
    scales: {
      x: {
        title: compactAxisTitle('Day of month'),
      },
      y: {
        ticks: {
          callback: (value: string | number) => formatCurrency(Number(value)),
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="cash-flow-recovery-trend-chart portal-chart-card">
    <template #title>
      <div class="cash-flow-recovery-trend-chart__title">
        <i class="pi pi-chart-line" aria-hidden="true" />
        <span>Recovery Trend</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="cash-flow-recovery-trend-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="cash-flow-recovery-trend-chart__canvas portal-chart-canvas portal-chart-canvas--tall">
          <Chart type="line" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="cash-flow-recovery-trend-chart__empty">
          No recovery trend data for the current period.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.cash-flow-recovery-trend-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.cash-flow-recovery-trend-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.cash-flow-recovery-trend-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { chartLegend, createChartOptions } from '@/services/chartLayout'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'
import type { DashboardPiutangAgingBucket } from '@/models/dashboard'

const props = defineProps<{
  buckets: DashboardPiutangAgingBucket[]
  loading: boolean
  title?: string
  emptyMessage?: string
}>()

const BUCKET_COLORS: Record<string, string> = {
  Current: '#22c55e',
  Days1To30: '#fbbf24',
  Days31To60: '#f97316',
  Days61To90: '#ef4444',
  DaysOver90: '#991b1b',
  Active: '#22c55e',
  SlowMoving: '#f97316',
  DeadStock: '#ef4444',
  NeverSold: '#64748b',
}

const chartTitle = computed(() => props.title ?? 'Aging Distribution')
const chartEmptyMessage = computed(() => props.emptyMessage ?? 'No outstanding piutang data.')

const sortedBuckets = computed(() =>
  [...props.buckets].sort((a, b) => a.SortOrder - b.SortOrder),
)

const hasData = computed(() =>
  sortedBuckets.value.some((b) => b.Amount > 0),
)

const chartData = computed(() => {
  const nonZero = sortedBuckets.value.filter((b) => b.Amount > 0)
  return {
    labels: nonZero.map((b) => b.BucketLabel),
    datasets: [
      {
        data: nonZero.map((b) => b.Amount),
        backgroundColor: nonZero.map((b) => BUCKET_COLORS[b.BucketKey] ?? '#94a3b8'),
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
  <Card class="aging-pie-chart portal-chart-card">
    <template #title>
      <div class="aging-pie-chart__title">
        <i class="pi pi-chart-pie" aria-hidden="true" />
        <span>{{ chartTitle }}</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="aging-pie-chart__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div v-if="hasData" class="aging-pie-chart__canvas portal-chart-canvas">
          <Chart type="pie" :data="chartData" :options="chartOptions" />
        </div>
        <p v-else class="aging-pie-chart__empty">
          {{ chartEmptyMessage }}
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.aging-pie-chart__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.aging-pie-chart__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.aging-pie-chart__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

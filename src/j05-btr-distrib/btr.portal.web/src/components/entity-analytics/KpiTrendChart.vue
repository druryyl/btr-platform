<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import { chartLegend, createChartOptions } from '@/services/chartLayout'
import { formatCurrency } from '@/services/formatters'
import type { CompareTrendOverlay, ProfileTrendSeries } from '@/models/entityAnalytics'

const overlayColors = ['#2563eb', '#dc2626', '#16a34a', '#9333ea', '#ea580c']

const props = defineProps<{
  series?: ProfileTrendSeries
  overlay?: CompareTrendOverlay
}>()

const title = computed(() => props.overlay?.DisplayName ?? props.series?.DisplayName ?? 'Trend')
const unit = computed(() => props.overlay?.Unit ?? props.series?.Unit ?? 'IDR')
const periodSemantics = computed(
  () => props.series?.PeriodSemantics ?? 'MTD',
)

const hasData = computed(() => {
  if (props.overlay) {
    return props.overlay.EntitySeries.some((entitySeries) =>
      entitySeries.Points.some((point) => point.Value != null),
    )
  }

  return props.series?.Points?.some((point) => point.Value != null) ?? false
})

const chartData = computed(() => {
  if (props.overlay) {
    const labels = props.overlay.EntitySeries[0]?.Points.map((point) => point.PeriodLabel) ?? []
    return {
      labels,
      datasets: props.overlay.EntitySeries.map((entitySeries, index) => ({
        type: 'line' as const,
        label: `${entitySeries.DisplayName} (${entitySeries.EntityCode})`,
        data: entitySeries.Points.map((point) => point.Value ?? null),
        borderColor: overlayColors[index % overlayColors.length],
        backgroundColor: 'transparent',
        borderWidth: 2,
        pointRadius: 3,
        fill: false,
        tension: 0.25,
      })),
    }
  }

  return {
    labels: props.series?.Points.map((point) => point.PeriodLabel) ?? [],
    datasets: [
      {
        type: 'line' as const,
        label: props.series?.DisplayName ?? 'Trend',
        data: props.series?.Points.map((point) => point.Value ?? null) ?? [],
        borderColor: overlayColors[0],
        backgroundColor: 'rgba(37, 99, 235, 0.1)',
        borderWidth: 2,
        pointRadius: 4,
        pointBackgroundColor: overlayColors[0],
        fill: true,
        tension: 0.25,
      },
    ],
  }
})

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      legend: props.overlay ? chartLegend.bottom() : chartLegend.hidden(),
      tooltip: {
        callbacks: {
          label: (context: { parsed: { y: number | null } }) =>
            ` ${formatCurrency(context.parsed.y ?? 0)}`,
        },
      },
    },
    scales: {
      y: {
        ticks: {
          callback: (value: number | string) => formatCurrency(Number(value)),
        },
      },
    },
  }),
)
</script>

<template>
  <Card class="kpi-trend-chart portal-chart-card">
    <template #title>
      <div class="kpi-trend-chart__header">
        <span>{{ title }}</span>
        <small v-if="periodSemantics" class="kpi-trend-chart__semantics">
          {{ periodSemantics }} · {{ unit }}
        </small>
      </div>
    </template>
    <template #content>
      <div v-if="hasData" class="kpi-trend-chart__canvas portal-chart-canvas portal-chart-canvas--short">
        <Chart type="line" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="kpi-trend-chart__empty">No historical data yet for this metric.</p>
    </template>
  </Card>
</template>

<style scoped>
.kpi-trend-chart__header {
  display: flex;
  flex-direction: column;
  gap: 0.15rem;
}

.kpi-trend-chart__semantics {
  color: var(--p-text-muted-color, #64748b);
  font-weight: 400;
}

.kpi-trend-chart__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
  font-size: 0.9rem;
}
</style>

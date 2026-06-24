<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'
import type { ProfileRankingSeries } from '@/models/entityAnalytics'

const props = defineProps<{
  series: ProfileRankingSeries
}>()

const hasData = computed(
  () => (props.series?.Points?.length ?? 0) > 0,
)

const chartData = computed(() => ({
  labels: props.series.Points.map((point) => point.PeriodLabel),
  datasets: [
    {
      type: 'line' as const,
      label: 'Rank',
      data: props.series.Points.map((point) => point.RankPosition),
      borderColor: '#059669',
      backgroundColor: 'rgba(5, 150, 105, 0.1)',
      borderWidth: 2,
      pointRadius: 4,
      pointBackgroundColor: '#059669',
      fill: true,
      tension: 0.25,
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
        label: (context: { parsed: { y: number | null }; dataIndex: number }) => {
          const point = props.series.Points[context.dataIndex]
          if (!point) return ''
          return ` Rank #${point.RankPosition} of ${point.PopulationSize} (${point.Percentile.toFixed(1)}th pct)`
        },
      },
    },
  },
  scales: {
    y: {
      reverse: true,
      min: 1,
      ticks: {
        stepSize: 1,
        callback: (value: number | string) => `#${value}`,
      },
      title: {
        display: true,
        text: 'Rank (1 = best)',
      },
    },
  },
}))
</script>

<template>
  <Card class="kpi-rank-chart">
    <template #title>
      <div class="kpi-rank-chart__header">
        <span class="kpi-rank-chart__title">{{ series.DisplayName }}</span>
        <span v-if="series.CurrentRank != null" class="kpi-rank-chart__current">
          Current: #{{ series.CurrentRank }}
          <span v-if="series.CurrentPercentile != null" class="kpi-rank-chart__percentile">
            ({{ series.CurrentPercentile.toFixed(1) }}th percentile)
          </span>
        </span>
      </div>
    </template>
    <template #content>
      <div v-if="hasData" class="kpi-rank-chart__stats">
        <span v-if="series.BestRank != null">Best: #{{ series.BestRank }}</span>
        <span v-if="series.WorstRank != null">Worst: #{{ series.WorstRank }}</span>
        <span v-if="series.CurrentPopulationSize != null">
          Population: {{ series.CurrentPopulationSize }}
        </span>
      </div>
      <div v-if="hasData" class="kpi-rank-chart__canvas">
        <Chart type="line" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="kpi-rank-chart__empty">No ranking history for this KPI.</p>
    </template>
  </Card>
</template>

<style scoped>
.kpi-rank-chart__header {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.5rem;
}

.kpi-rank-chart__title {
  font-weight: 600;
}

.kpi-rank-chart__current {
  font-size: 0.875rem;
  color: var(--p-text-muted-color, #64748b);
}

.kpi-rank-chart__percentile {
  margin-left: 0.25rem;
}

.kpi-rank-chart__stats {
  display: flex;
  flex-wrap: wrap;
  gap: 1rem;
  margin-bottom: 0.75rem;
  font-size: 0.875rem;
  color: var(--p-text-muted-color, #64748b);
}

.kpi-rank-chart__canvas {
  height: 220px;
}

.kpi-rank-chart__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
  font-size: 0.875rem;
}
</style>

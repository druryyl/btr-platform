<script setup lang="ts">
import { computed } from 'vue'
import Chart from 'primevue/chart'
import { chartLegend, createChartOptions } from '@/services/chartLayout'
import {
  PEER_AVERAGE_LABEL,
  PEER_AVERAGE_STYLE,
  resolveSeriesColor,
} from '@/components/entity-analytics/performanceSignatureTheme'

export interface SignatureAxis {
  KpiId: string
  DisplayName: string
  SignatureDimensionKey?: string | null
  Direction?: string | null
}

export interface SignatureSeries {
  label: string
  scores: Array<number | null>
  colorIndex?: number
}

const props = defineProps<{
  axes: SignatureAxis[]
  series: SignatureSeries[]
  peerAverage?: Array<number | null> | null
  showPeerAverage?: boolean
}>()

const labels = computed(() => props.axes.map((axis) => axis.DisplayName))

const chartData = computed(() => {
  const datasets: Array<Record<string, unknown>> = props.series.map((entry, index) => {
    const colorIndex = entry.colorIndex ?? index
    const colors = resolveSeriesColor(colorIndex)
    return {
      label: entry.label,
      data: entry.scores.map((score) => (score == null ? null : score)),
      fill: true,
      borderWidth: 3,
      pointRadius: 4,
      pointHoverRadius: 6,
      backgroundColor: colors.fill,
      borderColor: colors.border,
    }
  })

  if (props.showPeerAverage && props.peerAverage?.length) {
    datasets.push({
      label: PEER_AVERAGE_LABEL,
      data: props.peerAverage.map((score) => (score == null ? null : score)),
      fill: false,
      borderWidth: 2.5,
      pointRadius: 3,
      pointHoverRadius: 5,
      backgroundColor: PEER_AVERAGE_STYLE.fill,
      borderColor: PEER_AVERAGE_STYLE.border,
      borderDash: PEER_AVERAGE_STYLE.borderDash,
    })
  }

  return {
    labels: labels.value,
    datasets,
  }
})

const chartOptions = computed(() =>
  createChartOptions({
    layout: {
      padding: {
        top: 4,
        left: 8,
        right: 8,
      },
    },
    plugins: {
      legend: chartLegend.bottom(),
    },
    scales: {
      r: {
        min: 0,
        max: 100,
        ticks: {
          stepSize: 20,
          backdropPadding: 2,
          font: {
            size: 11,
            weight: 'normal' as const,
          },
        },
        pointLabels: {
          padding: 16,
          font: {
            size: 13,
            weight: 'normal' as const,
          },
        },
        grid: {
          circular: true,
        },
      },
    },
  }),
)

const hasData = computed(
  () =>
    props.axes.length > 0 &&
    props.series.some((entry) => entry.scores.some((score) => score != null)),
)
</script>

<template>
  <div v-if="hasData" class="performance-signature-chart">
    <div class="performance-signature-chart__plot portal-chart-canvas">
      <Chart type="radar" :data="chartData" :options="chartOptions" />
    </div>
  </div>
  <p v-else class="performance-signature-chart__empty">
    No performance signature scores available for this selection.
  </p>
</template>

<style scoped>
.performance-signature-chart {
  width: min(70%, 28rem);
  max-width: 100%;
  margin: 0 auto;
}

@media (min-width: 56rem) {
  .performance-signature-chart {
    width: min(100%, 32rem);
  }
}

.performance-signature-chart__plot {
  width: 100%;
  height: clamp(20rem, 48vw, 27rem);
  min-height: 20rem;
}

.performance-signature-chart__empty {
  margin: 0;
  color: var(--p-text-muted-color);
}
</style>

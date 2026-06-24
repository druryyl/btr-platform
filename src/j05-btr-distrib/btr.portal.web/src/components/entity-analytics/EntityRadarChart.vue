<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import Chart from 'primevue/chart'

export interface RadarAxis {
  KpiId: string
  DisplayName: string
  Direction?: string | null
}

export interface RadarSeries {
  label: string
  scores: Array<number | null>
}

const props = defineProps<{
  axes: RadarAxis[]
  series: RadarSeries[]
}>()

const labels = computed(() => props.axes.map((axis) => axis.DisplayName))

const chartData = computed(() => ({
  labels: labels.value,
  datasets: props.series.map((entry, index) => ({
    label: entry.label,
    data: entry.scores.map((score) => (score == null ? null : score)),
    fill: true,
    borderWidth: 2,
    pointRadius: 3,
    backgroundColor: `rgba(59, 130, 246, ${0.12 + index * 0.08})`,
    borderColor: index === 0 ? 'rgb(59, 130, 246)' : `hsl(${(index * 70) % 360}, 70%, 45%)`,
  })),
}))

const chartOptions = computed(() => ({
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: 'bottom' as const,
    },
  },
  scales: {
    r: {
      min: 0,
      max: 100,
      ticks: {
        stepSize: 20,
      },
    },
  },
}))

const hasData = computed(
  () =>
    props.axes.length > 0 &&
    props.series.some((entry) => entry.scores.some((score) => score != null)),
)
</script>

<template>
  <Card class="entity-radar-chart">
    <template #content>
      <div v-if="hasData" class="entity-radar-chart__canvas">
        <Chart type="radar" :data="chartData" :options="chartOptions" />
      </div>
      <p v-else class="entity-radar-chart__empty">No radar scores available for this selection.</p>
    </template>
  </Card>
</template>

<style scoped>
.entity-radar-chart__canvas {
  min-height: 320px;
}

.entity-radar-chart__empty {
  margin: 0;
  color: var(--p-text-muted-color);
}
</style>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import Chart from 'primevue/chart'
import { fetchPeerDistribution } from '@/api/entityAnalyticsApi'
import type { PeerDistributionResponse } from '@/models/entityAnalytics'
import { createChartOptions } from '@/services/chartLayout'
import { buildEntityColorMap } from '@/composables/useComparisonColors'

const props = defineProps<{
  entityType: string
  entityIds: string[]
  kpiId: string
  dimensionFilter?: string | null
}>()

const distributions = ref<Record<string, PeerDistributionResponse>>({})
const loading = ref(false)

const colors = () => buildEntityColorMap(props.entityIds)

async function load() {
  if (!props.entityIds.length || !props.kpiId) return
  loading.value = true
  try {
    const entries = await Promise.all(
      props.entityIds.map(async (id) => {
        const data = await fetchPeerDistribution({
          entityType: props.entityType,
          entityId: id,
          kpiId: props.kpiId,
          dimensionFilter: props.dimensionFilter ?? undefined,
        })
        return [id, data] as const
      }),
    )
    distributions.value = Object.fromEntries(entries)
  } finally {
    loading.value = false
  }
}

const chartData = computed(() => {
  const first = distributions.value[props.entityIds[0]]
  if (!first?.Bins?.length) return null

  return {
    labels: first.Bins.map((b) => b.Label),
    datasets: [
      {
        label: 'Peer distribution',
        data: first.Bins.map((b) => b.Count),
        backgroundColor: 'rgba(148, 163, 184, 0.45)',
        borderWidth: 0,
      },
    ],
  }
})

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      legend: { display: false },
      annotation: undefined,
    },
    scales: {
      x: { title: { display: true, text: firstKpiLabel.value } },
      y: { beginAtZero: true, title: { display: true, text: 'Entity count' } },
    },
  }),
)

const firstKpiLabel = computed(
  () => distributions.value[props.entityIds[0]]?.KpiDisplayName ?? 'Peer Position',
)

const summaryLines = computed(() =>
  props.entityIds.map((id) => {
    const d = distributions.value[id]
    if (!d) return null
    return {
      id,
      label: d.FormattedSelectedValue,
      percentile: d.SelectedPercentile,
      range: d.FormattedPeerRange,
      color: colors().get(id)?.border,
    }
  }).filter(Boolean),
)

onMounted(load)
watch(() => [props.entityIds, props.kpiId, props.dimensionFilter], load, { deep: true })
</script>

<template>
  <div class="iw-panel-card">
    <h3 class="iw-section-title">Peer Position</h3>
    <p class="iw-meta">Is this entity normal, unusual, or extreme for one KPI within its peer group?</p>
    <div v-if="loading" class="iw-skeleton" style="min-height: 10rem" />
    <template v-else>
      <div v-if="summaryLines.length" class="iw-peer-summary">
        <div v-for="line in summaryLines" :key="line!.id" class="iw-peer-summary__row">
          <span class="iw-peer-summary__swatch" :style="{ background: line!.color }" />
          <span class="iw-numeric">{{ line!.label }}</span>
          <span v-if="line!.percentile != null" class="iw-meta">
            — above {{ line!.percentile.toFixed(0) }}% of peers
          </span>
        </div>
        <p class="iw-meta">Peer range: {{ summaryLines[0]?.range }}</p>
      </div>
      <div v-if="chartData" class="portal-chart-canvas portal-chart-canvas--compact">
        <Chart type="bar" :data="chartData" :options="chartOptions" />
      </div>
    </template>
  </div>
</template>

<style scoped>
.iw-peer-summary {
  margin-bottom: 0.75rem;
}

.iw-peer-summary__row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  margin-bottom: 0.25rem;
}

.iw-peer-summary__swatch {
  width: 0.5rem;
  height: 0.5rem;
  border-radius: 50%;
}
</style>

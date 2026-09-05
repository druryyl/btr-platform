<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import Chart from 'primevue/chart'
import { fetchPeerDistribution } from '@/api/entityAnalyticsApi'
import type { PeerDistributionResponse, PopulationMapPoint, WorkspaceSelectedEntity } from '@/models/entityAnalytics'
import { createChartOptions } from '@/services/chartLayout'
import { formatBinRangeLabel } from '@/services/populationMapLayout'
import { resolvePeerGroupLabel, peerGroupDimensionHeading } from '@/services/peerGroupLabel'
import { buildEntityColorMap } from '@/composables/useComparisonColors'
import {
  buildPeerBarColors,
  buildPeerBinMarkers,
  ensurePeerBinMarkerPluginRegistered,
  peerBinMarkerPlugin,
  PEER_BIN_NEUTRAL_FILL,
} from '@/services/peerPositionChart'

ensurePeerBinMarkerPluginRegistered()

const props = defineProps<{
  entityType: string
  entities: WorkspaceSelectedEntity[]
  entityIds: string[]
  populationPoints?: PopulationMapPoint[]
  kpiId: string
  dimensionFilter?: string | null
  peerGroupRuleId?: string | null
}>()

const distributions = ref<Record<string, PeerDistributionResponse>>({})
const loading = ref(false)

const entityNameById = computed(() =>
  Object.fromEntries(props.entities.map((entity) => [entity.EntityId, entity.DisplayName])),
)

const populationDimensionById = computed(() =>
  Object.fromEntries(
    (props.populationPoints ?? [])
      .filter((point) => point.DimensionValue?.trim())
      .map((point) => [point.EntityId, point.DimensionValue!.trim()]),
  ),
)

const colorMap = computed(() => buildEntityColorMap(props.entityIds))

async function load() {
  if (!props.entityIds.length || !props.kpiId) return
  loading.value = true
  try {
    // Population Map filter matches default peer dimension (Wilayah / Principal);
    // skip it when peer group uses a different dimension (Klasifikasi / Category).
    const ruleId = props.peerGroupRuleId?.trim().toLowerCase() ?? ''
    const applyMapFilter =
      !ruleId || ruleId === 'customer-wilayah' || ruleId === 'item-principal'
    const entries = await Promise.all(
      props.entityIds.map(async (id) => {
        const data = await fetchPeerDistribution({
          entityType: props.entityType,
          entityId: id,
          kpiId: props.kpiId,
          dimensionFilter: applyMapFilter ? (props.dimensionFilter ?? undefined) : undefined,
          peerGroupRuleId: props.peerGroupRuleId ?? undefined,
        })
        return [id, data] as const
      }),
    )
    distributions.value = Object.fromEntries(entries)
  } finally {
    loading.value = false
  }
}

const primaryBins = computed(() => distributions.value[props.entityIds[0]]?.Bins ?? [])

const peerBinMarkers = computed(() =>
  buildPeerBinMarkers({
    bins: primaryBins.value,
    entityIds: props.entityIds,
    distributions: distributions.value,
    colorMap: colorMap.value,
  }),
)

const chartData = computed(() => {
  const first = distributions.value[props.entityIds[0]]
  if (!first?.Bins?.length) return null

  const unit = first.Unit
  const barColors = buildPeerBarColors(first.Bins, peerBinMarkers.value, PEER_BIN_NEUTRAL_FILL)

  return {
    labels: first.Bins.map((b, i, bins) =>
      formatBinRangeLabel(b.BinStart, b.BinEnd, unit, i === bins.length - 1),
    ),
    datasets: [
      {
        label: 'Peer distribution',
        data: first.Bins.map((b) => b.Count),
        backgroundColor: barColors.backgroundColor,
        borderColor: barColors.borderColor,
        borderWidth: barColors.borderWidth,
        borderSkipped: false,
      },
    ],
  }
})

const chartOptions = computed(() =>
  createChartOptions({
    plugins: {
      legend: { display: false },
      annotation: undefined,
      peerBinMarkers: {
        markers: peerBinMarkers.value,
      },
    },
    layout: {
      padding: { top: 14 },
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
      displayName: entityNameById.value[id] ?? id,
      label: d.FormattedSelectedValue,
      percentile: d.SelectedPercentile,
      range: d.FormattedPeerRange,
      peerGroupLabel: resolvePeerGroupLabel({
        entityType: props.entityType,
        distribution: d,
        populationDimensionValue: populationDimensionById.value[id] ?? null,
      }),
      color: colorMap.value.get(id)?.border,
    }
  }).filter(Boolean),
)

const sharedPeerGroupLabel = computed(() => {
  const labels = summaryLines.value
    .map((line) => line!.peerGroupLabel)
    .filter((label): label is string => Boolean(label?.trim()))

  if (labels.length === 0) return null
  if (props.entityIds.length === 1) return labels[0]
  return new Set(labels).size === 1 ? labels[0] : null
})

const peerGroupNameById = computed(() =>
  Object.fromEntries(
    props.entityIds.map((id) => {
      const d = distributions.value[id]
      const name =
        d?.PeerGroupDimensionValue?.trim()
        || populationDimensionById.value[id]
        || null
      return [id, name]
    }),
  ),
)

function peerGroupHeading(entityId: string): string | null {
  const name = peerGroupNameById.value[entityId]
  if (!name) return null

  const d = distributions.value[entityId]
  return peerGroupDimensionHeading({
    peerGroupRuleId: d?.PeerGroupRuleId ?? props.peerGroupRuleId,
    entityType: props.entityType,
    dimensionValue: name,
  })
}

function peerGroupCountLabel(entityId: string): string | null {
  const d = distributions.value[entityId]
  const size = d?.PeerGroupSize ?? 0
  if (size <= 0) return null

  const entityType = props.entityType.trim().toLowerCase()
  if (entityType === 'customer') return `${size} customers`
  if (entityType === 'item') return `${size} items`
  if (entityType === 'salesman') return `${size} active salesmen`
  if (entityType === 'supplier') return `${size} active suppliers`
  return `${size} peers`
}

const sharedPeerGroupHeading = computed(() => {
  if (props.entityIds.length !== 1) return null
  return peerGroupHeading(props.entityIds[0])
})

const sharedPeerGroupCount = computed(() => {
  if (props.entityIds.length !== 1) return null
  return peerGroupCountLabel(props.entityIds[0])
})

onMounted(load)
watch(
  () => [props.entityIds, props.kpiId, props.dimensionFilter, props.peerGroupRuleId],
  load,
  { deep: true },
)
</script>

<template>
  <div class="iw-panel-card">
    <h3 class="iw-section-title">Peer Position</h3>
    <p class="iw-meta">Is this entity normal, unusual, or extreme for one KPI within its peer group?</p>
    <div v-if="loading" class="iw-skeleton" style="min-height: 10rem" />
    <template v-else>
      <div v-if="summaryLines.length" class="iw-peer-summary">
        <p
          v-if="sharedPeerGroupHeading || sharedPeerGroupCount || sharedPeerGroupLabel"
          class="iw-peer-context"
        >
          <span v-if="sharedPeerGroupHeading" class="iw-peer-context__name">
            {{ sharedPeerGroupHeading }}
          </span>
          <span v-if="sharedPeerGroupCount" class="iw-peer-context__detail">
            {{ sharedPeerGroupCount }}
          </span>
          <span
            v-else-if="sharedPeerGroupLabel"
            class="iw-peer-context__detail"
          >
            {{ sharedPeerGroupLabel }}
          </span>
        </p>
        <div v-for="line in summaryLines" :key="line!.id" class="iw-peer-summary__block">
          <div class="iw-peer-summary__row">
            <span class="iw-peer-summary__swatch" :style="{ background: line!.color }" />
            <span class="iw-peer-summary__name">{{ line!.displayName }}</span>
            <span class="iw-numeric">{{ line!.label }}</span>
            <span v-if="line!.percentile != null" class="iw-meta">
              — above {{ line!.percentile.toFixed(0) }}% of peers
            </span>
          </div>
          <p
            v-if="!sharedPeerGroupHeading && (peerGroupHeading(line!.id) || line!.peerGroupLabel)"
            class="iw-peer-context iw-peer-context--inline"
          >
            <span v-if="peerGroupHeading(line!.id)" class="iw-peer-context__name">
              {{ peerGroupHeading(line!.id) }}
            </span>
            <span v-if="peerGroupCountLabel(line!.id)" class="iw-peer-context__detail">
              {{ peerGroupCountLabel(line!.id) }}
            </span>
            <span v-else-if="line!.peerGroupLabel" class="iw-peer-context__detail">
              {{ line!.peerGroupLabel }}
            </span>
          </p>
        </div>
        <p class="iw-meta">Peer range: {{ summaryLines[0]?.range }}</p>
      </div>
      <div v-if="chartData" class="portal-chart-canvas portal-chart-canvas--compact">
        <Chart
          type="bar"
          :data="chartData"
          :options="chartOptions"
          :plugins="[peerBinMarkerPlugin]"
        />
      </div>
    </template>
  </div>
</template>

<style scoped>
.iw-peer-summary {
  margin-bottom: 0.75rem;
}

.iw-peer-context {
  margin: 0 0 0.5rem;
  color: var(--iw-text-muted, #64748b);
  font-size: 0.8125rem;
  display: flex;
  flex-wrap: wrap;
  gap: 0.35rem;
  align-items: baseline;
}

.iw-peer-context__name {
  font-weight: 600;
  color: var(--iw-text-primary, #334155);
}

.iw-peer-context__detail::before {
  content: '·';
  margin-right: 0.35rem;
}

.iw-peer-context__name + .iw-peer-context__detail::before {
  content: '·';
  margin-right: 0.35rem;
}

.iw-peer-context--inline {
  margin: 0 0 0.5rem 1rem;
}

.iw-peer-summary__block {
  margin-bottom: 0.35rem;
}

.iw-peer-summary__row {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 0.5rem;
  margin-bottom: 0.15rem;
}

.iw-peer-summary__swatch {
  width: 0.5rem;
  height: 0.5rem;
  border-radius: 50%;
  flex-shrink: 0;
}

.iw-peer-summary__name {
  font-weight: 600;
}
</style>

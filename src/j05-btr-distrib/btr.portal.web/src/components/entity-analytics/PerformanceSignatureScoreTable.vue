<script setup lang="ts">
import {
  PEER_AVERAGE_LABEL,
  formatSignatureScore,
  resolveSeriesColor,
} from '@/components/entity-analytics/performanceSignatureTheme'
import type { SignatureAxis, SignatureSeries } from '@/components/entity-analytics/PerformanceSignatureChart.vue'

const props = defineProps<{
  axes: SignatureAxis[]
  series: SignatureSeries[]
  peerAverage?: Array<number | null> | null
  showPeerAverage?: boolean
}>()

function scoreForSeries(series: SignatureSeries, axisIndex: number): number | null {
  return series.scores[axisIndex] ?? null
}
</script>

<template>
  <div class="performance-signature-score-table" role="table" aria-label="Performance signature scores">
    <div class="performance-signature-score-table__header" role="row">
      <span class="performance-signature-score-table__dimension-header" role="columnheader">Dimension</span>
      <span
        v-for="(entry, index) in series"
        :key="entry.label"
        class="performance-signature-score-table__entity-header"
        role="columnheader"
      >
        <span
          class="performance-signature-score-table__color-dot"
          :style="{ backgroundColor: resolveSeriesColor(entry.colorIndex ?? index).border }"
          aria-hidden="true"
        />
        {{ entry.label }}
      </span>
      <span
        v-if="showPeerAverage"
        class="performance-signature-score-table__entity-header performance-signature-score-table__entity-header--peer"
        role="columnheader"
      >
        {{ PEER_AVERAGE_LABEL }}
      </span>
    </div>

    <div
      v-for="(axis, axisIndex) in axes"
      :key="axis.KpiId"
      class="performance-signature-score-table__row"
      role="row"
    >
      <span class="performance-signature-score-table__dimension" role="cell">
        <span class="performance-signature-score-table__dimension-label">{{ axis.DisplayName }}</span>
        <span class="performance-signature-score-table__leader" aria-hidden="true" />
      </span>
      <span
        v-for="entry in series"
        :key="`${axis.KpiId}-${entry.label}`"
        class="performance-signature-score-table__value"
        role="cell"
      >
        {{ formatSignatureScore(scoreForSeries(entry, axisIndex)) }}
      </span>
      <span
        v-if="showPeerAverage"
        class="performance-signature-score-table__value performance-signature-score-table__value--peer"
        role="cell"
      >
        {{ formatSignatureScore(peerAverage?.[axisIndex] ?? null) }}
      </span>
    </div>
  </div>
</template>

<style scoped>
.performance-signature-score-table {
  margin-top: 1rem;
  font-size: 0.875rem;
  font-variant-numeric: tabular-nums;
}

.performance-signature-score-table__header,
.performance-signature-score-table__row {
  display: grid;
  grid-template-columns: minmax(8rem, 1.4fr) repeat(var(--score-columns, 1), minmax(4rem, 1fr));
  gap: 0.5rem 0.75rem;
  align-items: baseline;
}

.performance-signature-score-table__header {
  margin-bottom: 0.35rem;
  color: var(--p-text-muted-color);
  font-size: 0.8125rem;
}

.performance-signature-score-table__entity-header {
  display: inline-flex;
  align-items: center;
  gap: 0.35rem;
  justify-self: end;
  text-align: right;
  font-weight: normal;
}

.performance-signature-score-table__entity-header--peer {
  color: var(--p-text-muted-color);
}

.performance-signature-score-table__color-dot {
  width: 0.55rem;
  height: 0.55rem;
  border-radius: 999px;
  flex-shrink: 0;
}

.performance-signature-score-table__row + .performance-signature-score-table__row {
  margin-top: 0.2rem;
}

.performance-signature-score-table__dimension {
  display: flex;
  align-items: baseline;
  gap: 0.35rem;
  min-width: 0;
}

.performance-signature-score-table__dimension-label {
  flex-shrink: 0;
  font-weight: normal;
}

.performance-signature-score-table__leader {
  flex: 1;
  min-width: 0.5rem;
  border-bottom: 1px dotted var(--p-content-border-color, #cbd5e1);
  transform: translateY(-0.2em);
}

.performance-signature-score-table__value {
  justify-self: end;
  text-align: right;
  font-weight: normal;
}

.performance-signature-score-table__value--peer {
  color: var(--p-text-muted-color);
}
</style>

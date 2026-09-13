<script setup lang="ts">
import { computed } from 'vue'
import type { PopulationMapPoint, PopulationMapResponse } from '@/models/entityAnalytics'
import type { AnalyzedPoint } from '@/services/populationProjection/populationProjectionEngine'
import { formatStatisticalClass } from '@/services/populationProjection/populationProjectionEngine'
import { isLowConfidencePoint, resolveBusinessAttentionTier } from '@/services/populationMapLayout'
import { resolveTooltipDimensionRow } from '@/services/populationTooltip'

const props = defineProps<{
  point: PopulationMapPoint
  population: PopulationMapResponse | null
  analyzed?: AnalyzedPoint | null
}>()

/** PIW-10: dimension row is rendered only when a meaningful dimension exists (IW-GAP-015). */
const dimensionRow = computed(() => resolveTooltipDimensionRow(props.point, props.population))
</script>

<template>
  <div class="iw-map-tooltip">
    <strong class="iw-map-tooltip__name">{{ point.DisplayName }}</strong>

    <div class="iw-map-tooltip__section">
      <div class="iw-map-tooltip__label">{{ population?.AxisXLabel ?? 'X' }}</div>
      <div class="iw-map-tooltip__value">{{ point.FormattedAxisX }}</div>
    </div>

    <div class="iw-map-tooltip__section">
      <div class="iw-map-tooltip__label">{{ population?.AxisYLabel ?? 'Y' }}</div>
      <div class="iw-map-tooltip__value">{{ point.FormattedAxisY }}</div>
    </div>

    <div
      v-if="point.SupplementaryLabel && point.FormattedSupplementaryValue"
      class="iw-map-tooltip__section"
    >
      <div class="iw-map-tooltip__label">{{ point.SupplementaryLabel }}</div>
      <div class="iw-map-tooltip__value">{{ point.FormattedSupplementaryValue }}</div>
    </div>

    <div
      v-if="population?.BubbleLabel && point.FormattedBubbleValue"
      class="iw-map-tooltip__section"
    >
      <div class="iw-map-tooltip__label">{{ population.BubbleLabel }}</div>
      <div class="iw-map-tooltip__value">{{ point.FormattedBubbleValue }}</div>
    </div>

    <div
      v-if="population?.BubbleColorLabel && point.FormattedBubbleColorValue"
      class="iw-map-tooltip__section"
    >
      <div class="iw-map-tooltip__label">{{ population.BubbleColorLabel }}</div>
      <div class="iw-map-tooltip__value">{{ point.FormattedBubbleColorValue }}</div>
    </div>

    <div v-if="analyzed" class="iw-map-tooltip__section">
      <div class="iw-map-tooltip__label">Classification</div>
      <div class="iw-map-tooltip__value">{{ formatStatisticalClass(analyzed.statisticalClass) }}</div>
    </div>

    <div v-if="isLowConfidencePoint(point)" class="iw-map-tooltip__section">
      <div class="iw-map-tooltip__label">Confidence</div>
      <div class="iw-map-tooltip__value">Low confidence — excluded from quadrants</div>
    </div>

    <div v-if="analyzed" class="iw-map-tooltip__section">
      <div class="iw-map-tooltip__label">Expected Behaviour</div>
      <div class="iw-map-tooltip__value">{{ analyzed.deviationLabel }}</div>
    </div>

    <div v-if="dimensionRow" class="iw-map-tooltip__section">
      <div class="iw-map-tooltip__label">{{ dimensionRow.label }}</div>
      <div class="iw-map-tooltip__value">{{ dimensionRow.value }}</div>
    </div>

    <div class="iw-map-tooltip__section">
      <div class="iw-map-tooltip__label">Attention</div>
      <div class="iw-map-tooltip__value">
        <template v-if="point.ActiveAttentionCount">
          {{ point.ActiveAttentionCount }} active signal(s)
        </template>
        <template v-else>No active signals</template>
      </div>
    </div>

    <div
      v-if="analyzed && resolveBusinessAttentionTier(point) !== 'normal'"
      class="iw-map-tooltip__section"
    >
      <div class="iw-map-tooltip__label">Business Signal</div>
      <div class="iw-map-tooltip__value">
        {{ resolveBusinessAttentionTier(point) === 'critical' ? 'Critical' : 'Attention' }}
      </div>
    </div>
  </div>
</template>

<style scoped>
.iw-map-tooltip__name {
  display: block;
  margin-bottom: 0.375rem;
  font-size: 0.8125rem;
}

.iw-map-tooltip__section {
  margin-top: 0.25rem;
}

.iw-map-tooltip__label {
  font-size: 0.6875rem;
  color: #94a3b8;
  text-transform: uppercase;
  letter-spacing: 0.02em;
}

.iw-map-tooltip__value {
  font-size: 0.75rem;
  color: #f8fafc;
}
</style>

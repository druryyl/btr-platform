<script setup lang="ts">
import type { EntityPerformanceProfileResponse } from '@/models/entityAnalytics'
import {
  PURCHASE_TO_SALES_OUT_INTERPRETATIONS,
  selectLensDerivedMetrics,
} from '@/services/investigationDerivedMetrics'

const props = defineProps<{
  profiles: Record<string, EntityPerformanceProfileResponse>
  entityIds: string[]
  metricIds?: string[] | null
  loading?: boolean
}>()

function derivedMetrics(profile: EntityPerformanceProfileResponse | undefined) {
  if (!profile) return []
  return selectLensDerivedMetrics(profile.KpiSummary?.Categories ?? [], props.metricIds)
}
</script>

<template>
  <div v-if="loading" class="iw-skeleton" style="min-height: 6rem" />
  <div v-else class="iw-panel-card">
    <h3 class="iw-section-title">Derived Investigation Metric</h3>
    <p class="iw-meta">
      Investigation-only derived metric. It is not a KPI, a ranking, or a sales-performance
      measure. Purchase-in is operational context.
    </p>

    <div
      v-for="entityId in entityIds"
      :key="entityId"
      class="iw-derived-metric"
    >
      <h4 v-if="entityIds.length > 1" class="iw-meta">
        {{ profiles[entityId]?.Overview?.DisplayName ?? entityId }}
      </h4>
      <div
        v-for="metric in derivedMetrics(profiles[entityId])"
        :key="`${entityId}-${metric.MetricId}`"
        class="iw-derived-metric__body"
      >
        <div class="iw-derived-metric__headline">
          <span class="iw-derived-metric__label">{{ metric.DisplayName }}</span>
          <span class="iw-derived-metric__value">{{ metric.FormattedValue }}</span>
        </div>
        <p
          v-if="metric.IsAvailable && metric.Interpretation"
          class="iw-derived-metric__interpretation"
        >
          {{ metric.Interpretation.Range }} · {{ metric.Interpretation.Meaning }}
        </p>
        <p v-else class="iw-meta">Not available for the current snapshot.</p>
        <p class="iw-meta">
          {{ metric.NumeratorLabel }} ({{ metric.NumeratorKpiId }}) ÷
          {{ metric.DenominatorLabel }} ({{ metric.DenominatorKpiId }})
        </p>
      </div>
    </div>

    <ul class="iw-derived-metric__guide">
      <li v-for="band in PURCHASE_TO_SALES_OUT_INTERPRETATIONS" :key="band.Code">
        <span class="iw-derived-metric__band">{{ band.Range }}</span>
        {{ band.Meaning }}
      </li>
    </ul>
  </div>
</template>

<style scoped>
.iw-derived-metric + .iw-derived-metric {
  margin-top: 1rem;
  padding-top: 1rem;
  border-top: 1px solid #e2e8f0;
}

.iw-derived-metric__body {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  margin-top: 0.5rem;
}

.iw-derived-metric__headline {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 1rem;
}

.iw-derived-metric__label {
  font-weight: 600;
}

.iw-derived-metric__value {
  font-size: var(--iw-kpi-value-size, 1.5rem);
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}

.iw-derived-metric__interpretation {
  margin: 0;
  font-size: 0.875rem;
  font-weight: 600;
  color: #1e293b;
}

.iw-derived-metric__guide {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1.25rem;
  margin: 1rem 0 0;
  padding: 0.75rem 0 0;
  border-top: 1px solid #e2e8f0;
  list-style: none;
  font-size: 0.75rem;
  color: var(--iw-text-meta, #94a3b8);
}

.iw-derived-metric__band {
  font-weight: 700;
  color: var(--iw-text-secondary, #64748b);
}
</style>

<script setup lang="ts">
import KpiCard from '@/components/KpiCard.vue'
import CompactValue from '@/components/dashboard/primitives/CompactValue.vue'
import type { EntityPerformanceProfileResponse, KpiEnvelope } from '@/models/entityAnalytics'
import { buildEntityColorMap } from '@/composables/useComparisonColors'
import { formatCurrencyCompact } from '@/services/formatters'
import { isIdrAxisUnit } from '@/services/populationProjection/robustStats'

const props = defineProps<{
  profiles: Record<string, EntityPerformanceProfileResponse>
  entityIds: string[]
  loading?: boolean
}>()

const colors = () => buildEntityColorMap(props.entityIds)

function headlineKpis(profile: EntityPerformanceProfileResponse) {
  const groups = profile.KpiSummary?.Categories ?? []
  return groups.flatMap((g) => g.Kpis).slice(0, 8)
}

function displayKpi(kpi: KpiEnvelope): { display: string; full: string | null } {
  const fallback = kpi.FormattedValue || kpi.TextValue || ''
  if (kpi.Value != null && isIdrAxisUnit(kpi.Unit)) {
    return {
      display: formatCurrencyCompact(kpi.Value),
      full: kpi.FormattedValue || null,
    }
  }
  return { display: fallback, full: null }
}
</script>

<template>
  <div v-if="loading" class="iw-skeleton" style="min-height: 6rem" />
  <div v-else class="iw-kpi-compare">
    <div
      v-for="entityId in entityIds"
      :key="entityId"
      class="iw-kpi-column"
      :style="{ borderTop: `3px solid ${colors().get(entityId)?.border ?? '#cbd5e1'}` }"
    >
      <div
        v-for="kpi in headlineKpis(profiles[entityId])"
        :key="`${entityId}-${kpi.KpiId}`"
      >
        <KpiCard :title="kpi.DisplayName || kpi.KpiId">
          <div
            class="iw-kpi-value"
            :title="displayKpi(kpi).full ?? undefined"
          >
            <CompactValue :value="displayKpi(kpi).display" />
          </div>
          <div v-if="kpi.PeriodLabel" class="iw-meta">{{ kpi.PeriodLabel }}</div>
        </KpiCard>
      </div>
    </div>
  </div>
</template>

<style scoped>
.iw-kpi-compare {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(12rem, 1fr));
  gap: 1rem;
}

.iw-kpi-column {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  padding-top: 0.5rem;
}
</style>

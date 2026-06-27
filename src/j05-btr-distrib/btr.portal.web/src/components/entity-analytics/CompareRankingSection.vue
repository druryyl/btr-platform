<script setup lang="ts">
import { computed } from 'vue'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import KpiRankChart from '@/components/entity-analytics/KpiRankChart.vue'
import type {
  CompareRankingEntity,
  CompareRankingOverlay,
  CompareRankingSection,
} from '@/models/entityAnalytics'

const props = defineProps<{
  section: CompareRankingSection | null | undefined
  loading?: boolean
}>()

function buildCompareRankingOverlays(entities: CompareRankingEntity[]): CompareRankingOverlay[] {
  const overlayMap = new Map<string, CompareRankingOverlay>()

  for (const entity of entities) {
    for (const series of entity.Ranking?.Series ?? []) {
      let overlay = overlayMap.get(series.KpiId)
      if (!overlay) {
        overlay = {
          KpiId: series.KpiId,
          DisplayName: series.DisplayName,
          Unit: series.Unit,
          RankingDirection: series.RankingDirection,
          EntitySeries: [],
        }
        overlayMap.set(series.KpiId, overlay)
      }

      overlay.EntitySeries.push({
        EntityCode: entity.EntityCode,
        DisplayName: entity.DisplayName,
        CurrentRank: series.CurrentRank,
        CurrentPercentile: series.CurrentPercentile,
        CurrentPopulationSize: series.CurrentPopulationSize,
        BestRank: series.BestRank,
        WorstRank: series.WorstRank,
        Points: series.Points,
      })
    }
  }

  return Array.from(overlayMap.values())
}

const overlays = computed(() => buildCompareRankingOverlays(props.section?.Entities ?? []))
</script>

<template>
  <ProfileSectionCard
    title="Ranking Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="overlays.length" class="compare-ranking-section">
      <KpiRankChart
        v-for="overlay in overlays"
        :key="overlay.KpiId"
        :overlay="overlay"
      />
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.compare-ranking-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>

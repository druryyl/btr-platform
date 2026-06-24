<script setup lang="ts">
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import KpiRankChart from '@/components/entity-analytics/KpiRankChart.vue'
import type { CompareRankingSection } from '@/models/entityAnalytics'

defineProps<{
  section: CompareRankingSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="Ranking Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Entities?.length" class="compare-ranking-section">
      <div
        v-for="entity in section.Entities"
        :key="entity.EntityCode"
        class="compare-ranking-section__entity"
      >
        <h3 class="compare-ranking-section__title">
          {{ entity.DisplayName }}
          <small>{{ entity.EntityCode }}</small>
        </h3>
        <KpiRankChart
          v-for="series in entity.Ranking?.Series ?? []"
          :key="`${entity.EntityCode}-${series.KpiId}`"
          :series="series"
        />
      </div>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.compare-ranking-section {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.compare-ranking-section__title {
  margin: 0 0 0.75rem;
  font-size: 0.95rem;
  font-weight: 600;
}

.compare-ranking-section__title small {
  display: block;
  color: var(--p-text-muted-color, #64748b);
  font-weight: 400;
}
</style>

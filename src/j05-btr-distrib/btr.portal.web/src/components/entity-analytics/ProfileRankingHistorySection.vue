<script setup lang="ts">
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import KpiRankChart from '@/components/entity-analytics/KpiRankChart.vue'
import type { ProfileRankingSection } from '@/models/entityAnalytics'

defineProps<{
  section: ProfileRankingSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="Ranking History"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Series?.length" class="profile-ranking-section">
      <KpiRankChart
        v-for="series in section.Series"
        :key="series.KpiId"
        :series="series"
        class="profile-ranking-section__chart"
      />
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-ranking-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>

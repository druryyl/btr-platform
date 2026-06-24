<script setup lang="ts">
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import KpiTrendChart from '@/components/entity-analytics/KpiTrendChart.vue'
import type { ProfileTrendSection } from '@/models/entityAnalytics'

defineProps<{
  section: ProfileTrendSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="Trend"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Series?.length" class="profile-trend-section">
      <KpiTrendChart
        v-for="series in section.Series"
        :key="series.KpiId"
        :series="series"
        class="profile-trend-section__chart"
      />
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-trend-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>

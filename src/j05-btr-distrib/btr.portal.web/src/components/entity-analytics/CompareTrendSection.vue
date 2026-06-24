<script setup lang="ts">
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import KpiTrendChart from '@/components/entity-analytics/KpiTrendChart.vue'
import type { CompareTrendSection } from '@/models/entityAnalytics'

defineProps<{
  section: CompareTrendSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="Trend Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Overlays?.length" class="compare-trend-section">
      <KpiTrendChart
        v-for="overlay in section.Overlays"
        :key="overlay.KpiId"
        :overlay="overlay"
      />
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.compare-trend-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}
</style>

<script setup lang="ts">
import { computed } from 'vue'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import EntityRadarChart from '@/components/entity-analytics/EntityRadarChart.vue'
import type { ProfileRadarSection } from '@/models/entityAnalytics'

const props = defineProps<{
  section: ProfileRadarSection | null | undefined
  loading?: boolean
}>()

const series = computed(() => {
  if (!props.section?.Axes?.length) return []
  return [
    {
      label: 'Score',
      scores: props.section.Axes.map((axis) => axis.Score),
    },
  ]
})
</script>

<template>
  <ProfileSectionCard
    title="Radar"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <template v-if="section?.IsAvailable">
      <p v-if="section.PeriodLabel || section.PeerGroupSize" class="profile-radar-section__meta">
        <span v-if="section.PeriodLabel">{{ section.PeriodLabel }}</span>
        <span v-if="section.PeerGroupSize != null"> · Peer group: {{ section.PeerGroupSize }}</span>
      </p>
      <EntityRadarChart :axes="section.Axes" :series="series" />
    </template>
    <p
      v-else-if="section?.UnavailableExplanation"
      class="profile-radar-section__explanation"
    >
      {{ section.UnavailableExplanation }}
    </p>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-radar-section__meta {
  margin: 0 0 0.75rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.profile-radar-section__explanation {
  margin: 0;
  color: var(--p-text-muted-color);
}
</style>

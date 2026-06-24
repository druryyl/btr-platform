<script setup lang="ts">
import { computed } from 'vue'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import EntityRadarChart from '@/components/entity-analytics/EntityRadarChart.vue'
import type { CompareRadarSection } from '@/models/entityAnalytics'

const props = defineProps<{
  section: CompareRadarSection | null | undefined
}>()

const series = computed(() =>
  (props.section?.Overlays ?? []).map((overlay) => ({
    label: overlay.DisplayName || overlay.EntityCode,
    scores: overlay.Scores,
  })),
)
</script>

<template>
  <ProfileSectionCard
    title="Radar Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
  >
    <template v-if="section?.IsAvailable">
      <p v-if="section.PeriodLabel || section.PeerGroupSize" class="radar-compare-section__meta">
        <span v-if="section.PeriodLabel">{{ section.PeriodLabel }}</span>
        <span v-if="section.PeerGroupSize != null"> · Peer group: {{ section.PeerGroupSize }}</span>
      </p>
      <EntityRadarChart :axes="section.Axes" :series="series" />
    </template>
  </ProfileSectionCard>
</template>

<style scoped>
.radar-compare-section__meta {
  margin: 0 0 0.75rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}
</style>

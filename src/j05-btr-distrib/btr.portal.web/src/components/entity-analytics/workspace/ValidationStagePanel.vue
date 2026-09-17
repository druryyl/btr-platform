<script setup lang="ts">
import { computed } from 'vue'
import ProfileEvidenceSection from '@/components/entity-analytics/ProfileEvidenceSection.vue'
import type { EntityPerformanceProfileResponse } from '@/models/entityAnalytics'

const props = defineProps<{
  entityIds: string[]
  profiles: Record<string, EntityPerformanceProfileResponse>
  loading?: boolean
  activeLensId?: string | null
}>()

const filteredEvidence = computed(() => {
  if (!props.entityIds.length || !props.profiles[props.entityIds[0]])
    return null
  const section = props.profiles[props.entityIds[0]].Evidence
  if (!section?.Links?.length) return section
  if (!props.activeLensId) return section
  return {
    ...section,
    Links: section.Links.filter(
      (link) => !link.LensId || link.LensId === props.activeLensId,
    ),
  }
})
</script>

<template>
  <div class="iw-panel-card">
    <h3 class="iw-section-title">Evidence</h3>
    <p class="iw-meta">What source data proves the conclusion?</p>
    <ProfileEvidenceSection
      v-if="entityIds.length >= 1 && profiles[entityIds[0]]"
      :section="filteredEvidence"
      :entity-code="profiles[entityIds[0]].Overview?.EntityCode"
      :loading="loading"
    />
  </div>
</template>

<script setup lang="ts">
import ProfileEvidenceSection from '@/components/entity-analytics/ProfileEvidenceSection.vue'
import ProfileRadarSection from '@/components/entity-analytics/ProfileRadarSection.vue'
import type { EntityPerformanceProfileResponse } from '@/models/entityAnalytics'

defineProps<{
  entityIds: string[]
  profiles: Record<string, EntityPerformanceProfileResponse>
  loading?: boolean
}>()
</script>

<template>
  <div class="iw-panel-card">
    <h3 class="iw-section-title">Evidence</h3>
    <p class="iw-meta">What source data proves the conclusion?</p>
    <ProfileEvidenceSection
      v-if="entityIds.length >= 1 && profiles[entityIds[0]]"
      :section="profiles[entityIds[0]].Evidence"
      :entity-code="profiles[entityIds[0]].Overview?.EntityCode"
      :loading="loading"
    />
    <div v-if="entityIds.length === 1 && profiles[entityIds[0]]" class="iw-signature">
      <h3 class="iw-section-title">Performance Signature</h3>
      <ProfileRadarSection
        :section="profiles[entityIds[0]].Radar"
        :loading="loading"
      />
    </div>
  </div>
</template>

<style scoped>
.iw-signature {
  margin-top: 1.25rem;
  padding-top: 1.25rem;
  border-top: 1px solid #e2e8f0;
}
</style>

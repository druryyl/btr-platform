<script setup lang="ts">
import ProfileAttentionHistorySection from '@/components/entity-analytics/ProfileAttentionHistorySection.vue'
import CompareAttentionSection from '@/components/entity-analytics/CompareAttentionSection.vue'
import type { EntityCompareResponse, EntityPerformanceProfileResponse } from '@/models/entityAnalytics'

defineProps<{
  entityIds: string[]
  profiles: Record<string, EntityPerformanceProfileResponse>
  compareBundle: EntityCompareResponse | null
  loading?: boolean
}>()
</script>

<template>
  <div class="iw-panel-card">
    <h3 class="iw-section-title">Signal History</h3>
    <p class="iw-meta">Are management signals new, recurring, resolved, or chronic?</p>
    <div v-if="entityIds.length > 1" class="iw-signal-columns">
      <CompareAttentionSection
        v-if="compareBundle"
        :section="compareBundle.AttentionComparison"
        :loading="loading"
      />
    </div>
    <ProfileAttentionHistorySection
      v-else-if="entityIds.length === 1 && profiles[entityIds[0]]"
      :section="profiles[entityIds[0]].Attention"
      :loading="loading"
    />
  </div>
</template>

<style scoped>
.iw-signal-columns {
  display: grid;
  gap: 1rem;
}
</style>

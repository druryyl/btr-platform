<script setup lang="ts">
import ProfileRankingHistorySection from '@/components/entity-analytics/ProfileRankingHistorySection.vue'
import CompareRankingSection from '@/components/entity-analytics/CompareRankingSection.vue'
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
    <h3 class="iw-section-title">Position History</h3>
    <p class="iw-meta">Did the entity's rank among peers change meaningfully?</p>
    <CompareRankingSection
      v-if="entityIds.length > 1 && compareBundle"
      :section="compareBundle.RankingComparison"
      :loading="loading"
    />
    <ProfileRankingHistorySection
      v-else-if="entityIds.length === 1 && profiles[entityIds[0]]"
      :section="profiles[entityIds[0]].Ranking"
      :loading="loading"
    />
  </div>
</template>

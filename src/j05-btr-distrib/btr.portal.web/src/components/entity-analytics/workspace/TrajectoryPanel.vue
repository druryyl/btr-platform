<script setup lang="ts">
import ProfileTrendSection from '@/components/entity-analytics/ProfileTrendSection.vue'
import CompareTrendSection from '@/components/entity-analytics/CompareTrendSection.vue'
import type { EntityCompareResponse, EntityPerformanceProfileResponse } from '@/models/entityAnalytics'

const props = defineProps<{
  entityIds: string[]
  profiles: Record<string, EntityPerformanceProfileResponse>
  compareBundle: EntityCompareResponse | null
  loading?: boolean
}>()
</script>

<template>
  <div class="iw-panel-card">
    <h3 class="iw-section-title">Trajectory</h3>
    <p class="iw-meta">Is the situation improving, deteriorating, temporary, or persistent?</p>
    <CompareTrendSection
      v-if="entityIds.length > 1 && compareBundle"
      :section="compareBundle.TrendComparison"
      :loading="loading"
    />
    <ProfileTrendSection
      v-else-if="entityIds.length === 1 && profiles[entityIds[0]]"
      :section="profiles[entityIds[0]].Trend"
      :loading="loading"
    />
  </div>
</template>

<script setup lang="ts">
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import ProfileOverviewSection from '@/components/entity-analytics/ProfileOverviewSection.vue'
import ProfileKpiSummarySection from '@/components/entity-analytics/ProfileKpiSummarySection.vue'
import ProfileComparisonSection from '@/components/entity-analytics/ProfileComparisonSection.vue'
import ProfileTrendSection from '@/components/entity-analytics/ProfileTrendSection.vue'
import ProfileRadarSection from '@/components/entity-analytics/ProfileRadarSection.vue'
import ProfileRankingHistorySection from '@/components/entity-analytics/ProfileRankingHistorySection.vue'
import ProfileAttentionHistorySection from '@/components/entity-analytics/ProfileAttentionHistorySection.vue'
import ProfileRelatedEntitiesSection from '@/components/entity-analytics/ProfileRelatedEntitiesSection.vue'
import ProfileEvidenceSection from '@/components/entity-analytics/ProfileEvidenceSection.vue'
import type { EntityPerformanceProfileResponse } from '@/models/entityAnalytics'

defineProps<{
  profile: EntityPerformanceProfileResponse | null
  loading?: boolean
  error?: string | null
}>()

const emit = defineEmits<{
  refresh: []
}>()
</script>

<template>
  <DashboardDetailLayout
    :title="profile?.Overview?.DisplayName || profile?.EntityId || 'Entity Performance Profile'"
    :subtitle="profile ? `${profile.EntityType} · ${profile.EntityId}` : 'Entity Analytics'"
    :loading="loading"
    :error="error"
    :generated-at="profile?.GeneratedAt"
    @refresh="emit('refresh')"
  >
    <Message
      v-if="profile && profile.IsAvailable === false && !loading"
      severity="info"
      :closable="false"
      class="entity-profile-shell__banner"
    >
      Snapshot data is not yet available for this entity. Run the Customer dashboard snapshot
      worker to populate Entity Analytics L0 data.
    </Message>

    <div class="entity-profile-shell__sections">
      <ProfileOverviewSection :section="profile?.Overview" :loading="loading" />
      <ProfileKpiSummarySection :section="profile?.KpiSummary" :loading="loading" />
      <ProfileComparisonSection :section="profile?.Comparison" :loading="loading" />
      <ProfileTrendSection :section="profile?.Trend" :loading="loading" />
      <ProfileRadarSection :section="profile?.Radar" :loading="loading" />
      <ProfileRankingHistorySection :section="profile?.Ranking" :loading="loading" />
      <ProfileAttentionHistorySection :section="profile?.Attention" :loading="loading" />
      <ProfileRelatedEntitiesSection :section="profile?.RelatedEntities" :loading="loading" />
      <ProfileEvidenceSection :section="profile?.Evidence" :loading="loading" />
    </div>
  </DashboardDetailLayout>
</template>

<style scoped>
.entity-profile-shell__banner {
  margin-bottom: 1rem;
}

.entity-profile-shell__sections {
  display: flex;
  flex-direction: column;
  gap: 0;
}
</style>

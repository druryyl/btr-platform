<script setup lang="ts">
import Message from 'primevue/message'
import Button from 'primevue/button'
import { RouterLink } from 'vue-router'
import { computed } from 'vue'
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
import { buildCompareRoute, getEntityAnalyticsNav } from '@/navigation/entityAnalyticsNavigation'

const props = defineProps<{
  profile: EntityPerformanceProfileResponse | null
  loading?: boolean
  error?: string | null
  entityCode?: string | null
}>()

const emit = defineEmits<{
  refresh: []
}>()

const resolvedEntityType = computed(
  () => props.profile?.EntityType ?? props.profile?.Overview?.EntityType ?? null,
)

const resolvedEntityCode = computed(
  () =>
    props.entityCode?.trim()
    || props.profile?.Overview?.EntityCode?.trim()
    || null,
)

const resolvedEntityId = computed(
  () => props.profile?.EntityId?.trim() || null,
)

const navConfig = computed(() => getEntityAnalyticsNav(resolvedEntityType.value))

const compareRoute = computed(() => {
  if (!navConfig.value) return null
  return buildCompareRoute(navConfig.value.entityType, resolvedEntityId.value)
})

const compareLabel = computed(() => {
  if (!navConfig.value) return null
  return `Compare ${navConfig.value.pluralLabel}`
})
</script>

<template>
  <DashboardDetailLayout
    :title="profile?.Overview?.DisplayName || profile?.EntityId || 'Entity Performance Profile'"
    :subtitle="profile ? `${profile.EntityType} · ${profile.Overview?.EntityCode || profile.EntityId}` : 'Entity Analytics'"
    :loading="loading"
    :error="error"
    :generated-at="profile?.GeneratedAt"
    @refresh="emit('refresh')"
  >
    <template #header-actions>
      <RouterLink
        v-if="compareRoute && compareLabel"
        v-slot="{ navigate }"
        :to="compareRoute"
        custom
      >
        <Button
          :label="compareLabel"
          icon="pi pi-chart-bar"
          outlined
          severity="secondary"
          @click="navigate"
        />
      </RouterLink>
      <RouterLink v-slot="{ navigate }" :to="{ name: 'entity-analytics-home' }" custom>
        <Button
          label="Entity Analytics"
          icon="pi pi-id-card"
          text
          severity="secondary"
          @click="navigate"
        />
      </RouterLink>
    </template>

    <Message
      v-if="profile && profile.IsAvailable === false && !loading"
      severity="info"
      :closable="false"
      class="entity-profile-shell__banner"
    >
      Snapshot data is not yet available for this entity. Run the domain dashboard snapshot worker
      to populate Entity Analytics data.
    </Message>

    <div class="entity-profile-shell__sections">
      <ProfileOverviewSection :section="profile?.Overview" :loading="loading" />
      <ProfileKpiSummarySection
        :section="profile?.KpiSummary"
        :entity-code="resolvedEntityCode"
        :loading="loading"
      />
      <ProfileComparisonSection :section="profile?.Comparison" :loading="loading" />
      <ProfileTrendSection :section="profile?.Trend" :loading="loading" />
      <ProfileRadarSection
        :section="profile?.Radar"
        :loading="loading"
        :entity-label="profile?.Overview?.DisplayName"
      />
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

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import KpiCard from '@/components/KpiCard.vue'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { ProfileKpiSummarySection } from '@/models/entityAnalytics'

const props = defineProps<{
  section: ProfileKpiSummarySection | null | undefined
  entityCode?: string | null
  loading?: boolean
}>()

function evidenceHref(baseRoute: string, filterDimension?: string | null): string {
  const entityCode = props.entityCode?.trim() ?? ''
  if (!entityCode || !filterDimension) return baseRoute
  const separator = baseRoute.includes('?') ? '&' : '?'
  return `${baseRoute}${separator}${encodeURIComponent(filterDimension)}=${encodeURIComponent(entityCode)}`
}
</script>

<template>
  <ProfileSectionCard
    title="KPI Summary"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Categories?.length" class="profile-kpi-summary">
      <div
        v-for="group in section.Categories"
        :key="group.Category"
        class="profile-kpi-summary__group"
      >
        <h3 class="profile-kpi-summary__category">{{ group.Category }}</h3>
        <div class="profile-kpi-summary__grid">
          <KpiCard
            v-for="kpi in group.Kpis"
            :key="kpi.KpiId"
            :title="kpi.DisplayName || kpi.KpiId"
          >
            <div class="profile-kpi-summary__value">
              {{ kpi.FormattedValue || kpi.TextValue || '—' }}
            </div>
            <div v-if="kpi.PeriodLabel" class="profile-kpi-summary__meta">
              {{ kpi.PeriodLabel }}
            </div>
            <RouterLink
              v-if="kpi.EvidenceRoute"
              :to="evidenceHref(kpi.EvidenceRoute, kpi.FilterDimension)"
              class="profile-kpi-summary__evidence"
            >
              View evidence
            </RouterLink>
          </KpiCard>
        </div>
      </div>
    </div>
    <template #unavailable>
      No snapshot KPIs are available for this entity yet.
    </template>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-kpi-summary {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.profile-kpi-summary__category {
  margin: 0 0 0.75rem;
  font-size: 1rem;
  font-weight: 600;
}

.profile-kpi-summary__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(14rem, 1fr));
  gap: 1rem;
}

.profile-kpi-summary__value {
  font-size: 1.5rem;
  font-weight: 600;
}

.profile-kpi-summary__meta {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.profile-kpi-summary__evidence {
  display: inline-block;
  margin-top: 0.5rem;
  color: var(--p-primary-color);
  font-size: 0.8125rem;
  font-weight: 600;
  text-decoration: none;
}

.profile-kpi-summary__evidence:hover {
  text-decoration: underline;
}
</style>

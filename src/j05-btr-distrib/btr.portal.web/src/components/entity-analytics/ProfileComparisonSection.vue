<script setup lang="ts">
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { ComparisonMetric, ProfileComparisonSection } from '@/models/entityAnalytics'

defineProps<{
  section: ProfileComparisonSection | null | undefined
  loading?: boolean
}>()

function formatDelta(value: number | null | undefined, unit: string): string {
  if (value == null) return '—'
  const prefix = value > 0 ? '+' : ''
  if (unit === 'Percent') return `${prefix}${value.toFixed(1)}%`
  if (unit === 'IDR') return `${prefix}${value.toLocaleString('en-US', { maximumFractionDigits: 0 })}`
  return `${prefix}${value.toLocaleString('en-US', { maximumFractionDigits: 2 })}`
}

function deltaClass(direction: string, delta: number | null | undefined): string {
  if (delta == null || delta === 0) return 'neutral'
  const favorable =
    (direction === 'HigherIsBetter' && delta > 0) ||
    (direction === 'LowerIsBetter' && delta < 0)
  return favorable ? 'favorable' : 'unfavorable'
}

function metricLabel(metric: ComparisonMetric): string {
  return metric.CurrentPeriodLabel || metric.DisplayName
}
</script>

<template>
  <ProfileSectionCard
    title="Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Metrics?.length" class="profile-comparison-section">
      <article
        v-for="metric in section.Metrics"
        :key="metric.KpiId"
        class="profile-comparison-section__card"
      >
        <header>
          <h3>{{ metric.DisplayName }}</h3>
          <p class="profile-comparison-section__current">
            {{ metric.CurrentFormatted }}
            <small>{{ metricLabel(metric) }}</small>
          </p>
        </header>
        <dl class="profile-comparison-section__deltas">
          <div>
            <dt>MoM</dt>
            <dd :class="deltaClass(metric.Direction, metric.MomDelta)">
              {{ formatDelta(metric.MomGrowthPercent, 'Percent') }}
              <small v-if="metric.PriorMonthFormatted">
                vs {{ metric.PriorMonthFormatted }}
                <span v-if="metric.PriorMonthPeriodLabel">({{ metric.PriorMonthPeriodLabel }})</span>
              </small>
            </dd>
          </div>
          <div>
            <dt>YoY</dt>
            <dd :class="deltaClass(metric.Direction, metric.YoyDelta)">
              {{ formatDelta(metric.YoyGrowthPercent, 'Percent') }}
              <small v-if="metric.PriorYearFormatted">
                vs {{ metric.PriorYearFormatted }}
                <span v-if="metric.PriorYearPeriodLabel">({{ metric.PriorYearPeriodLabel }})</span>
              </small>
            </dd>
          </div>
        </dl>
      </article>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-comparison-section {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(16rem, 1fr));
  gap: 1rem;
}

.profile-comparison-section__card {
  border: 1px solid var(--p-content-border-color, #e2e8f0);
  border-radius: 0.5rem;
  padding: 0.85rem;
}

.profile-comparison-section__card h3 {
  margin: 0 0 0.35rem;
  font-size: 0.95rem;
}

.profile-comparison-section__current {
  margin: 0 0 0.75rem;
  font-size: 1.1rem;
  font-weight: 600;
}

.profile-comparison-section__current small {
  display: block;
  color: var(--p-text-muted-color, #64748b);
  font-size: 0.8rem;
  font-weight: 400;
}

.profile-comparison-section__deltas {
  margin: 0;
  display: grid;
  gap: 0.5rem;
}

.profile-comparison-section__deltas dt {
  font-size: 0.75rem;
  text-transform: uppercase;
  color: var(--p-text-muted-color, #64748b);
}

.profile-comparison-section__deltas dd {
  margin: 0.15rem 0 0;
  font-weight: 600;
}

.profile-comparison-section__deltas dd small {
  display: block;
  color: var(--p-text-muted-color, #64748b);
  font-weight: 400;
  font-size: 0.8rem;
}

.profile-comparison-section__deltas .favorable {
  color: #15803d;
}

.profile-comparison-section__deltas .unfavorable {
  color: #b91c1c;
}

.profile-comparison-section__deltas .neutral {
  color: var(--p-text-color, #334155);
}
</style>

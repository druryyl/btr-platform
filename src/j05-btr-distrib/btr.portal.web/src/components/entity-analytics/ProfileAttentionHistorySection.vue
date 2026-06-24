<script setup lang="ts">
import { computed } from 'vue'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { ProfileAttentionEvent, ProfileAttentionSection } from '@/models/entityAnalytics'

const props = defineProps<{
  section: ProfileAttentionSection | null | undefined
  loading?: boolean
}>()

const activeSignals = computed(() =>
  (props.section?.Events ?? []).filter((event) => event.IsActive),
)

const resolvedSignals = computed(() =>
  (props.section?.Events ?? []).filter((event) => !event.IsActive),
)

function statusLabel(event: ProfileAttentionEvent): string {
  return event.IsActive ? 'Active' : 'Resolved'
}
</script>

<template>
  <ProfileSectionCard
    title="Attention History"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Events?.length" class="profile-attention-section">
      <div class="profile-attention-section__summary">
        <span>{{ section.ActiveSignalCount }} active</span>
        <span>{{ section.HistoricalSignalCount }} resolved</span>
      </div>

      <section v-if="activeSignals.length" class="profile-attention-section__group">
        <h3 class="profile-attention-section__heading">Active Signals</h3>
        <div
          v-for="event in activeSignals"
          :key="`${event.SignalCode}-active`"
          class="profile-attention-section__item profile-attention-section__item--active"
        >
          <div class="profile-attention-section__item-header">
            <strong>{{ event.SignalLabel }}</strong>
            <span class="profile-attention-section__status">{{ statusLabel(event) }}</span>
          </div>
          <dl class="profile-attention-section__meta">
            <div>
              <dt>First Seen</dt>
              <dd>{{ event.FirstSeen ?? '—' }}</dd>
            </div>
            <div>
              <dt>Last Seen</dt>
              <dd>{{ event.LastSeen ?? '—' }}</dd>
            </div>
            <div>
              <dt>Consecutive Periods</dt>
              <dd>{{ event.ConsecutivePeriods }}</dd>
            </div>
            <div>
              <dt>Occurrence Count</dt>
              <dd>{{ event.TotalOccurrences }}</dd>
            </div>
          </dl>
        </div>
      </section>

      <section v-if="resolvedSignals.length" class="profile-attention-section__group">
        <h3 class="profile-attention-section__heading">Resolved Signals</h3>
        <div
          v-for="event in resolvedSignals"
          :key="`${event.SignalCode}-resolved`"
          class="profile-attention-section__item profile-attention-section__item--resolved"
        >
          <div class="profile-attention-section__item-header">
            <strong>{{ event.SignalLabel }}</strong>
            <span class="profile-attention-section__status">{{ statusLabel(event) }}</span>
          </div>
          <dl class="profile-attention-section__meta">
            <div>
              <dt>First Seen</dt>
              <dd>{{ event.FirstSeen ?? '—' }}</dd>
            </div>
            <div>
              <dt>Last Seen</dt>
              <dd>{{ event.LastSeen ?? '—' }}</dd>
            </div>
            <div>
              <dt>Consecutive Periods</dt>
              <dd>{{ event.ConsecutivePeriods }}</dd>
            </div>
            <div>
              <dt>Occurrence Count</dt>
              <dd>{{ event.TotalOccurrences }}</dd>
            </div>
          </dl>
        </div>
      </section>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-attention-section {
  display: flex;
  flex-direction: column;
  gap: 1rem;
}

.profile-attention-section__summary {
  display: flex;
  gap: 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.profile-attention-section__group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.profile-attention-section__heading {
  margin: 0;
  font-size: 0.95rem;
  font-weight: 600;
}

.profile-attention-section__item {
  border: 1px solid var(--p-content-border-color);
  border-radius: 0.5rem;
  padding: 0.75rem 1rem;
}

.profile-attention-section__item--active {
  border-left: 3px solid var(--p-red-500, #ef4444);
}

.profile-attention-section__item--resolved {
  border-left: 3px solid var(--p-surface-400, #94a3b8);
}

.profile-attention-section__item-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 0.75rem;
  margin-bottom: 0.5rem;
}

.profile-attention-section__status {
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.profile-attention-section__meta {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(8rem, 1fr));
  gap: 0.5rem 1rem;
  margin: 0;
}

.profile-attention-section__meta dt {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}

.profile-attention-section__meta dd {
  margin: 0;
  font-size: 0.875rem;
}
</style>

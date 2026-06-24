<script setup lang="ts">
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { CompareAttentionSection } from '@/models/entityAnalytics'

defineProps<{
  section: CompareAttentionSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="Attention Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Entities?.length" class="compare-attention-section">
      <div
        v-for="entity in section.Entities"
        :key="entity.EntityCode"
        class="compare-attention-section__entity"
      >
        <h3 class="compare-attention-section__title">
          {{ entity.DisplayName }}
          <small>{{ entity.EntityCode }}</small>
        </h3>
        <p v-if="!entity.Attention?.IsAvailable" class="compare-attention-section__empty">
          No attention signals recorded.
        </p>
        <ul v-else class="compare-attention-section__list">
          <li
            v-for="event in entity.Attention.Events"
            :key="`${entity.EntityCode}-${event.SignalCode}`"
            :class="{ 'is-active': event.IsActive }"
          >
            <strong>{{ event.SignalLabel }}</strong>
            <span>{{ event.IsActive ? 'Active' : 'Resolved' }}</span>
            <small>{{ event.FirstSeen }} – {{ event.LastSeen }}</small>
          </li>
        </ul>
      </div>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.compare-attention-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr));
  gap: 1rem;
}

.compare-attention-section__title {
  margin: 0 0 0.5rem;
  font-size: 0.95rem;
  font-weight: 600;
}

.compare-attention-section__title small {
  display: block;
  color: var(--p-text-muted-color, #64748b);
  font-weight: 400;
}

.compare-attention-section__list {
  margin: 0;
  padding-left: 1.1rem;
}

.compare-attention-section__list li {
  margin-bottom: 0.5rem;
}

.compare-attention-section__list li.is-active strong {
  color: var(--p-primary-color, #2563eb);
}

.compare-attention-section__list span,
.compare-attention-section__list small {
  display: block;
  color: var(--p-text-muted-color, #64748b);
}

.compare-attention-section__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
  font-size: 0.875rem;
}
</style>

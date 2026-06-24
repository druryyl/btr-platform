<script setup lang="ts">
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { ProfileOverviewSection } from '@/models/entityAnalytics'
import { formatDateTime } from '@/services/formatters'

defineProps<{
  section: ProfileOverviewSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="Overview"
    :is-available="section?.IsAvailable !== false"
    :loading="loading"
  >
    <dl v-if="section" class="profile-overview">
      <div class="profile-overview__row">
        <dt>Entity</dt>
        <dd>{{ section.DisplayName || section.EntityCode || section.EntityId }}</dd>
      </div>
      <div class="profile-overview__row">
        <dt>Type</dt>
        <dd>{{ section.EntityType }}</dd>
      </div>
      <div class="profile-overview__row">
        <dt>Code</dt>
        <dd>{{ section.EntityCode }}</dd>
      </div>
      <div class="profile-overview__row">
        <dt>Status</dt>
        <dd>{{ section.IsActive ? 'Active' : 'Inactive' }}</dd>
      </div>
      <div v-if="section.GeneratedAt" class="profile-overview__row">
        <dt>Snapshot</dt>
        <dd>{{ formatDateTime(section.GeneratedAt) }}</dd>
      </div>
      <div
        v-for="(value, key) in section.Dimensions"
        :key="key"
        class="profile-overview__row"
      >
        <dt>{{ key }}</dt>
        <dd>{{ value }}</dd>
      </div>
    </dl>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-overview {
  display: grid;
  gap: 0.75rem;
  margin: 0;
}

.profile-overview__row {
  display: grid;
  grid-template-columns: 8rem 1fr;
  gap: 0.75rem;
}

.profile-overview__row dt {
  margin: 0;
  color: var(--p-text-muted-color);
  font-weight: 500;
}

.profile-overview__row dd {
  margin: 0;
}
</style>

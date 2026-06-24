<script setup lang="ts">
import { RouterLink } from 'vue-router'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { ProfileEvidenceSection } from '@/models/entityAnalytics'

defineProps<{
  section: ProfileEvidenceSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="Evidence"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <ul v-if="section?.Links?.length" class="profile-evidence">
      <li v-for="link in section.Links" :key="link.ReportRoute" class="profile-evidence__item">
        <div class="profile-evidence__meta">
          <span class="profile-evidence__category">{{ link.Category }}</span>
          <span class="profile-evidence__label">{{ link.Label }}</span>
        </div>
        <RouterLink :to="link.ReportRoute" class="profile-evidence__link">
          Open report
        </RouterLink>
      </li>
    </ul>
    <template #unavailable>
      Evidence links are not available for this entity yet.
    </template>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-evidence {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
  margin: 0;
  padding: 0;
  list-style: none;
}

.profile-evidence__item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.75rem 1rem;
  border: 1px solid var(--p-content-border-color);
  border-radius: var(--p-border-radius-md);
}

.profile-evidence__meta {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.profile-evidence__category {
  font-size: 0.75rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--p-text-muted-color);
}

.profile-evidence__label {
  font-weight: 600;
}

.profile-evidence__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
  white-space: nowrap;
}

.profile-evidence__link:hover {
  text-decoration: underline;
}
</style>

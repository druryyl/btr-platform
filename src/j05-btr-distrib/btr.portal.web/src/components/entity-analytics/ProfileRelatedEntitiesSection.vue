<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { ProfileRelatedEntitiesSection } from '@/models/entityAnalytics'

const props = defineProps<{
  section: ProfileRelatedEntitiesSection | null | undefined
  loading?: boolean
}>()

const blocks = computed(() => props.section?.Blocks ?? [])

function formatMetric(value: number | null | undefined): string {
  if (value == null)
    return '—'

  return new Intl.NumberFormat('id-ID', {
    style: 'currency',
    currency: 'IDR',
    maximumFractionDigits: 0,
  }).format(value)
}
</script>

<template>
  <ProfileSectionCard
    title="Related Entities"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="blocks.length" class="profile-related-entities">
      <section
        v-for="block in blocks"
        :key="block.RelationshipCode"
        class="profile-related-entities__block"
      >
        <h3 class="profile-related-entities__heading">
          {{ block.RelationshipLabel || block.DisplayName }}
        </h3>

        <div class="profile-related-entities__table-wrap">
          <table class="profile-related-entities__table">
            <thead>
              <tr>
                <th v-if="block.Rows.length > 1">#</th>
                <th>{{ block.TargetEntityType }}</th>
                <th v-if="block.Rows.some((row) => row.MetricValue != null)">MTD Omzet</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="row in block.Rows" :key="`${block.RelationshipCode}-${row.Rank}-${row.TargetEntityCode}`">
                <td v-if="block.Rows.length > 1">{{ row.Rank }}</td>
                <td>
                  <RouterLink
                    v-if="row.ProfileRoute"
                    :to="row.ProfileRoute"
                    class="profile-related-entities__link"
                  >
                    {{ row.TargetEntityName || row.DisplayName || row.TargetEntityCode }}
                  </RouterLink>
                  <span v-else>{{ row.TargetEntityName || row.DisplayName || row.TargetEntityCode }}</span>
                </td>
                <td v-if="block.Rows.some((r) => r.MetricValue != null)">
                  {{ formatMetric(row.MetricValue) }}
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.profile-related-entities {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.profile-related-entities__heading {
  margin: 0 0 0.75rem;
  font-size: 0.95rem;
  font-weight: 600;
}

.profile-related-entities__table-wrap {
  overflow-x: auto;
}

.profile-related-entities__table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.profile-related-entities__table th,
.profile-related-entities__table td {
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid var(--p-content-border-color);
  text-align: left;
}

.profile-related-entities__table th {
  color: var(--p-text-muted-color);
  font-weight: 600;
}

.profile-related-entities__link {
  color: var(--p-primary-color);
  text-decoration: none;
}

.profile-related-entities__link:hover {
  text-decoration: underline;
}
</style>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import type { CompareRelationshipSection } from '@/models/entityAnalytics'

defineProps<{
  section: CompareRelationshipSection | null | undefined
  loading?: boolean
}>()
</script>

<template>
  <ProfileSectionCard
    title="Relationship Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="section?.Entities?.length" class="compare-relationships-section">
      <div
        v-for="entity in section.Entities"
        :key="entity.EntityCode"
        class="compare-relationships-section__entity"
      >
        <h3 class="compare-relationships-section__title">
          {{ entity.DisplayName }}
          <small>{{ entity.EntityCode }}</small>
        </h3>
        <div
          v-for="block in entity.RelatedEntities?.Blocks ?? []"
          :key="`${entity.EntityCode}-${block.RelationshipCode}`"
          class="compare-relationships-section__block"
        >
          <h4>{{ block.RelationshipLabel }}</h4>
          <ul>
            <li v-for="row in block.Rows" :key="`${block.RelationshipCode}-${row.Rank}`">
              <RouterLink v-if="row.ProfileRoute" :to="row.ProfileRoute">
                {{ row.TargetEntityName || row.TargetEntityCode }}
              </RouterLink>
              <span v-else>{{ row.TargetEntityName || row.TargetEntityCode }}</span>
            </li>
          </ul>
        </div>
      </div>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.compare-relationships-section {
  display: flex;
  flex-direction: column;
  gap: 1.5rem;
}

.compare-relationships-section__title {
  margin: 0 0 0.75rem;
  font-size: 0.95rem;
  font-weight: 600;
}

.compare-relationships-section__title small {
  display: block;
  color: var(--p-text-muted-color, #64748b);
  font-weight: 400;
}

.compare-relationships-section__block h4 {
  margin: 0 0 0.35rem;
  font-size: 0.875rem;
}

.compare-relationships-section__block ul {
  margin: 0 0 0.75rem;
  padding-left: 1.1rem;
}
</style>

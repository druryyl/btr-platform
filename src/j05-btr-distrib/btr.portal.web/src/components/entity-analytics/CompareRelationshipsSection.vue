<script setup lang="ts">
import { computed } from 'vue'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import RelatedEntitiesBlocks from '@/components/entity-analytics/RelatedEntitiesBlocks.vue'
import type { CompareRelationshipSection } from '@/models/entityAnalytics'

const props = defineProps<{
  section: CompareRelationshipSection | null | undefined
  loading?: boolean
}>()

const entities = computed(() => props.section?.Entities ?? [])

function hasRelationshipBlocks(entity: (typeof entities.value)[number]): boolean {
  return (entity.RelatedEntities?.Blocks?.length ?? 0) > 0
}
</script>

<template>
  <ProfileSectionCard
    title="Relationship Comparison"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <div v-if="entities.length" class="compare-relationships-section">
      <div
        v-for="entity in entities"
        :key="entity.EntityCode"
        class="compare-relationships-section__entity"
      >
        <h3 class="compare-relationships-section__title">
          {{ entity.DisplayName }}
          <small>{{ entity.EntityCode }}</small>
        </h3>
        <p
          v-if="!entity.RelatedEntities?.IsAvailable || !hasRelationshipBlocks(entity)"
          class="compare-relationships-section__empty"
        >
          No related entities recorded.
        </p>
        <RelatedEntitiesBlocks
          v-else
          :blocks="entity.RelatedEntities.Blocks"
        />
      </div>
    </div>
  </ProfileSectionCard>
</template>

<style scoped>
.compare-relationships-section {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(14rem, 1fr));
  gap: 1rem;
}

.compare-relationships-section__title {
  margin: 0 0 0.5rem;
  font-size: 0.95rem;
  font-weight: 600;
}

.compare-relationships-section__title small {
  display: block;
  color: var(--p-text-muted-color, #64748b);
  font-weight: 400;
}

.compare-relationships-section__empty {
  margin: 0;
  color: var(--p-text-muted-color, #64748b);
  font-size: 0.875rem;
}
</style>

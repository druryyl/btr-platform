<script setup lang="ts">
import ProfileRelatedEntitiesSection from '@/components/entity-analytics/ProfileRelatedEntitiesSection.vue'
import type { EntityPerformanceProfileResponse, ProfileRelatedEntityRow } from '@/models/entityAnalytics'

const props = defineProps<{
  entityIds: string[]
  profiles: Record<string, EntityPerformanceProfileResponse>
  loading?: boolean
}>()

const emit = defineEmits<{
  navigateEntity: [payload: { entityType: string; entityId: string }]
}>()

function onNavigate(row: ProfileRelatedEntityRow) {
  if (!row.TargetEntityType || !row.EntityId) return
  emit('navigateEntity', {
    entityType: row.TargetEntityType,
    entityId: row.EntityId,
  })
}
</script>

<template>
  <div class="iw-panel-card">
    <h3 class="iw-section-title">Business Drivers</h3>
    <p class="iw-meta">Which related entities explain this situation?</p>
    <div
      v-for="entityId in entityIds"
      :key="entityId"
      class="iw-drivers-block"
    >
      <h4 v-if="entityIds.length > 1" class="iw-meta">
        {{ profiles[entityId]?.Overview?.DisplayName ?? entityId }}
      </h4>
      <ProfileRelatedEntitiesSection
        v-if="profiles[entityId]"
        :section="profiles[entityId].RelatedEntities"
        :loading="loading"
        workspace-mode
        @navigate="onNavigate"
      />
    </div>
  </div>
</template>

<style scoped>
.iw-drivers-block + .iw-drivers-block {
  margin-top: 1.25rem;
  padding-top: 1.25rem;
  border-top: 1px solid #e2e8f0;
}
</style>

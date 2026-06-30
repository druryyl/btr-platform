<script setup lang="ts">
import Tag from 'primevue/tag'
import type { WorkspaceSelectedEntity } from '@/models/entityAnalytics'

defineProps<{
  entities: WorkspaceSelectedEntity[]
  profilesActive: boolean[]
}>()
</script>

<template>
  <div class="iw-identity-grid">
    <div
      v-for="(entity, index) in entities"
      :key="entity.EntityId"
      class="iw-panel-card"
    >
      <h3 class="iw-identity-name">{{ entity.DisplayName }}</h3>
      <p class="iw-meta">{{ entity.EntityCode }}</p>
      <Tag
        :value="profilesActive[index] === false ? 'Inactive' : 'Active'"
        :severity="profilesActive[index] === false ? 'secondary' : 'success'"
      />
    </div>
  </div>
</template>

<style scoped>
.iw-identity-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(14rem, 1fr));
  gap: var(--iw-inner-gap, 0.75rem);
}
</style>

<script setup lang="ts">
import { computed } from 'vue'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import RelatedEntitiesBlocks from '@/components/entity-analytics/RelatedEntitiesBlocks.vue'
import type { ProfileRelatedEntitiesSection, ProfileRelatedEntityRow } from '@/models/entityAnalytics'

const props = defineProps<{
  section: ProfileRelatedEntitiesSection | null | undefined
  loading?: boolean
  workspaceMode?: boolean
}>()

const emit = defineEmits<{
  navigate: [row: ProfileRelatedEntityRow]
}>()

const blocks = computed(() => props.section?.Blocks ?? [])
</script>

<template>
  <ProfileSectionCard
    :title="workspaceMode ? undefined : 'Related Entities'"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <RelatedEntitiesBlocks
      v-if="blocks.length"
      :blocks="blocks"
      :workspace-mode="workspaceMode"
      @navigate="emit('navigate', $event)"
    />
  </ProfileSectionCard>
</template>

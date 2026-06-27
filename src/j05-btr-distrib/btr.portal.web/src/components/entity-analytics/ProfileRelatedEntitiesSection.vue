<script setup lang="ts">
import { computed } from 'vue'
import ProfileSectionCard from '@/components/entity-analytics/ProfileSectionCard.vue'
import RelatedEntitiesBlocks from '@/components/entity-analytics/RelatedEntitiesBlocks.vue'
import type { ProfileRelatedEntitiesSection } from '@/models/entityAnalytics'

const props = defineProps<{
  section: ProfileRelatedEntitiesSection | null | undefined
  loading?: boolean
}>()

const blocks = computed(() => props.section?.Blocks ?? [])
</script>

<template>
  <ProfileSectionCard
    title="Related Entities"
    :is-available="section?.IsAvailable"
    :unavailable-reason="section?.UnavailableReason"
    :loading="loading"
  >
    <RelatedEntitiesBlocks v-if="blocks.length" :blocks="blocks" />
  </ProfileSectionCard>
</template>

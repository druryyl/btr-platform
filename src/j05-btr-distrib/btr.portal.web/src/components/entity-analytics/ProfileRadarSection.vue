<script setup lang="ts">
import PerformanceSignatureSection from '@/components/entity-analytics/PerformanceSignatureSection.vue'
import type { ProfileRadarSection } from '@/models/entityAnalytics'

defineProps<{
  section: ProfileRadarSection | null | undefined
  loading?: boolean
  entityLabel?: string | null
}>()
</script>

<template>
  <PerformanceSignatureSection
    mode="profile"
    :section="section"
    :loading="loading"
    :entity-label="entityLabel"
  />
</template>

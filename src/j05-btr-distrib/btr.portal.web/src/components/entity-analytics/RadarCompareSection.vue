<script setup lang="ts">
import PerformanceSignatureSection from '@/components/entity-analytics/PerformanceSignatureSection.vue'
import type { CompareRadarSection } from '@/models/entityAnalytics'

defineProps<{
  section: CompareRadarSection | null | undefined
}>()
</script>

<template>
  <PerformanceSignatureSection mode="compare" :section="section" />
</template>

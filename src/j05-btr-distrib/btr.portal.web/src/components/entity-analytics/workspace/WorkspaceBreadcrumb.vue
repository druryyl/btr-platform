<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { MapPreset } from '@/models/entityAnalytics'

defineProps<{
  entityType: string
  preset: MapPreset | null
  selectedCount: number
  selectedLabel?: string | null
}>()
</script>

<template>
  <nav class="iw-breadcrumb" aria-label="Investigation breadcrumb">
    <RouterLink to="/analytics" class="iw-breadcrumb__link">Entity Analytics</RouterLink>
    <span class="iw-breadcrumb__sep">/</span>
    <span class="iw-breadcrumb__current">{{ entityType }}</span>
    <template v-if="preset">
      <span class="iw-breadcrumb__sep">/</span>
      <span class="iw-breadcrumb__current">{{ preset.DisplayName }}</span>
    </template>
    <template v-if="selectedCount > 0">
      <span class="iw-breadcrumb__sep">/</span>
      <span class="iw-breadcrumb__current">
        <template v-if="selectedCount === 1 && selectedLabel">{{ selectedLabel }}</template>
        <template v-else>{{ selectedCount }} entities selected</template>
      </span>
    </template>
  </nav>
</template>

<style scoped>
.iw-breadcrumb {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.375rem;
  font-size: 0.8125rem;
  color: var(--iw-text-secondary, #64748b);
}

.iw-breadcrumb__link {
  color: var(--p-primary-color, #3b82f6);
  text-decoration: none;
}

.iw-breadcrumb__link:hover {
  text-decoration: underline;
}

.iw-breadcrumb__sep {
  color: #cbd5e1;
}

.iw-breadcrumb__current {
  color: var(--iw-text-primary, #1e293b);
  font-weight: 500;
}
</style>

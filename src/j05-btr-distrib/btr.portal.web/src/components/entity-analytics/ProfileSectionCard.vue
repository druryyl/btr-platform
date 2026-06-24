<script setup lang="ts">
import Card from 'primevue/card'

defineProps<{
  title: string
  isAvailable?: boolean
  unavailableReason?: string | null
  loading?: boolean
}>()
</script>

<template>
  <Card class="profile-section">
    <template #title>{{ title }}</template>
    <template #content>
      <div v-if="loading" class="profile-section__empty">Loading…</div>
      <div v-else-if="isAvailable === false" class="profile-section__empty">
        <slot name="unavailable">
          {{ unavailableReason === 'NotImplemented'
            ? 'This section will be available in a future release.'
            : 'No data available for this section yet.' }}
        </slot>
      </div>
      <slot v-else />
    </template>
  </Card>
</template>

<style scoped>
.profile-section {
  margin-bottom: 1rem;
}

.profile-section__empty {
  color: var(--p-text-muted-color);
  font-size: 0.9375rem;
  padding: 0.5rem 0;
}
</style>

<script setup lang="ts">
defineProps<{
  label: string | null
  generatedAt?: string | null
}>()
</script>

<template>
  <div class="iw-scope-row">
    <span v-if="label" class="iw-scope-indicator">{{ label }}</span>
    <span v-if="generatedAt" class="iw-meta">Population data as of: {{ generatedAt }}</span>
  </div>
</template>

<style scoped>
.iw-scope-row {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 1rem;
}
</style>

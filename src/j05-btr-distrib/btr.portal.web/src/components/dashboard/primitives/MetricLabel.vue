<script setup lang="ts">
defineProps<{
  label: string
}>()
</script>

<template>
  <span class="metric-label">{{ label }}</span>
</template>

<style scoped>
.metric-label {
  font-size: 0.8125rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
  letter-spacing: 0.01em;
}
</style>

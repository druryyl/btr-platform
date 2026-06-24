<script setup lang="ts">
defineProps<{
  summary: string | null
  loading: boolean
}>()
</script>

<template>
  <section class="inventory-optimization-summary" aria-live="polite">
    <p v-if="loading" class="inventory-optimization-summary__text inventory-optimization-summary__text--muted">
      Loading recommendations…
    </p>
    <pre v-else-if="summary" class="inventory-optimization-summary__text">{{ summary }}</pre>
    <p v-else class="inventory-optimization-summary__text inventory-optimization-summary__text--muted">
      No optimization summary available.
    </p>
  </section>
</template>

<style scoped>
.inventory-optimization-summary {
  margin-bottom: 1rem;
  padding: 1rem 1.25rem;
  border-radius: 0.5rem;
  background: var(--p-surface-50, #f8fafc);
  border: 1px solid var(--p-surface-200);
}

.inventory-optimization-summary__text {
  margin: 0;
  font-size: 0.9375rem;
  line-height: 1.55;
  white-space: pre-wrap;
  font-family: inherit;
}

.inventory-optimization-summary__text--muted {
  color: var(--p-text-muted-color);
}
</style>

<script setup lang="ts">
defineProps<{
  summary: string | null
  loading: boolean
}>()
</script>

<template>
  <section class="collection-optimization-summary" aria-live="polite">
    <p
      v-if="loading"
      class="collection-optimization-summary__text collection-optimization-summary__text--muted"
    >
      Loading executive summary…
    </p>
    <p v-else-if="summary" class="collection-optimization-summary__text">
      {{ summary }}
    </p>
    <p v-else class="collection-optimization-summary__text collection-optimization-summary__text--muted">
      Executive summary unavailable.
    </p>
  </section>
</template>

<style scoped>
.collection-optimization-summary {
  margin-bottom: 1rem;
  padding: 1rem 1.25rem;
  border-radius: 0.75rem;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-200);
}

.collection-optimization-summary__text {
  margin: 0;
  font-size: 0.95rem;
  line-height: 1.55;
  white-space: pre-line;
}

.collection-optimization-summary__text--muted {
  color: var(--p-text-muted-color);
}
</style>

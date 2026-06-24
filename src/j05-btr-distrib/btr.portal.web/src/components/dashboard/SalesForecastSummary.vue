<script setup lang="ts">
defineProps<{
  summary: string | null
  loading: boolean
}>()
</script>

<template>
  <section class="sales-forecast-summary" aria-live="polite">
    <p v-if="loading" class="sales-forecast-summary__text sales-forecast-summary__text--muted">
      Loading executive summary…
    </p>
    <p v-else-if="summary" class="sales-forecast-summary__text">
      {{ summary }}
    </p>
    <p v-else class="sales-forecast-summary__text sales-forecast-summary__text--muted">
      Executive summary unavailable.
    </p>
  </section>
</template>

<style scoped>
.sales-forecast-summary {
  margin-bottom: 1rem;
  padding: 1rem 1.25rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-left: 4px solid var(--p-primary-color);
  border-radius: var(--p-border-radius);
}

.sales-forecast-summary__text {
  margin: 0;
  line-height: 1.6;
  color: var(--p-text-color);
}

.sales-forecast-summary__text--muted {
  color: var(--p-text-muted-color);
}
</style>

<script setup lang="ts">
defineProps<{
  summary: string | null
  loading: boolean
}>()
</script>

<template>
  <section class="customer-risk-forecast-summary" aria-live="polite">
    <p
      v-if="loading"
      class="customer-risk-forecast-summary__text customer-risk-forecast-summary__text--muted"
    >
      Loading executive summary…
    </p>
    <p v-else-if="summary" class="customer-risk-forecast-summary__text">
      {{ summary }}
    </p>
    <p v-else class="customer-risk-forecast-summary__text customer-risk-forecast-summary__text--muted">
      Executive summary unavailable.
    </p>
  </section>
</template>

<style scoped>
.customer-risk-forecast-summary {
  margin-bottom: 1rem;
  padding: 1rem 1.25rem;
  border-radius: 0.75rem;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-200);
}

.customer-risk-forecast-summary__text {
  margin: 0;
  font-size: 1rem;
  line-height: 1.55;
}

.customer-risk-forecast-summary__text--muted {
  color: var(--p-text-muted-color);
}
</style>

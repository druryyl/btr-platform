<script setup lang="ts">
defineProps<{
  summary: string | null
  disclaimer: string | null
  loading: boolean
}>()
</script>

<template>
  <section class="customer-portfolio-summary" aria-live="polite">
    <p
      v-if="loading"
      class="customer-portfolio-summary__text customer-portfolio-summary__text--muted"
    >
      Loading executive summary…
    </p>
    <template v-else>
      <p v-if="summary" class="customer-portfolio-summary__text">
        {{ summary }}
      </p>
      <p v-else class="customer-portfolio-summary__text customer-portfolio-summary__text--muted">
        Executive summary unavailable.
      </p>
      <p v-if="disclaimer" class="customer-portfolio-summary__disclaimer">
        {{ disclaimer }}
      </p>
    </template>
  </section>
</template>

<style scoped>
.customer-portfolio-summary {
  margin-bottom: 1rem;
  padding: 1rem 1.25rem;
  border-radius: 0.75rem;
  background: var(--p-surface-50);
  border: 1px solid var(--p-surface-200);
}

.customer-portfolio-summary__text {
  margin: 0;
  font-size: 0.95rem;
  line-height: 1.55;
  white-space: pre-line;
}

.customer-portfolio-summary__text--muted {
  color: var(--p-text-muted-color);
}

.customer-portfolio-summary__disclaimer {
  margin: 0.75rem 0 0;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  font-style: italic;
}
</style>

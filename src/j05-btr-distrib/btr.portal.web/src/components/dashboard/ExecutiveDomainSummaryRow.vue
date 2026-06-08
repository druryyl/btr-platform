<script setup lang="ts">
import { RouterLink } from 'vue-router'

defineProps<{
  domain: string
  summaryText: string
  route: string
  available: boolean
}>()
</script>

<template>
  <div class="executive-domain-summary-row">
    <div class="executive-domain-summary-row__content">
      <span class="executive-domain-summary-row__domain">{{ domain }}</span>
      <span class="executive-domain-summary-row__text">
        {{ available ? summaryText : `${domain} data unavailable` }}
      </span>
    </div>
    <RouterLink v-if="available" :to="route" class="executive-domain-summary-row__link">
      Details →
    </RouterLink>
  </div>
</template>

<style scoped>
.executive-domain-summary-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 1rem;
  padding: 0.875rem 0;
  border-bottom: 1px solid var(--p-surface-200);
}

.executive-domain-summary-row:last-child {
  border-bottom: none;
}

.executive-domain-summary-row__content {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  min-width: 0;
}

.executive-domain-summary-row__domain {
  font-weight: 700;
  font-size: 0.9rem;
  color: var(--p-text-color);
}

.executive-domain-summary-row__text {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.executive-domain-summary-row__link {
  flex-shrink: 0;
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
  white-space: nowrap;
}

.executive-domain-summary-row__link:hover {
  text-decoration: underline;
}
</style>

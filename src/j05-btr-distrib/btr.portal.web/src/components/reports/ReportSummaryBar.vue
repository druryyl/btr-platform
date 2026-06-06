<script setup lang="ts">
export interface SummaryItem {
  label: string
  value: string
}

defineProps<{
  items: SummaryItem[]
}>()
</script>

<template>
  <div v-if="items.length > 0" class="report-summary-bar">
    <div
      v-for="item in items"
      :key="item.label"
      class="report-summary-bar__item"
    >
      <span class="report-summary-bar__label">{{ item.label }}:</span>
      <span class="report-summary-bar__value">{{ item.value }}</span>
    </div>
  </div>
</template>

<style scoped>
.report-summary-bar {
  display: flex;
  flex-wrap: wrap;
  gap: 1.5rem;
  margin-top: 1rem;
  padding: 0.75rem 1rem;
  background: var(--p-surface-100);
  border-radius: var(--p-border-radius-md);
}

.report-summary-bar__item {
  display: flex;
  align-items: baseline;
  gap: 0.5rem;
}

.report-summary-bar__label {
  color: var(--p-text-muted-color);
}

.report-summary-bar__value {
  font-weight: 600;
}
</style>

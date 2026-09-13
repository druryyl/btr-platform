<script setup lang="ts">
import { computed } from 'vue'
import type { PrincipalDataCompleteness } from '@/services/principalDataCompleteness'
import { formatWithNoData } from '@/services/principalNoData'
import { formatPercent } from '@/services/formatters'

const props = defineProps<{
  completeness: PrincipalDataCompleteness | null
}>()

function completenessDisplay(value: number | null | undefined): string {
  return formatWithNoData(value, (v) => formatPercent(v))
}

const items = computed(() => {
  const summary = props.completeness
  if (!summary) return []
  return [
    { label: 'Coverage', value: completenessDisplay(summary.coverage.completenessPercentage) },
    { label: 'Target', value: completenessDisplay(summary.target.completenessPercentage) },
    { label: 'Return', value: completenessDisplay(summary.return.completenessPercentage) },
    {
      label: 'Contribution',
      value: completenessDisplay(summary.contribution.completenessPercentage),
    },
  ]
})
</script>

<template>
  <section
    class="principal-completeness"
    aria-label="Data completeness"
    data-testid="data-completeness"
  >
    <h2 class="principal-completeness__title">Data Completeness</h2>
    <ul v-if="items.length" class="principal-completeness__list">
      <li v-for="item in items" :key="item.label" class="principal-completeness__item">
        <span class="principal-completeness__label">{{ item.label }}</span>
        <span class="principal-completeness__value">{{ item.value }}</span>
      </li>
    </ul>
    <p v-else class="principal-completeness__empty">No Data</p>
  </section>
</template>

<style scoped>
.principal-completeness {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.principal-completeness__title {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.principal-completeness__list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
}

.principal-completeness__item {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.principal-completeness__label {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.principal-completeness__value {
  font-weight: 600;
}

.principal-completeness__empty {
  margin: 0;
  color: var(--p-text-muted-color);
}

@media (max-width: 900px) {
  .principal-completeness__list {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>

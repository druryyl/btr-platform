<script setup lang="ts">
export interface SalesForecastKpiMetric {
  label: string
  value: string
  hint?: string
  severity?: 'normal' | 'warning' | 'critical' | 'success' | 'muted'
}

defineProps<{
  metrics: SalesForecastKpiMetric[]
}>()

function valueClass(severity?: SalesForecastKpiMetric['severity']): string {
  if (!severity || severity === 'normal') return 'sales-forecast-kpi-row__value'
  return `sales-forecast-kpi-row__value sales-forecast-kpi-row__value--${severity}`
}
</script>

<template>
  <div class="sales-forecast-kpi-row">
    <div v-for="metric in metrics" :key="metric.label" class="metric">
      <span class="metric__label">{{ metric.label }}</span>
      <span :class="valueClass(metric.severity)">{{ metric.value }}</span>
      <span v-if="metric.hint" class="metric__hint">{{ metric.hint }}</span>
    </div>
  </div>
</template>

<style scoped>
.sales-forecast-kpi-row {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius);
}

.metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.metric__label {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.sales-forecast-kpi-row__value {
  font-size: 1.125rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.sales-forecast-kpi-row__value--success {
  color: #16a34a;
}

.sales-forecast-kpi-row__value--warning {
  color: #d97706;
}

.sales-forecast-kpi-row__value--critical {
  color: #dc2626;
}

.sales-forecast-kpi-row__value--muted {
  color: var(--p-text-muted-color);
}

.metric__hint {
  font-size: 0.75rem;
  color: var(--p-text-muted-color);
}

@media (max-width: 1100px) {
  .sales-forecast-kpi-row {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 640px) {
  .sales-forecast-kpi-row {
    grid-template-columns: 1fr;
  }
}
</style>

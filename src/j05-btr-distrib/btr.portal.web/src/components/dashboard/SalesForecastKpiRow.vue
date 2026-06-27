<script setup lang="ts">
import KpiChip from '@/components/dashboard/primitives/KpiChip.vue'

export interface SalesForecastKpiMetric {
  label: string
  value: string
  hint?: string
  severity?: 'normal' | 'warning' | 'critical' | 'success' | 'muted'
}

defineProps<{
  metrics: SalesForecastKpiMetric[]
}>()

interface ChipSpec {
  status: 'healthy' | 'warning' | 'critical'
  label: string
}

function chipSpec(severity?: SalesForecastKpiMetric['severity']): ChipSpec | null {
  if (severity === 'success') return { status: 'healthy', label: 'Good' }
  if (severity === 'warning') return { status: 'warning', label: 'Watch' }
  if (severity === 'critical') return { status: 'critical', label: 'Risk' }
  return null
}
</script>

<template>
  <div class="sales-forecast-kpi-row">
    <div v-for="metric in metrics" :key="metric.label" class="sfkpi-metric">
      <span class="sfkpi-metric__label">{{ metric.label }}</span>
      <div class="sfkpi-metric__value-row">
        <span
          class="sfkpi-metric__value"
          :class="`sfkpi-metric__value--${metric.severity ?? 'normal'}`"
        >{{ metric.value }}</span>
        <KpiChip
          v-if="chipSpec(metric.severity)"
          :label="chipSpec(metric.severity)!.label"
          :status="chipSpec(metric.severity)!.status"
          class="sfkpi-metric__chip"
        />
      </div>
      <span v-if="metric.hint" class="sfkpi-metric__hint">{{ metric.hint }}</span>
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
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
  transition: box-shadow var(--dashboard-transition);
}

.sales-forecast-kpi-row:hover {
  box-shadow: var(--dashboard-shadow-hover);
}

.sfkpi-metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.sfkpi-metric__label {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.sfkpi-metric__value-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.sfkpi-metric__value {
  font-size: 1.125rem;
  font-weight: 700;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
}

.sfkpi-metric__value--success {
  color: var(--kpi-status-healthy-color);
}

.sfkpi-metric__value--warning {
  color: var(--kpi-status-warning-color);
}

.sfkpi-metric__value--critical {
  color: var(--kpi-status-critical-color);
}

.sfkpi-metric__value--muted {
  color: var(--p-text-muted-color);
}

.sfkpi-metric__chip {
  font-size: 0.6875rem;
  padding: 0.1rem 0.4rem;
}

.sfkpi-metric__hint {
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

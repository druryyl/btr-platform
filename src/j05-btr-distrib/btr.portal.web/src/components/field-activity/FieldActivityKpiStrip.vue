<script setup lang="ts">
import KpiCard from '@/components/KpiCard.vue'
import type { FieldActivityKpis } from '@/models/fieldActivity'
import { formatNumber, formatPercent } from '@/services/formatters'

defineProps<{
  kpis: FieldActivityKpis | null
  loading?: boolean
}>()
</script>

<template>
  <div class="field-activity-kpi-strip">
    <KpiCard title="Planned" icon="pi pi-calendar" domain="salesman" :loading="loading">
      <div class="field-activity-kpi-strip__value">{{ formatNumber(kpis?.PlannedVisits ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Actual" icon="pi pi-check-circle" domain="salesman" :loading="loading">
      <div class="field-activity-kpi-strip__value">{{ formatNumber(kpis?.ActualVisits ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Effective" icon="pi pi-shopping-cart" domain="salesman" :loading="loading">
      <div class="field-activity-kpi-strip__value">{{ formatNumber(kpis?.EffectiveCalls ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Missed" icon="pi pi-times-circle" domain="salesman" :loading="loading">
      <div class="field-activity-kpi-strip__value">{{ formatNumber(kpis?.MissedVisits ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Unplanned" icon="pi pi-map-marker" domain="salesman" :loading="loading">
      <div class="field-activity-kpi-strip__value">{{ formatNumber(kpis?.UnplannedVisits ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Execution %" icon="pi pi-percentage" domain="salesman" :loading="loading">
      <div class="field-activity-kpi-strip__value">
        {{ kpis ? formatPercent(kpis.VisitExecutionPercent) : '—' }}
      </div>
    </KpiCard>
    <KpiCard title="Effective Call Rate" icon="pi pi-chart-line" domain="salesman" :loading="loading">
      <div class="field-activity-kpi-strip__value">
        {{ kpis ? formatPercent(kpis.EffectiveCallRate) : '—' }}
      </div>
    </KpiCard>
  </div>
</template>

<style scoped>
.field-activity-kpi-strip {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(7.5rem, 1fr));
  gap: 0.75rem;
}

.field-activity-kpi-strip__value {
  font-size: 1.5rem;
  font-weight: 700;
  line-height: 1.2;
  font-variant-numeric: tabular-nums;
}
</style>

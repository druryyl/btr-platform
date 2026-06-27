<script setup lang="ts">
import KpiCard from '@/components/KpiCard.vue'
import type { FieldActivityTeamKpis } from '@/models/fieldActivity'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

defineProps<{
  kpis: FieldActivityTeamKpis | null
  loading?: boolean
}>()
</script>

<template>
  <div class="field-activity-team-kpi-strip">
    <KpiCard title="Active Salesmen" icon="pi pi-users" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">
        {{ formatNumber(kpis?.ActiveSalesmenCount ?? 0) }}
      </div>
    </KpiCard>
    <KpiCard title="Planned" icon="pi pi-calendar" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">{{ formatNumber(kpis?.PlannedVisits ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Actual" icon="pi pi-check-circle" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">{{ formatNumber(kpis?.ActualVisits ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Execution %" icon="pi pi-percentage" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">
        {{ kpis ? formatPercent(kpis.VisitExecutionPercent) : '—' }}
      </div>
    </KpiCard>
    <KpiCard title="Effective Calls" icon="pi pi-shopping-cart" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">{{ formatNumber(kpis?.EffectiveCalls ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Effective Call Rate" icon="pi pi-chart-line" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">
        {{ kpis ? formatPercent(kpis.EffectiveCallRate) : '—' }}
      </div>
    </KpiCard>
    <KpiCard title="Missed" icon="pi pi-times-circle" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">{{ formatNumber(kpis?.MissedVisits ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Unplanned" icon="pi pi-map-marker" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">{{ formatNumber(kpis?.UnplannedVisits ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="GPS Valid Rate" icon="pi pi-compass" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">
        {{ kpis ? formatPercent(kpis.GpsValidRate) : '—' }}
      </div>
    </KpiCard>
    <KpiCard title="Orders" icon="pi pi-receipt" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">{{ formatNumber(kpis?.TotalOrders ?? 0) }}</div>
    </KpiCard>
    <KpiCard title="Order Value" icon="pi pi-wallet" :loading="loading">
      <div class="field-activity-team-kpi-strip__value">
        {{ formatCurrency(kpis?.TotalOmzet ?? 0) }}
      </div>
    </KpiCard>
  </div>
</template>

<style scoped>
.field-activity-team-kpi-strip {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(7.5rem, 1fr));
  gap: 0.75rem;
}

.field-activity-team-kpi-strip__value {
  font-size: 1.35rem;
  font-weight: 700;
  line-height: 1.2;
}
</style>

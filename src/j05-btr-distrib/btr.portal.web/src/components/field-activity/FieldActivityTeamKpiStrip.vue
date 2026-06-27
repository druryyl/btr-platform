<script setup lang="ts">
import { computed } from 'vue'
import DashboardMetricCard, {
  type KpiChipStatus,
} from '@/components/dashboard/primitives/DashboardMetricCard.vue'
import type { FieldActivityTeamKpis } from '@/models/fieldActivity'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  kpis: FieldActivityTeamKpis | null
  loading?: boolean
}>()

type ChipResult = { label: string; status: KpiChipStatus }

function executionChip(val: number | null | undefined): ChipResult {
  if (val == null) return { label: 'No Data', status: 'unknown' }
  if (val >= 100) return { label: 'On Target', status: 'healthy' }
  if (val >= 80) return { label: 'Good', status: 'stable' }
  if (val >= 60) return { label: 'Review', status: 'warning' }
  return { label: 'Low', status: 'critical' }
}

function gpsChip(val: number | null | undefined): ChipResult {
  if (val == null) return { label: 'No Data', status: 'unknown' }
  if (val >= 95) return { label: 'On Target', status: 'healthy' }
  if (val >= 80) return { label: 'Good', status: 'stable' }
  return { label: 'Review', status: 'warning' }
}

function effectiveRateChip(val: number | null | undefined): ChipResult {
  if (val == null) return { label: 'No Data', status: 'unknown' }
  if (val >= 70) return { label: 'On Target', status: 'healthy' }
  if (val >= 50) return { label: 'Good', status: 'stable' }
  if (val >= 30) return { label: 'Review', status: 'warning' }
  return { label: 'Low', status: 'critical' }
}

const execChip = computed(() => executionChip(props.kpis?.VisitExecutionPercent))
const gpsRateChip = computed(() => gpsChip(props.kpis?.GpsValidRate))
const effRateChip = computed(() => effectiveRateChip(props.kpis?.EffectiveCallRate))
</script>

<template>
  <div class="fa-team-kpi-strip">
    <!-- Execution group: Active Salesmen / Planned / Actual / Execution % -->
    <DashboardMetricCard
      title="Active Salesmen"
      :value="formatNumber(kpis?.ActiveSalesmenCount ?? 0)"
      icon="pi pi-users"
      kpi-group="execution"
      subtitle="Field active today"
      :loading="loading"
    />
    <DashboardMetricCard
      title="Planned"
      :value="formatNumber(kpis?.PlannedVisits ?? 0)"
      icon="pi pi-calendar"
      kpi-group="execution"
      subtitle="Scheduled visits"
      :loading="loading"
    />
    <DashboardMetricCard
      title="Actual"
      :value="formatNumber(kpis?.ActualVisits ?? 0)"
      icon="pi pi-check-circle"
      kpi-group="execution"
      subtitle="Executed visits"
      size="large"
      :loading="loading"
    />
    <DashboardMetricCard
      title="Execution %"
      :value="kpis ? formatPercent(kpis.VisitExecutionPercent) : '—'"
      icon="pi pi-percentage"
      kpi-group="execution"
      subtitle="vs plan"
      :progress="kpis?.VisitExecutionPercent"
      :chip-label="execChip.label"
      :chip-status="execChip.status"
      :loading="loading"
    />

    <!-- Productivity group: Effective Calls / Effective Call Rate / Orders / Order Value -->
    <DashboardMetricCard
      title="Effective Calls"
      :value="formatNumber(kpis?.EffectiveCalls ?? 0)"
      icon="pi pi-shopping-cart"
      kpi-group="productivity"
      subtitle="Customer contacted"
      :loading="loading"
    />
    <DashboardMetricCard
      title="Effective Call Rate"
      :value="kpis ? formatPercent(kpis.EffectiveCallRate) : '—'"
      icon="pi pi-chart-line"
      kpi-group="productivity"
      subtitle="of actual visits"
      :progress="kpis?.EffectiveCallRate"
      :chip-label="effRateChip.label"
      :chip-status="effRateChip.status"
      :loading="loading"
    />
    <DashboardMetricCard
      title="Orders"
      :value="formatNumber(kpis?.TotalOrders ?? 0)"
      icon="pi pi-receipt"
      kpi-group="productivity"
      subtitle="Sales orders"
      size="large"
      :loading="loading"
    />
    <DashboardMetricCard
      title="Order Value"
      :value="formatCurrency(kpis?.TotalOmzet ?? 0)"
      icon="pi pi-wallet"
      kpi-group="productivity"
      subtitle="Revenue today"
      size="large"
      :loading="loading"
    />

    <!-- Quality group: GPS Valid Rate -->
    <DashboardMetricCard
      title="GPS Valid Rate"
      :value="kpis ? formatPercent(kpis.GpsValidRate) : '—'"
      icon="pi pi-compass"
      kpi-group="quality"
      subtitle="Target >95%"
      :progress="kpis?.GpsValidRate"
      :chip-label="gpsRateChip.label"
      :chip-status="gpsRateChip.status"
      :loading="loading"
    />

    <!-- Issues group: Missed / Unplanned -->
    <DashboardMetricCard
      title="Missed"
      :value="formatNumber(kpis?.MissedVisits ?? 0)"
      icon="pi pi-times-circle"
      kpi-group="issues"
      subtitle="Planned but skipped"
      :loading="loading"
    />
    <DashboardMetricCard
      title="Unplanned"
      :value="formatNumber(kpis?.UnplannedVisits ?? 0)"
      icon="pi pi-map-marker"
      kpi-group="issues"
      subtitle="Off-schedule"
      :loading="loading"
    />
  </div>
</template>

<style scoped>
.fa-team-kpi-strip {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(10rem, 1fr));
  gap: 0.75rem;
}
</style>

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
  <div class="fa-kpi-strip">

    <!-- ── FIELD EXECUTION ──────────────────────────── -->
    <section class="fa-kpi-section">
      <div class="fa-kpi-section__label-row">
        <span class="fa-kpi-section__label">Field Execution</span>
        <div class="fa-kpi-section__rule" />
      </div>
      <div class="fa-kpi-section__grid fa-kpi-section__grid--4">
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
          kpi-group="activity"
          subtitle="Scheduled visits"
          :loading="loading"
        />
        <DashboardMetricCard
          title="Actual"
          :value="formatNumber(kpis?.ActualVisits ?? 0)"
          icon="pi pi-check-circle"
          kpi-group="activity"
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
      </div>
    </section>

    <!-- ── PRODUCTIVITY ─────────────────────────────── -->
    <section class="fa-kpi-section">
      <div class="fa-kpi-section__label-row">
        <span class="fa-kpi-section__label">Productivity</span>
        <div class="fa-kpi-section__rule" />
      </div>
      <div class="fa-kpi-section__grid fa-kpi-section__grid--4">
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
          kpi-group="execution"
          subtitle="Sales orders"
          size="large"
          :loading="loading"
        />
        <DashboardMetricCard
          title="Order Value"
          :value="formatCurrency(kpis?.TotalOmzet ?? 0)"
          icon="pi pi-wallet"
          kpi-group="execution"
          subtitle="Revenue today"
          size="large"
          :loading="loading"
        />
      </div>
    </section>

    <!-- ── QUALITY ──────────────────────────────────── -->
    <section class="fa-kpi-section">
      <div class="fa-kpi-section__label-row">
        <span class="fa-kpi-section__label">Quality</span>
        <div class="fa-kpi-section__rule" />
      </div>
      <div class="fa-kpi-section__grid fa-kpi-section__grid--3">
        <DashboardMetricCard
          title="GPS Valid Rate"
          :value="kpis ? formatPercent(kpis.GpsValidRate) : '—'"
          icon="pi pi-compass"
          kpi-group="monitoring"
          subtitle="Target >95%"
          :progress="kpis?.GpsValidRate"
          :chip-label="gpsRateChip.label"
          :chip-status="gpsRateChip.status"
          :loading="loading"
        />
        <DashboardMetricCard
          title="Missed Visit"
          :value="formatNumber(kpis?.MissedVisits ?? 0)"
          icon="pi pi-times-circle"
          kpi-group="risk"
          subtitle="Planned but skipped"
          :loading="loading"
        />
        <DashboardMetricCard
          title="Unplanned Visit"
          :value="formatNumber(kpis?.UnplannedVisits ?? 0)"
          icon="pi pi-map-marker"
          kpi-group="caution"
          subtitle="Off-schedule"
          :loading="loading"
        />
      </div>
    </section>

  </div>
</template>

<style scoped>
.fa-kpi-strip {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

/* Section wrapper */
.fa-kpi-section {
  display: flex;
  flex-direction: column;
  gap: 0.625rem;
}

/* "LABEL ─────────" header row */
.fa-kpi-section__label-row {
  display: flex;
  align-items: center;
  gap: 0.75rem;
}

.fa-kpi-section__label {
  font-size: 0.625rem;
  font-weight: 700;
  letter-spacing: 0.1em;
  text-transform: uppercase;
  color: var(--p-text-muted-color, #94a3b8);
  white-space: nowrap;
}

.fa-kpi-section__rule {
  flex: 1;
  height: 1px;
  background: var(--p-surface-200, #e2e8f0);
}

/* Card grids — fixed columns so Order Value has room */
.fa-kpi-section__grid {
  display: grid;
  gap: 0.75rem;
}

.fa-kpi-section__grid--4 {
  grid-template-columns: repeat(4, 1fr);
}

.fa-kpi-section__grid--3 {
  grid-template-columns: repeat(3, 1fr);
}

/* Responsive — collapse to 2 columns on narrow viewports */
@media (max-width: 768px) {
  .fa-kpi-section__grid--4,
  .fa-kpi-section__grid--3 {
    grid-template-columns: repeat(2, 1fr);
  }
}
</style>

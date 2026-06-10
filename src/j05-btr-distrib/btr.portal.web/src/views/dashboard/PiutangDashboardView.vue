<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import AgingPieChart from '@/components/dashboard/AgingPieChart.vue'
import PiutangCustomerRiskTable from '@/components/dashboard/PiutangCustomerRiskTable.vue'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import type { DashboardPiutangTopCustomerRiskRow } from '@/models/dashboard'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()
const sourceLabel = resolveInvestigationSourceLabel('/dashboard/piutang')

const riskRows = computed(
  () => (dashboard.piutang?.TopCustomerRisk ?? []) as Record<string, unknown>[],
)

function onRiskRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardPiutangTopCustomerRiskRow
  if (!item.Investigation) return
  navigateToInvestigation(router, item.Investigation, sourceLabel)
}

onMounted(() => {
  void dashboard.loadPiutang()
})
</script>

<template>
  <DashboardDetailLayout
    title="Piutang Dashboard"
    subtitle="Portfolio quality snapshot — all open receivables. Collection recovery metrics are on the Collection Dashboard."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.piutang?.GeneratedAt"
    @refresh="dashboard.loadPiutang()"
  >
    <div class="piutang-dashboard__kpi-row">
      <div class="metric">
        <span class="metric__label">Total Piutang</span>
        <span class="metric__value">
          {{ dashboard.piutang ? formatCurrency(dashboard.piutang.TotalPiutang) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Total Customer</span>
        <span class="metric__value">
          {{ dashboard.piutang ? formatNumber(dashboard.piutang.TotalCustomer) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Overdue Customer</span>
        <span class="metric__value">
          {{ dashboard.piutang ? formatNumber(dashboard.piutang.OverdueCustomer) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Overdue Piutang</span>
        <span class="metric__value">
          {{ dashboard.piutang ? formatCurrency(dashboard.piutang.OverduePiutang) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Piutang &gt; 90 Hari</span>
        <span class="metric__value">
          {{ dashboard.piutang ? formatCurrency(dashboard.piutang.AgingOver90Amount) : '—' }}
        </span>
        <span
          v-if="dashboard.piutang?.AgingOver90Percent != null"
          class="metric__subtitle"
        >
          {{ formatPercent(dashboard.piutang.AgingOver90Percent) }} of total
        </span>
      </div>
    </div>

    <div class="piutang-dashboard__concentration-row">
      <p class="piutang-dashboard__concentration-hint">
        Share of total open piutang held by largest customers.
      </p>
      <div class="piutang-dashboard__concentration-metrics">
        <div class="metric">
          <span class="metric__label">Top 10 Customer %</span>
          <span class="metric__value">
            {{
              dashboard.piutang?.Top10CustomerConcentrationPercent != null
                ? formatPercent(dashboard.piutang.Top10CustomerConcentrationPercent)
                : '—'
            }}
          </span>
        </div>
        <div class="metric">
          <span class="metric__label">Top 20 Customer %</span>
          <span class="metric__value">
            {{
              dashboard.piutang?.Top20CustomerConcentrationPercent != null
                ? formatPercent(dashboard.piutang.Top20CustomerConcentrationPercent)
                : '—'
            }}
          </span>
        </div>
      </div>
    </div>

    <AgingPieChart
      class="piutang-dashboard__section"
      :buckets="dashboard.piutang?.AgingBuckets ?? []"
      :loading="dashboard.loading"
    />

    <PiutangCustomerRiskTable
      class="piutang-dashboard__section"
      title="Top 20 Outstanding Customers — Aging Breakdown"
      :rows="riskRows"
      :loading="dashboard.loading"
      clickable
      empty-message="No outstanding customer data."
      @row-click="onRiskRowClick"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.piutang-dashboard__kpi-row {
  display: grid;
  grid-template-columns: repeat(5, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius);
}

.piutang-dashboard__concentration-row {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius);
}

.piutang-dashboard__concentration-hint {
  margin: 0 0 0.75rem;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.piutang-dashboard__concentration-metrics {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
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

.metric__value {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.metric__subtitle {
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.piutang-dashboard__section {
  margin-top: 1rem;
}

@media (max-width: 900px) {
  .piutang-dashboard__kpi-row {
    grid-template-columns: 1fr;
  }

  .piutang-dashboard__concentration-metrics {
    grid-template-columns: 1fr;
  }
}
</style>

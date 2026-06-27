<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
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
    <div class="piutang-dashboard__kpi-row" data-domain="finance">
      <DashboardMetric
        label="Total Piutang"
        :value="dashboard.piutang ? formatCurrency(dashboard.piutang.TotalPiutang) : '—'"
        :empty="!dashboard.piutang"
      />
      <DashboardMetric
        label="Total Customer"
        :value="dashboard.piutang ? formatNumber(dashboard.piutang.TotalCustomer) : '—'"
        :empty="!dashboard.piutang"
      />
      <DashboardMetric
        label="Overdue Customer"
        :value="dashboard.piutang ? formatNumber(dashboard.piutang.OverdueCustomer) : '—'"
        :empty="!dashboard.piutang"
      />
      <DashboardMetric
        label="Overdue Piutang"
        :value="dashboard.piutang ? formatCurrency(dashboard.piutang.OverduePiutang) : '—'"
        :empty="!dashboard.piutang"
      />
      <div class="piutang-kpi-aging">
        <DashboardMetric
          label="Piutang > 90 Hari"
          :value="dashboard.piutang ? formatCurrency(dashboard.piutang.AgingOver90Amount) : '—'"
          :empty="!dashboard.piutang"
        />
        <span
          v-if="dashboard.piutang?.AgingOver90Percent != null"
          class="piutang-kpi-aging__subtitle"
        >
          {{ formatPercent(dashboard.piutang.AgingOver90Percent) }} of total
        </span>
      </div>
    </div>

    <div class="piutang-dashboard__concentration-row" data-domain="finance">
      <p class="piutang-dashboard__concentration-hint">
        Share of total open piutang held by largest customers.
      </p>
      <div class="piutang-dashboard__concentration-metrics">
        <DashboardMetric
          label="Top 10 Customer %"
          :value="dashboard.piutang?.Top10CustomerConcentrationPercent != null
            ? formatPercent(dashboard.piutang.Top10CustomerConcentrationPercent) : '—'"
          :empty="dashboard.piutang?.Top10CustomerConcentrationPercent == null"
        />
        <DashboardMetric
          label="Top 20 Customer %"
          :value="dashboard.piutang?.Top20CustomerConcentrationPercent != null
            ? formatPercent(dashboard.piutang.Top20CustomerConcentrationPercent) : '—'"
          :empty="dashboard.piutang?.Top20CustomerConcentrationPercent == null"
        />
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
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
  transition: box-shadow var(--dashboard-transition);
}

.piutang-kpi-aging {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.piutang-kpi-aging__subtitle {
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.piutang-dashboard__concentration-row {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
  transition: box-shadow var(--dashboard-transition);
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

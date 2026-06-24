<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import InventoryForecastKpiGrid from '@/components/dashboard/InventoryForecastKpiGrid.vue'
import InventoryOptimizationActionHeat from '@/components/dashboard/InventoryOptimizationActionHeat.vue'
import InventoryOptimizationActionsTable from '@/components/dashboard/InventoryOptimizationActionsTable.vue'
import InventoryOptimizationClearanceTable from '@/components/dashboard/InventoryOptimizationClearanceTable.vue'
import InventoryOptimizationDelayTable from '@/components/dashboard/InventoryOptimizationDelayTable.vue'
import InventoryOptimizationImpactChart from '@/components/dashboard/InventoryOptimizationImpactChart.vue'
import InventoryOptimizationPriorityChart from '@/components/dashboard/InventoryOptimizationPriorityChart.vue'
import InventoryOptimizationReorderTable from '@/components/dashboard/InventoryOptimizationReorderTable.vue'
import InventoryOptimizationSummary from '@/components/dashboard/InventoryOptimizationSummary.vue'
import InventoryOptimizationTransferTable from '@/components/dashboard/InventoryOptimizationTransferTable.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'
import { formatCurrency } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const optimization = computed(() => dashboard.inventoryOptimization)

function healthSeverity(score: number): SalesForecastKpiMetric['severity'] {
  if (score >= 75) return 'success'
  if (score >= 50) return 'warning'
  return 'critical'
}

function categorySeverity(count: number): SalesForecastKpiMetric['severity'] {
  if (count > 0) return 'critical'
  return 'success'
}

const healthBudgetMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = optimization.value
  if (!data) return []

  return [
    {
      label: 'Inventory Health Score',
      value: String(data.InventoryHealthScore),
      severity: healthSeverity(data.InventoryHealthScore),
    },
    {
      label: 'Critical Actions',
      value: String(data.CriticalActionCount),
      severity: categorySeverity(data.CriticalActionCount),
    },
    {
      label: 'Recommended Purchase Budget',
      value: formatCurrency(data.RecommendedPurchaseBudgetIdr),
    },
    {
      label: 'Deferrable Spend',
      value: formatCurrency(data.DeferrableSpendIdr),
    },
  ]
})

const actionMixMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = optimization.value
  if (!data) return []

  return [
    { label: 'Purchase Now', value: String(data.PurchaseNowCount) },
    { label: 'Delay', value: String(data.DelayCount) },
    { label: 'Transfer', value: String(data.TransferCount) },
    { label: 'Clearance Review', value: String(data.ClearanceCount) },
  ]
})

onMounted(() => {
  void dashboard.loadInventoryOptimization()
})
</script>

<template>
  <DashboardDetailLayout
    title="Inventory Optimization Dashboard"
    subtitle="Action recommendations from forecast, risk, and purchasing context."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="optimization?.GeneratedAt ?? null"
    @refresh="dashboard.loadInventoryOptimization()"
  >
    <p v-if="optimization?.IsAvailable" class="inventory-optimization-dashboard__meta">
      Planning horizon: {{ optimization.PlanningHorizonDays }} days · Budget cap:
      {{ optimization.BudgetCapIdr != null ? formatCurrency(optimization.BudgetCapIdr) : 'Not configured' }}
    </p>

    <p v-else-if="!dashboard.loading" class="inventory-optimization-dashboard__banner">
      Inventory optimization data is not yet available. Run the snapshot refresh worker for the InventoryRisk domain.
    </p>

    <InventoryOptimizationSummary
      :summary="optimization?.ExecutiveSummary ?? null"
      :loading="dashboard.loading"
    />

    <template v-if="optimization?.IsAvailable !== false">
      <InventoryForecastKpiGrid :metrics="healthBudgetMetrics" />
      <InventoryForecastKpiGrid class="inventory-optimization-dashboard__section" :metrics="actionMixMetrics" />

      <InventoryOptimizationPriorityChart
        class="inventory-optimization-dashboard__section"
        :distribution="optimization?.PriorityDistribution ?? []"
        :loading="dashboard.loading"
      />

      <div class="inventory-optimization-dashboard__charts-row">
        <InventoryOptimizationImpactChart
          class="inventory-optimization-dashboard__chart-half"
          :purchase-impact-idr="optimization?.PurchaseImpactIdr ?? 0"
          :deferrable-spend-idr="optimization?.DeferrableSpendIdr ?? 0"
          :recoverable-capital-idr="optimization?.RecoverableCapitalIdr ?? 0"
          :loading="dashboard.loading"
        />
        <InventoryOptimizationActionHeat
          class="inventory-optimization-dashboard__chart-half"
          :heat-summary="optimization?.ActionHeatSummary ?? []"
          :loading="dashboard.loading"
        />
      </div>

      <InventoryOptimizationActionsTable
        class="inventory-optimization-dashboard__section"
        :actions="optimization?.TopActions ?? []"
        :loading="dashboard.loading"
      />
      <InventoryOptimizationReorderTable
        class="inventory-optimization-dashboard__section"
        :reorder-list="optimization?.ReorderList ?? []"
        :loading="dashboard.loading"
      />
      <InventoryOptimizationTransferTable
        class="inventory-optimization-dashboard__section"
        :transfer-list="optimization?.TransferList ?? []"
        :loading="dashboard.loading"
      />
      <InventoryOptimizationDelayTable
        class="inventory-optimization-dashboard__section"
        :delay-list="optimization?.DelayList ?? []"
        :loading="dashboard.loading"
      />
      <InventoryOptimizationClearanceTable
        class="inventory-optimization-dashboard__section"
        :clearance-list="optimization?.ClearanceList ?? []"
        :loading="dashboard.loading"
      />
    </template>

    <footer class="inventory-optimization-dashboard__footer">
      <p>
        Recommendations derive from Inventory Forecast, Inventory Risk, and Purchasing Management.
        <RouterLink :to="optimization?.Traceability?.InventoryForecastRoute ?? '/dashboard/inventory-forecast'">
          Inventory Forecast
        </RouterLink>
        ·
        <RouterLink :to="optimization?.Traceability?.InventoryRiskRoute ?? '/dashboard/inventory-risk'">
          Inventory Risk
        </RouterLink>
        ·
        <RouterLink
          :to="optimization?.Traceability?.PurchasingManagementRoute ?? '/dashboard/purchasing-management'"
        >
          Purchasing Management
        </RouterLink>
        ·
        <RouterLink :to="optimization?.Traceability?.InventoryReportRoute ?? '/reports/inventory'">
          Inventory Report
        </RouterLink>
        ·
        <RouterLink :to="optimization?.Traceability?.PurchasingReportRoute ?? '/reports/purchasing'">
          Purchasing Report
        </RouterLink>
      </p>
      <p v-if="optimization?.Traceability?.Disclaimer" class="inventory-optimization-dashboard__disclaimer">
        {{ optimization.Traceability.Disclaimer }}
      </p>
    </footer>
  </DashboardDetailLayout>
</template>

<style scoped>
.inventory-optimization-dashboard__meta {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.9375rem;
}

.inventory-optimization-dashboard__banner {
  margin: 0 0 1rem;
  padding: 0.75rem 1rem;
  border-radius: 0.5rem;
  background: var(--p-yellow-50, #fefce8);
  border: 1px solid var(--p-yellow-200, #fde68a);
}

.inventory-optimization-dashboard__section {
  margin-top: 1rem;
}

.inventory-optimization-dashboard__charts-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1rem;
}

.inventory-optimization-dashboard__chart-half {
  min-width: 0;
}

.inventory-optimization-dashboard__footer {
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--p-surface-200);
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.inventory-optimization-dashboard__footer a {
  margin-left: 0.35rem;
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.inventory-optimization-dashboard__disclaimer {
  margin-top: 0.75rem !important;
  font-style: italic;
}

@media (max-width: 900px) {
  .inventory-optimization-dashboard__charts-row {
    grid-template-columns: 1fr;
  }
}
</style>

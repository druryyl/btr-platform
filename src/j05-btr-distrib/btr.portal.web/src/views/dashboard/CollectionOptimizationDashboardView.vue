<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import InventoryForecastKpiGrid from '@/components/dashboard/InventoryForecastKpiGrid.vue'
import CollectionOptimizationSummary from '@/components/dashboard/collection-optimization/CollectionOptimizationSummary.vue'
import CollectionOptimizationActionChart from '@/components/dashboard/collection-optimization/CollectionOptimizationActionChart.vue'
import CollectionOptimizationWorkloadChart from '@/components/dashboard/collection-optimization/CollectionOptimizationWorkloadChart.vue'
import CollectionOptimizationImpactChart from '@/components/dashboard/collection-optimization/CollectionOptimizationImpactChart.vue'
import CollectionOptimizationPriorityTable from '@/components/dashboard/collection-optimization/CollectionOptimizationPriorityTable.vue'
import CollectionOptimizationQueueTabs from '@/components/dashboard/collection-optimization/CollectionOptimizationQueueTabs.vue'
import CollectionOptimizationImpactTable from '@/components/dashboard/collection-optimization/CollectionOptimizationImpactTable.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'
import { formatCurrency, formatPercent } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const optimization = computed(() => dashboard.collectionOptimization)
const kpi = computed(() => optimization.value?.Kpi ?? null)

const workloadMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    { label: 'Actions Today', value: String(data.ActionsTodayCount) },
    {
      label: 'Immediate Collection',
      value: String(data.ImmediateCollectionCount),
      severity: data.ImmediateCollectionCount > 0 ? 'critical' : 'success',
    },
    { label: 'Proactive Reminders', value: String(data.ProactiveReminderCount) },
    { label: 'Credit Review', value: String(data.CreditReviewCount) },
    { label: 'Sales Recovery', value: String(data.SalesRecoveryCount) },
    { label: 'Collection Impact', value: formatCurrency(data.CollectionImpactTotal) },
  ]
})

const contextMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    { label: 'Overdue Exposure', value: formatCurrency(data.OverdueExposure) },
    { label: 'Due Within 7 Days', value: formatCurrency(data.DueWithin7Days) },
    {
      label: 'Recovery vs Billing',
      value: formatPercent(data.RecoveryVsBillingPercent),
    },
    {
      label: 'Planning Confidence',
      value: data.PlanningConfidence,
      severity: data.PlanningConfidence === 'High' ? 'success' : data.PlanningConfidence === 'Low' ? 'muted' : 'warning',
    },
  ]
})

onMounted(() => {
  void dashboard.loadCollectionOptimization()
})
</script>

<template>
  <DashboardDetailLayout
    title="Collection Optimization Dashboard"
    subtitle="Today's prioritized collection actions from risk forecast and receivables."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="optimization?.GeneratedAt ?? null"
    @refresh="dashboard.loadCollectionOptimization()"
  >
    <p v-if="optimization?.IsAvailable === false && !dashboard.loading" class="collection-optimization-dashboard__banner">
      Collection optimization data is not yet available. Run the snapshot refresh worker for the Customer domain.
    </p>

    <CollectionOptimizationSummary
      :summary="kpi?.ExecutiveSummaryText ?? null"
      :loading="dashboard.loading"
    />

    <template v-if="optimization?.IsAvailable !== false">
      <InventoryForecastKpiGrid :metrics="workloadMetrics" />
      <InventoryForecastKpiGrid :metrics="contextMetrics" />

      <div class="collection-optimization-dashboard__charts">
        <CollectionOptimizationActionChart
          :distribution="optimization?.ActionDistribution ?? []"
          :loading="dashboard.loading"
        />
        <CollectionOptimizationWorkloadChart
          :workload="optimization?.Workload ?? []"
          :loading="dashboard.loading"
        />
        <CollectionOptimizationImpactChart
          :distribution="optimization?.ActionDistribution ?? []"
          :loading="dashboard.loading"
        />
      </div>

      <CollectionOptimizationPriorityTable
        :rows="optimization?.PriorityQueue ?? []"
        :loading="dashboard.loading"
      />

      <CollectionOptimizationQueueTabs
        :queues="optimization?.SpecializedQueues ?? []"
        :loading="dashboard.loading"
      />

      <CollectionOptimizationImpactTable
        :rows="optimization?.TopImpactOpportunities ?? []"
        :loading="dashboard.loading"
      />
    </template>

    <footer class="collection-optimization-dashboard__footer">
      <p class="collection-optimization-dashboard__links">
        <RouterLink to="/dashboard/customer-risk-forecast">Customer Risk Forecast</RouterLink>
        ·
        <RouterLink to="/dashboard/collection">Collection</RouterLink>
        ·
        <RouterLink to="/dashboard/customers">Customer Analytics</RouterLink>
        ·
        <RouterLink to="/dashboard/piutang">Piutang</RouterLink>
        ·
        <RouterLink to="/reports/piutang">Piutang Report</RouterLink>
        ·
        <RouterLink to="/reports/sales">Sales Report</RouterLink>
      </p>
      <p class="collection-optimization-dashboard__disclaimer">
        Recommendations are indicative operational guidance based on deterministic business rules.
        BTR Portal does not initiate collection contact, modify credit limits, or schedule visits.
        Execute actions in BTR Desktop or field operations.
      </p>
    </footer>
  </DashboardDetailLayout>
</template>

<style scoped>
.collection-optimization-dashboard__banner {
  margin: 0 0 1rem;
  padding: 0.75rem 1rem;
  border-radius: 0.5rem;
  background: var(--p-yellow-50);
  border: 1px solid var(--p-yellow-200);
}

.collection-optimization-dashboard__charts {
  display: grid;
  grid-template-columns: 1fr;
  gap: 1rem;
  margin-bottom: 1rem;
}

@media (min-width: 960px) {
  .collection-optimization-dashboard__charts {
    grid-template-columns: repeat(2, 1fr);
  }
}

.collection-optimization-dashboard__footer {
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--p-surface-200);
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.collection-optimization-dashboard__links {
  margin: 0 0 0.5rem;
}

.collection-optimization-dashboard__disclaimer {
  margin: 0;
  line-height: 1.5;
}
</style>

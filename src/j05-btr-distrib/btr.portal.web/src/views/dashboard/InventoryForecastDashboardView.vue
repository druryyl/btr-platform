<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import InventoryConsumptionTrendChart from '@/components/dashboard/InventoryConsumptionTrendChart.vue'
import InventoryForecastKpiGrid from '@/components/dashboard/InventoryForecastKpiGrid.vue'
import InventoryForecastLevelChart from '@/components/dashboard/InventoryForecastLevelChart.vue'
import InventoryForecastRisksTable from '@/components/dashboard/InventoryForecastRisksTable.vue'
import InventoryForecastSummary from '@/components/dashboard/InventoryForecastSummary.vue'
import InventoryPurchaseRecommendationsTable from '@/components/dashboard/InventoryPurchaseRecommendationsTable.vue'
import InventoryRiskHeatSummary from '@/components/dashboard/InventoryRiskHeatSummary.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const forecast = computed(() => dashboard.inventoryForecast)

function healthSeverity(score: number): SalesForecastKpiMetric['severity'] {
  if (score >= 75) return 'success'
  if (score >= 50) return 'warning'
  return 'critical'
}

function confidenceSeverity(confidence: string | null | undefined): SalesForecastKpiMetric['severity'] {
  if (confidence === 'Low') return 'muted'
  if (confidence === 'High') return 'success'
  return 'warning'
}

const positionMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  return [
    { label: 'Current Inventory Value', value: formatCurrency(data.CurrentInventoryValue) },
    { label: 'Projected Inventory Value @ H', value: formatCurrency(data.ProjectedInventoryValue) },
    {
      label: 'Avg Days of Supply',
      value: data.WeightedAverageDaysOfSupply != null ? data.WeightedAverageDaysOfSupply.toFixed(1) : '—',
    },
    {
      label: 'Inventory Health Score',
      value: String(data.InventoryHealthScore),
      severity: healthSeverity(data.InventoryHealthScore),
    },
  ]
})

const riskExposureMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  return [
    {
      label: 'Stock-Out Risk Items',
      value: String(data.StockOutRiskItemCount),
      severity: data.StockOutRiskItemCount > 0 ? 'critical' : 'success',
    },
    {
      label: 'Overstock Value',
      value: formatCurrency(data.OverstockValue),
      severity: data.OverstockValue > 0 ? 'warning' : 'normal',
    },
    {
      label: 'Understock Value',
      value: formatCurrency(data.UnderstockValue),
      severity: data.UnderstockValue > 0 ? 'critical' : 'success',
    },
    {
      label: 'At-Risk Inventory %',
      value: formatPercent(data.AtRiskInventoryPercent),
    },
  ]
})

const scenarioMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  return [
    { label: 'Best Case Projected', value: formatCurrency(data.BestCaseProjectedValue) },
    { label: 'Expected Projected', value: formatCurrency(data.ProjectedInventoryValue) },
    { label: 'Worst Case Projected', value: formatCurrency(data.WorstCaseProjectedValue) },
    {
      label: 'Forecast Confidence',
      value: data.ForecastConfidence,
      severity: confidenceSeverity(data.ForecastConfidence),
    },
  ]
})

const paceMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  return [
    {
      label: 'Avg Daily Consumption (units)',
      value: formatNumber(data.AverageDailyConsumptionUnits),
    },
    {
      label: 'Forecast Consumption @ H',
      value: formatNumber(data.ForecastConsumptionUnits),
    },
    {
      label: 'Inventory Coverage %',
      value: formatPercent(data.InventoryCoveragePercent),
    },
    {
      label: 'Turnover Forecast',
      value: data.InventoryTurnoverForecast != null ? data.InventoryTurnoverForecast.toFixed(2) : '—',
    },
  ]
})

onMounted(() => {
  void dashboard.loadInventoryForecast()
})
</script>

<template>
  <DashboardDetailLayout
    title="Inventory Forecast Dashboard"
    subtitle="30-day forward planning — active inventory (BrgId-first)."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="forecast?.GeneratedAt ?? null"
    @refresh="dashboard.loadInventoryForecast()"
  >
    <p v-if="forecast?.IsAvailable" class="inventory-forecast-dashboard__meta">
      Horizon: {{ forecast.PlanningHorizonDays }} days from
      {{ new Date(forecast.BusinessDate).toLocaleDateString('id-ID') }}
    </p>

    <p v-else-if="!dashboard.loading" class="inventory-forecast-dashboard__banner">
      Inventory forecast data is not yet available. Run the snapshot refresh worker for the InventoryRisk domain.
    </p>

    <InventoryForecastSummary
      :summary="forecast?.ExecutiveSummary ?? null"
      :loading="dashboard.loading"
    />

    <template v-if="forecast?.IsAvailable !== false">
      <InventoryForecastKpiGrid :metrics="positionMetrics" />
      <InventoryForecastKpiGrid :metrics="riskExposureMetrics" />
      <InventoryForecastKpiGrid :metrics="scenarioMetrics" />

      <InventoryForecastLevelChart
        class="inventory-forecast-dashboard__section"
        :projected-level="forecast?.ProjectedLevel ?? []"
        :loading="dashboard.loading"
      />

      <div class="inventory-forecast-dashboard__charts-row">
        <InventoryConsumptionTrendChart
          class="inventory-forecast-dashboard__chart-half"
          :daily-consumption="forecast?.DailyConsumption ?? []"
          :loading="dashboard.loading"
        />

        <InventoryRiskHeatSummary
          class="inventory-forecast-dashboard__chart-half"
          :heat-summary="forecast?.HeatSummary ?? []"
          :loading="dashboard.loading"
        />
      </div>

      <InventoryForecastKpiGrid class="inventory-forecast-dashboard__section" :metrics="paceMetrics" />

      <InventoryForecastRisksTable
        class="inventory-forecast-dashboard__section"
        :risks="forecast?.TopRisks ?? []"
        :loading="dashboard.loading"
      />

      <InventoryPurchaseRecommendationsTable
        class="inventory-forecast-dashboard__section"
        :recommendations="forecast?.PurchaseRecommendations ?? []"
        :loading="dashboard.loading"
      />
    </template>

    <footer class="inventory-forecast-dashboard__footer">
      <p>
        Forecast uses 30-day Faktur sales qty. Position rules match Inventory Dashboard.
        <RouterLink :to="forecast?.Traceability?.InventoryReportRoute ?? '/reports/inventory'">
          Inventory Report
        </RouterLink>
        ·
        <RouterLink :to="forecast?.Traceability?.InventoryDashboardRoute ?? '/dashboard/inventory'">
          Inventory Dashboard
        </RouterLink>
        ·
        <RouterLink :to="forecast?.Traceability?.InventoryRiskDashboardRoute ?? '/dashboard/inventory-risk'">
          Inventory Risk
        </RouterLink>
        ·
        <RouterLink
          :to="forecast?.Traceability?.PurchasingManagementRoute ?? '/dashboard/purchasing'"
        >
          Purchasing Dashboard
        </RouterLink>
      </p>
      <p v-if="forecast?.Traceability?.Disclaimer" class="inventory-forecast-dashboard__disclaimer">
        {{ forecast.Traceability.Disclaimer }}
      </p>
    </footer>
  </DashboardDetailLayout>
</template>

<style scoped>
.inventory-forecast-dashboard__meta {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.9375rem;
}

.inventory-forecast-dashboard__banner {
  margin: 0 0 1rem;
  padding: 0.75rem 1rem;
  border-radius: 0.5rem;
  background: var(--p-yellow-50, #fefce8);
  border: 1px solid var(--p-yellow-200, #fde68a);
  color: var(--p-text-color);
}

.inventory-forecast-dashboard__section {
  margin-top: 1rem;
}

.inventory-forecast-dashboard__charts-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1rem;
}

.inventory-forecast-dashboard__chart-half {
  min-width: 0;
}

.inventory-forecast-dashboard__footer {
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--p-surface-200);
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.inventory-forecast-dashboard__footer p {
  margin: 0;
}

.inventory-forecast-dashboard__footer a {
  margin-left: 0.35rem;
  color: var(--p-primary-color);
  text-decoration: none;
  font-weight: 600;
}

.inventory-forecast-dashboard__footer a:hover {
  text-decoration: underline;
}

.inventory-forecast-dashboard__disclaimer {
  margin-top: 0.75rem !important;
  font-style: italic;
}

@media (max-width: 900px) {
  .inventory-forecast-dashboard__charts-row {
    grid-template-columns: 1fr;
  }
}
</style>

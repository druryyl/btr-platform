<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import CustomerRiskForecastAttentionList from '@/components/dashboard/CustomerRiskForecastAttentionList.vue'
import CustomerRiskForecastCategoryChart from '@/components/dashboard/CustomerRiskForecastCategoryChart.vue'
import CustomerRiskForecastCustomersTable from '@/components/dashboard/CustomerRiskForecastCustomersTable.vue'
import CustomerRiskForecastExposureChart from '@/components/dashboard/CustomerRiskForecastExposureChart.vue'
import CustomerRiskForecastKpiGrid from '@/components/dashboard/CustomerRiskForecastKpiGrid.vue'
import CustomerRiskForecastRecommendations from '@/components/dashboard/CustomerRiskForecastRecommendations.vue'
import CustomerRiskForecastSignalMixChart from '@/components/dashboard/CustomerRiskForecastSignalMixChart.vue'
import CustomerRiskForecastSummary from '@/components/dashboard/CustomerRiskForecastSummary.vue'
import CustomerRiskForecastWilayahChart from '@/components/dashboard/CustomerRiskForecastWilayahChart.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'
import { formatCurrency, formatPercent } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

const forecast = computed(() => dashboard.customerRiskForecast)
const kpi = computed(() => forecast.value?.Kpi ?? null)

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

const portfolioMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    { label: 'Horizon (days)', value: String(data.HorizonDays) },
    {
      label: 'Customers At Risk',
      value: String(data.CustomersForecastedAtRisk),
      severity: data.CustomersForecastedAtRisk > 0 ? 'warning' : 'success',
    },
    {
      label: 'Portfolio Health Score',
      value: String(data.PortfolioHealthScore),
      severity: healthSeverity(data.PortfolioHealthScore),
    },
    {
      label: 'Forecast Confidence',
      value: data.ForecastConfidence,
      severity: confidenceSeverity(data.ForecastConfidence),
    },
  ]
})

const exposureMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    {
      label: 'Elevated Risk Receivable',
      value: formatCurrency(data.ElevatedRiskReceivable),
      severity: data.ElevatedRiskReceivable > 0 ? 'warning' : 'success',
    },
    {
      label: 'Elevated Risk %',
      value: formatPercent(data.ElevatedRiskReceivablePercent),
    },
    { label: 'Total Piutang', value: formatCurrency(data.TotalPiutang) },
    {
      label: 'High / Critical Customers',
      value: `${data.HighRiskCustomerCount} / ${data.CriticalCustomerCount}`,
      severity:
        data.CriticalCustomerCount > 0
          ? 'critical'
          : data.HighRiskCustomerCount > 0
            ? 'warning'
            : 'success',
    },
  ]
})

const categoryMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    { label: 'Healthy', value: String(data.HealthyCount), severity: 'success' },
    { label: 'Watch', value: String(data.WatchCount) },
    { label: 'Attention', value: String(data.AttentionCount), severity: 'warning' },
    {
      label: 'High Risk',
      value: String(data.HighRiskCount),
      severity: data.HighRiskCount > 0 ? 'warning' : 'normal',
    },
  ]
})

const signalMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    { label: 'Payment Delay', value: String(data.PaymentDelaySignalCount) },
    { label: 'Credit Limit', value: String(data.CreditLimitSignalCount) },
    { label: 'Inactivity', value: String(data.InactivitySignalCount) },
    { label: 'Purchase Decline', value: String(data.PurchaseDeclineSignalCount) },
  ]
})

const collectionMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    {
      label: 'Collection Risk',
      value: String(data.CollectionRiskSignalCount),
      severity: data.CollectionRiskSignalCount > 0 ? 'warning' : 'success',
    },
    {
      label: 'Critical Category',
      value: String(data.CriticalCount),
      severity: data.CriticalCount > 0 ? 'critical' : 'success',
    },
    {
      label: 'High Risk Category',
      value: String(data.HighRiskCount),
      severity: data.HighRiskCount > 0 ? 'warning' : 'success',
    },
    {
      label: 'Customers Forecasted At Risk',
      value: String(data.CustomersForecastedAtRisk),
    },
  ]
})

onMounted(() => {
  void dashboard.loadCustomerRiskForecast()
})
</script>

<template>
  <DashboardDetailLayout
    title="Customer Risk Forecast Dashboard"
    subtitle="Forward-looking customer risk — deterministic signals over the forecast horizon."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="forecast?.GeneratedAt ?? null"
    @refresh="dashboard.loadCustomerRiskForecast()"
  >
    <p v-if="forecast?.IsAvailable" class="customer-risk-forecast-dashboard__meta">
      Horizon: {{ kpi?.HorizonDays ?? '—' }} days from
      {{ new Date(forecast.BusinessDate).toLocaleDateString('id-ID') }}
    </p>

    <p v-else-if="!dashboard.loading" class="customer-risk-forecast-dashboard__banner">
      Customer risk forecast data is not yet available. Run the snapshot refresh worker.
    </p>

    <CustomerRiskForecastSummary
      :summary="kpi?.ExecutiveSummaryText ?? null"
      :loading="dashboard.loading"
    />

    <template v-if="forecast?.IsAvailable !== false">
      <CustomerRiskForecastKpiGrid :metrics="portfolioMetrics" />
      <CustomerRiskForecastKpiGrid :metrics="exposureMetrics" />
      <CustomerRiskForecastKpiGrid :metrics="categoryMetrics" />
      <CustomerRiskForecastKpiGrid :metrics="signalMetrics" />
      <CustomerRiskForecastKpiGrid :metrics="collectionMetrics" />

      <div class="customer-risk-forecast-dashboard__charts-row">
        <CustomerRiskForecastCategoryChart
          class="customer-risk-forecast-dashboard__chart-half"
          :distribution="forecast?.CategoryDistribution ?? []"
          :loading="dashboard.loading"
        />

        <CustomerRiskForecastExposureChart
          class="customer-risk-forecast-dashboard__chart-half"
          :elevated-risk-receivable="kpi?.ElevatedRiskReceivable ?? 0"
          :total-piutang="kpi?.TotalPiutang ?? 0"
          :loading="dashboard.loading"
        />
      </div>

      <div class="customer-risk-forecast-dashboard__charts-row">
        <CustomerRiskForecastWilayahChart
          class="customer-risk-forecast-dashboard__chart-half"
          :wilayah="forecast?.TopWilayah ?? []"
          :loading="dashboard.loading"
        />

        <CustomerRiskForecastSignalMixChart
          class="customer-risk-forecast-dashboard__chart-half"
          :signal-mix="forecast?.SignalMix ?? []"
          :loading="dashboard.loading"
        />
      </div>

      <CustomerRiskForecastCustomersTable
        class="customer-risk-forecast-dashboard__section"
        :customers="forecast?.TopCustomers ?? []"
        :loading="dashboard.loading"
      />

      <CustomerRiskForecastAttentionList
        class="customer-risk-forecast-dashboard__section"
        :items="forecast?.AttentionList ?? []"
        :loading="dashboard.loading"
      />

      <CustomerRiskForecastRecommendations
        class="customer-risk-forecast-dashboard__section"
        :recommendations="forecast?.Recommendations ?? []"
        :loading="dashboard.loading"
      />
    </template>

    <footer class="customer-risk-forecast-dashboard__footer">
      <p class="customer-risk-forecast-dashboard__disclaimer">
        Forecasts are indicative decision support based on deterministic business rules. BTR Portal
        does not modify credit limits, suspend customers, or schedule collections. Confirm account
        status in BTR Desktop before acting.
      </p>
      <p class="customer-risk-forecast-dashboard__links">
        <RouterLink to="/dashboard/customers">Customers Dashboard</RouterLink>
        ·
        <RouterLink to="/dashboard/piutang">Piutang Dashboard</RouterLink>
        ·
        <RouterLink to="/dashboard/collection">Collection Dashboard</RouterLink>
        ·
        <RouterLink to="/dashboard/cash-flow-forecast">Cash Flow Forecast</RouterLink>
        ·
        <RouterLink to="/reports/piutang">Piutang Report</RouterLink>
        ·
        <RouterLink to="/reports/sales">Sales Report</RouterLink>
      </p>
    </footer>
  </DashboardDetailLayout>
</template>

<style scoped>
.customer-risk-forecast-dashboard__meta {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.9375rem;
}

.customer-risk-forecast-dashboard__banner {
  margin: 0 0 1rem;
  padding: 0.75rem 1rem;
  border-radius: var(--p-content-border-radius);
  background: var(--p-yellow-50);
  border: 1px solid var(--p-yellow-200);
  color: var(--p-text-color);
  font-size: 0.9375rem;
}

.customer-risk-forecast-dashboard__section {
  margin-top: 1rem;
}

.customer-risk-forecast-dashboard__charts-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1rem;
}

.customer-risk-forecast-dashboard__chart-half {
  min-width: 0;
}

.customer-risk-forecast-dashboard__footer {
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--p-surface-200);
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.customer-risk-forecast-dashboard__disclaimer {
  margin: 0 0 0.75rem;
  line-height: 1.5;
}

.customer-risk-forecast-dashboard__links {
  margin: 0;
}

.customer-risk-forecast-dashboard__links a {
  color: var(--p-primary-color);
  text-decoration: none;
  font-weight: 600;
}

.customer-risk-forecast-dashboard__links a:hover {
  text-decoration: underline;
}

@media (max-width: 900px) {
  .customer-risk-forecast-dashboard__charts-row {
    grid-template-columns: 1fr;
  }
}
</style>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { RouterLink } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import CustomerPortfolioSummary from '@/components/dashboard/customer-portfolio/CustomerPortfolioSummary.vue'
import CustomerPortfolioKpiGrid from '@/components/dashboard/customer-portfolio/CustomerPortfolioKpiGrid.vue'
import CustomerPortfolioLifecycleChart from '@/components/dashboard/customer-portfolio/CustomerPortfolioLifecycleChart.vue'
import CustomerPortfolioTierChart from '@/components/dashboard/customer-portfolio/CustomerPortfolioTierChart.vue'
import CustomerPortfolioFilterBar from '@/components/dashboard/customer-portfolio/CustomerPortfolioFilterBar.vue'
import CustomerPortfolioPriorityTable from '@/components/dashboard/customer-portfolio/CustomerPortfolioPriorityTable.vue'
import CustomerPortfolioActionSegments from '@/components/dashboard/customer-portfolio/CustomerPortfolioActionSegments.vue'
import CustomerPortfolioConcentrationTables from '@/components/dashboard/customer-portfolio/CustomerPortfolioConcentrationTables.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'
import { formatCurrency, formatPercent } from '@/services/formatters'
import {
  collectDistinctFilterValues,
  createDefaultPortfolioFilters,
  filterPortfolioCustomers,
} from '@/services/customerPortfolioSignals'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const portfolio = computed(() => dashboard.customerPortfolio)
const kpi = computed(() => portfolio.value?.Kpi ?? null)
const filters = ref(createDefaultPortfolioFilters())

const filterOptions = computed(() =>
  collectDistinctFilterValues(portfolio.value?.Customers ?? []),
)

const filteredCustomers = computed(() =>
  filterPortfolioCustomers(portfolio.value?.Customers ?? [], filters.value),
)

const filteredPriorityQueue = computed(() => {
  const customerCodes = new Set(filteredCustomers.value.map((row) => row.CustomerCode))
  return (portfolio.value?.PriorityQueue ?? []).filter((row) =>
    customerCodes.has(row.CustomerCode),
  )
})

function healthSeverity(score: number): SalesForecastKpiMetric['severity'] {
  if (score >= 75) return 'success'
  if (score >= 50) return 'warning'
  return 'critical'
}

const healthMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    {
      label: 'Portfolio Health Score',
      value: String(data.PortfolioHealthScore),
      severity: healthSeverity(data.PortfolioHealthScore),
    },
    {
      label: 'Portfolio Healthy %',
      value: formatPercent(data.PortfolioHealthyPercent),
    },
    {
      label: 'Attention Customers',
      value: String(data.AttentionCustomerCount),
      severity: data.AttentionCustomerCount > 0 ? 'warning' : 'success',
    },
    {
      label: 'Customers At Risk',
      value: String(data.CustomersAtRiskCount),
      severity: data.CustomersAtRiskCount > 0 ? 'warning' : 'success',
    },
  ]
})

const strategicMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    {
      label: 'Strategic Customers',
      value: String(data.StrategicCustomerCount),
    },
    {
      label: 'Strategic At Risk',
      value: String(data.StrategicAtRiskCount),
      severity: data.StrategicAtRiskCount > 0 ? 'critical' : 'success',
    },
    {
      label: 'Working Capital Tied',
      value: formatCurrency(data.WorkingCapitalTiedAmount),
    },
    {
      label: 'Total Customers',
      value: String(data.TotalCustomerCount),
    },
  ]
})

const lifecycleMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = kpi.value
  if (!data) return []

  return [
    { label: 'Never Purchased', value: String(data.NeverPurchasedCount) },
    {
      label: 'Dormant',
      value: String(data.DormantCount),
      severity: data.DormantCount > 0 ? 'warning' : 'success',
    },
    {
      label: 'Declining',
      value: String(data.DecliningCount),
      severity: data.DecliningCount > 0 ? 'warning' : 'success',
    },
    { label: 'Total MTD Omzet', value: formatCurrency(data.TotalMtdOmzet) },
  ]
})

onMounted(() => {
  void dashboard.loadCustomerPortfolio()
})
</script>

<template>
  <DashboardDetailLayout
    title="Customer Portfolio Dashboard"
    subtitle="Portfolio management actions composed from customer analytics, risk forecast, and collection optimization."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="portfolio?.GeneratedAt ?? null"
    @refresh="dashboard.loadCustomerPortfolio()"
  >
    <p v-if="portfolio?.IsAvailable === false && !dashboard.loading" class="customer-portfolio-dashboard__banner">
      Customer portfolio data is not yet available. Run the snapshot refresh worker for the Customer domain.
    </p>

    <CustomerPortfolioSummary
      :summary="kpi?.ExecutiveSummaryText ?? null"
      :disclaimer="kpi?.ValueDisclaimerText ?? null"
      :loading="dashboard.loading"
    />

    <template v-if="portfolio?.IsAvailable !== false">
      <CustomerPortfolioKpiGrid :metrics="healthMetrics" />
      <CustomerPortfolioKpiGrid :metrics="strategicMetrics" />
      <CustomerPortfolioKpiGrid :metrics="lifecycleMetrics" />

      <div class="customer-portfolio-dashboard__charts">
        <CustomerPortfolioLifecycleChart
          :distribution="portfolio?.LifecycleDistribution ?? []"
          :loading="dashboard.loading"
        />
        <CustomerPortfolioTierChart
          :distribution="portfolio?.TierDistribution ?? []"
          :loading="dashboard.loading"
        />
      </div>

      <CustomerPortfolioFilterBar v-model:filters="filters" :options="filterOptions" />

      <CustomerPortfolioPriorityTable
        :rows="filteredPriorityQueue"
        :loading="dashboard.loading"
      />

      <CustomerPortfolioActionSegments
        :rows="filteredPriorityQueue"
        :loading="dashboard.loading"
      />

      <CustomerPortfolioConcentrationTables
        :top-omzet="portfolio?.TopOmzet ?? []"
        :top-piutang="portfolio?.TopPiutang ?? []"
        :loading="dashboard.loading"
      />
    </template>

    <footer class="customer-portfolio-dashboard__footer">
      <p class="customer-portfolio-dashboard__links">
        <RouterLink to="/dashboard/customers">Customer Analytics</RouterLink>
        ·
        <RouterLink to="/dashboard/customer-risk-forecast">Customer Risk Forecast</RouterLink>
        ·
        <RouterLink to="/dashboard/collection-optimization">Collection Optimization</RouterLink>
        ·
        <RouterLink to="/reports/customers">Customer Report</RouterLink>
        ·
        <RouterLink to="/reports/sales">Sales Report</RouterLink>
        ·
        <RouterLink to="/reports/piutang">Piutang Report</RouterLink>
      </p>
      <p class="customer-portfolio-dashboard__disclaimer">
        Portfolio recommendations are read-only management guidance based on deterministic business
        rules. Customer Value = Omzet Proxy, NOT profitability. BTR Portal does not modify credit
        limits, suspend accounts, or initiate customer contact.
      </p>
    </footer>
  </DashboardDetailLayout>
</template>

<style scoped>
.customer-portfolio-dashboard__banner {
  margin: 0 0 1rem;
  padding: 0.75rem 1rem;
  border-radius: 0.5rem;
  background: var(--p-yellow-50);
  border: 1px solid var(--p-yellow-200);
}

.customer-portfolio-dashboard__charts {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.customer-portfolio-dashboard__footer {
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--p-surface-200);
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.customer-portfolio-dashboard__links {
  margin: 0 0 0.5rem;
}

.customer-portfolio-dashboard__disclaimer {
  margin: 0;
  line-height: 1.5;
}

@media (max-width: 960px) {
  .customer-portfolio-dashboard__charts {
    grid-template-columns: 1fr;
  }
}
</style>

<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DailyPaceTrendChart from '@/components/dashboard/DailyPaceTrendChart.vue'
import ForecastRiskCard from '@/components/dashboard/ForecastRiskCard.vue'
import ForecastVsTargetChart from '@/components/dashboard/ForecastVsTargetChart.vue'
import SalesForecastKpiRow from '@/components/dashboard/SalesForecastKpiRow.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'
import SalesForecastSummary from '@/components/dashboard/SalesForecastSummary.vue'
import WeeklyTrendChart from '@/components/dashboard/WeeklyTrendChart.vue'
import { formatCurrency, formatCurrencyCompact, formatPercent } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

const forecast = computed(() => dashboard.salesForecast)

const periodLabel = computed(() => {
  if (!forecast.value) return '—'
  const date = new Date(forecast.value.PeriodYear, forecast.value.PeriodMonth - 1, 1)
  return date.toLocaleDateString('id-ID', { month: 'long', year: 'numeric' })
})

const dayProgressLabel = computed(() => {
  if (!forecast.value) return ''
  return `Day ${forecast.value.DaysElapsed} of ${forecast.value.DaysInMonth}`
})

const actualVsForecastMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  return [
    {
      label: 'Current Sales',
      value: formatCurrencyCompact(data.CurrentSales),
      title: formatCurrency(data.CurrentSales),
    },
    {
      label: 'Current Achievement',
      value: formatPercent(data.CurrentAchievementPercent),
    },
    {
      label: 'Forecast Sales',
      value: formatCurrencyCompact(data.ForecastSales),
      title: formatCurrency(data.ForecastSales),
    },
    {
      label: 'Forecast Achievement',
      value: formatPercent(data.ForecastAchievementPercent),
    },
  ]
})

const paceGapMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  const requiredSeverity =
    data.RequiredDailySeverity === 'Critical'
      ? 'critical'
      : data.RequiredDailySeverity === 'Warning'
        ? 'warning'
        : 'normal'

  return [
    {
      label: 'Daily Average Sales',
      value: formatCurrencyCompact(data.DailyAverageSales),
      title: formatCurrency(data.DailyAverageSales),
    },
    {
      label: 'Required Daily Sales',
      value:
        data.RequiredDailySales != null
          ? formatCurrencyCompact(data.RequiredDailySales)
          : '—',
      title:
        data.RequiredDailySales != null
          ? formatCurrency(data.RequiredDailySales)
          : undefined,
      severity: requiredSeverity,
    },
    {
      label: 'Target Gap',
      value: formatCurrencyCompact(data.TargetGap),
      title: formatCurrency(data.TargetGap),
      severity: data.TargetGap > 0 ? 'warning' : 'success',
    },
    {
      label: 'Days Remaining',
      value: String(data.DaysRemaining),
      hint: 'calendar days',
    },
  ]
})

const scenarioMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  const confidenceSeverity =
    data.ForecastConfidence === 'Low'
      ? 'muted'
      : data.ForecastConfidence === 'High'
        ? 'success'
        : 'warning'

  return [
    {
      label: 'Best Case',
      value: formatCurrencyCompact(data.BestCaseSales),
      title: formatCurrency(data.BestCaseSales),
    },
    {
      label: 'Expected',
      value: formatCurrencyCompact(data.ForecastSales),
      title: formatCurrency(data.ForecastSales),
    },
    {
      label: 'Worst Case',
      value: formatCurrencyCompact(data.WorstCaseSales),
      title: formatCurrency(data.WorstCaseSales),
    },
    {
      label: 'Forecast Confidence',
      value: data.ForecastConfidence,
      severity: confidenceSeverity,
    },
  ]
})

const principalForecast = computed(() => forecast.value?.PrincipalForecast ?? null)

const principalItems = computed(() => principalForecast.value?.Items ?? [])

function formatAmount(value: number | null | undefined): string {
  return value == null ? '—' : formatCurrency(value)
}

onMounted(() => {
  void dashboard.loadSalesForecast()
})
</script>

<template>
  <DashboardDetailLayout
    title="Sales Forecast Dashboard"
    subtitle="Current month forecast — invoiced sales (Faktur)."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="forecast?.GeneratedAt ?? null"
    @refresh="dashboard.loadSalesForecast()"
  >
    <p v-if="forecast" class="sales-forecast-dashboard__meta">
      Period: {{ periodLabel }} · {{ dayProgressLabel }} · As of
      {{ new Date(forecast.BusinessDate).toLocaleDateString('id-ID') }}
    </p>

    <SalesForecastSummary
      :summary="forecast?.ExecutiveSummary ?? null"
      :loading="dashboard.loading"
    />

    <SalesForecastKpiRow :metrics="actualVsForecastMetrics" />
    <SalesForecastKpiRow :metrics="paceGapMetrics" />
    <SalesForecastKpiRow :metrics="scenarioMetrics" />

    <DailyPaceTrendChart
      class="sales-forecast-dashboard__section"
      :daily-pace="forecast?.DailyPace ?? []"
      :loading="dashboard.loading"
    />

    <div class="sales-forecast-dashboard__charts-row">
      <ForecastVsTargetChart
        class="sales-forecast-dashboard__chart-half"
        :data="forecast?.ForecastVsTarget ?? null"
        :loading="dashboard.loading"
      />

      <WeeklyTrendChart
        class="sales-forecast-dashboard__chart-half"
        :weekly-trend="forecast?.WeeklyTrend ?? []"
        :loading="dashboard.loading"
        title="Weekly Pace"
        empty-message="No weekly omzet data for the current period."
      />
    </div>

    <ForecastRiskCard
      class="sales-forecast-dashboard__section"
      :risk-band="forecast?.ForecastRiskBand ?? null"
      :forecast-achievement-percent="forecast?.ForecastAchievementPercent ?? null"
      :required-daily-sales="forecast?.RequiredDailySales ?? null"
      :target-gap="forecast?.TargetGap ?? 0"
      :loading="dashboard.loading"
    />

    <section
      v-if="forecast"
      class="sales-forecast-dashboard__principal"
      aria-label="Principal forecast"
    >
      <h2>Principal forecast</h2>
      <p class="sales-forecast-dashboard__principal-note">
        Presentation of current-month
        {{ principalForecast?.SalesOutKpiId || 'PRN-SALES-001' }}
        history compared with
        {{ principalForecast?.TargetKpiId || 'PRN-TGT-001' }}.
        Pace uses the existing sales forecast method. This is not a registry KPI and is not used to rank Principals.
        {{ principalForecast?.CompanyForecastNote }}
      </p>

      <p v-if="principalForecast?.IsAvailable" class="sales-forecast-dashboard__principal-note">
        Sum of Principal forecasts:
        {{ formatCurrency(principalForecast?.SumOfPrincipalForecasts ?? 0) }}.
        Company forecast remains
        {{ formatCurrency(forecast.ForecastSales) }}.
      </p>

      <section class="sales-forecast-dashboard__disclosure" aria-label="Principal forecast disclosure">
        <h3>Principal forecast disclosure</h3>
        <ul>
          <li
            v-for="statement in principalForecast?.Disclosures ?? []"
            :key="statement"
          >
            {{ statement }}
          </li>
        </ul>
      </section>

      <DataTable
        :value="principalItems"
        striped-rows
        class="sales-forecast-dashboard__principal-table"
      >
        <template #empty>
          <p class="sales-forecast-dashboard__principal-empty">
            No Principal Sales-Out history for the current period.
          </p>
        </template>
        <Column field="PrincipalName" header="Principal" />
        <Column header="Principal Sales-Out (PRN-SALES-001)">
          <template #body="{ data }">
            {{ formatAmount(data.PrincipalSalesOutAmount) }}
          </template>
        </Column>
        <Column header="Principal Target (PRN-TGT-001)">
          <template #body="{ data }">
            {{ formatAmount(data.PrincipalTargetAmount) }}
          </template>
        </Column>
        <Column header="Forecast">
          <template #body="{ data }">
            {{ formatAmount(data.ForecastAmount) }}
          </template>
        </Column>
        <Column header="Daily average">
          <template #body="{ data }">
            {{ formatAmount(data.DailyAverageSales) }}
          </template>
        </Column>
        <Column header="Required daily">
          <template #body="{ data }">
            {{ formatAmount(data.RequiredDailySales) }}
          </template>
        </Column>
        <Column header="Target gap">
          <template #body="{ data }">
            {{ formatAmount(data.TargetGap) }}
          </template>
        </Column>
      </DataTable>
    </section>

    <footer class="sales-forecast-dashboard__footer">
      <p>
        Forecast based on invoiced Faktur omzet through the business date. Same rules as
        Sales Dashboard.
        <RouterLink to="/reports/sales">View evidence → Sales Report</RouterLink>
      </p>
    </footer>
  </DashboardDetailLayout>
</template>

<style scoped>
.sales-forecast-dashboard__meta {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.9375rem;
}

.sales-forecast-dashboard__section {
  margin-top: 1rem;
}

.sales-forecast-dashboard__charts-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1rem;
}

.sales-forecast-dashboard__chart-half {
  min-width: 0;
}

.sales-forecast-dashboard__principal {
  margin-top: 1.5rem;
}

.sales-forecast-dashboard__principal h2,
.sales-forecast-dashboard__disclosure h3 {
  margin: 0 0 0.75rem;
  font-size: 1rem;
}

.sales-forecast-dashboard__principal-note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
}

.sales-forecast-dashboard__disclosure {
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.sales-forecast-dashboard__disclosure ul {
  margin: 0;
  padding-left: 1.25rem;
}

.sales-forecast-dashboard__disclosure li + li {
  margin-top: 0.35rem;
}

.sales-forecast-dashboard__principal-empty {
  margin: 0;
  color: var(--p-text-muted-color);
}

.sales-forecast-dashboard__footer {
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--p-surface-200);
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.sales-forecast-dashboard__footer p {
  margin: 0;
}

.sales-forecast-dashboard__footer a {
  margin-left: 0.35rem;
  color: var(--p-primary-color);
  text-decoration: none;
  font-weight: 600;
}

.sales-forecast-dashboard__footer a:hover {
  text-decoration: underline;
}

@media (max-width: 900px) {
  .sales-forecast-dashboard__charts-row {
    grid-template-columns: 1fr;
  }
}
</style>

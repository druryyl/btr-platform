<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DailyPaceTrendChart from '@/components/dashboard/DailyPaceTrendChart.vue'
import ForecastRiskCard from '@/components/dashboard/ForecastRiskCard.vue'
import ForecastVsTargetChart from '@/components/dashboard/ForecastVsTargetChart.vue'
import SalesForecastKpiRow from '@/components/dashboard/SalesForecastKpiRow.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'
import SalesForecastSummary from '@/components/dashboard/SalesForecastSummary.vue'
import WeeklyTrendChart from '@/components/dashboard/WeeklyTrendChart.vue'
import { formatCurrency, formatPercent } from '@/services/formatters'
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
    { label: 'Current Sales', value: formatCurrency(data.CurrentSales) },
    {
      label: 'Current Achievement',
      value: formatPercent(data.CurrentAchievementPercent),
    },
    { label: 'Forecast Sales', value: formatCurrency(data.ForecastSales) },
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
    { label: 'Daily Average Sales', value: formatCurrency(data.DailyAverageSales) },
    {
      label: 'Required Daily Sales',
      value:
        data.RequiredDailySales != null
          ? formatCurrency(data.RequiredDailySales)
          : '—',
      severity: requiredSeverity,
    },
    {
      label: 'Target Gap',
      value: formatCurrency(data.TargetGap),
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
    { label: 'Best Case', value: formatCurrency(data.BestCaseSales) },
    { label: 'Expected', value: formatCurrency(data.ForecastSales) },
    { label: 'Worst Case', value: formatCurrency(data.WorstCaseSales) },
    {
      label: 'Forecast Confidence',
      value: data.ForecastConfidence,
      severity: confidenceSeverity,
    },
  ]
})

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

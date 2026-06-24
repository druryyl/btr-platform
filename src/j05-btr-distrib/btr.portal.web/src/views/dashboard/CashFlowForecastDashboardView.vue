<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { RouterLink } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import CashFlowCollectionRisksTable from '@/components/dashboard/CashFlowCollectionRisksTable.vue'
import CashFlowDailyPaceChart from '@/components/dashboard/CashFlowDailyPaceChart.vue'
import CashFlowForecastSummary from '@/components/dashboard/CashFlowForecastSummary.vue'
import CashFlowForecastVsBillingChart from '@/components/dashboard/CashFlowForecastVsBillingChart.vue'
import CashFlowKpiGrid from '@/components/dashboard/CashFlowKpiGrid.vue'
import type { SalesForecastKpiMetric } from '@/components/dashboard/SalesForecastKpiRow.vue'
import CashFlowRecoveryTrendChart from '@/components/dashboard/CashFlowRecoveryTrendChart.vue'
import { formatCurrency, formatPercent } from '@/services/formatters'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()

const forecast = computed(() => dashboard.cashFlowForecast)

const periodLabel = computed(() => {
  if (!forecast.value) return '—'
  const date = new Date(forecast.value.PeriodYear, forecast.value.PeriodMonth - 1, 1)
  return date.toLocaleDateString('id-ID', { month: 'long', year: 'numeric' })
})

const dayProgressLabel = computed(() => {
  if (!forecast.value) return ''
  return `Day ${forecast.value.DaysElapsed} of ${forecast.value.DaysInMonth}`
})

function recoveryBandSeverity(band: string | null | undefined): SalesForecastKpiMetric['severity'] {
  if (band === 'Healthy') return 'success'
  if (band === 'Warning') return 'warning'
  if (band === 'Critical') return 'critical'
  return 'normal'
}

const cashPositionMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  return [
    { label: 'Cash Collected MTD', value: formatCurrency(data.CashCollectedMtd) },
    { label: 'Expected Cash Collection', value: formatCurrency(data.ExpectedCashCollection) },
    {
      label: 'Projected Month-End Collection',
      value: formatCurrency(data.ExpectedCashCollection),
    },
    {
      label: 'Collection Forecast %',
      value: formatPercent(data.CollectionForecastPercent),
      severity: recoveryBandSeverity(data.ForecastRiskBand),
    },
  ]
})

const paceTargetMetrics = computed((): SalesForecastKpiMetric[] => {
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
      label: 'Daily Cash Collection Average',
      value: formatCurrency(data.DailyCashCollectionAverage),
    },
    {
      label: 'Required Daily Collection',
      value:
        data.RequiredDailyCollection != null
          ? formatCurrency(data.RequiredDailyCollection)
          : '—',
      severity: requiredSeverity,
    },
    {
      label: 'Remaining Collection Target',
      value: formatCurrency(data.RemainingCollectionTarget),
      severity: data.RemainingCollectionTarget > 0 ? 'warning' : 'success',
    },
    {
      label: 'Remaining Calendar Days',
      value: String(data.DaysRemaining),
      hint: 'calendar days',
    },
  ]
})

const recoveryScenarioMetrics = computed((): SalesForecastKpiMetric[] => {
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
      label: 'Recovery vs Billing (Actual)',
      value: formatPercent(data.RecoveryVsBillingPercent),
      severity: recoveryBandSeverity(
        data.RecoveryVsBillingPercent != null && data.RecoveryVsBillingPercent >= 100
          ? 'Healthy'
          : data.RecoveryVsBillingPercent != null && data.RecoveryVsBillingPercent >= 80
            ? 'Warning'
            : data.RecoveryVsBillingPercent != null
              ? 'Critical'
              : undefined,
      ),
    },
    {
      label: 'Recovery vs Billing Forecast',
      value: formatPercent(data.RecoveryVsBillingForecastPercent),
      severity: recoveryBandSeverity(data.ForecastRiskBand),
    },
    {
      label: 'Best / Exp / Worst Cash',
      value: `${formatCurrency(data.BestCaseCash)} / ${formatCurrency(data.ExpectedCashCollection)} / ${formatCurrency(data.WorstCaseCash)}`,
    },
    {
      label: 'Forecast Confidence',
      value: data.ForecastConfidence,
      severity: confidenceSeverity,
    },
  ]
})

const receivableContextMetrics = computed((): SalesForecastKpiMetric[] => {
  const data = forecast.value
  if (!data) return []

  return [
    {
      label: 'Outstanding Due Remaining',
      value: formatCurrency(data.OutstandingDueRemaining),
    },
    { label: 'Overdue Outstanding', value: formatCurrency(data.OverdueOutstanding) },
    {
      label: 'Collection Gap',
      value: formatCurrency(data.CollectionGap),
      severity: data.CollectionGap > 0 ? 'warning' : 'success',
    },
    {
      label: 'Forecast Variance (Cash)',
      value: formatCurrency(data.ForecastVarianceCash),
    },
  ]
})

onMounted(() => {
  void dashboard.loadCashFlowForecast()
})
</script>

<template>
  <DashboardDetailLayout
    title="Cash Flow Forecast Dashboard"
    subtitle="Current month forecast — projected cash collection and recovery pace."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="forecast?.GeneratedAt ?? null"
    @refresh="dashboard.loadCashFlowForecast()"
  >
    <p v-if="forecast?.IsAvailable" class="cash-flow-forecast-dashboard__meta">
      Period: {{ periodLabel }} · {{ dayProgressLabel }} · As of
      {{ new Date(forecast.BusinessDate).toLocaleDateString('id-ID') }}
    </p>

    <CashFlowForecastSummary
      :summary="forecast?.ExecutiveSummary ?? null"
      :loading="dashboard.loading"
    />

    <template v-if="forecast?.IsAvailable !== false">
      <CashFlowKpiGrid :metrics="cashPositionMetrics" />
      <CashFlowKpiGrid :metrics="paceTargetMetrics" />
      <CashFlowKpiGrid :metrics="recoveryScenarioMetrics" />
      <CashFlowKpiGrid :metrics="receivableContextMetrics" />

      <CashFlowDailyPaceChart
        class="cash-flow-forecast-dashboard__section"
        :daily-pace="forecast?.DailyPace ?? []"
        :loading="dashboard.loading"
      />

      <div class="cash-flow-forecast-dashboard__charts-row">
        <CashFlowForecastVsBillingChart
          class="cash-flow-forecast-dashboard__chart-half"
          :billing="forecast?.MonthFakturOmzet ?? 0"
          :cash-mtd="forecast?.CashCollectedMtd ?? 0"
          :projected-cash="forecast?.ExpectedCashCollection ?? 0"
          :loading="dashboard.loading"
        />

        <CashFlowRecoveryTrendChart
          class="cash-flow-forecast-dashboard__chart-half"
          :recovery-trend="forecast?.RecoveryTrend ?? []"
          :loading="dashboard.loading"
        />
      </div>

      <CashFlowCollectionRisksTable
        class="cash-flow-forecast-dashboard__section"
        :risks="forecast?.CollectionRisks ?? []"
        :loading="dashboard.loading"
      />
    </template>

    <footer class="cash-flow-forecast-dashboard__footer">
      <p>
        Forecast based on pelunasan through the business date. Cash MTD matches Collection
        Dashboard. Billing matches Sales Dashboard.
        <RouterLink to="/reports/piutang">View receivable evidence → Piutang Report</RouterLink>
        ·
        <RouterLink to="/dashboard/collection">Collection Dashboard</RouterLink>
      </p>
    </footer>
  </DashboardDetailLayout>
</template>

<style scoped>
.cash-flow-forecast-dashboard__meta {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.9375rem;
}

.cash-flow-forecast-dashboard__section {
  margin-top: 1rem;
}

.cash-flow-forecast-dashboard__charts-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1rem;
}

.cash-flow-forecast-dashboard__chart-half {
  min-width: 0;
}

.cash-flow-forecast-dashboard__footer {
  margin-top: 1.5rem;
  padding-top: 1rem;
  border-top: 1px solid var(--p-surface-200);
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.cash-flow-forecast-dashboard__footer p {
  margin: 0;
}

.cash-flow-forecast-dashboard__footer a {
  margin-left: 0.35rem;
  color: var(--p-primary-color);
  text-decoration: none;
  font-weight: 600;
}

.cash-flow-forecast-dashboard__footer a:hover {
  text-decoration: underline;
}

@media (max-width: 900px) {
  .cash-flow-forecast-dashboard__charts-row {
    grid-template-columns: 1fr;
  }
}
</style>

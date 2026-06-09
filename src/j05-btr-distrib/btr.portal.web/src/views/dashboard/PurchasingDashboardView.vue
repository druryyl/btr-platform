<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import PurchasingAttentionCards from '@/components/dashboard/PurchasingAttentionCards.vue'
import PurchasingSummaryRow from '@/components/dashboard/PurchasingSummaryRow.vue'
import PurchasingAttentionList from '@/components/dashboard/PurchasingAttentionList.vue'
import PurchasingPrincipalExposureTable from '@/components/dashboard/PurchasingPrincipalExposureTable.vue'
import PurchasingNavigationSection from '@/components/dashboard/PurchasingNavigationSection.vue'
import PostingStatusPieChart from '@/components/dashboard/PostingStatusPieChart.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import WeeklyTrendChart from '@/components/dashboard/WeeklyTrendChart.vue'
import type { DashboardSalesWeekTrendItem } from '@/models/dashboard'
import { navigateToReport } from '@/services/navigateToReport'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()

const managementUnavailable = computed(
  () => dashboard.purchasing != null && !dashboard.purchasing.IsManagementAvailable,
)

const top10Columns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'MtdPurchaseAmount', header: 'MTD Purchase' },
  { field: 'PercentOfPurchase', header: '% of Purchase' },
]

const top10Rows = computed(() =>
  (dashboard.purchasing?.PrincipalExposure ?? []).map((row) => ({
    Rank: row.Rank,
    PrincipalName: row.PrincipalName,
    MtdPurchaseAmount: row.MtdPurchaseAmount,
    PercentOfPurchase: row.PercentOfPurchase,
    ReportRoute: row.ReportRoute,
  })),
)

const weeklyTrendForChart = computed((): DashboardSalesWeekTrendItem[] =>
  (dashboard.purchasing?.WeeklyTrend ?? []).map((week) => ({
    WeekStart: week.WeekStart,
    WeekEnd: week.WeekEnd,
    WeekLabel: week.WeekLabel,
    RecognizedAmount: week.PurchaseAmount,
  })),
)

function onTop10RowClick(row: Record<string, unknown>): void {
  const principalName = String(row.PrincipalName ?? '')
  const reportRoute = String(row.ReportRoute ?? '/reports/purchasing')
  if (principalName) {
    navigateToReport(router, reportRoute, principalName)
  }
}

onMounted(() => {
  void dashboard.loadPurchasing()
})
</script>

<template>
  <DashboardDetailLayout
    title="Purchasing Management Dashboard"
    subtitle="Which suppliers and purchasing activities require management attention? Current Month Purchasing — Management Attention View"
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.purchasing?.GeneratedAt ?? null"
    @refresh="dashboard.loadPurchasing()"
  >
    <Message
      v-if="dashboard.purchasing && !dashboard.purchasing.IsDataFresh"
      severity="warn"
      :closable="false"
      class="purchasing-dashboard__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <Message
      v-if="managementUnavailable"
      severity="info"
      :closable="false"
      class="purchasing-dashboard__banner"
    >
      Management attention data is not yet available. V1 statistics below may still be shown.
    </Message>

    <section class="purchasing-dashboard__section">
      <h2 class="purchasing-dashboard__section-title">Purchasing Attention Cards</h2>
      <PurchasingAttentionCards
        :cards="dashboard.purchasing?.AttentionCards ?? null"
        :loading="dashboard.loading"
        :unavailable="managementUnavailable"
      />
    </section>

    <PurchasingSummaryRow
      class="purchasing-dashboard__section"
      :summary="dashboard.purchasing?.Summary ?? null"
      :loading="dashboard.loading && !managementUnavailable"
    />

    <PurchasingAttentionList
      class="purchasing-dashboard__section"
      :items="dashboard.purchasing?.AttentionList ?? []"
      :loading="dashboard.loading"
    />

    <section class="purchasing-dashboard__charts">
      <WeeklyTrendChart
        :weekly-trend="weeklyTrendForChart"
        :loading="dashboard.loading"
        title="Weekly Purchase Trend"
        empty-message="No weekly purchase data for the current period."
      />

      <PostingStatusPieChart
        :items="dashboard.purchasing?.PostingStatusBreakdown ?? []"
        :loading="dashboard.loading"
      />
    </section>

    <section class="purchasing-dashboard__section purchasing-dashboard__rankings">
      <Top10RankingTable
        title="Top 10 Principals"
        :columns="top10Columns"
        :rows="top10Rows as Record<string, unknown>[]"
        :loading="dashboard.loading"
        value-field="MtdPurchaseAmount"
        percent-field="PercentOfPurchase"
        empty-message="No principal ranking data for the current period."
        clickable
        @row-click="onTop10RowClick"
      />

      <PurchasingPrincipalExposureTable
        :items="dashboard.purchasing?.PrincipalExposure ?? []"
        :loading="dashboard.loading"
      />
    </section>

    <PurchasingNavigationSection
      class="purchasing-dashboard__section"
      :navigation="dashboard.purchasing?.Navigation ?? null"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.purchasing-dashboard__banner {
  margin-bottom: 1rem;
}

.purchasing-dashboard__section {
  margin-top: 1.5rem;
}

.purchasing-dashboard__section-title {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.purchasing-dashboard__charts {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1.5rem;
}

.purchasing-dashboard__rankings {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
}

@media (max-width: 1100px) {
  .purchasing-dashboard__charts,
  .purchasing-dashboard__rankings {
    grid-template-columns: 1fr;
  }
}
</style>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import PlatformSnapshotHealthBanners from '@/components/platform/PlatformSnapshotHealthBanners.vue'
import { usePresentationStore } from '@/stores/presentationStore'
import PurchasingAttentionCards from '@/components/dashboard/PurchasingAttentionCards.vue'
import PurchasingSummaryRow from '@/components/dashboard/PurchasingSummaryRow.vue'
import PurchasingAttentionList from '@/components/dashboard/PurchasingAttentionList.vue'
import PurchasingPrincipalExposureTable from '@/components/dashboard/PurchasingPrincipalExposureTable.vue'
import PurchasingNavigationSection from '@/components/dashboard/PurchasingNavigationSection.vue'
import PostingStatusPieChart from '@/components/dashboard/PostingStatusPieChart.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import WeeklyTrendChart from '@/components/dashboard/WeeklyTrendChart.vue'
import type { DashboardPurchasingPrincipalExposureItem, DashboardSalesWeekTrendItem } from '@/models/dashboard'
import { PROFILE_ROW_CLICK_HINT } from '@/navigation/entityAnalyticsNavigation'
import { PURCHASING_ATTENTION_SIGNAL_ALL } from '@/services/purchasingAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const presentation = usePresentationStore()
const router = useRouter()
const sourceLabel = resolveInvestigationSourceLabel('/dashboard/purchasing')
const attentionSignalFilter = ref(PURCHASING_ATTENTION_SIGNAL_ALL)

const managementUnavailable = computed(
  () => dashboard.purchasing != null && !dashboard.purchasing.IsManagementAvailable,
)

const top10Columns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'PrincipalName', header: 'Principal' },
  { field: 'MtdPurchaseAmount', header: 'MTD Purchase' },
  { field: 'PercentOfPurchase', header: '% of Purchase' },
]

const top10Rows = computed(
  () => (dashboard.purchasing?.PrincipalExposure ?? []) as Record<string, unknown>[],
)

const weeklyTrendForChart = computed((): DashboardSalesWeekTrendItem[] =>
  (dashboard.purchasing?.WeeklyTrend ?? []).map((week) => ({
    WeekStart: week.WeekStart,
    WeekEnd: week.WeekEnd,
    WeekLabel: week.WeekLabel,
    RecognizedAmount: week.PurchaseAmount,
  })),
)

const sectionNavItems = [
  { id: 'purchasing-attention-cards', label: 'Attention Cards' },
  { id: 'purchasing-summary', label: 'Summary' },
  { id: 'purchasing-attention-list', label: 'Attention List' },
  { id: 'purchasing-charts', label: 'Charts' },
  { id: 'purchasing-rankings', label: 'Rankings' },
]

function onTop10RowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardPurchasingPrincipalExposureItem
  if (item.ProfileRoute) {
    void router.push(item.ProfileRoute)
    return
  }

  if (!item.Investigation) return
  navigateToInvestigation(router, item.Investigation, sourceLabel)
}

function setAttentionSignalFilter(signalKey: string): void {
  attentionSignalFilter.value = signalKey
}

function onRefresh(): void {
  attentionSignalFilter.value = PURCHASING_ATTENTION_SIGNAL_ALL
  void dashboard.loadPurchasing()
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
    @refresh="onRefresh"
  >
    <PlatformSnapshotHealthBanners
      v-if="dashboard.purchasing"
      :is-data-fresh="dashboard.purchasing.IsDataFresh"
    />

    <Message
      v-if="managementUnavailable && !presentation.hidePlatformDiagnostics"
      severity="info"
      :closable="false"
      class="purchasing-dashboard__banner"
    >
      Management attention data is not yet available. V1 statistics below may still be shown.
    </Message>

    <nav class="purchasing-dashboard__section-nav" aria-label="Dashboard sections">
      <a
        v-for="item in sectionNavItems"
        :key="item.id"
        :href="`#${item.id}`"
        class="purchasing-dashboard__section-nav-link"
      >
        {{ item.label }}
      </a>
    </nav>

    <section id="purchasing-attention-cards" class="purchasing-dashboard__section">
      <h2 class="purchasing-dashboard__section-title">Purchasing Attention Cards</h2>
      <PurchasingAttentionCards
        :cards="dashboard.purchasing?.AttentionCards ?? null"
        :loading="dashboard.loading"
        :unavailable="managementUnavailable"
        @filter-by-signal="setAttentionSignalFilter"
      />
    </section>

    <PurchasingSummaryRow
      id="purchasing-summary"
      class="purchasing-dashboard__section"
      :summary="dashboard.purchasing?.Summary ?? null"
      :loading="dashboard.loading && !managementUnavailable"
    />

    <PurchasingAttentionList
      id="purchasing-attention-list"
      v-model:signal-filter="attentionSignalFilter"
      class="purchasing-dashboard__section"
      :items="dashboard.purchasing?.AttentionList ?? []"
      :loading="dashboard.loading"
    />

    <section id="purchasing-charts" class="purchasing-dashboard__charts">
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

    <section id="purchasing-rankings" class="purchasing-dashboard__section purchasing-dashboard__rankings">
      <Top10RankingTable
        title="Top 10 Principals"
        :columns="top10Columns"
        :rows="top10Rows as Record<string, unknown>[]"
        :loading="dashboard.loading"
        value-field="MtdPurchaseAmount"
        percent-field="PercentOfPurchase"
        empty-message="No principal ranking data for the current period."
        :click-hint="PROFILE_ROW_CLICK_HINT"
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

.purchasing-dashboard__section-nav {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem 1rem;
  position: sticky;
  top: 0;
  z-index: 2;
  margin-bottom: 1.5rem;
  padding: 0.75rem 0;
  background: var(--p-content-background);
  border-bottom: 1px solid var(--p-content-border-color);
}

.purchasing-dashboard__section-nav-link {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
}

.purchasing-dashboard__section-nav-link:hover {
  text-decoration: underline;
}

.purchasing-dashboard__section {
  margin-top: 1.5rem;
  scroll-margin-top: 3.5rem;
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
  scroll-margin-top: 3.5rem;
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

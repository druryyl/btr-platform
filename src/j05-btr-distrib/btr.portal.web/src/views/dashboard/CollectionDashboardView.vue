<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import CollectionAttentionCardGroup from '@/components/dashboard/CollectionAttentionCardGroup.vue'
import CollectionRecoverySummary from '@/components/dashboard/CollectionRecoverySummary.vue'
import CollectionAgingRiskSummary from '@/components/dashboard/CollectionAgingRiskSummary.vue'
import CollectionAttentionList from '@/components/dashboard/CollectionAttentionList.vue'
import CollectionNavigationSection from '@/components/dashboard/CollectionNavigationSection.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import { navigateToReport } from '@/services/navigateToReport'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()

const cards = computed(() => dashboard.collection?.AttentionCards)
const unavailable = computed(() => dashboard.collection != null && !dashboard.collection.IsAvailable)

const customerRankingRows = computed(() =>
  (dashboard.collection?.TopOverdueCustomers ?? []).map((row) => ({
    Rank: row.Rank,
    EntityCode: row.EntityCode,
    EntityName: row.EntityName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
    ReportRoute: row.ReportRoute,
  })),
)

const salesmanRankingRows = computed(() =>
  (dashboard.collection?.TopOverdueSalesmen ?? []).map((row) => ({
    Rank: row.Rank,
    EntityCode: row.EntityCode,
    EntityName: row.EntityName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
    ReportRoute: row.ReportRoute,
  })),
)

const wilayahRankingRows = computed(() =>
  (dashboard.collection?.TopOverdueWilayah ?? []).map((row) => ({
    Rank: row.Rank,
    EntityCode: row.EntityCode,
    EntityName: row.EntityName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
  })),
)

const rankingColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'EntityCode', header: 'Code' },
  { field: 'EntityName', header: 'Name' },
  { field: 'Amount', header: 'Overdue' },
  { field: 'PercentOfTotal', header: '% of Total Overdue' },
]

function onRankingRowClick(row: Record<string, unknown>): void {
  const entityName = String(row.EntityName ?? '')
  const reportRoute = String(row.ReportRoute ?? '')
  if (entityName && reportRoute) {
    navigateToReport(router, reportRoute, entityName)
  }
}

onMounted(() => {
  void dashboard.loadCollection()
})
</script>

<template>
  <DashboardDetailLayout
    title="Collection Dashboard"
    subtitle="Current month recovery + open overdue exposure. Dashboard uses all open balances; Piutang Report may default to a period filter."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.collection?.GeneratedAt"
    @refresh="dashboard.loadCollection()"
  >
    <Message
      v-if="dashboard.collection && !dashboard.collection.IsDataFresh"
      severity="warn"
      :closable="false"
      class="collection-dashboard__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <section class="collection-dashboard__section">
      <h2 class="collection-dashboard__section-title">Collection Attention Cards</h2>
      <div class="collection-dashboard__cards">
        <CollectionAttentionCardGroup
          title="Exposure"
          icon="pi pi-exclamation-circle"
          :loading="dashboard.loading"
          :requires-attention="cards?.ExposureRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Overdue Exposure</span>
            <span class="metric__value">
              {{ cards ? formatCurrency(cards.OverdueExposure) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">&gt;90d Exposure</span>
            <span class="metric__value">
              {{ cards ? formatCurrency(cards.AgingOver90Exposure) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Overdue Concentration %</span>
            <span class="metric__value">
              {{ cards?.OverdueConcentrationPercent != null ? formatPercent(cards.OverdueConcentrationPercent) : '—' }}
            </span>
          </div>
        </CollectionAttentionCardGroup>

        <CollectionAttentionCardGroup
          title="Recovery"
          icon="pi pi-wallet"
          :loading="dashboard.loading"
          :requires-attention="cards?.RecoveryRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Cash Collected MTD</span>
            <span class="metric__value">
              {{ cards ? formatCurrency(cards.CashCollectedMtd) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Recovery vs Billing %</span>
            <span class="metric__value">
              {{ cards?.RecoveryVsBillingPercent != null ? formatPercent(cards.RecoveryVsBillingPercent) : '—' }}
            </span>
          </div>
        </CollectionAttentionCardGroup>

        <CollectionAttentionCardGroup
          title="Portfolio"
          icon="pi pi-clock"
          :loading="dashboard.loading"
          :requires-attention="cards?.PortfolioRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Legacy Debt Count</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.LegacyDebtCount) : '—' }}
            </span>
          </div>
        </CollectionAttentionCardGroup>
      </div>
    </section>

    <CollectionRecoverySummary
      class="collection-dashboard__section"
      :summary="dashboard.collection?.RecoverySummary ?? null"
      :loading="dashboard.loading"
    />

    <CollectionAgingRiskSummary
      class="collection-dashboard__section"
      :buckets="dashboard.collection?.AgingRiskSummary ?? []"
      :loading="dashboard.loading"
    />

    <CollectionAttentionList
      class="collection-dashboard__section"
      :items="dashboard.collection?.AttentionList ?? []"
      :loading="dashboard.loading"
    />

    <section class="collection-dashboard__section">
      <h2 class="collection-dashboard__section-title">Top Overdue Customers</h2>
      <Top10RankingTable
        title="Top 10 Overdue Customers"
        :columns="rankingColumns"
        :rows="customerRankingRows"
        :loading="dashboard.loading"
        value-field="Amount"
        percent-field="PercentOfTotal"
        empty-message="No overdue customer ranking data."
        clickable
        @row-click="onRankingRowClick"
      />
    </section>

    <section class="collection-dashboard__section">
      <h2 class="collection-dashboard__section-title">Top Overdue Salesmen</h2>
      <Top10RankingTable
        title="Top 10 Overdue Salesmen"
        :columns="rankingColumns"
        :rows="salesmanRankingRows"
        :loading="dashboard.loading"
        value-field="Amount"
        percent-field="PercentOfTotal"
        empty-message="No overdue salesman ranking data."
        clickable
        @row-click="onRankingRowClick"
      />
    </section>

    <section class="collection-dashboard__section">
      <h2 class="collection-dashboard__section-title">Top Overdue Wilayah</h2>
      <Top10RankingTable
        title="Top 10 Overdue Wilayah"
        :columns="rankingColumns"
        :rows="wilayahRankingRows"
        :loading="dashboard.loading"
        value-field="Amount"
        percent-field="PercentOfTotal"
        empty-message="No overdue wilayah ranking data."
      />
    </section>

    <CollectionNavigationSection
      class="collection-dashboard__section"
      :navigation="dashboard.collection?.Navigation ?? null"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.collection-dashboard__banner {
  margin-bottom: 1rem;
}

.collection-dashboard__section {
  margin-bottom: 1.5rem;
}

.collection-dashboard__section-title {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.collection-dashboard__cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}

.metric__label {
  display: block;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.metric__value {
  display: block;
  font-size: 1.25rem;
  font-weight: 700;
  margin-top: 0.25rem;
}
</style>

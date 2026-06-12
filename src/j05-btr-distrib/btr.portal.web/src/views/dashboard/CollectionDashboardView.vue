<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import PlatformSnapshotHealthBanners from '@/components/platform/PlatformSnapshotHealthBanners.vue'
import CollectionAttentionCardGroup from '@/components/dashboard/CollectionAttentionCardGroup.vue'
import CollectionRecoverySummary from '@/components/dashboard/CollectionRecoverySummary.vue'
import CollectionAgingRiskSummary from '@/components/dashboard/CollectionAgingRiskSummary.vue'
import CollectionAttentionList from '@/components/dashboard/CollectionAttentionList.vue'
import CollectionNavigationSection from '@/components/dashboard/CollectionNavigationSection.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import type { DashboardCollectionRankingRow } from '@/models/dashboard'
import { COLLECTION_ATTENTION_SIGNAL_ALL } from '@/services/collectionAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()
const sourceLabel = resolveInvestigationSourceLabel('/dashboard/collection')
const attentionSignalFilter = ref(COLLECTION_ATTENTION_SIGNAL_ALL)

const cards = computed(() => dashboard.collection?.AttentionCards)
const unavailable = computed(() => dashboard.collection != null && !dashboard.collection.IsAvailable)

const customerRankingRows = computed(
  () => (dashboard.collection?.TopOverdueCustomers ?? []) as Record<string, unknown>[],
)

const salesmanRankingRows = computed(
  () => (dashboard.collection?.TopOverdueSalesmen ?? []) as Record<string, unknown>[],
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

const sectionNavItems = [
  { id: 'collection-attention-cards', label: 'Attention Cards' },
  { id: 'collection-recovery-summary', label: 'Recovery' },
  { id: 'collection-aging-risk', label: 'Aging Risk' },
  { id: 'collection-attention-list', label: 'Attention List' },
  { id: 'collection-rankings', label: 'Rankings' },
]

function onRankingRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardCollectionRankingRow
  if (!item.Investigation) return
  navigateToInvestigation(router, item.Investigation, sourceLabel)
}

function setAttentionSignalFilter(signalKey: string): void {
  attentionSignalFilter.value = signalKey
}

function onRefresh(): void {
  attentionSignalFilter.value = COLLECTION_ATTENTION_SIGNAL_ALL
  void dashboard.loadCollection()
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
    @refresh="onRefresh"
  >
    <PlatformSnapshotHealthBanners
      v-if="dashboard.collection"
      :is-data-fresh="dashboard.collection.IsDataFresh"
    />

    <nav class="collection-dashboard__section-nav" aria-label="Dashboard sections">
      <a
        v-for="item in sectionNavItems"
        :key="item.id"
        :href="`#${item.id}`"
        class="collection-dashboard__section-nav-link"
      >
        {{ item.label }}
      </a>
    </nav>

    <section id="collection-attention-cards" class="collection-dashboard__section">
      <h2 class="collection-dashboard__section-title">Collection Attention Cards</h2>
      <div class="collection-dashboard__cards">
        <CollectionAttentionCardGroup
          title="Exposure"
          icon="pi pi-exclamation-circle"
          href="#collection-attention-list"
          :loading="dashboard.loading"
          :requires-attention="cards?.ExposureRequiresAttention"
          :unavailable="unavailable"
          @anchor-navigate="setAttentionSignalFilter('ChronicOverdue')"
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
          href="#collection-attention-list"
          :loading="dashboard.loading"
          :requires-attention="cards?.RecoveryRequiresAttention"
          :unavailable="unavailable"
          @anchor-navigate="setAttentionSignalFilter('LowRecoveryVsBilling')"
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
          href="#collection-attention-list"
          :loading="dashboard.loading"
          :requires-attention="cards?.PortfolioRequiresAttention"
          :unavailable="unavailable"
          @anchor-navigate="setAttentionSignalFilter('LegacyDebt')"
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

    <div id="collection-recovery-summary" class="collection-dashboard__section">
      <CollectionRecoverySummary
        :summary="dashboard.collection?.RecoverySummary ?? null"
        :loading="dashboard.loading"
      />
    </div>

    <div id="collection-aging-risk" class="collection-dashboard__section">
      <CollectionAgingRiskSummary
        :buckets="dashboard.collection?.AgingRiskSummary ?? []"
        :loading="dashboard.loading"
      />
    </div>

    <CollectionAttentionList
      id="collection-attention-list"
      v-model:signal-filter="attentionSignalFilter"
      class="collection-dashboard__section"
      :items="dashboard.collection?.AttentionList ?? []"
      :loading="dashboard.loading"
    />

    <section id="collection-rankings" class="collection-dashboard__section">
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

      <h2 class="collection-dashboard__section-title collection-dashboard__section-title--spaced">
        Top Overdue Salesmen
      </h2>
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

      <h2 class="collection-dashboard__section-title collection-dashboard__section-title--spaced">
        Top Overdue Wilayah
      </h2>
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

.collection-dashboard__section-nav {
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

.collection-dashboard__section-nav-link {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
}

.collection-dashboard__section-nav-link:hover {
  text-decoration: underline;
}

.collection-dashboard__section {
  margin-bottom: 1.5rem;
  scroll-margin-top: 3.5rem;
}

.collection-dashboard__section-title {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.collection-dashboard__section-title--spaced {
  margin-top: 1.5rem;
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

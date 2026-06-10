<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import Message from 'primevue/message'
import InputSwitch from 'primevue/inputswitch'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import SalesmanAttentionCardGroup from '@/components/dashboard/SalesmanAttentionCardGroup.vue'
import SalesmanAttentionList from '@/components/dashboard/SalesmanAttentionList.vue'
import SalesmanSegmentationSection from '@/components/dashboard/SalesmanSegmentationSection.vue'
import SalesmanNavigationSection from '@/components/dashboard/SalesmanNavigationSection.vue'
import SalesmanDetailDrawer from '@/components/dashboard/SalesmanDetailDrawer.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import type {
  DashboardSalesmanAttentionItem,
  DashboardSalesmanRankingRow,
} from '@/models/dashboard'
import { formatNumber, formatPercent } from '@/services/formatters'
import {
  filterActiveSalesmen,
  SALESMAN_ATTENTION_SIGNAL_ALL,
} from '@/services/salesmanAttentionSignals'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const attentionSignalFilter = ref(SALESMAN_ATTENTION_SIGNAL_ALL)
const showInactiveSalesmen = ref(false)
const drawerVisible = ref(false)
const selectedSalesPersonId = ref<string | null>(null)
const selectedSalesPersonName = ref<string | null>(null)

const cards = computed(() => dashboard.salesman?.AttentionCards)
const filters = computed(() => dashboard.salesman?.Filters)
const unavailable = computed(() => dashboard.salesman != null && !dashboard.salesman.IsAvailable)

const toolbarSubtitle = computed(() => {
  if (showInactiveSalesmen.value) {
    return 'Showing all salesmen including inactive.'
  }

  const exposurePercent = filters.value?.ExposureTopPercent ?? 20
  return `Showing active salesmen only (current-month Faktur). High exposure signals use top ${exposurePercent}% threshold.`
})

const filteredAttentionList = computed(() =>
  filterActiveSalesmen(dashboard.salesman?.AttentionList ?? [], showInactiveSalesmen.value),
)

const omzetRankingRows = computed(() =>
  filterActiveSalesmen(
    (dashboard.salesman?.PerformanceRankings?.TopOmzet ?? []) as DashboardSalesmanRankingRow[],
    showInactiveSalesmen.value,
  ) as Record<string, unknown>[],
)

const achievementRankingRows = computed(() =>
  filterActiveSalesmen(
    (dashboard.salesman?.PerformanceRankings?.TopAchievement ?? []) as DashboardSalesmanRankingRow[],
    showInactiveSalesmen.value,
  ) as Record<string, unknown>[],
)

const piutangRankingRows = computed(() =>
  filterActiveSalesmen(
    (dashboard.salesman?.ExposureRankings?.TopPiutang ?? []) as DashboardSalesmanRankingRow[],
    showInactiveSalesmen.value,
  ) as Record<string, unknown>[],
)

const omzetColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'SalesPersonCode', header: 'Code' },
  { field: 'SalesPersonName', header: 'Salesman' },
  { field: 'Amount', header: 'Omzet' },
  { field: 'PercentOfTotal', header: '% of Total' },
]

const achievementColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'SalesPersonCode', header: 'Code' },
  { field: 'SalesPersonName', header: 'Salesman' },
  { field: 'AchievementPercent', header: 'Achievement %' },
  { field: 'Amount', header: 'Omzet' },
]

const piutangColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'SalesPersonCode', header: 'Code' },
  { field: 'SalesPersonName', header: 'Salesman' },
  { field: 'Amount', header: 'Outstanding' },
  { field: 'PercentOfTotal', header: '% of Total' },
]

const sectionNavItems = [
  { id: 'salesman-attention-cards', label: 'Attention Cards' },
  { id: 'salesman-toolbar', label: 'Filters' },
  { id: 'salesman-attention-list', label: 'Attention List' },
  { id: 'salesman-performance-rankings', label: 'Performance Rankings' },
  { id: 'salesman-exposure-rankings', label: 'Exposure Rankings' },
  { id: 'salesman-segmentation', label: 'Segmentation' },
]

function openSalesmanDetail(salesPersonId: string, salesPersonName: string): void {
  selectedSalesPersonId.value = salesPersonId
  selectedSalesPersonName.value = salesPersonName
  drawerVisible.value = true
}

function onRankingRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardSalesmanRankingRow
  openSalesmanDetail(item.SalesPersonId, item.SalesPersonName)
}

function onAttentionSalesmanClick(item: DashboardSalesmanAttentionItem): void {
  openSalesmanDetail(item.SalesPersonId, item.SalesPersonName)
}

function setAttentionSignalFilter(signalKey: string): void {
  attentionSignalFilter.value = signalKey
}

function onRefresh(): void {
  attentionSignalFilter.value = SALESMAN_ATTENTION_SIGNAL_ALL
  void dashboard.loadSalesman()
}

onMounted(() => {
  void dashboard.loadSalesman()
})
</script>

<template>
  <DashboardDetailLayout
    title="Salesman Performance"
    subtitle="Which salesman requires management attention? Current month sales + open piutang by salesman."
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.salesman?.GeneratedAt"
    @refresh="onRefresh"
  >
    <Message
      v-if="dashboard.salesman && !dashboard.salesman.IsDataFresh"
      severity="warn"
      :closable="false"
      class="salesman-dashboard__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <nav class="salesman-dashboard__section-nav" aria-label="Dashboard sections">
      <a
        v-for="item in sectionNavItems"
        :key="item.id"
        :href="`#${item.id}`"
        class="salesman-dashboard__section-nav-link"
      >
        {{ item.label }}
      </a>
    </nav>

    <section id="salesman-attention-cards" class="salesman-dashboard__section">
      <h2 class="salesman-dashboard__section-title">Attention Cards</h2>
      <div class="salesman-dashboard__cards">
        <SalesmanAttentionCardGroup
          title="Performance"
          icon="pi pi-chart-line"
          to="/dashboard/sales"
          :loading="dashboard.loading"
          :requires-attention="cards?.PerformanceRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">Below Target</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.BelowTargetCount) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Missing Target Setup</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.MissingTargetSetupCount) : '—' }}
            </span>
          </div>
        </SalesmanAttentionCardGroup>

        <SalesmanAttentionCardGroup
          title="Collection Exposure"
          icon="pi pi-wallet"
          to="/dashboard/piutang"
          :loading="dashboard.loading"
          :requires-attention="cards?.CollectionRequiresAttention"
          :unavailable="unavailable"
        >
          <div class="metric">
            <span class="metric__label">High Overdue Exposure</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.HighOverdueExposureCount) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">High Piutang Exposure</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.HighPiutangExposureCount) : '—' }}
            </span>
          </div>
        </SalesmanAttentionCardGroup>

        <SalesmanAttentionCardGroup
          title="Portfolio"
          icon="pi pi-briefcase"
          href="#salesman-attention-list"
          :loading="dashboard.loading"
          :requires-attention="cards?.PortfolioRequiresAttention"
          :unavailable="unavailable"
          @anchor-navigate="setAttentionSignalFilter('DormantCustomerPortfolio')"
        >
          <div class="metric">
            <span class="metric__label">Dormant Portfolio</span>
            <span class="metric__value">
              {{ cards ? formatNumber(cards.DormantPortfolioCount) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Omzet Salesman %</span>
            <span class="metric__value">
              {{ cards?.TopOmzetSalesmanPercent != null ? formatPercent(cards.TopOmzetSalesmanPercent) : '—' }}
            </span>
          </div>
          <div class="metric">
            <span class="metric__label">Top Piutang Salesman %</span>
            <span class="metric__value">
              {{ cards?.TopPiutangSalesmanPercent != null ? formatPercent(cards.TopPiutangSalesmanPercent) : '—' }}
            </span>
          </div>
        </SalesmanAttentionCardGroup>
      </div>
    </section>

    <section id="salesman-toolbar" class="salesman-dashboard__section salesman-dashboard__toolbar">
      <div class="salesman-dashboard__toolbar-row">
        <label class="salesman-dashboard__toggle">
          <InputSwitch v-model="showInactiveSalesmen" />
          <span>Show Inactive Salesmen</span>
        </label>
      </div>
      <p class="salesman-dashboard__toolbar-subtitle">{{ toolbarSubtitle }}</p>
    </section>

    <section id="salesman-attention-list" class="salesman-dashboard__section">
      <SalesmanAttentionList
        v-model:signal-filter="attentionSignalFilter"
        :items="filteredAttentionList"
        :loading="dashboard.loading"
        @salesman-click="onAttentionSalesmanClick"
      />
    </section>

    <section id="salesman-performance-rankings" class="salesman-dashboard__section">
      <h2 class="salesman-dashboard__section-title">Performance Rankings</h2>
      <div class="salesman-dashboard__rankings">
        <Top10RankingTable
          title="Top 10 Omzet (current month)"
          :columns="omzetColumns"
          :rows="omzetRankingRows"
          :loading="dashboard.loading"
          value-field="Amount"
          percent-field="PercentOfTotal"
          empty-message="No omzet ranking data."
          clickable
          @row-click="onRankingRowClick"
        />
        <Top10RankingTable
          title="Top 10 Achievement %"
          :columns="achievementColumns"
          :rows="achievementRankingRows"
          :loading="dashboard.loading"
          value-field="Amount"
          empty-message="No achievement ranking data."
          clickable
          @row-click="onRankingRowClick"
        />
      </div>
    </section>

    <section id="salesman-exposure-rankings" class="salesman-dashboard__section">
      <h2 class="salesman-dashboard__section-title">Exposure Rankings</h2>
      <Top10RankingTable
        title="Top 10 Piutang (all open)"
        :columns="piutangColumns"
        :rows="piutangRankingRows"
        :loading="dashboard.loading"
        value-field="Amount"
        percent-field="PercentOfTotal"
        empty-message="No piutang ranking data."
        clickable
        @row-click="onRankingRowClick"
      />
    </section>

    <SalesmanSegmentationSection
      id="salesman-segmentation"
      class="salesman-dashboard__section"
      :by-wilayah="dashboard.salesman?.Segmentation?.ByWilayah ?? []"
      :by-segment="dashboard.salesman?.Segmentation?.BySegment ?? []"
      :active-summary="dashboard.salesman?.Segmentation?.ActiveSummary ?? null"
      :inactive-summary="dashboard.salesman?.Segmentation?.InactiveSummary ?? null"
      :loading="dashboard.loading"
    />

    <SalesmanNavigationSection
      class="salesman-dashboard__section"
      :navigation="dashboard.salesman?.Navigation ?? null"
    />

    <SalesmanDetailDrawer
      v-model:visible="drawerVisible"
      :sales-person-id="selectedSalesPersonId"
      :sales-person-name="selectedSalesPersonName"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.salesman-dashboard__banner {
  margin-bottom: 1rem;
}

.salesman-dashboard__section-nav {
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

.salesman-dashboard__section-nav-link {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
}

.salesman-dashboard__section-nav-link:hover {
  text-decoration: underline;
}

.salesman-dashboard__section {
  margin-bottom: 1.5rem;
  scroll-margin-top: 3.5rem;
}

.salesman-dashboard__section-title {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.salesman-dashboard__cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 1rem;
}

.salesman-dashboard__rankings {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 1rem;
}

.salesman-dashboard__toolbar-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 1rem;
}

.salesman-dashboard__toggle {
  display: inline-flex;
  align-items: center;
  gap: 0.75rem;
  font-weight: 600;
}

.salesman-dashboard__toolbar-subtitle {
  margin: 0.75rem 0 0;
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
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

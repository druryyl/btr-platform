<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import PlatformSnapshotHealthBanners from '@/components/platform/PlatformSnapshotHealthBanners.vue'
import { usePresentationStore } from '@/stores/presentationStore'
import ExecutiveAttentionCard from '@/components/dashboard/ExecutiveAttentionCard.vue'
import KpiCard from '@/components/KpiCard.vue'
import AgingPieChart from '@/components/dashboard/AgingPieChart.vue'
import InventoryHorizontalBarChart from '@/components/dashboard/InventoryHorizontalBarChart.vue'
import InventoryRiskAttentionList from '@/components/dashboard/InventoryRiskAttentionList.vue'
import InventoryRiskNavigationSection from '@/components/dashboard/InventoryRiskNavigationSection.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import { PROFILE_ROW_CLICK_HINT } from '@/navigation/entityAnalyticsNavigation'
import { INVENTORY_RISK_ATTENTION_SIGNAL_ALL } from '@/services/inventoryRiskAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'
import { useDashboardStore } from '@/stores/dashboardStore'
import type { DashboardInventoryBreakdownItem, DashboardInventoryRiskRankingRow } from '@/models/dashboard'

const dashboard = useDashboardStore()
const presentation = usePresentationStore()
const router = useRouter()
const sourceLabel = resolveInvestigationSourceLabel('/dashboard/inventory-risk')
const attentionSignalFilter = ref(INVENTORY_RISK_ATTENTION_SIGNAL_ALL)

const cards = computed(() => dashboard.inventoryRisk?.AttentionCards)
const unavailable = computed(() => dashboard.inventoryRisk != null && !dashboard.inventoryRisk.IsAvailable)

const agingBuckets = computed(() =>
  (dashboard.inventoryRisk?.AgingBuckets ?? []).map((bucket) => ({
    BucketKey: bucket.BucketKey,
    BucketLabel: bucket.BucketLabel,
    Amount: bucket.Amount,
    SortOrder: bucket.SortOrder,
  })),
)

const categoryRiskItems = computed<DashboardInventoryBreakdownItem[]>(() =>
  (dashboard.inventoryRisk?.CategoryRiskExposure ?? []).map((item) => ({
    Name: item.Name,
    InventoryValue: item.AtRiskValue,
  })),
)

const supplierRiskItems = computed<DashboardInventoryBreakdownItem[]>(() =>
  (dashboard.inventoryRisk?.SupplierRiskExposure ?? []).map((item) => ({
    Name: item.Name,
    InventoryValue: item.AtRiskValue,
  })),
)

const deadRankingRows = computed(
  () => (dashboard.inventoryRisk?.Rankings?.TopDead ?? []) as Record<string, unknown>[],
)

const slowRankingRows = computed(
  () => (dashboard.inventoryRisk?.Rankings?.TopSlow ?? []) as Record<string, unknown>[],
)

const deadColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'BrgCode', header: 'Code' },
  { field: 'BrgName', header: 'Item' },
  { field: 'InventoryValue', header: 'Value' },
  { field: 'DaysSinceLastFaktur', header: 'Days Idle' },
  { field: 'PercentOfAtRisk', header: '% of Dead Stock' },
]

const slowColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'BrgCode', header: 'Code' },
  { field: 'BrgName', header: 'Item' },
  { field: 'InventoryValue', header: 'Value' },
  { field: 'DaysSinceLastFaktur', header: 'Days Idle' },
  { field: 'PercentOfAtRisk', header: '% of Slow Moving' },
]

const sectionNavItems = [
  { id: 'inventory-risk-attention-cards', label: 'Attention Cards' },
  { id: 'inventory-risk-charts', label: 'Risk Exposure' },
  { id: 'inventory-risk-attention-list', label: 'Attention List' },
  { id: 'inventory-risk-rankings', label: 'Rankings' },
]

function onRankingRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardInventoryRiskRankingRow
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

function navigateToAttentionList(signalKey: string): void {
  setAttentionSignalFilter(signalKey)
  document.getElementById('inventory-risk-attention-list')?.scrollIntoView({ behavior: 'smooth' })
}

function onRefresh(): void {
  attentionSignalFilter.value = INVENTORY_RISK_ATTENTION_SIGNAL_ALL
  void dashboard.loadInventoryRisk()
}

onMounted(() => {
  void dashboard.loadInventoryRisk()
})
</script>

<template>
  <DashboardDetailLayout
    title="Slow Moving & Dead Stock Dashboard"
    subtitle="Inventory health — items requiring management attention"
    :loading="dashboard.loading"
    :error="dashboard.error"
    :generated-at="dashboard.inventoryRisk?.GeneratedAt"
    @refresh="onRefresh"
  >
    <PlatformSnapshotHealthBanners
      v-if="dashboard.inventoryRisk"
      :is-data-fresh="dashboard.inventoryRisk.IsDataFresh"
    />

    <Message
      v-if="unavailable && !presentation.hidePlatformDiagnostics"
      severity="info"
      :closable="false"
      class="inventory-risk-dashboard__banner"
    >
      Inventory risk data is not yet available. Run the snapshot refresh worker for the InventoryRisk domain.
    </Message>

    <nav
      v-if="!unavailable"
      class="inventory-risk-dashboard__section-nav"
      aria-label="Dashboard sections"
    >
      <a
        v-for="item in sectionNavItems"
        :key="item.id"
        :href="`#${item.id}`"
        class="inventory-risk-dashboard__section-nav-link"
      >
        {{ item.label }}
      </a>
    </nav>

    <section
      v-if="!unavailable"
      id="inventory-risk-attention-cards"
      class="inventory-risk-dashboard__section"
    >
      <h2 class="inventory-risk-dashboard__section-title">Inventory Attention Cards</h2>
      <div class="inventory-risk-dashboard__kpi-row">
        <KpiCard
          title="Dead Stock Item Count"
          icon="pi pi-box"
          :loading="dashboard.loading"
          class="inventory-risk-dashboard__kpi-card--clickable"
          @click="navigateToAttentionList('DeadStock')"
        >
          <span class="metric__value">
            {{ cards ? formatNumber(cards.DeadStockItemCount) : '—' }}
          </span>
        </KpiCard>
        <KpiCard
          title="Dead Stock Value"
          icon="pi pi-wallet"
          :loading="dashboard.loading"
          class="inventory-risk-dashboard__kpi-card--clickable"
          @click="navigateToAttentionList('DeadStock')"
        >
          <span class="metric__value">
            {{ cards ? formatCurrency(cards.DeadStockValue) : '—' }}
          </span>
        </KpiCard>
        <KpiCard
          title="Slow Moving Item Count"
          icon="pi pi-clock"
          :loading="dashboard.loading"
          class="inventory-risk-dashboard__kpi-card--clickable"
          @click="navigateToAttentionList('SlowMoving')"
        >
          <span class="metric__value">
            {{ cards ? formatNumber(cards.SlowMovingItemCount) : '—' }}
          </span>
        </KpiCard>
        <KpiCard
          title="Slow Moving Value"
          icon="pi pi-chart-line"
          :loading="dashboard.loading"
          class="inventory-risk-dashboard__kpi-card--clickable"
          @click="navigateToAttentionList('SlowMoving')"
        >
          <span class="metric__value">
            {{ cards ? formatCurrency(cards.SlowMovingValue) : '—' }}
          </span>
        </KpiCard>
        <KpiCard
          title="At-Risk Inventory %"
          icon="pi pi-percentage"
          :loading="dashboard.loading"
        >
          <span class="metric__value">
            {{ cards ? formatPercent(cards.AtRiskInventoryPercent) : '—' }}
          </span>
        </KpiCard>
      </div>

      <ExecutiveAttentionCard
        v-if="cards?.RequiresAttention"
        title="Inventory Risk"
        icon="pi pi-exclamation-triangle"
        route="/dashboard/inventory-risk"
        :loading="dashboard.loading"
        :requires-attention="cards.RequiresAttention"
        :unavailable="unavailable"
        class="inventory-risk-dashboard__attention-card"
      >
        <div class="metric">
          <span class="metric__label">At-Risk Inventory</span>
          <span class="metric__value">
            {{ formatPercent(cards.AtRiskInventoryPercent) }}
          </span>
        </div>
      </ExecutiveAttentionCard>
    </section>

    <div
      v-if="!unavailable"
      id="inventory-risk-charts"
      class="inventory-risk-dashboard__charts-row inventory-risk-dashboard__section"
    >
      <AgingPieChart
        title="Inventory Aging Distribution"
        empty-message="No inventory aging data."
        :buckets="agingBuckets"
        :loading="dashboard.loading"
      />
      <InventoryHorizontalBarChart
        title="Category Risk Exposure"
        :items="categoryRiskItems"
        :loading="dashboard.loading"
      />
    </div>

    <InventoryHorizontalBarChart
      v-if="!unavailable"
      class="inventory-risk-dashboard__section"
      title="Supplier Risk Exposure"
      :items="supplierRiskItems"
      :loading="dashboard.loading"
    />

    <section
      v-if="!unavailable"
      id="inventory-risk-attention-list"
      class="inventory-risk-dashboard__section"
    >
      <InventoryRiskAttentionList
        v-model:signal-filter="attentionSignalFilter"
        :items="dashboard.inventoryRisk?.AttentionList ?? []"
        :loading="dashboard.loading"
      />
    </section>

    <section
      v-if="!unavailable"
      id="inventory-risk-rankings"
      class="inventory-risk-dashboard__section"
    >
      <h2 class="inventory-risk-dashboard__section-title">Top 10 Rankings</h2>
      <div class="inventory-risk-dashboard__rankings">
        <Top10RankingTable
          title="Top 10 Dead Stock by Value"
          :columns="deadColumns"
          :rows="deadRankingRows"
          :loading="dashboard.loading"
          value-field="InventoryValue"
          percent-field="PercentOfAtRisk"
          empty-message="No dead stock ranking data."
          :click-hint="PROFILE_ROW_CLICK_HINT"
          clickable
          @row-click="onRankingRowClick"
        />
        <Top10RankingTable
          title="Top 10 Slow Moving by Value"
          :columns="slowColumns"
          :rows="slowRankingRows"
          :loading="dashboard.loading"
          value-field="InventoryValue"
          percent-field="PercentOfAtRisk"
          empty-message="No slow moving ranking data."
          :click-hint="PROFILE_ROW_CLICK_HINT"
          clickable
          @row-click="onRankingRowClick"
        />
      </div>
    </section>

    <InventoryRiskNavigationSection
      class="inventory-risk-dashboard__section"
      :navigation="dashboard.inventoryRisk?.Navigation ?? null"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.inventory-risk-dashboard__banner {
  margin-bottom: 1rem;
}

.inventory-risk-dashboard__section-nav {
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

.inventory-risk-dashboard__section-nav-link {
  font-size: 0.875rem;
  font-weight: 600;
  color: var(--p-primary-color);
  text-decoration: none;
}

.inventory-risk-dashboard__section-nav-link:hover {
  text-decoration: underline;
}

.inventory-risk-dashboard__section {
  margin-bottom: 1.5rem;
  scroll-margin-top: 3.5rem;
}

.inventory-risk-dashboard__section-title {
  margin: 0 0 1rem;
  font-size: 1.25rem;
}

.inventory-risk-dashboard__kpi-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(180px, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
}

.inventory-risk-dashboard__kpi-card--clickable {
  cursor: pointer;
}

.inventory-risk-dashboard__attention-card {
  max-width: 320px;
}

.inventory-risk-dashboard__charts-row {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
  gap: 1rem;
}

.inventory-risk-dashboard__rankings {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(320px, 1fr));
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

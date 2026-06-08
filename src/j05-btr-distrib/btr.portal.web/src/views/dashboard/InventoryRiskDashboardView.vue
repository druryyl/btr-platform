<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import Message from 'primevue/message'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import ExecutiveAttentionCard from '@/components/dashboard/ExecutiveAttentionCard.vue'
import KpiCard from '@/components/KpiCard.vue'
import AgingPieChart from '@/components/dashboard/AgingPieChart.vue'
import InventoryHorizontalBarChart from '@/components/dashboard/InventoryHorizontalBarChart.vue'
import InventoryRiskAttentionList from '@/components/dashboard/InventoryRiskAttentionList.vue'
import InventoryRiskNavigationSection from '@/components/dashboard/InventoryRiskNavigationSection.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import { navigateToReport } from '@/services/navigateToReport'
import { useDashboardStore } from '@/stores/dashboardStore'
import type { DashboardInventoryBreakdownItem } from '@/models/dashboard'

const dashboard = useDashboardStore()
const router = useRouter()

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

const deadRankingRows = computed(() =>
  (dashboard.inventoryRisk?.Rankings?.TopDead ?? []).map((row) => ({
    Rank: row.Rank,
    BrgCode: row.BrgCode,
    BrgName: row.BrgName,
    InventoryValue: row.InventoryValue,
    DaysSinceLastFaktur: row.DaysSinceLastFaktur,
    PercentOfAtRisk: row.PercentOfAtRisk,
    ReportRoute: row.ReportRoute,
  })),
)

const slowRankingRows = computed(() =>
  (dashboard.inventoryRisk?.Rankings?.TopSlow ?? []).map((row) => ({
    Rank: row.Rank,
    BrgCode: row.BrgCode,
    BrgName: row.BrgName,
    InventoryValue: row.InventoryValue,
    DaysSinceLastFaktur: row.DaysSinceLastFaktur,
    PercentOfAtRisk: row.PercentOfAtRisk,
    ReportRoute: row.ReportRoute,
  })),
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

function onRankingRowClick(row: Record<string, unknown>): void {
  const brgName = String(row.BrgName ?? '')
  const reportRoute = String(row.ReportRoute ?? '/reports/inventory')
  if (brgName) {
    navigateToReport(router, reportRoute, brgName)
  }
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
    @refresh="dashboard.loadInventoryRisk()"
  >
    <Message
      v-if="dashboard.inventoryRisk && !dashboard.inventoryRisk.IsDataFresh"
      severity="warn"
      :closable="false"
      class="inventory-risk-dashboard__banner"
    >
      ⚠ Dashboard Data Not Fresh
    </Message>

    <Message
      v-if="unavailable"
      severity="info"
      :closable="false"
      class="inventory-risk-dashboard__banner"
    >
      Inventory risk data is not yet available. Run the snapshot refresh worker for the InventoryRisk domain.
    </Message>

    <section v-if="!unavailable" class="inventory-risk-dashboard__section">
      <h2 class="inventory-risk-dashboard__section-title">Inventory Attention Cards</h2>
      <div class="inventory-risk-dashboard__kpi-row">
        <KpiCard
          title="Dead Stock Item Count"
          icon="pi pi-box"
          :loading="dashboard.loading"
        >
          <span class="metric__value">
            {{ cards ? formatNumber(cards.DeadStockItemCount) : '—' }}
          </span>
        </KpiCard>
        <KpiCard
          title="Dead Stock Value"
          icon="pi pi-wallet"
          :loading="dashboard.loading"
        >
          <span class="metric__value">
            {{ cards ? formatCurrency(cards.DeadStockValue) : '—' }}
          </span>
        </KpiCard>
        <KpiCard
          title="Slow Moving Item Count"
          icon="pi pi-clock"
          :loading="dashboard.loading"
        >
          <span class="metric__value">
            {{ cards ? formatNumber(cards.SlowMovingItemCount) : '—' }}
          </span>
        </KpiCard>
        <KpiCard
          title="Slow Moving Value"
          icon="pi pi-chart-line"
          :loading="dashboard.loading"
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

    <div v-if="!unavailable" class="inventory-risk-dashboard__charts-row">
      <AgingPieChart
        class="inventory-risk-dashboard__section"
        title="Inventory Aging Distribution"
        empty-message="No inventory aging data."
        :buckets="agingBuckets"
        :loading="dashboard.loading"
      />
      <InventoryHorizontalBarChart
        class="inventory-risk-dashboard__section"
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

    <InventoryRiskAttentionList
      v-if="!unavailable"
      id="inventory-risk-attention-list"
      class="inventory-risk-dashboard__section"
      :items="dashboard.inventoryRisk?.AttentionList ?? []"
      :loading="dashboard.loading"
    />

    <section v-if="!unavailable" class="inventory-risk-dashboard__section">
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

.inventory-risk-dashboard__section {
  margin-bottom: 1.5rem;
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

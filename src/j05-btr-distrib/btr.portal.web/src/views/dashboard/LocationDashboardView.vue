<script setup lang="ts">

import { computed, onMounted } from 'vue'

import { useRouter } from 'vue-router'

import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import DashboardMetric from '@/components/dashboard/primitives/DashboardMetric.vue'
import PlatformSnapshotHealthBanners from '@/components/platform/PlatformSnapshotHealthBanners.vue'

import LocationAttentionCardGroup from '@/components/dashboard/LocationAttentionCardGroup.vue'

import LocationAttentionList from '@/components/dashboard/LocationAttentionList.vue'

import LocationNavigationSection from '@/components/dashboard/LocationNavigationSection.vue'

import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'

import { formatNumber, formatPercent } from '@/services/formatters'

import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToDashboard, navigateToInvestigation } from '@/services/navigateToInvestigation'

import type { DashboardLocationRankingRow } from '@/models/dashboard'
import { useDashboardStore } from '@/stores/dashboardStore'



const dashboard = useDashboardStore()

const router = useRouter()

const sourceLabel = resolveInvestigationSourceLabel('/dashboard/locations')

const cards = computed(() => dashboard.location?.AttentionCards)

const unavailable = computed(() => dashboard.location != null && !dashboard.location.IsAvailable)



const warehouseRankingColumns = [

  { field: 'Rank', header: 'Rank' },

  { field: 'EntityCode', header: 'Code' },

  { field: 'EntityName', header: 'Warehouse' },

  { field: 'Amount', header: 'Amount' },

  { field: 'PercentOfTotal', header: '% of Total' },

]



const wilayahRankingColumns = [

  { field: 'Rank', header: 'Rank' },

  { field: 'EntityName', header: 'Wilayah' },

  { field: 'Amount', header: 'MTD Omzet' },

  { field: 'PercentOfTotal', header: '% of Total' },

]



function mapWarehouseRows(rows: DashboardLocationRankingRow[] | undefined) {
  return (rows ?? []) as unknown as Record<string, unknown>[]
}



const inventoryRankingRows = computed(() => mapWarehouseRows(dashboard.location?.TopWarehouseInventory))

const atRiskRankingRows = computed(() => mapWarehouseRows(dashboard.location?.TopWarehouseAtRisk))

const salesRankingRows = computed(() => mapWarehouseRows(dashboard.location?.TopWarehouseSales))

const purchasingRankingRows = computed(() => mapWarehouseRows(dashboard.location?.TopWarehousePurchasing))



const wilayahRankingRows = computed(() =>

  (dashboard.location?.TopWilayahSales ?? []).map((row) => ({

    Rank: row.Rank,

    EntityName: row.EntityName,

    Amount: row.Amount,

    PercentOfTotal: row.PercentOfTotal,

    DashboardRoute: row.DashboardRoute,

  })),

)



function onInventoryRankingRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardLocationRankingRow
  if (!item.Investigation) return
  navigateToInvestigation(router, item.Investigation, sourceLabel)
}

function onAtRiskRankingRowClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardLocationRankingRow
  const dashboardRoute = item.Investigation?.DashboardRoute ?? '/dashboard/inventory-risk'
  navigateToDashboard(router, dashboardRoute)
}



function onWilayahRankingRowClick(): void {

  void router.push('/dashboard/collection')

}



onMounted(() => {

  void dashboard.loadLocation()

})

</script>



<template>

  <DashboardDetailLayout

    title="Branch / Warehouse Performance Dashboard"

    subtitle="Location concentration across warehouses (inventory, sales, purchasing) and territories (sales contribution). Receivable risk by wilayah — see Collection Dashboard."

    :loading="dashboard.loading"

    :error="dashboard.error"

    :generated-at="dashboard.location?.GeneratedAt"

    @refresh="dashboard.loadLocation()"

  >

    <PlatformSnapshotHealthBanners
      v-if="dashboard.location"
      :is-data-fresh="dashboard.location.IsDataFresh"
    />

    <section class="location-dashboard__section">

      <h2 class="location-dashboard__section-title">Location Attention Cards</h2>

      <div class="location-dashboard__cards">

        <LocationAttentionCardGroup
          title="Inventory Concentration"
          icon="pi pi-box"
          domain="inventory"
          :loading="dashboard.loading"
          :unavailable="unavailable"
        >
          <DashboardMetric
            label="Top Warehouse Inventory %"
            :value="cards?.Top1WarehouseInventoryPercent != null ? formatPercent(cards.Top1WarehouseInventoryPercent) : '—'"
            :empty="cards?.Top1WarehouseInventoryPercent == null"
          />
          <DashboardMetric
            label="Top 3 Warehouse Inventory %"
            :value="cards?.Top3WarehouseInventoryPercent != null ? formatPercent(cards.Top3WarehouseInventoryPercent) : '—'"
            :empty="cards?.Top3WarehouseInventoryPercent == null"
          />
        </LocationAttentionCardGroup>

        <LocationAttentionCardGroup
          title="At-Risk Concentration"
          icon="pi pi-exclamation-triangle"
          domain="alert"
          :loading="dashboard.loading"
          :unavailable="unavailable"
        >
          <DashboardMetric
            label="Top Warehouse At-Risk %"
            :value="cards?.Top1WarehouseAtRiskPercent != null ? formatPercent(cards.Top1WarehouseAtRiskPercent) : '—'"
            :empty="cards?.Top1WarehouseAtRiskPercent == null"
          />
        </LocationAttentionCardGroup>

        <LocationAttentionCardGroup
          title="Sales Concentration"
          icon="pi pi-chart-line"
          domain="sales"
          :loading="dashboard.loading"
          :unavailable="unavailable"
        >
          <DashboardMetric
            label="Top Warehouse Sales %"
            :value="cards?.Top1WarehouseSalesPercent != null ? formatPercent(cards.Top1WarehouseSalesPercent) : '—'"
            :empty="cards?.Top1WarehouseSalesPercent == null"
          />
          <DashboardMetric
            label="Top Wilayah Sales %"
            :value="cards?.Top1WilayahSalesPercent != null ? formatPercent(cards.Top1WilayahSalesPercent) : '—'"
            :empty="cards?.Top1WilayahSalesPercent == null"
          />
        </LocationAttentionCardGroup>

        <LocationAttentionCardGroup
          title="Operational Signals"
          icon="pi pi-map-marker"
          domain="inventory"
          :loading="dashboard.loading"
          :unavailable="unavailable"
        >
          <DashboardMetric
            label="Inactive Warehouse With Stock"
            :value="cards ? formatNumber(cards.InactiveWarehouseWithStockCount) : '—'"
            :empty="!cards"
          />
          <DashboardMetric
            label="Stock Without Sales"
            :value="cards ? formatNumber(cards.WarehouseNoSalesWithInventoryCount) : '—'"
            :empty="!cards"
          />
        </LocationAttentionCardGroup>

      </div>

    </section>



    <section class="location-dashboard__section">

      <h2 class="location-dashboard__section-title">Top Warehouse by Inventory</h2>

      <Top10RankingTable

        title="Top 10 Warehouse by Inventory Value"

        :columns="warehouseRankingColumns"

        :rows="inventoryRankingRows"

        :loading="dashboard.loading"

        value-field="Amount"

        percent-field="PercentOfTotal"

        empty-message="No warehouse inventory ranking data."

        clickable

        @row-click="onInventoryRankingRowClick"

      />

    </section>



    <section class="location-dashboard__section">

      <h2 class="location-dashboard__section-title">Top Warehouse by At-Risk Inventory</h2>

      <Top10RankingTable

        title="Top 10 Warehouse by At-Risk Value"

        :columns="warehouseRankingColumns"

        :rows="atRiskRankingRows"

        :loading="dashboard.loading"

        value-field="Amount"

        percent-field="PercentOfTotal"

        empty-message="No warehouse at-risk ranking data."

        clickable

        @row-click="onAtRiskRankingRowClick"

      />

    </section>



    <section class="location-dashboard__section">

      <h2 class="location-dashboard__section-title">Top Warehouse by Sales</h2>

      <Top10RankingTable

        title="Top 10 Warehouse by MTD Omzet"

        :columns="warehouseRankingColumns"

        :rows="salesRankingRows"

        :loading="dashboard.loading"

        value-field="Amount"

        percent-field="PercentOfTotal"

        empty-message="No warehouse sales ranking data."

      />

    </section>



    <section class="location-dashboard__section">

      <h2 class="location-dashboard__section-title">Top Warehouse by Purchasing</h2>

      <Top10RankingTable

        title="Top 10 Warehouse by MTD Purchase"

        :columns="warehouseRankingColumns"

        :rows="purchasingRankingRows"

        :loading="dashboard.loading"

        value-field="Amount"

        percent-field="PercentOfTotal"

        empty-message="No warehouse purchasing ranking data."

      />

    </section>



    <section class="location-dashboard__section">

      <h2 class="location-dashboard__section-title">Top Wilayah by Sales</h2>

      <Top10RankingTable

        title="Top 10 Wilayah by MTD Omzet"

        :columns="wilayahRankingColumns"

        :rows="wilayahRankingRows"

        :loading="dashboard.loading"

        value-field="Amount"

        percent-field="PercentOfTotal"

        empty-message="No wilayah sales ranking data."

        clickable

        @row-click="onWilayahRankingRowClick"

      />

    </section>



    <LocationAttentionList

      class="location-dashboard__section"

      :items="dashboard.location?.AttentionList ?? []"

      :loading="dashboard.loading"

    />



    <LocationNavigationSection

      class="location-dashboard__section"

      :navigation="dashboard.location?.Navigation ?? null"

    />

  </DashboardDetailLayout>

</template>



<style scoped>

.location-dashboard__banner {

  margin-bottom: 1rem;

}



.location-dashboard__section {

  margin-bottom: 1.5rem;

}



.location-dashboard__section-title {

  margin: 0 0 1rem;

  font-size: 1.25rem;

}



.location-dashboard__cards {

  display: grid;

  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));

  gap: 1rem;

}




</style>


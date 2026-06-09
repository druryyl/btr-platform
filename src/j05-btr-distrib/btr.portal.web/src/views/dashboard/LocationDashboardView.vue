<script setup lang="ts">

import { computed, onMounted } from 'vue'

import { useRouter } from 'vue-router'

import Message from 'primevue/message'

import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'

import LocationAttentionCardGroup from '@/components/dashboard/LocationAttentionCardGroup.vue'

import LocationAttentionList from '@/components/dashboard/LocationAttentionList.vue'

import LocationNavigationSection from '@/components/dashboard/LocationNavigationSection.vue'

import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'

import { formatNumber, formatPercent } from '@/services/formatters'

import { navigateToReport } from '@/services/navigateToReport'

import type { DashboardLocationRankingRow } from '@/models/dashboard'
import { useDashboardStore } from '@/stores/dashboardStore'



const dashboard = useDashboardStore()

const router = useRouter()



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
  return (rows ?? []).map((row) => ({
    Rank: row.Rank,
    EntityCode: row.EntityCode,
    EntityName: row.EntityName,
    Amount: row.Amount,
    PercentOfTotal: row.PercentOfTotal,
    ReportRoute: row.ReportRoute,
  }))
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

  const entityName = String(row.EntityName ?? '')

  const reportRoute = String(row.ReportRoute ?? '/reports/inventory')

  if (entityName) {

    navigateToReport(router, reportRoute, entityName)

  }

}



function onAtRiskRankingRowClick(): void {

  void router.push('/dashboard/inventory-risk')

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

    <Message

      v-if="dashboard.location && !dashboard.location.IsDataFresh"

      severity="warn"

      :closable="false"

      class="location-dashboard__banner"

    >

      ⚠ Dashboard Data Not Fresh

    </Message>



    <section class="location-dashboard__section">

      <h2 class="location-dashboard__section-title">Location Attention Cards</h2>

      <div class="location-dashboard__cards">

        <LocationAttentionCardGroup

          title="Inventory Concentration"

          icon="pi pi-box"

          :loading="dashboard.loading"

          :unavailable="unavailable"

        >

          <div class="metric">

            <span class="metric__label">Top Warehouse Inventory %</span>

            <span class="metric__value">

              {{ cards?.Top1WarehouseInventoryPercent != null ? formatPercent(cards.Top1WarehouseInventoryPercent) : '—' }}

            </span>

          </div>

          <div class="metric">

            <span class="metric__label">Top 3 Warehouse Inventory %</span>

            <span class="metric__value">

              {{ cards?.Top3WarehouseInventoryPercent != null ? formatPercent(cards.Top3WarehouseInventoryPercent) : '—' }}

            </span>

          </div>

        </LocationAttentionCardGroup>



        <LocationAttentionCardGroup

          title="At-Risk Concentration"

          icon="pi pi-exclamation-triangle"

          :loading="dashboard.loading"

          :unavailable="unavailable"

        >

          <div class="metric">

            <span class="metric__label">Top Warehouse At-Risk %</span>

            <span class="metric__value">

              {{ cards?.Top1WarehouseAtRiskPercent != null ? formatPercent(cards.Top1WarehouseAtRiskPercent) : '—' }}

            </span>

          </div>

        </LocationAttentionCardGroup>



        <LocationAttentionCardGroup

          title="Sales Concentration"

          icon="pi pi-chart-line"

          :loading="dashboard.loading"

          :unavailable="unavailable"

        >

          <div class="metric">

            <span class="metric__label">Top Warehouse Sales %</span>

            <span class="metric__value">

              {{ cards?.Top1WarehouseSalesPercent != null ? formatPercent(cards.Top1WarehouseSalesPercent) : '—' }}

            </span>

          </div>

          <div class="metric">

            <span class="metric__label">Top Wilayah Sales %</span>

            <span class="metric__value">

              {{ cards?.Top1WilayahSalesPercent != null ? formatPercent(cards.Top1WilayahSalesPercent) : '—' }}

            </span>

          </div>

        </LocationAttentionCardGroup>



        <LocationAttentionCardGroup

          title="Operational Signals"

          icon="pi pi-map-marker"

          :loading="dashboard.loading"

          :unavailable="unavailable"

        >

          <div class="metric">

            <span class="metric__label">Inactive Warehouse With Stock</span>

            <span class="metric__value">

              {{ cards ? formatNumber(cards.InactiveWarehouseWithStockCount) : '—' }}

            </span>

          </div>

          <div class="metric">

            <span class="metric__label">Stock Without Sales</span>

            <span class="metric__value">

              {{ cards ? formatNumber(cards.WarehouseNoSalesWithInventoryCount) : '—' }}

            </span>

          </div>

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


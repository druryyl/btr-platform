<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Message from 'primevue/message'
import InvestigationBreadcrumb from '@/components/reports/InvestigationBreadcrumb.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import ReportSummaryBar from '@/components/reports/ReportSummaryBar.vue'
import { useReportInvestigationFilter } from '@/composables/useReportInvestigationFilter'
import { useReportInvestigationHydration } from '@/composables/useReportInvestigationHydration'
import { formatCurrency, formatDateTime } from '@/services/formatters'
import { summarizeInventoryRows } from '@/services/reportSummaryHelpers'
import { useInventoryReportStore } from '@/stores/inventoryReportStore'
import type { InventoryReportRow } from '@/models/reports'

const inventoryReport = useInventoryReportStore()
const route = useRoute()
const { freeText } = storeToRefs(inventoryReport)
const { breadcrumb, brgId, warehouseId, hydrateFromRoute } = useReportInvestigationHydration()

interface InventoryReportTableRow extends InventoryReportRow {
  dataKey: string
}

const sourceRows = computed<InventoryReportTableRow[]>(() =>
  (inventoryReport.report?.Rows ?? []).map((row) => ({
    ...row,
    dataKey: `${row.BrgId}|${row.WarehouseName}`,
  })),
)

const { filteredRows, hasActiveFilter } = useReportInvestigationFilter(
  sourceRows,
  ['ItemDisplay', 'WarehouseName'],
  freeText,
  { brgId, warehouseId },
)

const summaryItems = computed(() => {
  if (!inventoryReport.report?.Summary) return []

  const summary = hasActiveFilter.value
    ? summarizeInventoryRows(filteredRows.value)
    : inventoryReport.report.Summary

  return [
    {
      label: 'Total Inventory Value',
      value: formatCurrency(summary.TotalInventoryValue),
    },
    {
      label: 'Total Item',
      value: String(summary.TotalItem),
    },
  ]
})

onMounted(() => {
  const hydration = hydrateFromRoute(route)

  if (hydration.freeText) {
    inventoryReport.freeText = hydration.freeText
  }

  void inventoryReport.loadReport()
})
</script>

<template>
  <div class="inventory-report">
    <div class="inventory-report__header">
      <div>
        <h1>Inventory Report</h1>
        <p>Stock balance snapshot (qty &gt; 0).</p>
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="inventoryReport.loading"
        @click="inventoryReport.loadReport()"
      />
    </div>

    <InvestigationBreadcrumb :context="breadcrumb" />

    <Message v-if="inventoryReport.error" severity="error" :closable="false">
      {{ inventoryReport.error }}
    </Message>

    <Card>
      <template #content>
        <ReportFilterBar
          v-model:free-text="inventoryReport.freeText"
          :loading="inventoryReport.loading"
          :show-date-range="false"
        />

        <p v-if="hasActiveFilter" class="inventory-report__filter-hint">
          Summary reflects filtered rows.
        </p>

        <DataTable
          :value="filteredRows"
          :loading="inventoryReport.loading"
          paginator
          :rows="25"
          :rows-per-page-options="[10, 25, 50, 100]"
          striped-rows
          removable-sort
          data-key="dataKey"
          class="inventory-report__table"
        >
          <template #empty>
            <div class="inventory-report__empty">
              <i class="pi pi-inbox inventory-report__empty-icon" />
              <p>No inventory items with quantity on hand.</p>
            </div>
          </template>

          <Column field="ItemDisplay" header="Item" sortable />
          <Column field="WarehouseName" header="Warehouse" sortable />
          <Column field="Qty" header="Qty" sortable>
            <template #body="{ data }">
              {{ data.Qty }}
            </template>
          </Column>
          <Column field="Hpp" header="HPP" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.Hpp) }}
            </template>
          </Column>
          <Column field="NilaiSediaan" header="Nilai Sediaan" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.NilaiSediaan) }}
            </template>
          </Column>
        </DataTable>

        <ReportSummaryBar :items="summaryItems" />

        <p v-if="inventoryReport.report" class="inventory-report__helper">
          Totals are aggregated by item across warehouses. Row values show per-warehouse balance.
        </p>

        <div v-if="inventoryReport.report" class="inventory-report__meta">
          Updated {{ formatDateTime(inventoryReport.report.GeneratedAt) }}
        </div>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.inventory-report__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.inventory-report__header h1 {
  margin: 0 0 0.25rem;
  font-size: 1.75rem;
}

.inventory-report__header p {
  margin: 0;
  color: var(--p-text-muted-color);
}

.inventory-report__filter-hint {
  margin: 0 0 0.75rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.inventory-report__table {
  margin-top: 0.5rem;
}

.inventory-report__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2rem 1rem;
  color: var(--p-text-muted-color);
}

.inventory-report__empty-icon {
  font-size: 2rem;
}

.inventory-report__empty p {
  margin: 0;
}

.inventory-report__helper {
  margin: 0.75rem 0 0;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.inventory-report__meta {
  margin-top: 0.5rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  text-align: right;
}
</style>

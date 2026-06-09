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
import { formatCurrency, formatDate, formatDateTime } from '@/services/formatters'
import { summarizePurchasingRows } from '@/services/reportSummaryHelpers'
import { usePurchasingReportStore } from '@/stores/purchasingReportStore'

const route = useRoute()
const purchasingReport = usePurchasingReportStore()
const { freeText } = storeToRefs(purchasingReport)
const { breadcrumb, supplierId, postingFilter, hydrateFromRoute } = useReportInvestigationHydration()

const sourceRows = computed(() => purchasingReport.report?.Rows ?? [])
const { filteredRows: textFilteredRows, hasActiveFilter } = useReportInvestigationFilter(
  sourceRows,
  ['InvoiceCode', 'SupplierName', 'WarehouseName', 'PostingStok'],
  freeText,
  { supplierId },
)

const filteredRows = computed(() => {
  if (!postingFilter.value) {
    return textFilteredRows.value
  }

  return textFilteredRows.value.filter(
    (row) => row.PostingStok === postingFilter.value,
  )
})

const periodLabel = computed(() => {
  if (!purchasingReport.report) {
    return ''
  }

  return `${formatDate(purchasingReport.report.PeriodFrom)} – ${formatDate(purchasingReport.report.PeriodTo)}`
})

const summaryItems = computed(() => {
  if (!purchasingReport.report?.Summary) return []

  const summary = hasActiveFilter.value || postingFilter.value
    ? summarizePurchasingRows(filteredRows.value)
    : purchasingReport.report.Summary

  return [
    {
      label: 'Grand Total Purchase',
      value: formatCurrency(summary.GrandTotalPurchase),
    },
    {
      label: 'Total Invoice',
      value: String(summary.TotalInvoice),
    },
  ]
})

function postingStokClass(value: string): string {
  if (value === 'SUDAH') return 'purchasing-report__posting--done'
  if (value === 'BELUM') return 'purchasing-report__posting--pending'
  return ''
}

onMounted(() => {
  const hydration = hydrateFromRoute(route)

  if (hydration.freeText) {
    purchasingReport.freeText = hydration.freeText
  }

  void purchasingReport.loadReport()
})
</script>

<template>
  <div class="purchasing-report">
    <div class="purchasing-report__header">
      <div>
        <h1>Purchasing Report</h1>
        <p v-if="purchasingReport.report">
          Purchase invoices for {{ periodLabel }}.
        </p>
        <p v-else>
          Purchase invoices for the current month.
        </p>
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="purchasingReport.loading"
        @click="purchasingReport.loadReport()"
      />
    </div>

    <InvestigationBreadcrumb :context="breadcrumb" />

    <Message v-if="purchasingReport.error" severity="error" :closable="false">
      {{ purchasingReport.error }}
    </Message>

    <Card>
      <template #content>
        <ReportFilterBar
          v-model:from="purchasingReport.from"
          v-model:to="purchasingReport.to"
          v-model:free-text="purchasingReport.freeText"
          :loading="purchasingReport.loading"
          @apply="purchasingReport.loadReport()"
        />

        <p v-if="hasActiveFilter || postingFilter" class="purchasing-report__filter-hint">
          Summary reflects filtered rows<span v-if="postingFilter"> (Posting: {{ postingFilter }})</span>.
        </p>

        <DataTable
          :value="filteredRows"
          :loading="purchasingReport.loading"
          paginator
          :rows="25"
          :rows-per-page-options="[10, 25, 50, 100]"
          striped-rows
          removable-sort
          data-key="InvoiceCode"
          class="purchasing-report__table"
        >
          <template #empty>
            <div class="purchasing-report__empty">
              <i class="pi pi-inbox purchasing-report__empty-icon" />
              <p>No purchase invoices found for this period.</p>
            </div>
          </template>

          <Column field="InvoiceCode" header="Invoice" sortable />
          <Column field="InvoiceDate" header="Date" sortable>
            <template #body="{ data }">
              {{ formatDate(data.InvoiceDate) }}
            </template>
          </Column>
          <Column field="SupplierName" header="Supplier" sortable />
          <Column field="WarehouseName" header="Warehouse" sortable />
          <Column field="Total" header="Total" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.Total) }}
            </template>
          </Column>
          <Column field="Disc" header="Disc" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.Disc) }}
            </template>
          </Column>
          <Column field="Tax" header="Tax" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.Tax) }}
            </template>
          </Column>
          <Column field="GrandTotal" header="Grand Total" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.GrandTotal) }}
            </template>
          </Column>
          <Column field="PostingStok" header="Posting Stok" sortable>
            <template #body="{ data }">
              <span
                v-if="data.PostingStok === 'SUDAH' || data.PostingStok === 'BELUM'"
                :class="postingStokClass(data.PostingStok)"
              >
                {{ data.PostingStok }}
              </span>
              <span v-else class="purchasing-report__posting--empty">—</span>
            </template>
          </Column>
        </DataTable>

        <ReportSummaryBar :items="summaryItems" />

        <div v-if="purchasingReport.report" class="purchasing-report__meta">
          Updated {{ formatDateTime(purchasingReport.report.GeneratedAt) }}
        </div>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.purchasing-report__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.purchasing-report__header h1 {
  margin: 0 0 0.25rem;
  font-size: 1.75rem;
}

.purchasing-report__header p {
  margin: 0;
  color: var(--p-text-muted-color);
}

.purchasing-report__filter-hint {
  margin: 0 0 0.75rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.purchasing-report__table {
  margin-top: 0.5rem;
}

.purchasing-report__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2rem 1rem;
  color: var(--p-text-muted-color);
}

.purchasing-report__empty-icon {
  font-size: 2rem;
}

.purchasing-report__empty p {
  margin: 0;
}

.purchasing-report__posting--done {
  color: var(--p-green-700);
  font-weight: 600;
}

.purchasing-report__posting--pending {
  color: var(--p-amber-700);
  font-weight: 600;
}

.purchasing-report__posting--empty {
  color: var(--p-text-muted-color);
}

.purchasing-report__meta {
  margin-top: 1rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  text-align: right;
}
</style>

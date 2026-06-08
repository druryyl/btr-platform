<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Message from 'primevue/message'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import { useReportFreeTextFilter } from '@/composables/useReportFreeTextFilter'
import { formatCurrency, formatDate, formatDateTime } from '@/services/formatters'
import { useSalesReportStore } from '@/stores/salesReportStore'

const route = useRoute()
const salesReport = useSalesReportStore()
const { freeText } = storeToRefs(salesReport)

const sourceRows = computed(() => salesReport.report?.Rows ?? [])
const { filteredRows, hasFreeTextFilter } = useReportFreeTextFilter(
  sourceRows,
  ['FakturCode', 'CustomerName', 'SalesName', 'Status'],
  freeText,
)

const periodLabel = computed(() => {
  if (!salesReport.report) {
    return ''
  }

  return `${formatDate(salesReport.report.PeriodFrom)} – ${formatDate(salesReport.report.PeriodTo)}`
})

onMounted(() => {
  if (typeof route.query.q === 'string' && route.query.q.trim()) {
    salesReport.freeText = route.query.q.trim()
  }

  void salesReport.loadReport()
})
</script>

<template>
  <div class="sales-report">
    <div class="sales-report__header">
      <div>
        <h1>Sales Report</h1>
        <p v-if="salesReport.report">
          Faktur jual for {{ periodLabel }}.
        </p>
        <p v-else>
          Faktur jual for the current month.
        </p>
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="salesReport.loading"
        @click="salesReport.loadReport()"
      />
    </div>

    <Message v-if="salesReport.error" severity="error" :closable="false">
      {{ salesReport.error }}
    </Message>

    <Card>
      <template #content>
        <ReportFilterBar
          v-model:from="salesReport.from"
          v-model:to="salesReport.to"
          v-model:free-text="salesReport.freeText"
          :loading="salesReport.loading"
          @apply="salesReport.loadReport()"
        />

        <p v-if="hasFreeTextFilter" class="sales-report__filter-hint">
          Showing rows matching your search filter.
        </p>

        <DataTable
          :value="filteredRows"
          :loading="salesReport.loading"
          paginator
          :rows="25"
          :rows-per-page-options="[10, 25, 50, 100]"
          striped-rows
          removable-sort
          data-key="FakturCode"
          class="sales-report__table"
        >
          <template #empty>
            <div class="sales-report__empty">
              <i class="pi pi-inbox sales-report__empty-icon" />
              <p>No faktur found for this period.</p>
            </div>
          </template>

          <Column field="FakturDate" header="Date" sortable>
            <template #body="{ data }">
              {{ formatDate(data.FakturDate) }}
            </template>
          </Column>
          <Column field="FakturCode" header="Faktur" sortable />
          <Column field="CustomerName" header="Customer" sortable />
          <Column field="SalesName" header="Sales" sortable />
          <Column field="FakturTotal" header="Total" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.FakturTotal) }}
            </template>
          </Column>
          <Column field="Status" header="Status" sortable>
            <template #body="{ data }">
              <span v-if="data.Status">{{ data.Status }}</span>
              <span v-else class="sales-report__status-empty">—</span>
            </template>
          </Column>
        </DataTable>

        <div v-if="salesReport.report" class="sales-report__meta">
          Updated {{ formatDateTime(salesReport.report.GeneratedAt) }}
        </div>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.sales-report__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.sales-report__header h1 {
  margin: 0 0 0.25rem;
  font-size: 1.75rem;
}

.sales-report__header p {
  margin: 0;
  color: var(--p-text-muted-color);
}

.sales-report__filter-hint {
  margin: 0 0 0.75rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.sales-report__table {
  margin-top: 0.5rem;
}

.sales-report__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2rem 1rem;
  color: var(--p-text-muted-color);
}

.sales-report__empty-icon {
  font-size: 2rem;
}

.sales-report__empty p {
  margin: 0;
}

.sales-report__status-empty {
  color: var(--p-text-muted-color);
}

.sales-report__meta {
  margin-top: 1rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  text-align: right;
}
</style>

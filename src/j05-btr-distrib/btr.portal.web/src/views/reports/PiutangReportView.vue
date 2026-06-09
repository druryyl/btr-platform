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
import { piutangDateFieldLabel } from '@/services/reportFilterDefaults'
import { summarizePiutangRows } from '@/services/reportSummaryHelpers'
import { usePiutangReportStore } from '@/stores/piutangReportStore'

const route = useRoute()
const piutangReport = usePiutangReportStore()
const { freeText } = storeToRefs(piutangReport)
const { breadcrumb, customerId, hydrateFromRoute } = useReportInvestigationHydration()

const sourceRows = computed(() => piutangReport.report?.Rows ?? [])
const { filteredRows, hasActiveFilter } = useReportInvestigationFilter(
  sourceRows,
  ['CustomerName', 'SalesName', 'FakturCode', 'CustomerCode'],
  freeText,
  { customerId },
)

const periodLabel = computed(() => {
  if (!piutangReport.report) {
    return ''
  }

  if (piutangReport.report.AllOpenBalances) {
    return 'All open balances'
  }

  const fieldLabel = piutangDateFieldLabel(piutangReport.dateField)
  return `${fieldLabel}: ${formatDate(piutangReport.report.PeriodFrom)} – ${formatDate(piutangReport.report.PeriodTo)}`
})

const summaryItems = computed(() => {
  if (!piutangReport.report?.Summary) return []

  const summary = hasActiveFilter.value
    ? summarizePiutangRows(filteredRows.value)
    : piutangReport.report.Summary

  return [
    {
      label: 'Total Piutang',
      value: formatCurrency(summary.TotalPiutang),
    },
    {
      label: 'Total Customer',
      value: String(summary.TotalCustomer),
    },
  ]
})

onMounted(() => {
  const hydration = hydrateFromRoute(route)

  if (hydration.freeText) {
    piutangReport.freeText = hydration.freeText
  }

  void piutangReport.loadReport({ allOpenBalances: hydration.allOpenBalances })
})
</script>

<template>
  <div class="piutang-report">
    <div class="piutang-report__header">
      <div>
        <h1>Piutang Report</h1>
        <p v-if="piutangReport.report">
          Open receivables — {{ periodLabel }}.
        </p>
        <p v-else>
          Open receivables (outstanding balance only).
        </p>
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="piutangReport.loading"
        @click="piutangReport.loadReport()"
      />
    </div>

    <InvestigationBreadcrumb :context="breadcrumb" />

    <Message v-if="piutangReport.error" severity="error" :closable="false">
      {{ piutangReport.error }}
    </Message>

    <Card>
      <template #content>
        <ReportFilterBar
          v-model:from="piutangReport.from"
          v-model:to="piutangReport.to"
          v-model:free-text="piutangReport.freeText"
          v-model:date-field="piutangReport.dateField"
          :loading="piutangReport.loading"
          show-date-field
          @apply="piutangReport.loadReport()"
        />

        <p v-if="hasActiveFilter" class="piutang-report__filter-hint">
          Summary reflects filtered rows.
        </p>

        <DataTable
          :value="filteredRows"
          :loading="piutangReport.loading"
          paginator
          :rows="25"
          :rows-per-page-options="[10, 25, 50, 100]"
          striped-rows
          removable-sort
          data-key="FakturCode"
          class="piutang-report__table"
        >
          <template #empty>
            <div class="piutang-report__empty">
              <i class="pi pi-inbox piutang-report__empty-icon" />
              <p>No open receivables found.</p>
            </div>
          </template>

          <Column field="CustomerName" header="Customer" sortable />
          <Column field="SalesName" header="Sales" sortable />
          <Column field="FakturCode" header="Faktur" sortable />
          <Column field="FakturDate" header="Tanggal" sortable>
            <template #body="{ data }">
              {{ formatDate(data.FakturDate) }}
            </template>
          </Column>
          <Column field="JatuhTempo" header="Jatuh Tempo" sortable>
            <template #body="{ data }">
              <span class="piutang-report__jatuh-tempo">
                {{ formatDate(data.JatuhTempo) }}
              </span>
            </template>
          </Column>
          <Column field="TotalJual" header="Total Jual" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.TotalJual) }}
            </template>
          </Column>
          <Column field="KurangBayar" header="Kurang Bayar" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.KurangBayar) }}
            </template>
          </Column>
        </DataTable>

        <ReportSummaryBar :items="summaryItems" />

        <div v-if="piutangReport.report" class="piutang-report__meta">
          Updated {{ formatDateTime(piutangReport.report.GeneratedAt) }}
        </div>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.piutang-report__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.piutang-report__header h1 {
  margin: 0 0 0.25rem;
  font-size: 1.75rem;
}

.piutang-report__header p {
  margin: 0;
  color: var(--p-text-muted-color);
}

.piutang-report__filter-hint {
  margin: 0 0 0.75rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.piutang-report__table {
  margin-top: 0.5rem;
}

.piutang-report__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2rem 1rem;
  color: var(--p-text-muted-color);
}

.piutang-report__empty-icon {
  font-size: 2rem;
}

.piutang-report__empty p {
  margin: 0;
}

.piutang-report__jatuh-tempo {
  font-weight: 600;
  color: var(--p-primary-700);
}

.piutang-report__meta {
  margin-top: 1rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  text-align: right;
}
</style>

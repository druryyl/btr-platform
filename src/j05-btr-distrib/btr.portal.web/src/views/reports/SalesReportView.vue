<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Message from 'primevue/message'
import Select from 'primevue/select'
import InvestigationBreadcrumb from '@/components/reports/InvestigationBreadcrumb.vue'
import ReportFilterBar from '@/components/reports/ReportFilterBar.vue'
import { useReportInvestigationFilter } from '@/composables/useReportInvestigationFilter'
import { useReportInvestigationHydration } from '@/composables/useReportInvestigationHydration'
import { formatCurrency, formatDate, formatDateTime } from '@/services/formatters'
import { useSalesReportStore } from '@/stores/salesReportStore'
import type { SalesReportPrincipalRow } from '@/models/reports'

const route = useRoute()
const salesReport = useSalesReportStore()
const { freeText, supplierId } = storeToRefs(salesReport)
const { breadcrumb, salesmanId, hydrateFromRoute } = useReportInvestigationHydration()

const sourceRows = computed(() => salesReport.report?.Rows ?? [])
const { filteredRows, hasActiveFilter } = useReportInvestigationFilter(
  sourceRows,
  ['FakturCode', 'CustomerName', 'SalesName', 'Status'],
  freeText,
  { salesmanId },
)

const periodLabel = computed(() => {
  if (!salesReport.report) {
    return ''
  }

  return `${formatDate(salesReport.report.PeriodFrom)} – ${formatDate(salesReport.report.PeriodTo)}`
})

const headerTotalLabel = computed(
  () => salesReport.report?.HeaderTotalLabel || 'Header Total',
)

const principalOptions = computed(() => [
  { label: 'All Principals', value: '' },
  ...(salesReport.report?.Principals ?? []).map((row) => ({
    label: row.PrincipalName || row.SupplierId,
    value: row.SupplierId,
  })),
])

const selectedPrincipalLabel = computed(() => {
  if (salesReport.report?.SelectedPrincipalName) {
    return salesReport.report.SelectedPrincipalName
  }

  return supplierId.value || ''
})

function onPrincipalChange(): void {
  void salesReport.loadReport()
}

function onPrincipalRowClick(event: { data: SalesReportPrincipalRow }): void {
  if (!event?.data?.SupplierId) {
    return
  }

  salesReport.supplierId = event.data.SupplierId
  void salesReport.loadReport()
}

onMounted(() => {
  const hydration = hydrateFromRoute(route)

  if (hydration.freeText) {
    salesReport.freeText = hydration.freeText
  }

  const routeSupplierId = route.query.supplierId
  if (typeof routeSupplierId === 'string' && routeSupplierId.trim()) {
    salesReport.supplierId = routeSupplierId.trim()
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

    <InvestigationBreadcrumb :context="breadcrumb" />

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

        <div class="sales-report__principal-filter">
          <label class="sales-report__label" for="sales-report-principal">Principal</label>
          <Select
            id="sales-report-principal"
            v-model="supplierId"
            :options="principalOptions"
            option-label="label"
            option-value="value"
            :disabled="salesReport.loading"
            class="sales-report__principal-select"
            @change="onPrincipalChange"
          />
        </div>

        <p v-if="hasActiveFilter" class="sales-report__filter-hint">
          Showing header rows matching your search filter. Principal amounts are not allocated from header totals.
        </p>
      </template>
    </Card>

    <section
      class="sales-report__principal"
      data-kpi="PRN-SALES-001"
      aria-label="PRN-SALES-001 Principal Sales-Out"
    >
      <h2>PRN-SALES-001 Principal Sales-Out</h2>
      <p class="sales-report__measure">
        The Principal amount is PRN-SALES-001 Principal Sales-Out from Faktur Item.
        It is not required to equal Faktur GrandTotal.
      </p>

      <section class="sales-report__disclosure" aria-label="Principal Sales-Out disclosure">
        <h3>Principal Sales-Out disclosure</h3>
        <ul>
          <li
            v-for="statement in salesReport.report?.Disclosures ?? []"
            :key="statement"
          >
            {{ statement }}
          </li>
        </ul>
      </section>

      <p v-if="salesReport.report" class="sales-report__exception">
        Unknown Principal exception lines {{ salesReport.report.UnknownPrincipalExceptionCount }}.
        Those lines are excluded from Principal amounts and are not assigned to a Principal.
      </p>

      <DataTable
        :value="salesReport.report?.Principals ?? []"
        :loading="salesReport.loading"
        paginator
        :rows="25"
        :rows-per-page-options="[10, 25, 50, 100]"
        striped-rows
        data-key="SupplierId"
        class="sales-report__table sales-report__principal-table"
        @row-click="onPrincipalRowClick"
      >
        <template #empty>
          <div class="sales-report__empty">
            <p>No Principal Sales-Out for this period.</p>
          </div>
        </template>

        <Column field="PrincipalName" header="Principal" />
        <Column field="SupplierId" header="SupplierId" />
        <Column field="PrincipalSalesOutAmount" header="Principal Sales-Out">
          <template #body="{ data }">
            {{ formatCurrency(data.PrincipalSalesOutAmount) }}
          </template>
        </Column>
      </DataTable>

      <div class="sales-report__evidence">
        <h3>Faktur Item evidence</h3>
        <p v-if="selectedPrincipalLabel" class="sales-report__identity">
          {{ selectedPrincipalLabel }}
          <span v-if="supplierId">· SupplierId {{ supplierId }}</span>
          <span v-if="periodLabel">· {{ periodLabel }}</span>
        </p>
        <p v-else class="sales-report__filter-hint">
          Select a Principal to open Faktur Item evidence for this period.
        </p>

        <DataTable
          :value="salesReport.report?.EvidenceLines ?? []"
          :loading="salesReport.loading"
          paginator
          :rows="25"
          :rows-per-page-options="[10, 25, 50, 100]"
          striped-rows
          data-key="FakturItemId"
        >
          <template #empty>
            <div class="sales-report__empty">
              <p>No Faktur Item evidence for this Principal.</p>
            </div>
          </template>

          <Column field="FakturDate" header="Tanggal">
            <template #body="{ data }">
              {{ formatDate(data.FakturDate) }}
            </template>
          </Column>
          <Column field="FakturCode" header="Faktur" />
          <Column field="FakturItemId" header="Faktur Item" />
          <Column field="BrgId" header="BrgId" />
          <Column field="PrincipalSalesOutAmount" header="Principal Sales-Out">
            <template #body="{ data }">
              {{ formatCurrency(data.PrincipalSalesOutAmount) }}
            </template>
          </Column>
        </DataTable>

        <p v-if="supplierId" class="sales-report__total">
          Principal Sales-Out {{ formatCurrency(salesReport.report?.SelectedPrincipalSalesOutAmount ?? 0) }}
        </p>
      </div>
    </section>

    <Card>
      <template #content>
        <h2>Faktur header totals</h2>
        <p class="sales-report__measure">
          {{ headerTotalLabel }} is the Faktur header total, not Principal sales.
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
              <p>No Faktur found for this period.</p>
            </div>
          </template>

          <Column field="FakturDate" header="Tanggal" sortable>
            <template #body="{ data }">
              {{ formatDate(data.FakturDate) }}
            </template>
          </Column>
          <Column field="FakturCode" header="Faktur" sortable />
          <Column field="CustomerName" header="Customer" sortable />
          <Column field="SalesName" header="Sales" sortable />
          <Column field="FakturTotal" :header="headerTotalLabel" sortable>
            <template #body="{ data }">
              {{ formatCurrency(data.FakturTotal) }}
            </template>
          </Column>
          <Column field="Status" header="Status" sortable />
        </DataTable>

        <p v-if="salesReport.report" class="sales-report__total">
          {{ headerTotalLabel }} {{ formatCurrency(salesReport.report.HeaderTotalAmount) }}
        </p>

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

.sales-report__header h1,
.sales-report__principal h2,
.sales-report__evidence h3,
.sales-report h2 {
  margin: 0 0 0.25rem;
}

.sales-report__header h1 {
  font-size: 1.75rem;
}

.sales-report__header p,
.sales-report__measure,
.sales-report__identity,
.sales-report__exception,
.sales-report__total {
  margin: 0 0 0.75rem;
  color: var(--p-text-muted-color);
}

.sales-report__principal-filter {
  margin-top: 1rem;
}

.sales-report__label {
  display: block;
  margin-bottom: 0.35rem;
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--p-text-muted-color);
}

.sales-report__principal-select {
  min-width: 16rem;
}

.sales-report__filter-hint {
  margin: 0.75rem 0 0;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.sales-report__principal,
.sales-report__disclosure {
  margin: 1.5rem 0;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.sales-report__disclosure h3,
.sales-report__evidence h3 {
  font-size: 1rem;
}

.sales-report__disclosure ul {
  margin: 0;
  padding-left: 1.25rem;
}

.sales-report__disclosure li + li {
  margin-top: 0.35rem;
}

.sales-report__evidence {
  margin-top: 1.5rem;
}

.sales-report__table {
  margin-top: 0.5rem;
}

.sales-report__principal-table :deep(tr) {
  cursor: pointer;
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

.sales-report__meta {
  margin-top: 1rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  text-align: right;
}
</style>

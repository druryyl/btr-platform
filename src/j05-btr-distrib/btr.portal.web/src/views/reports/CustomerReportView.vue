<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { storeToRefs } from 'pinia'
import Button from 'primevue/button'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import InputText from 'primevue/inputtext'
import Message from 'primevue/message'
import Tag from 'primevue/tag'
import InvestigationBreadcrumb from '@/components/reports/InvestigationBreadcrumb.vue'
import ReportSummaryBar from '@/components/reports/ReportSummaryBar.vue'
import { useReportInvestigationFilter } from '@/composables/useReportInvestigationFilter'
import { useReportInvestigationHydration } from '@/composables/useReportInvestigationHydration'
import { formatCurrency, formatDate, formatDateTime, formatPercent } from '@/services/formatters'
import { actionBadgeSeverity } from '@/services/customerPortfolioSignals'
import { useCustomerReportStore } from '@/stores/customerReportStore'

const route = useRoute()
const customerReport = useCustomerReportStore()
const { freeText } = storeToRefs(customerReport)
const { breadcrumb, customerCode, hydrateFromRoute } = useReportInvestigationHydration()

const sourceRows = computed(() => customerReport.report?.Rows ?? [])
const { filteredRows, hasActiveFilter } = useReportInvestigationFilter(
  sourceRows,
  ['CustomerCode', 'CustomerName', 'WilayahName', 'SalesPersonName', 'PrimaryActionLabel'],
  freeText,
  { customerCode },
)

const summaryItems = computed(() => {
  if (!customerReport.report?.Summary) return []

  const summary = hasActiveFilter.value
    ? {
        TotalCustomerCount: filteredRows.value.length,
        TotalMtdOmzet: filteredRows.value.reduce((total, row) => total + row.MtdOmzet, 0),
        TotalOpenBalance: filteredRows.value.reduce((total, row) => total + row.OpenBalance, 0),
      }
    : customerReport.report.Summary

  return [
    { label: 'Total Customers', value: String(summary.TotalCustomerCount) },
    { label: 'Total MTD Omzet', value: formatCurrency(summary.TotalMtdOmzet) },
    { label: 'Total Open Balance', value: formatCurrency(summary.TotalOpenBalance) },
  ]
})

const disclaimer = computed(
  () =>
    customerReport.report?.Rows[0]?.ValueDisclaimer ??
    'Customer Value = Omzet Proxy, NOT profitability.',
)

onMounted(() => {
  const hydration = hydrateFromRoute(route)

  if (hydration.freeText) {
    customerReport.freeText = hydration.freeText
  }

  void customerReport.loadReport({ customerCode: hydration.customerCode })
})
</script>

<template>
  <div class="customer-report">
    <div class="customer-report__header">
      <div>
        <h1>Customer Report</h1>
        <p>Materialized portfolio customer rows for investigation and drill-down.</p>
      </div>
      <Button
        label="Refresh"
        icon="pi pi-refresh"
        outlined
        :loading="customerReport.loading"
        @click="customerReport.loadReport()"
      />
    </div>

    <InvestigationBreadcrumb :context="breadcrumb" />

    <Message v-if="customerReport.error" severity="error" :closable="false">
      {{ customerReport.error }}
    </Message>

    <Card>
      <template #content>
        <div class="customer-report__filters">
          <span class="p-input-icon-left customer-report__search">
            <i class="pi pi-search" />
            <InputText v-model="customerReport.freeText" placeholder="Search customer code or name" />
          </span>
          <InputText
            v-model="customerReport.customerCode"
            placeholder="Customer code"
            class="customer-report__code"
          />
          <Button
            label="Apply"
            icon="pi pi-filter"
            :loading="customerReport.loading"
            @click="customerReport.loadReport()"
          />
        </div>

        <ReportSummaryBar v-if="summaryItems.length > 0" :items="summaryItems" />

        <p v-if="hasActiveFilter" class="customer-report__filter-hint">
          Summary reflects filtered rows.
        </p>

        <Message severity="info" :closable="false" class="customer-report__disclaimer">
          {{ disclaimer }}
        </Message>

        <DataTable
          :value="filteredRows"
          :loading="customerReport.loading"
          paginator
          :rows="25"
          :rows-per-page-options="[10, 25, 50, 100]"
          striped-rows
          removable-sort
          data-key="CustomerCode"
          class="customer-report__table"
        >
          <template #empty>
            <div class="customer-report__empty">
              <i class="pi pi-inbox customer-report__empty-icon" />
              <p>No customers found for the current filter.</p>
            </div>
          </template>

          <Column field="CustomerCode" header="Code" sortable />
          <Column field="CustomerName" header="Customer" sortable />
          <Column field="WilayahName" header="Wilayah" sortable />
          <Column field="Klasifikasi" header="Klasifikasi" sortable />
          <Column field="TierLabel" header="Tier" sortable />
          <Column field="LifecycleLabel" header="Lifecycle" sortable />
          <Column header="Action" sortable field="PrimaryActionLabel">
            <template #body="{ data }">
              <Tag
                :value="data.PrimaryActionLabel"
                :severity="actionBadgeSeverity(data.PrimaryActionKey)"
              />
            </template>
          </Column>
          <Column field="ActionOwner" header="Owner" sortable />
          <Column field="SalesPersonName" header="Salesman" sortable />
          <Column field="MtdOmzet" header="MTD Omzet" sortable>
            <template #body="{ data }">{{ formatCurrency(data.MtdOmzet) }}</template>
          </Column>
          <Column field="OpenBalance" header="Open Balance" sortable>
            <template #body="{ data }">{{ formatCurrency(data.OpenBalance) }}</template>
          </Column>
          <Column field="OverdueBalance" header="Overdue" sortable>
            <template #body="{ data }">
              {{ data.OverdueBalance != null ? formatCurrency(data.OverdueBalance) : '—' }}
            </template>
          </Column>
          <Column field="M29Category" header="Risk" sortable />
          <Column field="LastPurchaseDate" header="Last Purchase" sortable>
            <template #body="{ data }">
              {{ data.LastPurchaseDate ? formatDate(data.LastPurchaseDate) : '—' }}
            </template>
          </Column>
          <Column field="SalesmanAchievementPercent" header="Sales Achievement" sortable>
            <template #body="{ data }">
              {{ formatPercent(data.SalesmanAchievementPercent) }}
            </template>
          </Column>
        </DataTable>

        <div v-if="customerReport.report" class="customer-report__meta">
          Updated {{ formatDateTime(customerReport.report.GeneratedAt) }}
        </div>
      </template>
    </Card>
  </div>
</template>

<style scoped>
.customer-report__header {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.customer-report__header h1 {
  margin: 0 0 0.25rem;
  font-size: 1.75rem;
}

.customer-report__header p {
  margin: 0;
  color: var(--p-text-muted-color);
}

.customer-report__filters {
  display: flex;
  flex-wrap: wrap;
  gap: 0.75rem;
  margin-bottom: 1rem;
}

.customer-report__search {
  flex: 1 1 240px;
}

.customer-report__code {
  width: 180px;
}

.customer-report__filter-hint {
  margin: 0 0 0.75rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
}

.customer-report__disclaimer {
  margin-bottom: 1rem;
}

.customer-report__table {
  margin-top: 0.5rem;
}

.customer-report__empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 0.5rem;
  padding: 2rem 1rem;
  color: var(--p-text-muted-color);
}

.customer-report__empty-icon {
  font-size: 2rem;
}

.customer-report__empty p {
  margin: 0;
}

.customer-report__meta {
  margin-top: 1rem;
  font-size: 0.85rem;
  color: var(--p-text-muted-color);
  text-align: right;
}
</style>

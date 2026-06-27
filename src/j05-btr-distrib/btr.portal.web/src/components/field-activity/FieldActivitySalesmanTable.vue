<script setup lang="ts">
import { computed, ref } from 'vue'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import InputText from 'primevue/inputtext'
import Tag from 'primevue/tag'
import type { FieldActivitySalesmanOverviewRow } from '@/models/fieldActivity'
import {
  bandClass,
  effectiveCallBand,
  executionBand,
  gpsValidBand,
  statusLabel,
  statusSeverity,
} from '@/services/fieldActivityKpiBands'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  rows: FieldActivitySalesmanOverviewRow[]
  loading?: boolean
}>()

const emit = defineEmits<{
  rowClick: [row: FieldActivitySalesmanOverviewRow]
}>()

const globalFilter = ref('')

const filteredRows = computed(() => {
  const query = globalFilter.value.trim().toLowerCase()
  if (!query) return props.rows

  return props.rows.filter((row) =>
    `${row.SalesPersonCode} ${row.SalesPersonName}`.toLowerCase().includes(query),
  )
})

function onRowClick(event: { data: FieldActivitySalesmanOverviewRow }): void {
  emit('rowClick', event.data)
}
</script>

<template>
  <Card class="field-activity-salesman-table">
    <template #title>
      <div class="field-activity-salesman-table__header">
        <span>Salesman Performance</span>
        <span class="p-input-icon-left field-activity-salesman-table__search">
          <i class="pi pi-search" />
          <InputText v-model="globalFilter" placeholder="Search code or name" />
        </span>
      </div>
    </template>

    <template #content>
      <DataTable
        :value="filteredRows"
        :loading="loading"
        striped-rows
        paginator
        :rows="25"
        sort-field="VisitExecutionPercent"
        :sort-order="1"
        removable-sort
        class="field-activity-salesman-table__grid"
        @row-click="onRowClick"
      >
        <Column field="Rank" header="#" sortable style="width: 4rem" />
        <Column field="SalesPersonCode" header="Code" sortable />
        <Column field="SalesPersonName" header="Name" sortable />
        <Column field="PlannedVisits" header="Planned" sortable>
          <template #body="{ data }">{{ formatNumber(data.PlannedVisits) }}</template>
        </Column>
        <Column field="ActualVisits" header="Actual" sortable>
          <template #body="{ data }">{{ formatNumber(data.ActualVisits) }}</template>
        </Column>
        <Column field="VisitExecutionPercent" header="Execution %" sortable>
          <template #body="{ data }">
            <span :class="bandClass(executionBand(data.VisitExecutionPercent))">
              {{ formatPercent(data.VisitExecutionPercent) }}
            </span>
          </template>
        </Column>
        <Column field="EffectiveCalls" header="Effective" sortable>
          <template #body="{ data }">{{ formatNumber(data.EffectiveCalls) }}</template>
        </Column>
        <Column field="EffectiveCallRate" header="Eff. Rate" sortable>
          <template #body="{ data }">
            <span :class="bandClass(effectiveCallBand(data.EffectiveCallRate))">
              {{ formatPercent(data.EffectiveCallRate) }}
            </span>
          </template>
        </Column>
        <Column field="MissedVisits" header="Missed" sortable />
        <Column field="UnplannedVisits" header="Unplanned" sortable />
        <Column field="GpsValidPercent" header="GPS Valid %" sortable>
          <template #body="{ data }">
            <span :class="bandClass(gpsValidBand(data.GpsValidPercent))">
              {{ formatPercent(data.GpsValidPercent) }}
            </span>
          </template>
        </Column>
        <Column field="OrdersCount" header="Orders" sortable />
        <Column field="OmzetAmount" header="Order Value" sortable>
          <template #body="{ data }">{{ formatCurrency(data.OmzetAmount) }}</template>
        </Column>
        <Column field="StatusCode" header="Status" sortable>
          <template #body="{ data }">
            <Tag
              :value="statusLabel(data.StatusCode)"
              :severity="statusSeverity(data.StatusCode)"
            />
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.field-activity-salesman-table__header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.field-activity-salesman-table__search {
  min-width: 14rem;
}

:deep(.field-activity-salesman-table__grid .p-datatable-tbody > tr) {
  cursor: pointer;
}

:deep(.field-activity-kpi-band) {
  font-weight: 600;
}

:deep(.field-activity-kpi-band--good) {
  color: var(--p-green-600, #16a34a);
}

:deep(.field-activity-kpi-band--warn) {
  color: var(--p-yellow-600, #ca8a04);
}

:deep(.field-activity-kpi-band--bad) {
  color: var(--p-red-600, #dc2626);
}

:deep(.field-activity-kpi-band--neutral) {
  color: var(--p-text-muted-color, #64748b);
}
</style>

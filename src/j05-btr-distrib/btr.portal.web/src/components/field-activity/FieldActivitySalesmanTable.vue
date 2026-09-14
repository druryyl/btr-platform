<script setup lang="ts">
import { computed, ref } from 'vue'
import Card from 'primevue/card'
import Column from 'primevue/column'
import ColumnGroup from 'primevue/columngroup'
import DataTable from 'primevue/datatable'
import InputText from 'primevue/inputtext'
import Row from 'primevue/row'
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
        <span>Salesman Scoreboard</span>
        <span class="p-input-icon-left field-activity-salesman-table__search">
          <i class="pi pi-search" />
          <InputText v-model="globalFilter" placeholder="Search code or name" />
        </span>
      </div>
    </template>

    <template #content>
      <div class="field-activity-salesman-table__table-panel">
        <DataTable
          :value="filteredRows"
          :loading="loading"
          striped-rows
          scrollable
          scroll-height="flex"
          sort-field="VisitExecutionPercent"
          :sort-order="1"
          removable-sort
          class="field-activity-salesman-table__grid"
          @row-click="onRowClick"
        >
          <ColumnGroup type="header">
            <Row>
              <Column header="#" field="Rank" :rowspan="2" sortable style="width: 4rem" />
              <Column header="Code" field="SalesPersonCode" :rowspan="2" sortable />
              <Column header="Name" field="SalesPersonName" :rowspan="2" sortable />
              <Column header="ACTIVITY" :colspan="5" />
              <Column header="PRODUCTIVITY" :colspan="2" />
              <Column header="OUTCOME" :colspan="2" />
              <Column header="SIGNAL" :colspan="2" />
            </Row>
            <Row>
              <Column header="Planned" field="PlannedVisits" sortable />
              <Column header="Actual" field="ActualVisits" sortable />
              <Column header="Execution %" field="VisitExecutionPercent" sortable />
              <Column header="Missed" field="MissedVisits" sortable />
              <Column header="Unplanned" field="UnplannedVisits" sortable />
              <Column header="Effective" field="EffectiveCalls" sortable />
              <Column header="Eff. Rate" field="EffectiveCallRate" sortable />
              <Column header="Orders" field="OrdersCount" sortable />
              <Column header="Order Value" field="OmzetAmount" sortable />
              <Column header="GPS Valid %" field="GpsValidPercent" sortable />
              <Column header="Status" field="StatusCode" sortable />
            </Row>
          </ColumnGroup>

          <Column field="Rank" style="width: 4rem" />
          <Column field="SalesPersonCode" />
          <Column field="SalesPersonName" />
          <Column field="PlannedVisits">
            <template #body="{ data }">{{ formatNumber(data.PlannedVisits) }}</template>
          </Column>
          <Column field="ActualVisits">
            <template #body="{ data }">{{ formatNumber(data.ActualVisits) }}</template>
          </Column>
          <Column field="VisitExecutionPercent">
            <template #body="{ data }">
              <span :class="bandClass(executionBand(data.VisitExecutionPercent))">
                {{ formatPercent(data.VisitExecutionPercent) }}
              </span>
            </template>
          </Column>
          <Column field="MissedVisits" />
          <Column field="UnplannedVisits" />
          <Column field="EffectiveCalls">
            <template #body="{ data }">{{ formatNumber(data.EffectiveCalls) }}</template>
          </Column>
          <Column field="EffectiveCallRate">
            <template #body="{ data }">
              <span :class="bandClass(effectiveCallBand(data.EffectiveCallRate))">
                {{ formatPercent(data.EffectiveCallRate) }}
              </span>
            </template>
          </Column>
          <Column field="OrdersCount" />
          <Column field="OmzetAmount">
            <template #body="{ data }">{{ formatCurrency(data.OmzetAmount) }}</template>
          </Column>
          <Column field="GpsValidPercent">
            <template #body="{ data }">
              <span :class="bandClass(gpsValidBand(data.GpsValidPercent))">
                {{ formatPercent(data.GpsValidPercent) }}
              </span>
            </template>
          </Column>
          <Column field="StatusCode">
            <template #body="{ data }">
              <Tag
                :value="statusLabel(data.StatusCode)"
                :severity="statusSeverity(data.StatusCode)"
              />
            </template>
          </Column>
        </DataTable>
      </div>
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

.field-activity-salesman-table__table-panel {
  max-height: 32rem;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

:deep(.field-activity-salesman-table__grid) {
  flex: 1;
  min-height: 0;
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

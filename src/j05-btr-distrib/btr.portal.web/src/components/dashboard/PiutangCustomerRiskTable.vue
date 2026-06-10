<script setup lang="ts">
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'

const props = defineProps<{
  title: string
  rows: Record<string, unknown>[]
  loading: boolean
  emptyMessage: string
  clickable?: boolean
}>()

const emit = defineEmits<{
  rowClick: [row: Record<string, unknown>]
}>()

const columns = [
  { field: 'CustomerName', header: 'Customer' },
  { field: 'TotalPiutang', header: 'Total' },
  { field: 'CurrentAmount', header: 'Current' },
  { field: 'Aging30Amount', header: '1–30' },
  { field: 'Aging60Amount', header: '31–60' },
  { field: 'Aging90Amount', header: '61–90' },
  { field: 'AgingOver90Amount', header: '>90' },
]

const currencyFields = new Set([
  'TotalPiutang',
  'CurrentAmount',
  'Aging30Amount',
  'Aging60Amount',
  'Aging90Amount',
  'AgingOver90Amount',
])

function onRowClick(event: { data: Record<string, unknown> }): void {
  if (!props.clickable) return
  emit('rowClick', event.data)
}

function formatCell(field: string, value: unknown): string {
  if (currencyFields.has(field)) {
    return formatCurrency(value as number)
  }

  return String(value ?? '')
}
</script>

<template>
  <Card class="piutang-customer-risk-table">
    <template #title>
      <div class="piutang-customer-risk-table__title">
        <i class="pi pi-list" aria-hidden="true" />
        <span>{{ title }}</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="piutang-customer-risk-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <div v-else class="piutang-customer-risk-table__scroll">
        <DataTable
          :value="rows"
          striped-rows
          class="piutang-customer-risk-table__table"
          :class="{ 'piutang-customer-risk-table__table--clickable': clickable }"
          @row-click="onRowClick"
        >
          <template #empty>
            <p class="piutang-customer-risk-table__empty">{{ emptyMessage }}</p>
          </template>

          <Column
            v-for="col in columns"
            :key="col.field"
            :field="col.field"
            :header="col.header"
          >
            <template #body="{ data }">
              {{ formatCell(col.field, data[col.field]) }}
            </template>
          </Column>
        </DataTable>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.piutang-customer-risk-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.piutang-customer-risk-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.piutang-customer-risk-table__scroll {
  overflow-x: auto;
}

.piutang-customer-risk-table__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.piutang-customer-risk-table__table--clickable :deep(.p-datatable-tbody > tr) {
  cursor: pointer;
}
</style>

<script setup lang="ts">
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency, formatPercent } from '@/services/formatters'

const props = defineProps<{
  title: string
  columns: { field: string; header: string }[]
  rows: object[]
  loading: boolean
  valueField: string
  percentField?: string
  emptyMessage: string
  clickable?: boolean
  clickHint?: string
}>()

const emit = defineEmits<{
  rowClick: [row: Record<string, unknown>]
}>()

function onRowClick(event: { data: object }): void {
  if (!props.clickable) return
  emit('rowClick', event.data as Record<string, unknown>)
}

function formatCell(field: string, value: unknown, valueField: string, percentField?: string): string {
  if (field === valueField) {
    return formatCurrency(value as number)
  }

  if (percentField && field === percentField) {
    return value != null ? formatPercent(value as number) : '—'
  }

  return String(value ?? '')
}
</script>

<template>
  <Card class="top10-ranking-table">
    <template #title>
      <div class="top10-ranking-table__title-block">
        <div class="top10-ranking-table__title">
          <i class="pi pi-list" aria-hidden="true" />
          <span>{{ title }}</span>
          <i
            v-if="clickable && clickHint"
            class="pi pi-id-card top10-ranking-table__profile-icon"
            aria-hidden="true"
            title="Performance Profile"
          />
        </div>
        <p v-if="clickable && clickHint" class="top10-ranking-table__hint">{{ clickHint }}</p>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="top10-ranking-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <DataTable
        v-else
        :value="rows"
        striped-rows
        class="top10-ranking-table__table"
        :class="{ 'top10-ranking-table__table--clickable': clickable }"
        @row-click="onRowClick"
      >
        <template #empty>
          <p class="top10-ranking-table__empty">{{ emptyMessage }}</p>
        </template>

        <Column
          v-for="col in columns"
          :key="col.field"
          :field="col.field"
          :header="col.header"
        >
          <template #body="{ data }">
            {{ formatCell(col.field, (data as Record<string, unknown>)[col.field], valueField, percentField) }}
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.top10-ranking-table__title-block {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.top10-ranking-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.top10-ranking-table__profile-icon {
  color: var(--p-primary-color);
  font-size: 0.95rem;
}

.top10-ranking-table__hint {
  margin: 0;
  font-size: 0.8125rem;
  font-weight: 400;
  color: var(--p-text-muted-color);
}

.top10-ranking-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.top10-ranking-table__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.top10-ranking-table__table--clickable :deep(.p-datatable-tbody > tr) {
  cursor: pointer;
}

.top10-ranking-table__table--clickable :deep(.p-datatable-tbody > tr:hover) {
  background: var(--p-surface-100);
}
</style>

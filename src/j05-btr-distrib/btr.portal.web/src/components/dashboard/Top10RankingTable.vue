<script setup lang="ts">
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency } from '@/services/formatters'

defineProps<{
  title: string
  columns: { field: string; header: string }[]
  rows: Record<string, unknown>[]
  loading: boolean
  valueField: string
  emptyMessage: string
}>()
</script>

<template>
  <Card class="top10-ranking-table">
    <template #title>
      <div class="top10-ranking-table__title">
        <i class="pi pi-list" aria-hidden="true" />
        <span>{{ title }}</span>
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
            <span v-if="col.field === valueField">
              {{ formatCurrency(data[col.field] as number) }}
            </span>
            <span v-else>{{ data[col.field] }}</span>
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.top10-ranking-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
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
</style>

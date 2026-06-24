<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import ProgressSpinner from 'primevue/progressspinner'
import { formatCurrency, formatDate } from '@/services/formatters'
import type { DashboardInventoryOptimizationReorderItem } from '@/models/dashboard'

const props = defineProps<{
  reorderList: DashboardInventoryOptimizationReorderItem[]
  loading: boolean
}>()

const rows = computed(() => props.reorderList ?? [])

function categorySeverity(category: string): 'danger' | 'warn' | 'info' | 'secondary' {
  if (category === 'Critical') return 'danger'
  if (category === 'High') return 'warn'
  if (category === 'Medium') return 'info'
  return 'secondary'
}
</script>

<template>
  <Card>
    <template #title>Recommended Reorder List</template>
    <template #content>
      <div v-if="loading" class="io-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <DataTable v-else-if="rows.length > 0" :value="rows" striped-rows size="small">
        <Column field="BrgName" header="Item" />
        <Column field="SupplierName" header="Supplier" />
        <Column header="Rec Qty">
          <template #body="{ data }">{{ data.RecommendedPurchaseQty }}</template>
        </Column>
        <Column header="Est Cost">
          <template #body="{ data }">{{ formatCurrency(data.EstimatedCostIdr) }}</template>
        </Column>
        <Column header="DOS">
          <template #body="{ data }">
            {{ data.DaysOfSupply != null ? data.DaysOfSupply.toFixed(1) : '—' }}
          </template>
        </Column>
        <Column header="Reorder Date">
          <template #body="{ data }">
            {{ data.ReorderDate ? formatDate(data.ReorderDate) : '—' }}
          </template>
        </Column>
        <Column header="Cat.">
          <template #body="{ data }">
            <Tag :severity="categorySeverity(data.Category)" :value="data.Category" />
          </template>
        </Column>
      </DataTable>
      <p v-else class="io-table__empty">No purchase recommendations.</p>
    </template>
  </Card>
</template>

<style scoped>
.io-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.io-table__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

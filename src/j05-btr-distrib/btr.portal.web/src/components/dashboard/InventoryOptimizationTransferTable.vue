<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardInventoryOptimizationTransferItem } from '@/models/dashboard'

const props = defineProps<{
  transferList: DashboardInventoryOptimizationTransferItem[]
  loading: boolean
}>()

const rows = computed(() => props.transferList ?? [])

function categorySeverity(category: string): 'danger' | 'warn' | 'info' | 'secondary' {
  if (category === 'Critical') return 'danger'
  if (category === 'High') return 'warn'
  if (category === 'Medium') return 'info'
  return 'secondary'
}
</script>

<template>
  <Card>
    <template #title>Warehouse Rebalancing</template>
    <template #content>
      <div v-if="loading" class="io-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <DataTable v-else-if="rows.length > 0" :value="rows" striped-rows size="small">
        <Column field="BrgName" header="Item" />
        <Column field="WarehouseFromName" header="From WH" />
        <Column field="WarehouseToName" header="To WH" />
        <Column header="Transfer Qty">
          <template #body="{ data }">{{ data.TransferQty }}</template>
        </Column>
        <Column header="Dest DOS">
          <template #body="{ data }">
            {{ data.DestDaysOfSupply != null ? data.DestDaysOfSupply.toFixed(1) : '—' }}
          </template>
        </Column>
        <Column header="Category">
          <template #body="{ data }">
            <Tag :severity="categorySeverity(data.Category)" :value="data.Category" />
          </template>
        </Column>
      </DataTable>
      <p v-else class="io-table__empty">No transfer recommendations.</p>
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

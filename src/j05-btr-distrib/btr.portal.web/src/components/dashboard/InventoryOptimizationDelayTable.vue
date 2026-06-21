<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardInventoryOptimizationDelayItem } from '@/models/dashboard'

const props = defineProps<{
  delayList: DashboardInventoryOptimizationDelayItem[]
  loading: boolean
}>()

const rows = computed(() => props.delayList ?? [])
</script>

<template>
  <Card>
    <template #title>Overstock &amp; Delay Purchasing</template>
    <template #content>
      <div v-if="loading" class="io-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <DataTable v-else-if="rows.length > 0" :value="rows" striped-rows size="small">
        <Column field="BrgName" header="Item" />
        <Column field="SupplierName" header="Supplier" />
        <Column header="DOS">
          <template #body="{ data }">
            {{ data.DaysOfSupply != null ? data.DaysOfSupply.toFixed(1) : '—' }}
          </template>
        </Column>
        <Column field="MovementClass" header="Movement" />
        <Column field="ActionLabel" header="Action" />
        <Column field="ReasonText" header="Reason" />
      </DataTable>
      <p v-else class="io-table__empty">No delay recommendations.</p>
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

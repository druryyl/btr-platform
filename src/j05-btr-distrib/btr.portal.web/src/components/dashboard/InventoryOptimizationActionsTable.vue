<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import ProgressSpinner from 'primevue/progressspinner'
import { RouterLink } from 'vue-router'
import { formatCurrency } from '@/services/formatters'
import type { DashboardInventoryOptimizationActionItem } from '@/models/dashboard'

const props = defineProps<{
  actions: DashboardInventoryOptimizationActionItem[]
  loading: boolean
}>()

const rows = computed(() => props.actions ?? [])

function categorySeverity(category: string): 'danger' | 'warn' | 'info' | 'secondary' {
  if (category === 'Critical') return 'danger'
  if (category === 'High') return 'warn'
  if (category === 'Medium') return 'info'
  return 'secondary'
}
</script>

<template>
  <Card>
    <template #title>Top Optimization Actions</template>
    <template #content>
      <div v-if="loading" class="io-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>
      <template v-else>
        <DataTable v-if="rows.length > 0" :value="rows" striped-rows size="small" paginator :rows="10">
          <Column header="Priority" field="PriorityScore" />
          <Column header="Category">
            <template #body="{ data }">
              <Tag :severity="categorySeverity(data.Category)" :value="data.Category" />
            </template>
          </Column>
          <Column field="ActionLabel" header="Action" />
          <Column header="Item / Pair">
            <template #body="{ data }">
              <span v-if="data.WarehouseFromName">
                {{ data.BrgName }} ({{ data.WarehouseFromName }} → {{ data.WarehouseToName }})
              </span>
              <span v-else>{{ data.BrgName }}</span>
            </template>
          </Column>
          <Column field="ReasonText" header="Reason" />
          <Column header="Impact">
            <template #body="{ data }">{{ formatCurrency(data.ImpactValueIdr) }}</template>
          </Column>
          <Column header="">
            <template #body="{ data }">
              <RouterLink v-if="data.DrillDownRoute" :to="data.DrillDownRoute" class="io-table__link">
                →
              </RouterLink>
            </template>
          </Column>
        </DataTable>
        <p v-else class="io-table__empty">No optimization actions for the current refresh.</p>
      </template>
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

.io-table__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}
</style>

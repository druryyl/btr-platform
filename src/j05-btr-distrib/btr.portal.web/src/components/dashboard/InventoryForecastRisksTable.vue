<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import ProgressSpinner from 'primevue/progressspinner'
import { RouterLink } from 'vue-router'
import { formatCurrency, formatDate } from '@/services/formatters'
import type { DashboardInventoryForecastRiskItem } from '@/models/dashboard'

const props = defineProps<{
  risks: DashboardInventoryForecastRiskItem[]
  loading: boolean
}>()

const rows = computed(() => props.risks ?? [])

function urgencySeverity(urgency: string): 'danger' | 'warn' | 'info' | 'secondary' {
  if (urgency === 'Critical') return 'danger'
  if (urgency === 'High') return 'warn'
  if (urgency === 'Medium') return 'info'
  return 'secondary'
}
</script>

<template>
  <Card class="inventory-forecast-risks-table">
    <template #title>
      <div class="inventory-forecast-risks-table__title">
        <i class="pi pi-exclamation-triangle" aria-hidden="true" />
        <span>Top Inventory Risks</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="inventory-forecast-risks-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <DataTable
          v-if="rows.length > 0"
          :value="rows"
          striped-rows
          size="small"
          class="inventory-forecast-risks-table__grid"
        >
          <Column field="BrgName" header="Item" />
          <Column field="SignalLabel" header="Signal" />
          <Column header="DOS">
            <template #body="{ data }">
              {{ data.DaysOfSupply != null ? data.DaysOfSupply.toFixed(1) : '—' }}
            </template>
          </Column>
          <Column header="Stock-Out Date">
            <template #body="{ data }">
              {{ data.StockOutDate ? formatDate(data.StockOutDate) : '—' }}
            </template>
          </Column>
          <Column header="Value">
            <template #body="{ data }">
              {{ formatCurrency(data.ValueAmount) }}
            </template>
          </Column>
          <Column header="Urgency">
            <template #body="{ data }">
              <Tag :severity="urgencySeverity(data.Urgency)" :value="data.Urgency" />
            </template>
          </Column>
          <Column header="">
            <template #body="{ data }">
              <RouterLink
                v-if="data.ReportRoute"
                :to="{ path: data.ReportRoute, query: data.EntityCode ? { q: data.EntityCode } : {} }"
                class="inventory-forecast-risks-table__link"
              >
                Investigate
              </RouterLink>
            </template>
          </Column>
        </DataTable>

        <p v-else class="inventory-forecast-risks-table__empty">
          No inventory forecast risks identified for the current refresh.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.inventory-forecast-risks-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.inventory-forecast-risks-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-forecast-risks-table__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.inventory-forecast-risks-table__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.inventory-forecast-risks-table__link:hover {
  text-decoration: underline;
}
</style>

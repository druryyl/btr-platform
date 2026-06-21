<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import ProgressSpinner from 'primevue/progressspinner'
import { RouterLink } from 'vue-router'
import { formatDate, formatNumber } from '@/services/formatters'
import type { DashboardInventoryForecastRecommendationItem } from '@/models/dashboard'

const props = defineProps<{
  recommendations: DashboardInventoryForecastRecommendationItem[]
  loading: boolean
}>()

const rows = computed(() => props.recommendations ?? [])

function urgencySeverity(urgency: string): 'danger' | 'warn' | 'info' | 'secondary' {
  if (urgency === 'Critical') return 'danger'
  if (urgency === 'High') return 'warn'
  if (urgency === 'Medium') return 'info'
  return 'secondary'
}
</script>

<template>
  <Card class="inventory-purchase-recommendations-table">
    <template #title>
      <div class="inventory-purchase-recommendations-table__title">
        <i class="pi pi-shopping-cart" aria-hidden="true" />
        <span>Purchasing Recommendations</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="inventory-purchase-recommendations-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <DataTable
          v-if="rows.length > 0"
          :value="rows"
          striped-rows
          size="small"
          class="inventory-purchase-recommendations-table__grid"
        >
          <Column field="BrgName" header="Item" />
          <Column field="SupplierName" header="Supplier" />
          <Column header="Reorder Date">
            <template #body="{ data }">
              {{ data.ReorderDate ? formatDate(data.ReorderDate) : '—' }}
            </template>
          </Column>
          <Column header="Rec. Qty">
            <template #body="{ data }">
              {{ formatNumber(data.RecommendedPurchaseQty) }}
            </template>
          </Column>
          <Column header="ADC">
            <template #body="{ data }">
              {{ formatNumber(data.AverageDailyConsumption) }}
            </template>
          </Column>
          <Column header="Current Qty">
            <template #body="{ data }">
              {{ formatNumber(data.CurrentQty) }}
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
                class="inventory-purchase-recommendations-table__link"
              >
                View
              </RouterLink>
            </template>
          </Column>
        </DataTable>

        <p v-else class="inventory-purchase-recommendations-table__empty">
          No purchase recommendations for the current forecast refresh.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.inventory-purchase-recommendations-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.inventory-purchase-recommendations-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-purchase-recommendations-table__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.inventory-purchase-recommendations-table__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.inventory-purchase-recommendations-table__link:hover {
  text-decoration: underline;
}
</style>

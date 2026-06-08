<script setup lang="ts">
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardInventoryRiskAttentionItem } from '@/models/dashboard'
import { formatCurrency, formatNumber } from '@/services/formatters'
import { navigateToReport } from '@/services/navigateToReport'

defineProps<{
  items: DashboardInventoryRiskAttentionItem[]
  loading: boolean
}>()

const router = useRouter()

function formatDays(days: number | null | undefined): string {
  if (days == null) {
    return 'Never'
  }

  return formatNumber(days)
}

function openReport(item: DashboardInventoryRiskAttentionItem): void {
  navigateToReport(router, item.ReportRoute, item.BrgName)
}

function onRowClick(event: { data: DashboardInventoryRiskAttentionItem }): void {
  openReport(event.data)
}
</script>

<template>
  <Card class="inventory-risk-attention-list">
    <template #title>
      <div class="inventory-risk-attention-list__title">
        <i class="pi pi-exclamation-triangle" aria-hidden="true" />
        <span>Inventory Attention List</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="inventory-risk-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <DataTable
        v-else
        :value="items"
        striped-rows
        class="inventory-risk-attention-list__table inventory-risk-attention-list__table--clickable"
        @row-click="onRowClick"
      >
        <template #empty>
          <p class="inventory-risk-attention-list__empty">No inventory items require attention.</p>
        </template>

        <Column field="BrgCode" header="Code" />
        <Column field="BrgName" header="Item" />
        <Column field="KategoriName" header="Category" />
        <Column field="SupplierName" header="Supplier" />
        <Column field="Qty" header="Qty" />
        <Column header="Value">
          <template #body="{ data }">
            {{ formatCurrency(data.InventoryValue) }}
          </template>
        </Column>
        <Column header="Days Since Last Faktur">
          <template #body="{ data }">
            {{ formatDays(data.DaysSinceLastFaktur) }}
          </template>
        </Column>
        <Column field="SignalLabel" header="Signal" />
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.inventory-risk-attention-list__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.inventory-risk-attention-list__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.inventory-risk-attention-list__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.inventory-risk-attention-list__table--clickable :deep(.p-datatable-tbody > tr) {
  cursor: pointer;
}
</style>

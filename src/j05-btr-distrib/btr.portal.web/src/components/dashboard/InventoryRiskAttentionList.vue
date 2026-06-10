<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import ProgressSpinner from 'primevue/progressspinner'
import SelectButton from 'primevue/selectbutton'
import type { DashboardInventoryRiskAttentionItem } from '@/models/dashboard'
import { formatCurrency, formatNumber } from '@/services/formatters'
import {
  countInventoryRiskAttentionBySignal,
  filterInventoryRiskAttentionItems,
  INVENTORY_RISK_ATTENTION_SIGNAL_ALL,
  INVENTORY_RISK_ATTENTION_SIGNAL_KEYS,
  INVENTORY_RISK_ATTENTION_SIGNAL_LABELS,
} from '@/services/inventoryRiskAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

const props = defineProps<{
  items: DashboardInventoryRiskAttentionItem[]
  loading: boolean
}>()

const signalFilter = defineModel<string>('signalFilter', { default: INVENTORY_RISK_ATTENTION_SIGNAL_ALL })

const router = useRouter()
const route = useRoute()
const first = ref(0)
const rows = ref(25)

const signalCounts = computed(() => countInventoryRiskAttentionBySignal(props.items))

const filterOptions = computed(() => [
  { label: `All (${props.items.length})`, value: INVENTORY_RISK_ATTENTION_SIGNAL_ALL },
  ...INVENTORY_RISK_ATTENTION_SIGNAL_KEYS.map((key) => ({
    label: `${INVENTORY_RISK_ATTENTION_SIGNAL_LABELS[key]} (${signalCounts.value[key]})`,
    value: key,
  })),
])

const filteredItems = computed(() =>
  filterInventoryRiskAttentionItems(props.items, signalFilter.value),
)

const emptyMessage = computed(() => {
  if (props.items.length === 0) {
    return 'No inventory items require attention.'
  }

  return 'No inventory items match this signal.'
})

watch(signalFilter, () => {
  first.value = 0
})

watch(
  () => props.items,
  () => {
    first.value = 0
  },
)

function formatDays(days: number | null | undefined): string {
  if (days == null) {
    return 'Never'
  }

  return formatNumber(days)
}

function investigate(item: DashboardInventoryRiskAttentionItem): void {
  if (!item.Investigation) return
  navigateToInvestigation(
    router,
    item.Investigation,
    resolveInvestigationSourceLabel(route.path),
  )
}

function onRowClick(event: { data: DashboardInventoryRiskAttentionItem }): void {
  investigate(event.data)
}
</script>

<template>
  <Card class="inventory-risk-attention-list">
    <template #title>
      <div class="inventory-risk-attention-list__title">
        <div class="inventory-risk-attention-list__title-row">
          <i class="pi pi-exclamation-triangle" aria-hidden="true" />
          <span>Inventory Attention List</span>
        </div>
        <span v-if="!loading" class="inventory-risk-attention-list__count">
          {{ items.length }} attention {{ items.length === 1 ? 'item' : 'items' }}
        </span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="inventory-risk-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div class="inventory-risk-attention-list__filters">
          <SelectButton
            v-model="signalFilter"
            :options="filterOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            aria-label="Filter by attention signal"
          />
        </div>

        <div class="inventory-risk-attention-list__table-panel">
          <DataTable
            v-model:first="first"
            :value="filteredItems"
            paginator
            :rows="rows"
            :rows-per-page-options="[10, 25, 50]"
            striped-rows
            class="inventory-risk-attention-list__table inventory-risk-attention-list__table--clickable"
            @row-click="onRowClick"
          >
            <template #empty>
              <p class="inventory-risk-attention-list__empty">{{ emptyMessage }}</p>
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
        </div>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.inventory-risk-attention-list__title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  width: 100%;
}

.inventory-risk-attention-list__title-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.inventory-risk-attention-list__count {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.inventory-risk-attention-list__filters {
  margin-bottom: 1rem;
}

.inventory-risk-attention-list__table-panel {
  max-height: 28rem;
  overflow: auto;
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

<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import SelectButton from 'primevue/selectbutton'
import type { DashboardCustomerAttentionItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  countCustomerAttentionBySignal,
  CUSTOMER_ATTENTION_SIGNAL_ALL,
  CUSTOMER_ATTENTION_SIGNAL_KEYS,
  CUSTOMER_ATTENTION_SIGNAL_LABELS,
  filterCustomerAttentionItems,
} from '@/services/customerAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

const props = defineProps<{
  items: DashboardCustomerAttentionItem[]
  loading: boolean
}>()

const signalFilter = defineModel<string>('signalFilter', { default: CUSTOMER_ATTENTION_SIGNAL_ALL })

const router = useRouter()
const route = useRoute()
const first = ref(0)
const rows = ref(25)

const signalCounts = computed(() => countCustomerAttentionBySignal(props.items))

const filterOptions = computed(() => [
  { label: `All (${props.items.length})`, value: CUSTOMER_ATTENTION_SIGNAL_ALL },
  ...CUSTOMER_ATTENTION_SIGNAL_KEYS.map((key) => ({
    label: `${CUSTOMER_ATTENTION_SIGNAL_LABELS[key]} (${signalCounts.value[key]})`,
    value: key,
  })),
])

const filteredItems = computed(() =>
  filterCustomerAttentionItems(props.items, signalFilter.value),
)

const emptyMessage = computed(() => {
  if (props.items.length === 0) {
    return 'No customers require attention.'
  }

  return 'No customers match this signal.'
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

function formatValue(item: DashboardCustomerAttentionItem): string {
  if (item.ValueText) {
    return item.ValueText
  }

  if (item.ValueAmount != null) {
    return formatCurrency(item.ValueAmount)
  }

  return '—'
}

function investigate(item: DashboardCustomerAttentionItem): void {
  if (!item.Investigation) return
  navigateToInvestigation(
    router,
    item.Investigation,
    resolveInvestigationSourceLabel(route.path),
  )
}
</script>

<template>
  <Card class="customer-attention-list">
    <template #title>
      <div class="customer-attention-list__title">
        <div class="customer-attention-list__title-row">
          <i class="pi pi-exclamation-triangle" aria-hidden="true" />
          <span>Customer Attention List</span>
        </div>
        <span v-if="!loading" class="customer-attention-list__count">
          {{ items.length }} attention {{ items.length === 1 ? 'item' : 'items' }}
        </span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div class="customer-attention-list__filters">
          <SelectButton
            v-model="signalFilter"
            :options="filterOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            aria-label="Filter by attention signal"
          />
          <p class="customer-attention-list__hint">
            Cards count customers; this list counts customer × signal rows.
          </p>
        </div>

        <div class="customer-attention-list__table-panel">
          <DataTable
            v-model:first="first"
            :value="filteredItems"
            paginator
            :rows="rows"
            :rows-per-page-options="[10, 25, 50]"
            striped-rows
            class="customer-attention-list__table"
          >
            <template #empty>
              <p class="customer-attention-list__empty">{{ emptyMessage }}</p>
            </template>

            <Column field="CustomerCode" header="Code" />
            <Column field="CustomerName" header="Customer" />
            <Column field="SignalLabel" header="Signal" />
            <Column header="Value">
              <template #body="{ data }">
                {{ formatValue(data) }}
              </template>
            </Column>
            <Column field="WilayahName" header="Wilayah" />
            <Column header="">
              <template #body="{ data }">
                <Button
                  v-if="data.Investigation"
                  label="Investigate"
                  text
                  size="small"
                  @click="investigate(data)"
                />
              </template>
            </Column>
          </DataTable>
        </div>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-attention-list__title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  width: 100%;
}

.customer-attention-list__title-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-attention-list__count {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.customer-attention-list__filters {
  margin-bottom: 1rem;
}

.customer-attention-list__hint {
  margin: 0.5rem 0 0;
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.customer-attention-list__table-panel {
  max-height: 28rem;
  overflow: auto;
}

.customer-attention-list__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-attention-list__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

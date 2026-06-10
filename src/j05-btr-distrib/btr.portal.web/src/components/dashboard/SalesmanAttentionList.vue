<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import SelectButton from 'primevue/selectbutton'
import type { DashboardSalesmanAttentionItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  countSalesmanAttentionBySignal,
  filterSalesmanAttentionItems,
  SALESMAN_ATTENTION_SIGNAL_ALL,
  SALESMAN_ATTENTION_SIGNAL_KEYS,
  SALESMAN_ATTENTION_SIGNAL_LABELS,
} from '@/services/salesmanAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

const props = defineProps<{
  items: DashboardSalesmanAttentionItem[]
  loading: boolean
}>()

const emit = defineEmits<{
  salesmanClick: [item: DashboardSalesmanAttentionItem]
}>()

const signalFilter = defineModel<string>('signalFilter', { default: SALESMAN_ATTENTION_SIGNAL_ALL })

const router = useRouter()
const route = useRoute()
const first = ref(0)
const rows = ref(25)

const signalCounts = computed(() => countSalesmanAttentionBySignal(props.items))

const filterOptions = computed(() => [
  { label: `All (${props.items.length})`, value: SALESMAN_ATTENTION_SIGNAL_ALL },
  ...SALESMAN_ATTENTION_SIGNAL_KEYS.map((key) => ({
    label: `${SALESMAN_ATTENTION_SIGNAL_LABELS[key]} (${signalCounts.value[key]})`,
    value: key,
  })),
])

const filteredItems = computed(() =>
  filterSalesmanAttentionItems(props.items, signalFilter.value),
)

const emptyMessage = computed(() => {
  if (props.items.length === 0) {
    return 'No salesmen require attention.'
  }

  return 'No salesmen match this signal.'
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

function formatValue(item: DashboardSalesmanAttentionItem): string {
  if (item.ValueText) {
    return item.ValueText
  }

  if (item.ValueAmount != null) {
    return formatCurrency(item.ValueAmount)
  }

  return '—'
}

function investigate(item: DashboardSalesmanAttentionItem): void {
  if (!item.Investigation) return
  navigateToInvestigation(
    router,
    item.Investigation,
    resolveInvestigationSourceLabel(route.path),
  )
}
</script>

<template>
  <Card class="salesman-attention-list">
    <template #title>
      <div class="salesman-attention-list__title">
        <div class="salesman-attention-list__title-row">
          <i class="pi pi-exclamation-triangle" aria-hidden="true" />
          <span>Salesman Attention List</span>
        </div>
        <span v-if="!loading" class="salesman-attention-list__count">
          {{ items.length }} attention {{ items.length === 1 ? 'item' : 'items' }}
        </span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="salesman-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div class="salesman-attention-list__filters">
          <SelectButton
            v-model="signalFilter"
            :options="filterOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            aria-label="Filter by attention signal"
          />
          <p class="salesman-attention-list__hint">
            Cards count salesmen; this list counts salesman × signal rows.
          </p>
        </div>

        <div class="salesman-attention-list__table-panel">
          <DataTable
            v-model:first="first"
            :value="filteredItems"
            paginator
            :rows="rows"
            :rows-per-page-options="[10, 25, 50]"
            striped-rows
            class="salesman-attention-list__table"
          >
            <template #empty>
              <p class="salesman-attention-list__empty">{{ emptyMessage }}</p>
            </template>

            <Column field="SalesPersonCode" header="Code" />
            <Column header="Salesman">
              <template #body="{ data }">
                <button
                  type="button"
                  class="salesman-attention-list__name-link"
                  @click="emit('salesmanClick', data)"
                >
                  {{ data.SalesPersonName }}
                </button>
              </template>
            </Column>
            <Column field="SignalLabel" header="Signal" />
            <Column header="Detail">
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
.salesman-attention-list__title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  width: 100%;
}

.salesman-attention-list__title-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.salesman-attention-list__count {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.salesman-attention-list__filters {
  margin-bottom: 1rem;
}

.salesman-attention-list__hint {
  margin: 0.5rem 0 0;
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.salesman-attention-list__table-panel {
  max-height: 28rem;
  overflow: auto;
}

.salesman-attention-list__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.salesman-attention-list__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.salesman-attention-list__name-link {
  padding: 0;
  border: none;
  background: none;
  color: var(--p-primary-color);
  font: inherit;
  font-weight: 600;
  cursor: pointer;
  text-align: left;
}

.salesman-attention-list__name-link:hover {
  text-decoration: underline;
}
</style>

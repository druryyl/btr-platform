<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Button from 'primevue/button'
import Paginator from 'primevue/paginator'
import ProgressSpinner from 'primevue/progressspinner'
import SelectButton from 'primevue/selectbutton'
import type { DashboardCollectionAttentionItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  countCollectionAttentionBySignal,
  COLLECTION_ATTENTION_SIGNAL_ALL,
  COLLECTION_ATTENTION_SIGNAL_KEYS,
  COLLECTION_ATTENTION_SIGNAL_LABELS,
  filterCollectionAttentionItems,
} from '@/services/collectionAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

const props = defineProps<{
  items: DashboardCollectionAttentionItem[]
  loading: boolean
}>()

const signalFilter = defineModel<string>('signalFilter', { default: COLLECTION_ATTENTION_SIGNAL_ALL })

const router = useRouter()
const route = useRoute()
const first = ref(0)
const rows = ref(25)

const signalCounts = computed(() => countCollectionAttentionBySignal(props.items))

const filterOptions = computed(() => [
  { label: `All (${props.items.length})`, value: COLLECTION_ATTENTION_SIGNAL_ALL },
  ...COLLECTION_ATTENTION_SIGNAL_KEYS.map((key) => ({
    label: `${COLLECTION_ATTENTION_SIGNAL_LABELS[key]} (${signalCounts.value[key]})`,
    value: key,
  })),
])

const filteredItems = computed(() =>
  filterCollectionAttentionItems(props.items, signalFilter.value),
)

const paginatedItems = computed(() =>
  filteredItems.value.slice(first.value, first.value + rows.value),
)

const pageReport = computed(() => {
  const total = filteredItems.value.length
  if (total === 0) {
    return 'No rows to display'
  }

  const start = first.value + 1
  const end = Math.min(first.value + rows.value, total)
  return `Showing ${start}–${end} of ${total}`
})

const emptyMessage = computed(() => {
  if (props.items.length === 0) {
    return 'No collection attention items.'
  }

  return 'No items match this signal.'
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

watch(filteredItems, (items) => {
  if (first.value >= items.length && items.length > 0) {
    first.value = 0
  }
})

function onPage(event: { first: number; rows: number }): void {
  first.value = event.first
  rows.value = event.rows
}

function formatValue(item: DashboardCollectionAttentionItem): string {
  if (item.ValueText) {
    return item.ValueText
  }

  if (item.ValueAmount != null) {
    return formatCurrency(item.ValueAmount)
  }

  return '—'
}

function canOpenReport(item: DashboardCollectionAttentionItem): boolean {
  return item.ReportRoute != null && item.EntityType !== 'Wilayah'
}

function investigate(item: DashboardCollectionAttentionItem): void {
  if (!canOpenReport(item) || !item.Investigation) return
  navigateToInvestigation(
    router,
    item.Investigation,
    resolveInvestigationSourceLabel(route.path),
  )
}
</script>

<template>
  <Card class="collection-attention-list">
    <template #title>
      <div class="collection-attention-list__title">
        <div class="collection-attention-list__title-row">
          <i class="pi pi-exclamation-triangle" aria-hidden="true" />
          <span>Collection Attention List</span>
        </div>
        <span v-if="!loading" class="collection-attention-list__count">
          {{ items.length }} attention {{ items.length === 1 ? 'item' : 'items' }}
        </span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="collection-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div class="collection-attention-list__filters">
          <p class="collection-attention-list__filter-label">Filter by signal</p>
          <SelectButton
            v-model="signalFilter"
            :options="filterOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            aria-label="Filter by attention signal"
            class="collection-attention-list__signal-select"
          />
          <p class="collection-attention-list__hint">
            Cards count entities; this list counts entity × signal rows.
          </p>
        </div>

        <p v-if="filteredItems.length > 0" class="collection-attention-list__page-report">
          {{ pageReport }}
        </p>

        <div class="collection-attention-list__table-panel">
          <DataTable
            :value="paginatedItems"
            striped-rows
            scrollable
            scroll-height="flex"
            class="collection-attention-list__table"
          >
            <template #empty>
              <p class="collection-attention-list__empty">{{ emptyMessage }}</p>
            </template>

            <Column field="EntityType" header="Type" />
            <Column field="EntityName" header="Name" />
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
                  v-if="canOpenReport(data) && data.Investigation"
                  label="Investigate"
                  text
                  size="small"
                  @click="investigate(data)"
                />
              </template>
            </Column>
          </DataTable>
        </div>

        <Paginator
          v-if="filteredItems.length > 0"
          class="collection-attention-list__paginator"
          :first="first"
          :rows="rows"
          :total-records="filteredItems.length"
          :rows-per-page-options="[10, 25, 50]"
          template="FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          @page="onPage"
        />
      </template>
    </template>
  </Card>
</template>

<style scoped>
.collection-attention-list__title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  width: 100%;
}

.collection-attention-list__title-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.collection-attention-list__count {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.collection-attention-list__filters {
  margin-bottom: 1rem;
}

.collection-attention-list__filter-label {
  margin: 0 0 0.5rem;
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--p-text-color);
}

.collection-attention-list__signal-select {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.collection-attention-list__hint {
  margin: 0.5rem 0 0;
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.collection-attention-list__page-report {
  margin: 0 0 0.75rem;
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--p-text-muted-color);
}

.collection-attention-list__table-panel {
  max-height: 28rem;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.collection-attention-list__table {
  flex: 1;
  min-height: 0;
}

.collection-attention-list__paginator {
  margin-top: 0.75rem;
}

.collection-attention-list__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.collection-attention-list__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

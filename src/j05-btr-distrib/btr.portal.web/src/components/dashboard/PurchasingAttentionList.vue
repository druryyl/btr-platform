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
import type { DashboardPurchasingAttentionItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  countPurchasingAttentionBySignal,
  PURCHASING_ATTENTION_SIGNAL_ALL,
  PURCHASING_ATTENTION_SIGNAL_KEYS,
  PURCHASING_ATTENTION_SIGNAL_LABELS,
  filterPurchasingAttentionItems,
} from '@/services/purchasingAttentionSignals'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

const props = defineProps<{
  items: DashboardPurchasingAttentionItem[]
  loading: boolean
}>()

const signalFilter = defineModel<string>('signalFilter', { default: PURCHASING_ATTENTION_SIGNAL_ALL })

const router = useRouter()
const route = useRoute()
const first = ref(0)
const rows = ref(25)

const signalCounts = computed(() => countPurchasingAttentionBySignal(props.items))

const filterOptions = computed(() => [
  { label: `All (${props.items.length})`, value: PURCHASING_ATTENTION_SIGNAL_ALL },
  ...PURCHASING_ATTENTION_SIGNAL_KEYS.map((key) => ({
    label: `${PURCHASING_ATTENTION_SIGNAL_LABELS[key]} (${signalCounts.value[key]})`,
    value: key,
  })),
])

const filteredItems = computed(() =>
  filterPurchasingAttentionItems(props.items, signalFilter.value),
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
    return 'No purchasing attention items.'
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

function formatValue(item: DashboardPurchasingAttentionItem): string {
  if (item.ValueText) {
    return item.ValueText
  }

  if (item.ValueAmount != null) {
    return formatCurrency(item.ValueAmount)
  }

  return '—'
}

function canOpenReport(item: DashboardPurchasingAttentionItem): boolean {
  return item.ReportRoute != null && item.EntityType !== 'Company'
}

function openProfile(item: DashboardPurchasingAttentionItem): void {
  if (!item.ProfileRoute) return
  void router.push(item.ProfileRoute)
}

function investigate(item: DashboardPurchasingAttentionItem): void {
  if (!canOpenReport(item) || !item.Investigation) return
  navigateToInvestigation(
    router,
    item.Investigation,
    resolveInvestigationSourceLabel(route.path),
  )
}
</script>

<template>
  <Card class="purchasing-attention-list">
    <template #title>
      <div class="purchasing-attention-list__title">
        <div class="purchasing-attention-list__title-row">
          <i class="pi pi-exclamation-triangle" aria-hidden="true" />
          <span>Purchasing Attention List</span>
        </div>
        <span v-if="!loading" class="purchasing-attention-list__count">
          {{ items.length }} attention {{ items.length === 1 ? 'item' : 'items' }}
        </span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="purchasing-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div class="purchasing-attention-list__filters">
          <p class="purchasing-attention-list__filter-label">Filter by signal</p>
          <SelectButton
            v-model="signalFilter"
            :options="filterOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            aria-label="Filter by attention signal"
            class="purchasing-attention-list__signal-select"
          />
          <p class="purchasing-attention-list__hint">
            Cards count principals; this list counts entity × signal rows.
          </p>
        </div>

        <p v-if="filteredItems.length > 0" class="purchasing-attention-list__page-report">
          {{ pageReport }}
        </p>

        <div class="purchasing-attention-list__table-panel">
          <DataTable
            :value="paginatedItems"
            striped-rows
            scrollable
            scroll-height="flex"
            class="purchasing-attention-list__table"
          >
            <template #empty>
              <p class="purchasing-attention-list__empty">{{ emptyMessage }}</p>
            </template>

            <Column field="EntityName" header="Entity">
              <template #body="{ data }">
                <button
                  v-if="data.ProfileRoute"
                  type="button"
                  class="purchasing-attention-list__entity-link"
                  @click="openProfile(data)"
                >
                  {{ data.EntityName }}
                </button>
                <span v-else>{{ data.EntityName }}</span>
              </template>
            </Column>
            <Column field="SignalLabel" header="Signal" />
            <Column header="Amount">
              <template #body="{ data }">
                {{ data.ValueAmount != null ? formatCurrency(data.ValueAmount) : '—' }}
              </template>
            </Column>
            <Column header="Context">
              <template #body="{ data }">
                {{ formatValue(data) }}
              </template>
            </Column>
            <Column header="">
              <template #body="{ data }">
                <Button
                  v-if="data.ProfileRoute"
                  label="Profile"
                  icon="pi pi-id-card"
                  text
                  size="small"
                  title="Open Performance Profile"
                  @click="openProfile(data)"
                />
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
          class="purchasing-attention-list__paginator"
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
.purchasing-attention-list__title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  width: 100%;
}

.purchasing-attention-list__title-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.purchasing-attention-list__count {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.purchasing-attention-list__filters {
  margin-bottom: 1rem;
}

.purchasing-attention-list__filter-label {
  margin: 0 0 0.5rem;
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--p-text-color);
}

.purchasing-attention-list__signal-select {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.purchasing-attention-list__hint {
  margin: 0.5rem 0 0;
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.purchasing-attention-list__page-report {
  margin: 0 0 0.75rem;
  font-size: 0.8125rem;
  font-weight: 600;
  color: var(--p-text-muted-color);
}

.purchasing-attention-list__table-panel {
  max-height: 28rem;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

.purchasing-attention-list__table {
  flex: 1;
  min-height: 0;
}

.purchasing-attention-list__paginator {
  margin-top: 0.75rem;
}

.purchasing-attention-list__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.purchasing-attention-list__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.purchasing-attention-list__entity-link {
  padding: 0;
  border: 0;
  background: none;
  color: var(--p-primary-color);
  cursor: pointer;
  text-align: left;
}
</style>

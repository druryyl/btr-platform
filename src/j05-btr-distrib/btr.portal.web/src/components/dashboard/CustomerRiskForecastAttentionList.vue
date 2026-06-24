<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import ProgressSpinner from 'primevue/progressspinner'
import SelectButton from 'primevue/selectbutton'
import { RouterLink } from 'vue-router'
import type { DashboardCustomerRiskForecastAttentionItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  countCustomerRiskForecastAttentionBySignalFamily,
  CUSTOMER_RISK_FORECAST_SIGNAL_ALL,
  CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_KEYS,
  CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_LABELS,
  filterCustomerRiskForecastAttentionItems,
} from '@/services/customerRiskForecastSignals'

const props = defineProps<{
  items: DashboardCustomerRiskForecastAttentionItem[]
  loading: boolean
}>()

const signalFilter = defineModel<string>('signalFilter', {
  default: CUSTOMER_RISK_FORECAST_SIGNAL_ALL,
})

const first = ref(0)
const rows = ref(25)

const signalCounts = computed(() => countCustomerRiskForecastAttentionBySignalFamily(props.items))

const filterOptions = computed(() => [
  { label: `All (${props.items.length})`, value: CUSTOMER_RISK_FORECAST_SIGNAL_ALL },
  ...CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_KEYS.map((key) => ({
    label: `${CUSTOMER_RISK_FORECAST_SIGNAL_FAMILY_LABELS[key]} (${signalCounts.value[key]})`,
    value: key,
  })),
])

const filteredItems = computed(() =>
  filterCustomerRiskForecastAttentionItems(props.items, signalFilter.value),
)

const emptyMessage = computed(() => {
  if (props.items.length === 0) {
    return 'No customer risk signals require attention.'
  }

  return 'No signals match this family.'
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

function formatValue(item: DashboardCustomerRiskForecastAttentionItem): string {
  if (item.Amount != null) {
    return formatCurrency(item.Amount)
  }

  return '—'
}
</script>

<template>
  <Card class="customer-risk-forecast-attention-list">
    <template #title>
      <div class="customer-risk-forecast-attention-list__title">
        <div class="customer-risk-forecast-attention-list__title-row">
          <i class="pi pi-exclamation-triangle" aria-hidden="true" />
          <span>Customer Risk Attention List</span>
        </div>
        <span v-if="!loading" class="customer-risk-forecast-attention-list__count">
          {{ items.length }} attention {{ items.length === 1 ? 'item' : 'items' }}
        </span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-risk-forecast-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <div class="customer-risk-forecast-attention-list__filters">
          <SelectButton
            v-model="signalFilter"
            :options="filterOptions"
            option-label="label"
            option-value="value"
            :allow-empty="false"
            aria-label="Filter by signal family"
          />
          <p class="customer-risk-forecast-attention-list__hint">
            Deterministic rule-based signals for the forecast horizon.
          </p>
        </div>

        <div class="customer-risk-forecast-attention-list__table-panel">
          <DataTable
            v-model:first="first"
            :value="filteredItems"
            paginator
            :rows="rows"
            :rows-per-page-options="[10, 25, 50]"
            striped-rows
            class="customer-risk-forecast-attention-list__table"
          >
            <template #empty>
              <p class="customer-risk-forecast-attention-list__empty">{{ emptyMessage }}</p>
            </template>

            <Column field="CustomerCode" header="Code" />
            <Column field="CustomerName" header="Customer" />
            <Column field="SignalLabel" header="Signal" />
            <Column field="Severity" header="Severity" />
            <Column header="Amount">
              <template #body="{ data }">
                {{ formatValue(data) }}
              </template>
            </Column>
            <Column field="HorizonText" header="Horizon" />
            <Column field="Explanation" header="Explanation" />
            <Column header="">
              <template #body="{ data }">
                <RouterLink
                  v-if="data.ReportRoute"
                  :to="data.ReportRoute"
                  class="customer-risk-forecast-attention-list__link"
                >
                  Investigate
                </RouterLink>
              </template>
            </Column>
          </DataTable>
        </div>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-risk-forecast-attention-list__title {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  width: 100%;
}

.customer-risk-forecast-attention-list__title-row {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-risk-forecast-attention-list__count {
  font-size: 0.875rem;
  font-weight: 500;
  color: var(--p-text-muted-color);
}

.customer-risk-forecast-attention-list__filters {
  margin-bottom: 1rem;
}

.customer-risk-forecast-attention-list__hint {
  margin: 0.5rem 0 0;
  font-size: 0.8125rem;
  color: var(--p-text-muted-color);
}

.customer-risk-forecast-attention-list__table-panel {
  max-height: 28rem;
  overflow: auto;
}

.customer-risk-forecast-attention-list__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-risk-forecast-attention-list__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.customer-risk-forecast-attention-list__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.customer-risk-forecast-attention-list__link:hover {
  text-decoration: underline;
}
</style>

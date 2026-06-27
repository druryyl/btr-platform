<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import { RouterLink } from 'vue-router'
import { formatCurrency } from '@/services/formatters'
import type { DashboardCashFlowCollectionRiskItem } from '@/models/dashboard'

const props = defineProps<{
  risks: DashboardCashFlowCollectionRiskItem[]
  loading: boolean
}>()

const rows = computed(() => props.risks ?? [])
</script>

<template>
  <Card class="cash-flow-collection-risks-table">
    <template #title>
      <div class="cash-flow-collection-risks-table__title">
        <i class="pi pi-exclamation-triangle" aria-hidden="true" />
        <span>Top Collection Risks</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="cash-flow-collection-risks-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <DataTable
          v-if="rows.length > 0"
          :value="rows"
          striped-rows
          size="small"
          class="cash-flow-collection-risks-table__grid"
        >
          <Column field="RiskLabel" header="Risk Type" />
          <Column field="EntityName" header="Entity" />
          <Column header="Amount" body-class="dash-numeric" header-class="dash-numeric">
            <template #body="{ data }">
              {{ formatCurrency(data.Amount) }}
            </template>
          </Column>
          <Column field="DueOrAgingText" header="Due / Aging" />
          <Column field="RuleExplanation" header="Rule" />
          <Column header="">
            <template #body="{ data }">
              <RouterLink
                v-if="data.ReportRoute"
                :to="data.ReportRoute"
                class="cash-flow-collection-risks-table__link"
              >
                Investigate
              </RouterLink>
            </template>
          </Column>
        </DataTable>

        <p v-else class="cash-flow-collection-risks-table__empty">
          No collection risks identified for the current forecast refresh.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.cash-flow-collection-risks-table {
  border-radius: var(--dashboard-radius);
  box-shadow: var(--dashboard-shadow-idle);
  transition: box-shadow var(--dashboard-transition);
}

.cash-flow-collection-risks-table__grid :deep(.p-datatable-thead > tr > th) {
  background: var(--dashboard-table-header-bg);
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--p-text-muted-color);
  border-bottom: 1px solid var(--p-surface-200);
}

.cash-flow-collection-risks-table__grid :deep(.p-datatable-tbody > tr:hover) {
  background: var(--dashboard-table-row-hover);
}

.cash-flow-collection-risks-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.cash-flow-collection-risks-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.cash-flow-collection-risks-table__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.cash-flow-collection-risks-table__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.cash-flow-collection-risks-table__link:hover {
  text-decoration: underline;
}
</style>

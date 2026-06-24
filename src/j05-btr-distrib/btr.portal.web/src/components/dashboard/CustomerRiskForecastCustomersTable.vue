<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import ProgressSpinner from 'primevue/progressspinner'
import { RouterLink } from 'vue-router'
import { formatCurrency, formatPercent } from '@/services/formatters'
import type { DashboardCustomerRiskForecastCustomerItem } from '@/models/dashboard'
import { categoryBadgeSeverity } from '@/services/customerRiskForecastSignals'

const props = defineProps<{
  customers: DashboardCustomerRiskForecastCustomerItem[]
  loading: boolean
}>()

const rows = computed(() => props.customers ?? [])
</script>

<template>
  <Card class="customer-risk-forecast-customers-table">
    <template #title>
      <div class="customer-risk-forecast-customers-table__title">
        <i class="pi pi-users" aria-hidden="true" />
        <span>Top Customers by Risk Priority</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-risk-forecast-customers-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <template v-else>
        <DataTable
          v-if="rows.length > 0"
          :value="rows"
          striped-rows
          size="small"
          paginator
          :rows="10"
          class="customer-risk-forecast-customers-table__grid"
        >
          <Column header="Priority" field="RiskPriorityScore" />
          <Column header="Category">
            <template #body="{ data }">
              <Tag
                :severity="categoryBadgeSeverity(data.Category)"
                :value="data.CategoryLabel"
              />
            </template>
          </Column>
          <Column field="CustomerCode" header="Code" />
          <Column field="CustomerName" header="Customer" />
          <Column field="WilayahName" header="Wilayah" />
          <Column field="SalesPersonName" header="Salesman" />
          <Column header="Open Balance">
            <template #body="{ data }">
              {{ formatCurrency(data.OpenBalance) }}
            </template>
          </Column>
          <Column header="Overdue">
            <template #body="{ data }">
              {{ formatCurrency(data.OverdueBalance) }}
            </template>
          </Column>
          <Column header="Due in Horizon">
            <template #body="{ data }">
              {{ formatCurrency(data.DueWithinHorizon) }}
            </template>
          </Column>
          <Column field="PrimarySignalLabel" header="Primary Signal" />
          <Column header="Decline">
            <template #body="{ data }">
              {{ data.DeclineRatio != null ? formatPercent(data.DeclineRatio * 100) : '—' }}
            </template>
          </Column>
          <Column header="">
            <template #body="{ data }">
              <RouterLink
                v-if="data.DrillDownRoute"
                :to="data.DrillDownRoute"
                class="customer-risk-forecast-customers-table__link"
              >
                Drill down
              </RouterLink>
              <RouterLink
                v-else-if="data.ReportRoute"
                :to="data.ReportRoute"
                class="customer-risk-forecast-customers-table__link"
              >
                Report
              </RouterLink>
            </template>
          </Column>
        </DataTable>

        <p v-else class="customer-risk-forecast-customers-table__empty">
          No elevated-risk customers identified for the current forecast refresh.
        </p>
      </template>
    </template>
  </Card>
</template>

<style scoped>
.customer-risk-forecast-customers-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-risk-forecast-customers-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-risk-forecast-customers-table__empty {
  margin: 0;
  padding: 2rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.customer-risk-forecast-customers-table__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
}

.customer-risk-forecast-customers-table__link:hover {
  text-decoration: underline;
}
</style>

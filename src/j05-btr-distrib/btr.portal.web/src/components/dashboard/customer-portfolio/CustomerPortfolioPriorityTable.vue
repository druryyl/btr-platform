<script setup lang="ts">
import { ref } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import { RouterLink } from 'vue-router'
import type { DashboardCustomerPortfolioPriorityRow } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  actionBadgeSeverity,
  actionOwnerBadgeSeverity,
} from '@/services/customerPortfolioSignals'

defineProps<{
  rows: DashboardCustomerPortfolioPriorityRow[]
  loading: boolean
}>()

const expandedRows = ref<DashboardCustomerPortfolioPriorityRow[]>([])
</script>

<template>
  <Card class="customer-portfolio-priority-table">
    <template #title>Portfolio Priority Queue</template>
    <template #content>
      <DataTable
        v-model:expandedRows="expandedRows"
        :value="rows"
        :loading="loading"
        data-key="SortOrder"
        size="small"
        striped-rows
        paginator
        :rows="15"
      >
        <Column expander style="width: 3rem" />
        <Column field="SortOrder" header="#" style="width: 3rem" />
        <Column field="CustomerCode" header="Code" />
        <Column field="CustomerName" header="Customer" />
        <Column field="TierLabel" header="Tier" />
        <Column field="LifecycleLabel" header="Lifecycle" />
        <Column header="Action">
          <template #body="{ data }">
            <Tag
              :value="data.PrimaryActionLabel"
              :severity="actionBadgeSeverity(data.PrimaryActionKey)"
            />
          </template>
        </Column>
        <Column field="PortfolioPriorityScore" header="Priority" />
        <Column header="MTD Omzet">
          <template #body="{ data }">{{ formatCurrency(data.MtdOmzet) }}</template>
        </Column>
        <Column header="Open Balance">
          <template #body="{ data }">{{ formatCurrency(data.OpenBalance) }}</template>
        </Column>
        <Column field="M29Category" header="Risk" />
        <Column header="Owner">
          <template #body="{ data }">
            <Tag
              v-if="data.ActionOwner"
              :value="data.ActionOwner"
              :severity="actionOwnerBadgeSeverity(data.ActionOwner)"
            />
          </template>
        </Column>
        <Column header="Links">
          <template #body="{ data }">
            <div class="customer-portfolio-priority-table__links">
              <RouterLink
                v-if="data.CustomerReportRoute"
                :to="data.CustomerReportRoute"
                class="customer-portfolio-priority-table__link"
              >
                Report
              </RouterLink>
              <RouterLink
                v-if="data.M30LinkRoute && data.PrimaryActionKey === 'Collect'"
                :to="data.M30LinkRoute"
                class="customer-portfolio-priority-table__link"
              >
                M30
              </RouterLink>
              <RouterLink
                v-if="data.DrillDownRouteM29"
                :to="data.DrillDownRouteM29"
                class="customer-portfolio-priority-table__link"
              >
                M29
              </RouterLink>
            </div>
          </template>
        </Column>
        <template #expansion="{ data }">
          <div class="customer-portfolio-priority-table__detail">
            <p><strong>Reason:</strong> {{ data.ActionReasonText }}</p>
            <p><strong>Rules:</strong> {{ data.TriggeredRuleIds }}</p>
            <p><strong>Salesman:</strong> {{ data.SalesPersonName || '—' }}</p>
            <p><strong>Wilayah:</strong> {{ data.WilayahName || '—' }}</p>
          </div>
        </template>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.customer-portfolio-priority-table__detail {
  padding: 0.5rem 1rem 1rem;
  font-size: 0.9rem;
  line-height: 1.5;
}

.customer-portfolio-priority-table__detail p {
  margin: 0 0 0.5rem;
}

.customer-portfolio-priority-table__links {
  display: flex;
  flex-wrap: wrap;
  gap: 0.5rem;
}

.customer-portfolio-priority-table__link {
  color: var(--p-primary-color);
  font-weight: 600;
  text-decoration: none;
  font-size: 0.85rem;
}

.customer-portfolio-priority-table__link:hover {
  text-decoration: underline;
}
</style>

<script setup lang="ts">
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import type { DashboardCollectionOptimizationImpactItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import { actionCategoryBadgeSeverity } from '@/services/collectionOptimizationSignals'
import {
  CU03_ROUTED_SALESMAN_LABEL,
  CU03_ROUTED_SALESMAN_NOTE,
} from '@/services/collectionOptimizationRouting'

defineProps<{
  rows: DashboardCollectionOptimizationImpactItem[]
  loading: boolean
}>()
</script>

<template>
  <Card class="collection-optimization-impact-table">
    <template #title>
      <span>Top Impact Opportunities</span>
      <p class="collection-optimization-impact-table__note">{{ CU03_ROUTED_SALESMAN_NOTE }}</p>
    </template>
    <template #content>
      <DataTable :value="rows" :loading="loading" size="small" striped-rows>
        <Column field="CustomerName" header="Customer" />
        <Column field="CollectionImpactAmount" header="Impact">
          <template #body="{ data }">{{ formatCurrency(data.CollectionImpactAmount) }}</template>
        </Column>
        <Column field="ActionCategoryLabel" header="Action">
          <template #body="{ data }">
            <Tag
              :value="data.ActionCategoryLabel"
              :severity="actionCategoryBadgeSeverity(data.ActionCategoryKey)"
            />
          </template>
        </Column>
        <Column field="OverdueBalance" header="Overdue">
          <template #body="{ data }">{{ formatCurrency(data.OverdueBalance) }}</template>
        </Column>
        <Column field="DueWithin7Days" header="Due 7d">
          <template #body="{ data }">{{ formatCurrency(data.DueWithin7Days) }}</template>
        </Column>
        <Column field="SalesPersonName" :header="CU03_ROUTED_SALESMAN_LABEL" />
        <Column field="WilayahName" header="Wilayah" />
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.collection-optimization-impact-table__note {
  margin: 0.5rem 0 0;
  font-size: 0.875rem;
  font-weight: 400;
  line-height: 1.5;
  color: var(--p-text-muted-color);
}
</style>

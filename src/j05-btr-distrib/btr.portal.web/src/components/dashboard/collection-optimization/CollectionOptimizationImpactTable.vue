<script setup lang="ts">
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import type { DashboardCollectionOptimizationImpactItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import { actionCategoryBadgeSeverity } from '@/services/collectionOptimizationSignals'

defineProps<{
  rows: DashboardCollectionOptimizationImpactItem[]
  loading: boolean
}>()
</script>

<template>
  <Card class="collection-optimization-impact-table">
    <template #title>Top Impact Opportunities</template>
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
        <Column field="SalesPersonName" header="Salesman" />
        <Column field="WilayahName" header="Wilayah" />
      </DataTable>
    </template>
  </Card>
</template>

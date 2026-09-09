<script setup lang="ts">
import { ref } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import Tag from 'primevue/tag'
import type { DashboardCollectionOptimizationPriorityItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  actionCategoryBadgeSeverity,
  actionOwnerBadgeSeverity,
} from '@/services/collectionOptimizationSignals'
import {
  CU03_ACTION_ROUTE_LABEL,
  CU03_ACTION_ROUTE_NOTE,
} from '@/services/collectionOptimizationRouting'

defineProps<{
  rows: DashboardCollectionOptimizationPriorityItem[]
  loading: boolean
}>()

const expandedRows = ref<DashboardCollectionOptimizationPriorityItem[]>([])
</script>

<template>
  <Card class="collection-optimization-priority-table">
    <template #title>
      <span>Today's Collection Priorities</span>
      <p class="collection-optimization-priority-table__note">{{ CU03_ACTION_ROUTE_NOTE }}</p>
    </template>
    <template #content>
      <DataTable
        v-model:expandedRows="expandedRows"
        :value="rows"
        :loading="loading"
        data-key="SortOrder"
        size="small"
        striped-rows
      >
        <Column expander style="width: 3rem" />
        <Column field="SortOrder" header="#" style="width: 3rem" />
        <Column field="CustomerName" header="Customer" />
        <Column field="ActionCategoryLabel" header="Action">
          <template #body="{ data }">
            <Tag
              :value="data.ActionCategoryLabel"
              :severity="actionCategoryBadgeSeverity(data.ActionCategoryKey)"
            />
          </template>
        </Column>
        <Column field="CollectionPriorityScore" header="Priority" />
        <Column field="CollectionImpactAmount" header="Impact">
          <template #body="{ data }">{{ formatCurrency(data.CollectionImpactAmount) }}</template>
        </Column>
        <Column field="M29Category" header="Risk" />
        <Column field="ActionOwner" :header="CU03_ACTION_ROUTE_LABEL">
          <template #body="{ data }">
            <Tag
              v-if="data.ActionOwner"
              :value="data.ActionOwner"
              :severity="actionOwnerBadgeSeverity(data.ActionOwner)"
            />
          </template>
        </Column>
        <template #expansion="{ data }">
          <div class="collection-optimization-priority-table__detail">
            <p><strong>Why selected:</strong> {{ data.SelectionReasonText }}</p>
            <p><strong>Why priority:</strong> {{ data.PriorityReasonText }}</p>
            <p><strong>Why action:</strong> {{ data.ActionReasonText }}</p>
            <p><strong>Rules:</strong> {{ data.TriggeredRuleIds }}</p>
          </div>
        </template>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.collection-optimization-priority-table__note {
  margin: 0.5rem 0 0;
  font-size: 0.875rem;
  font-weight: 400;
  line-height: 1.5;
  color: var(--p-text-muted-color);
}

.collection-optimization-priority-table__detail {
  padding: 0.5rem 1rem 1rem;
  font-size: 0.9rem;
  line-height: 1.5;
}

.collection-optimization-priority-table__detail p {
  margin: 0 0 0.5rem;
}
</style>

<script setup lang="ts">
import { computed, ref } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import SelectButton from 'primevue/selectbutton'
import Tag from 'primevue/tag'
import type { DashboardCollectionOptimizationQueueItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import {
  actionCategoryBadgeSeverity,
  COLLECTION_OPTIMIZATION_QUEUE_KEYS,
  COLLECTION_OPTIMIZATION_QUEUE_LABELS,
  filterQueuesByKey,
} from '@/services/collectionOptimizationSignals'

const props = defineProps<{
  queues: DashboardCollectionOptimizationQueueItem[]
  loading: boolean
}>()

const activeQueue = ref('ProactiveReminder')

const queueOptions = COLLECTION_OPTIMIZATION_QUEUE_KEYS.map((key) => ({
  label: COLLECTION_OPTIMIZATION_QUEUE_LABELS[key],
  value: key,
}))

const filteredRows = computed(() => filterQueuesByKey(props.queues ?? [], activeQueue.value))
</script>

<template>
  <Card class="collection-optimization-queue-tabs">
    <template #title>
      <div class="collection-optimization-queue-tabs__header">
        <span>Specialized Queues</span>
        <SelectButton
          v-model="activeQueue"
          :options="queueOptions"
          option-label="label"
          option-value="value"
          size="small"
        />
      </div>
    </template>
    <template #content>
      <DataTable :value="filteredRows" :loading="loading" size="small" striped-rows>
        <Column field="CustomerName" header="Customer" />
        <Column field="ActionCategoryLabel" header="Action">
          <template #body="{ data }">
            <Tag
              :value="data.ActionCategoryLabel"
              :severity="actionCategoryBadgeSeverity(data.ActionCategoryKey)"
            />
          </template>
        </Column>
        <Column field="CollectionImpactAmount" header="Impact">
          <template #body="{ data }">{{ formatCurrency(data.CollectionImpactAmount) }}</template>
        </Column>
        <Column field="M29Category" header="Risk" />
        <Column field="ActionOwner" header="Owner" />
        <Column field="QueueReasonText" header="Reason" />
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.collection-optimization-queue-tabs__header {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

@media (min-width: 768px) {
  .collection-optimization-queue-tabs__header {
    flex-direction: row;
    align-items: center;
    justify-content: space-between;
  }
}
</style>

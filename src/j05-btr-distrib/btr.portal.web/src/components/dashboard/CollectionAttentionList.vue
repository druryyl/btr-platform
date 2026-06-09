<script setup lang="ts">
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCollectionAttentionItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import { navigateToReport } from '@/services/navigateToReport'

defineProps<{
  items: DashboardCollectionAttentionItem[]
  loading: boolean
}>()

const router = useRouter()

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

function openReport(item: DashboardCollectionAttentionItem): void {
  if (!canOpenReport(item)) return
  navigateToReport(router, item.ReportRoute!, item.EntityName)
}
</script>

<template>
  <Card class="collection-attention-list">
    <template #title>
      <div class="collection-attention-list__title">
        <i class="pi pi-exclamation-triangle" aria-hidden="true" />
        <span>Collection Attention List</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="collection-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <DataTable
        v-else
        :value="items"
        striped-rows
        class="collection-attention-list__table"
      >
        <template #empty>
          <p class="collection-attention-list__empty">No collection attention items.</p>
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
              v-if="canOpenReport(data)"
              icon="pi pi-arrow-right"
              text
              rounded
              severity="secondary"
              aria-label="Open Piutang report"
              @click="openReport(data)"
            />
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.collection-attention-list__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
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

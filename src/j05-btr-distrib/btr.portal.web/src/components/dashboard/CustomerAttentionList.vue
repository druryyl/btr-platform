<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardCustomerAttentionItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

defineProps<{
  items: DashboardCustomerAttentionItem[]
  loading: boolean
}>()

const router = useRouter()
const route = useRoute()

function formatValue(item: DashboardCustomerAttentionItem): string {
  if (item.ValueText) {
    return item.ValueText
  }

  if (item.ValueAmount != null) {
    return formatCurrency(item.ValueAmount)
  }

  return '—'
}

function investigate(item: DashboardCustomerAttentionItem): void {
  if (!item.Investigation) return
  navigateToInvestigation(
    router,
    item.Investigation,
    resolveInvestigationSourceLabel(route.path),
  )
}
</script>

<template>
  <Card class="customer-attention-list">
    <template #title>
      <div class="customer-attention-list__title">
        <i class="pi pi-exclamation-triangle" aria-hidden="true" />
        <span>Customer Attention List</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="customer-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <DataTable
        v-else
        :value="items"
        striped-rows
        class="customer-attention-list__table"
      >
        <template #empty>
          <p class="customer-attention-list__empty">No customers require attention.</p>
        </template>

        <Column field="CustomerCode" header="Code" />
        <Column field="CustomerName" header="Customer" />
        <Column field="SignalLabel" header="Signal" />
        <Column header="Value">
          <template #body="{ data }">
            {{ formatValue(data) }}
          </template>
        </Column>
        <Column field="WilayahName" header="Wilayah" />
        <Column header="">
          <template #body="{ data }">
            <Button
              v-if="data.Investigation"
              label="Investigate"
              text
              size="small"
              @click="investigate(data)"
            />
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.customer-attention-list__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.customer-attention-list__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.customer-attention-list__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

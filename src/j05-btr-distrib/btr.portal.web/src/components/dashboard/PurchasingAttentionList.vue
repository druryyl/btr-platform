<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import Button from 'primevue/button'
import ProgressSpinner from 'primevue/progressspinner'
import type { DashboardPurchasingAttentionItem } from '@/models/dashboard'
import { formatCurrency } from '@/services/formatters'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'

defineProps<{
  items: DashboardPurchasingAttentionItem[]
  loading: boolean
}>()

const router = useRouter()
const route = useRoute()

function formatValue(item: DashboardPurchasingAttentionItem): string {
  if (item.ValueText) {
    return item.ValueText
  }

  if (item.ValueAmount != null) {
    return formatCurrency(item.ValueAmount)
  }

  return '—'
}

function canOpenReport(item: DashboardPurchasingAttentionItem): boolean {
  return item.ReportRoute != null && item.EntityType !== 'Company'
}

function investigate(item: DashboardPurchasingAttentionItem): void {
  if (!canOpenReport(item) || !item.Investigation) return
  navigateToInvestigation(
    router,
    item.Investigation,
    resolveInvestigationSourceLabel(route.path),
  )
}
</script>

<template>
  <Card class="purchasing-attention-list">
    <template #title>
      <div class="purchasing-attention-list__title">
        <i class="pi pi-exclamation-triangle" aria-hidden="true" />
        <span>Purchasing Attention List</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="purchasing-attention-list__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <DataTable
        v-else
        :value="items"
        striped-rows
        class="purchasing-attention-list__table"
      >
        <template #empty>
          <p class="purchasing-attention-list__empty">No purchasing attention items.</p>
        </template>

        <Column field="EntityName" header="Entity" />
        <Column field="SignalLabel" header="Signal" />
        <Column header="Amount">
          <template #body="{ data }">
            {{ data.ValueAmount != null ? formatCurrency(data.ValueAmount) : '—' }}
          </template>
        </Column>
        <Column header="Context">
          <template #body="{ data }">
            {{ formatValue(data) }}
          </template>
        </Column>
        <Column header="">
          <template #body="{ data }">
            <Button
              v-if="canOpenReport(data) && data.Investigation"
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
.purchasing-attention-list__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.purchasing-attention-list__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.purchasing-attention-list__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}
</style>

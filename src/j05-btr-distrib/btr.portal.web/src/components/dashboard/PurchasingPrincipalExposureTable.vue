<script setup lang="ts">
import { useRouter } from 'vue-router'
import Card from 'primevue/card'
import Column from 'primevue/column'
import DataTable from 'primevue/datatable'
import ProgressSpinner from 'primevue/progressspinner'
import Tag from 'primevue/tag'
import type { DashboardPurchasingPrincipalExposureItem } from '@/models/dashboard'
import { formatCurrency, formatPercent } from '@/services/formatters'
import { navigateToReport } from '@/services/navigateToReport'

defineProps<{
  items: DashboardPurchasingPrincipalExposureItem[]
  loading: boolean
}>()

const router = useRouter()

function onRowClick(event: { data: DashboardPurchasingPrincipalExposureItem }): void {
  const row = event.data
  if (row.ProfileRoute) {
    void router.push(row.ProfileRoute)
    return
  }

  if (row.ReportRoute && row.PrincipalName) {
    navigateToReport(router, row.ReportRoute, row.PrincipalName)
  }
}
</script>

<template>
  <Card class="purchasing-exposure-table">
    <template #title>
      <div class="purchasing-exposure-table__title">
        <i class="pi pi-chart-bar" aria-hidden="true" />
        <span>Principal Exposure Comparison</span>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="purchasing-exposure-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <DataTable
        v-else
        :value="items"
        striped-rows
        class="purchasing-exposure-table__table purchasing-exposure-table__table--clickable"
        @row-click="onRowClick"
      >
        <template #empty>
          <p class="purchasing-exposure-table__empty">No principal exposure data.</p>
        </template>

        <Column field="Rank" header="Rank" />
        <Column field="PrincipalName" header="Principal" />
        <Column header="MTD Purchase">
          <template #body="{ data }">
            {{ formatCurrency(data.MtdPurchaseAmount) }}
            <span v-if="data.PercentOfPurchase != null" class="purchasing-exposure-table__pct">
              ({{ formatPercent(data.PercentOfPurchase) }})
            </span>
          </template>
        </Column>
        <Column header="Inventory Value">
          <template #body="{ data }">
            {{
              data.InventoryValue != null
                ? `${formatCurrency(data.InventoryValue)}${data.PercentOfInventory != null ? ` (${formatPercent(data.PercentOfInventory)})` : ''}`
                : '—'
            }}
          </template>
        </Column>
        <Column header="At-Risk Value">
          <template #body="{ data }">
            {{
              data.AtRiskValue != null
                ? `${formatCurrency(data.AtRiskValue)}${data.PercentOfAtRisk != null ? ` (${formatPercent(data.PercentOfAtRisk)})` : ''}`
                : '—'
            }}
          </template>
        </Column>
        <Column header="Flags">
          <template #body="{ data }">
            <Tag v-if="data.IsCompoundDependency" value="Compound" severity="warn" class="purchasing-exposure-table__tag" />
            <Tag v-if="data.IsInventoryNoPurchase" value="No Purchase" severity="info" class="purchasing-exposure-table__tag" />
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.purchasing-exposure-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
}

.purchasing-exposure-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.purchasing-exposure-table__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.purchasing-exposure-table__table--clickable :deep(.p-datatable-tbody > tr) {
  cursor: pointer;
}

.purchasing-exposure-table__pct {
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.purchasing-exposure-table__tag {
  margin-right: 0.25rem;
}
</style>

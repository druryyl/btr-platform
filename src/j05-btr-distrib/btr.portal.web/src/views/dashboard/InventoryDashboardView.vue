<script setup lang="ts">
import { computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import DashboardDetailLayout from '@/components/dashboard/DashboardDetailLayout.vue'
import InventoryHorizontalBarChart from '@/components/dashboard/InventoryHorizontalBarChart.vue'
import Top10RankingTable from '@/components/dashboard/Top10RankingTable.vue'
import { formatCurrency, formatNumber } from '@/services/formatters'
import type { DashboardInventoryRankingItem } from '@/models/dashboard'
import { resolveInvestigationSourceLabel } from '@/services/investigationSourceLabels'
import { navigateToInvestigation } from '@/services/navigateToInvestigation'
import { useDashboardStore } from '@/stores/dashboardStore'

const dashboard = useDashboardStore()
const router = useRouter()
const sourceLabel = resolveInvestigationSourceLabel('/dashboard/inventory')

const categoryColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'Name', header: 'Category' },
  { field: 'InventoryValue', header: 'Inventory Value' },
]

const supplierColumns = [
  { field: 'Rank', header: 'Rank' },
  { field: 'Name', header: 'Supplier' },
  { field: 'InventoryValue', header: 'Inventory Value' },
]

const categoryRows = computed(
  () => (dashboard.inventory?.TopCategories ?? []) as Record<string, unknown>[],
)

const supplierRows = computed(
  () => (dashboard.inventory?.TopSuppliers ?? []) as Record<string, unknown>[],
)

function onRankingClick(row: Record<string, unknown>): void {
  const item = row as unknown as DashboardInventoryRankingItem
  if (!item.Investigation) return
  navigateToInvestigation(router, item.Investigation, sourceLabel)
}

onMounted(() => {
  void dashboard.loadInventory()
})
</script>

<template>
  <DashboardDetailLayout
    title="Inventory Dashboard"
    subtitle="Point-in-time stock snapshot — excludes In-Transit, zero qty items."
    :loading="dashboard.loading"
    :error="dashboard.error"
    @refresh="dashboard.loadInventory()"
  >
    <div class="inventory-dashboard__kpi-row">
      <div class="metric">
        <span class="metric__label">Total Inventory Value</span>
        <span class="metric__value">
          {{ dashboard.inventory ? formatCurrency(dashboard.inventory.TotalInventoryValue) : '—' }}
        </span>
      </div>
      <div class="metric">
        <span class="metric__label">Total Item</span>
        <span class="metric__value">
          {{ dashboard.inventory ? formatNumber(dashboard.inventory.TotalItem) : '—' }}
        </span>
      </div>
    </div>

    <InventoryHorizontalBarChart
      class="inventory-dashboard__section"
      title="Inventory by Category"
      :items="dashboard.inventory?.CategoryBreakdown ?? []"
      :loading="dashboard.loading"
    />

    <InventoryHorizontalBarChart
      class="inventory-dashboard__section"
      title="Inventory by Supplier"
      :items="dashboard.inventory?.SupplierBreakdown ?? []"
      :loading="dashboard.loading"
    />

    <Top10RankingTable
      class="inventory-dashboard__section"
      title="Top 10 Categories"
      :columns="categoryColumns"
      :rows="categoryRows"
      :loading="dashboard.loading"
      value-field="InventoryValue"
      clickable
      empty-message="No category data available."
      @row-click="onRankingClick"
    />

    <Top10RankingTable
      class="inventory-dashboard__section"
      title="Top 10 Suppliers"
      :columns="supplierColumns"
      :rows="supplierRows"
      :loading="dashboard.loading"
      value-field="InventoryValue"
      clickable
      empty-message="No supplier data available."
      @row-click="onRankingClick"
    />
  </DashboardDetailLayout>
</template>

<style scoped>
.inventory-dashboard__kpi-row {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  margin-bottom: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--p-border-radius);
}

.metric {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.metric__label {
  font-size: 0.875rem;
  color: var(--p-text-muted-color);
}

.metric__value {
  font-size: 1.25rem;
  font-weight: 700;
  color: var(--p-text-color);
}

.inventory-dashboard__section {
  margin-top: 1rem;
}

@media (max-width: 900px) {
  .inventory-dashboard__kpi-row {
    grid-template-columns: 1fr;
  }
}
</style>

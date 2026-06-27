<script setup lang="ts">
import { computed } from 'vue'
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import ProgressSpinner from 'primevue/progressspinner'
import RankingBadge from '@/components/dashboard/primitives/RankingBadge.vue'
import type { DashboardDomain } from '@/services/dashboardDomains'
import { formatCurrency, formatPercent } from '@/services/formatters'
import { formatDashboardEmpty } from '@/services/dashboardEmptyStates'

const props = defineProps<{
  title: string
  columns: { field: string; header: string }[]
  rows: object[]
  loading: boolean
  valueField: string
  percentField?: string
  emptyMessage: string
  clickable?: boolean
  clickHint?: string
  domain?: DashboardDomain
}>()

const emit = defineEmits<{
  rowClick: [row: Record<string, unknown>]
}>()

const numericFields = computed(() => new Set([props.valueField, props.percentField].filter(Boolean)))

function onRowClick(event: { data: object }): void {
  if (!props.clickable) return
  emit('rowClick', event.data as Record<string, unknown>)
}

function formatCell(field: string, value: unknown, valueField: string, percentField?: string): string {
  if (field === valueField) {
    if (value == null) return formatDashboardEmpty('no-data')
    return formatCurrency(value as number)
  }

  if (percentField && field === percentField) {
    return value != null ? formatPercent(value as number) : formatDashboardEmpty('unknown')
  }

  return String(value ?? '')
}

function isNumericField(field: string): boolean {
  return numericFields.value.has(field)
}

function rowClass(data: object): string | undefined {
  const rank = (data as Record<string, unknown>).Rank
  if (rank === 1) return 'dashboard-table-row--top'
  return undefined
}

function parseRank(value: unknown): number | null {
  if (typeof value === 'number' && Number.isFinite(value)) return value
  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value)
    return Number.isFinite(parsed) ? parsed : null
  }
  return null
}
</script>

<template>
  <Card
    class="top10-ranking-table"
    :data-domain="domain"
  >
    <template #title>
      <div class="top10-ranking-table__title-block">
        <div class="top10-ranking-table__title">
          <span class="top10-ranking-table__icon-wrap">
            <i class="pi pi-list" aria-hidden="true" />
          </span>
          <span>{{ title }}</span>
          <i
            v-if="clickable && clickHint"
            class="pi pi-id-card top10-ranking-table__profile-icon"
            aria-hidden="true"
            title="Performance Profile"
          />
        </div>
        <p v-if="clickable && clickHint" class="top10-ranking-table__hint">{{ clickHint }}</p>
      </div>
    </template>

    <template #content>
      <div v-if="loading" class="top10-ranking-table__loading">
        <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
      </div>

      <DataTable
        v-else
        :value="rows"
        class="top10-ranking-table__table dashboard-table"
        :class="{ 'top10-ranking-table__table--clickable': clickable }"
        :row-class="rowClass"
        @row-click="onRowClick"
      >
        <template #empty>
          <p class="top10-ranking-table__empty">{{ emptyMessage }}</p>
        </template>

        <Column
          v-for="col in columns"
          :key="col.field"
          :field="col.field"
          :header="col.header"
          :body-class="isNumericField(col.field) ? 'dashboard-table__numeric' : undefined"
          :header-class="isNumericField(col.field) ? 'dashboard-table__numeric' : undefined"
        >
          <template #body="{ data }">
            <RankingBadge
              v-if="col.field === 'Rank' && parseRank((data as Record<string, unknown>)[col.field]) != null"
              :rank="parseRank((data as Record<string, unknown>)[col.field])!"
            />
            <span
              v-else
              :class="{ 'dashboard-table__value-emphasis': col.field === valueField }"
            >
              {{ formatCell(col.field, (data as Record<string, unknown>)[col.field], valueField, percentField) }}
            </span>
          </template>
        </Column>
      </DataTable>
    </template>
  </Card>
</template>

<style scoped>
.top10-ranking-table {
  border-radius: var(--dashboard-radius);
  box-shadow: var(--dashboard-shadow-idle);
  transition:
    box-shadow var(--dashboard-transition),
    transform var(--dashboard-transition);
}

.top10-ranking-table:hover {
  box-shadow: var(--dashboard-shadow-hover);
}

.top10-ranking-table__title-block {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
}

.top10-ranking-table__title {
  display: flex;
  align-items: center;
  gap: 0.5rem;
  font-weight: 700;
}

.top10-ranking-table__icon-wrap {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 1.625rem;
  height: 1.625rem;
  border-radius: var(--dashboard-radius-sm);
  background: color-mix(in srgb, var(--dashboard-domain-color, var(--p-primary-color)) 12%, white);
  color: var(--dashboard-domain-color, var(--p-primary-color));
  font-size: 0.8125rem;
}

.top10-ranking-table__profile-icon {
  color: var(--dashboard-domain-color, var(--p-primary-color));
  font-size: 0.95rem;
}

.top10-ranking-table__hint {
  margin: 0;
  font-size: 0.8125rem;
  font-weight: 400;
  color: var(--p-text-muted-color);
}

.top10-ranking-table__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.top10-ranking-table__empty {
  margin: 0;
  padding: 1.5rem 0;
  text-align: center;
  color: var(--p-text-muted-color);
}

.top10-ranking-table__table--clickable :deep(.p-datatable-tbody > tr) {
  cursor: pointer;
}

.dashboard-table :deep(.p-datatable-thead > tr > th) {
  background: var(--dashboard-table-header-bg);
  font-size: 0.8125rem;
  font-weight: 700;
  color: var(--p-text-muted-color);
  border-bottom: 1px solid var(--p-surface-200);
}

.dashboard-table :deep(.p-datatable-tbody > tr:nth-child(even)) {
  background: var(--dashboard-table-row-alt);
}

.dashboard-table :deep(.p-datatable-tbody > tr:hover) {
  background: color-mix(
    in srgb,
    var(--dashboard-domain-color, #2563eb) 6%,
    var(--dashboard-table-row-hover)
  );
}

.dashboard-table :deep(.dashboard-table-row--top) {
  background: var(--dashboard-table-row-top) !important;
}

.dashboard-table :deep(.dashboard-table__numeric) {
  text-align: right;
  font-variant-numeric: tabular-nums;
}

.dashboard-table :deep(.dashboard-table__value-emphasis) {
  font-weight: 700;
  font-variant-numeric: tabular-nums;
}
</style>

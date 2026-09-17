<script setup lang="ts">
import { RouterLink } from 'vue-router'
import type { ProfileRelationshipBlock, ProfileRelatedEntityRow } from '@/models/entityAnalytics'
import {
  isLastInvoicingSalesmanRelationship,
  LAST_INVOICING_SALESMAN_NOTE,
} from '@/services/customerLastInvoicingSalesman'

defineProps<{
  blocks: ProfileRelationshipBlock[]
  workspaceMode?: boolean
}>()

const emit = defineEmits<{
  navigate: [row: ProfileRelatedEntityRow]
}>()

function formatMetric(value: number | null | undefined): string {
  if (value == null)
    return '—'

  return new Intl.NumberFormat('id-ID', {
    style: 'currency',
    currency: 'IDR',
    maximumFractionDigits: 0,
  }).format(value)
}

function relationshipNote(block: ProfileRelationshipBlock): string | null {
  return isLastInvoicingSalesmanRelationship(block.RelationshipCode)
    ? LAST_INVOICING_SALESMAN_NOTE
    : null
}

function barWidth(value: number | null | undefined, block: ProfileRelationshipBlock): string {
  if (value == null) return '0%'
  const max = Math.max(...block.Rows.map((r) => r.MetricValue ?? 0), 1)
  return `${Math.round((value / max) * 100)}%`
}

function blockHasStatus(block: ProfileRelationshipBlock): boolean {
  return block.Rows.some((r) => !!r.RelationshipStatus)
}

function blockHasLastTransaction(block: ProfileRelationshipBlock): boolean {
  return block.Rows.some((r) => !!r.LastTransactionDate)
}

function formatStatus(status: string | null | undefined): string {
  if (!status) return ''
  return status
}

function statusClass(status: string | null | undefined): string {
  if (!status) return ''
  if (status === 'Active') return 'related-entities-blocks__status--active'
  if (status === 'Dormant') return 'related-entities-blocks__status--dormant'
  return ''
}

function formatLastTransaction(date: string | null | undefined): string {
  if (!date) return '—'
  const d = new Date(date)
  if (isNaN(d.getTime())) return '—'
  return d.toLocaleDateString('id-ID', { year: 'numeric', month: 'short', day: 'numeric' })
}
</script>

<template>
  <div class="related-entities-blocks">
    <section
      v-for="block in blocks"
      :key="block.RelationshipCode"
      class="related-entities-blocks__block"
    >
      <h3 class="related-entities-blocks__heading">
        {{ block.RelationshipLabel || block.DisplayName }}
      </h3>
      <p
        v-if="relationshipNote(block)"
        class="related-entities-blocks__note"
      >
        {{ relationshipNote(block) }}
      </p>

      <div class="related-entities-blocks__table-wrap">
        <table class="related-entities-blocks__table">
          <thead>
            <tr>
              <th v-if="block.Rows.length > 1">#</th>
              <th>{{ block.TargetEntityType }}</th>
              <th v-if="blockHasStatus(block)">Status</th>
              <th v-if="blockHasLastTransaction(block)">Last Transaction</th>
              <th v-if="block.Rows.some((row) => row.MetricValue != null)" class="iw-numeric">Value</th>
            </tr>
          </thead>
          <tbody>
            <tr
              v-for="row in block.Rows"
              :key="`${block.RelationshipCode}-${row.Rank}-${row.TargetEntityCode}`"
            >
              <td v-if="block.Rows.length > 1">{{ row.Rank }}</td>
              <td>
                <button
                  v-if="workspaceMode"
                  type="button"
                  class="related-entities-blocks__link related-entities-blocks__button"
                  @click="emit('navigate', row)"
                >
                  {{ row.TargetEntityName || row.DisplayName || row.TargetEntityCode }}
                </button>
                <RouterLink
                  v-else-if="row.ProfileRoute"
                  :to="row.ProfileRoute"
                  class="related-entities-blocks__link"
                >
                  {{ row.TargetEntityName || row.DisplayName || row.TargetEntityCode }}
                </RouterLink>
                <span v-else>{{ row.TargetEntityName || row.DisplayName || row.TargetEntityCode }}</span>
              </td>
              <td v-if="blockHasStatus(block)">
                <span v-if="row.RelationshipStatus" :class="['related-entities-blocks__status', statusClass(row.RelationshipStatus)]">
                  {{ formatStatus(row.RelationshipStatus) }}
                </span>
              </td>
              <td v-if="blockHasLastTransaction(block)">
                {{ formatLastTransaction(row.LastTransactionDate) }}
              </td>
              <td v-if="block.Rows.some((r) => r.MetricValue != null)" class="iw-numeric related-entities-blocks__value-cell">
                <span>{{ formatMetric(row.MetricValue) }}</span>
                <span
                  class="related-entities-blocks__bar"
                  :style="{ width: barWidth(row.MetricValue, block) }"
                />
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </div>
</template>

<style scoped>
.related-entities-blocks {
  display: flex;
  flex-direction: column;
  gap: 1.25rem;
}

.related-entities-blocks__heading {
  margin: 0 0 0.35rem;
  font-size: 0.95rem;
  font-weight: 600;
}

.related-entities-blocks__note {
  margin: 0 0 0.75rem;
  color: var(--p-text-muted-color, #64748b);
  font-size: 0.8125rem;
  line-height: 1.4;
}

.related-entities-blocks__table-wrap {
  overflow-x: auto;
}

.related-entities-blocks__table {
  width: 100%;
  border-collapse: collapse;
  font-size: 0.875rem;
}

.related-entities-blocks__table th,
.related-entities-blocks__table td {
  padding: 0.5rem 0.75rem;
  border-bottom: 1px solid var(--p-content-border-color);
  text-align: left;
}

.related-entities-blocks__table th {
  color: var(--p-text-muted-color);
  font-weight: 600;
}

.related-entities-blocks__link {
  color: var(--p-primary-color);
  text-decoration: none;
}

.related-entities-blocks__link:hover {
  text-decoration: underline;
}

.related-entities-blocks__button {
  background: none;
  border: none;
  padding: 0;
  cursor: pointer;
  font: inherit;
  text-align: left;
}

.related-entities-blocks__value-cell {
  position: relative;
  min-width: 8rem;
}

.related-entities-blocks__bar {
  display: block;
  height: 4px;
  margin-top: 0.25rem;
  background: rgba(59, 130, 246, 0.35);
  border-radius: 2px;
}

.related-entities-blocks__status {
  display: inline-block;
  padding: 0.125rem 0.5rem;
  border-radius: 0.25rem;
  font-size: 0.75rem;
  font-weight: 600;
  line-height: 1.4;
}

.related-entities-blocks__status--active {
  background: rgba(34, 197, 94, 0.12);
  color: rgb(22, 163, 74);
}

.related-entities-blocks__status--dormant {
  background: rgba(234, 179, 8, 0.12);
  color: rgb(202, 138, 4);
}
</style>

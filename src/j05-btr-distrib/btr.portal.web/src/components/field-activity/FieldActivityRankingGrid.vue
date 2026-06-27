<script setup lang="ts">
import Card from 'primevue/card'
import DataTable from 'primevue/datatable'
import Column from 'primevue/column'
import SelectButton from 'primevue/selectbutton'
import { computed, ref } from 'vue'
import type { FieldActivityRankingEntry, FieldActivityRankingSection } from '@/models/fieldActivity'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  rankings: FieldActivityRankingSection | null
  loading?: boolean
}>()

const emit = defineEmits<{
  rowClick: [entry: FieldActivityRankingEntry]
}>()

const limitOptions = [
  { label: 'Top 5', value: 5 },
  { label: 'Top 10', value: 10 },
]

const limit = ref(5)

type RankingCard = {
  key: string
  title: string
  rows: FieldActivityRankingEntry[]
  format: 'percent' | 'number' | 'currency'
}

const cards = computed((): RankingCard[] => {
  const r = props.rankings
  if (!r) return []

  return [
    { key: 'topExec', title: 'Top Visit Execution', rows: r.TopVisitExecution, format: 'percent' },
    { key: 'bottomExec', title: 'Bottom Visit Execution', rows: r.BottomVisitExecution, format: 'percent' },
    { key: 'topEff', title: 'Top Effective Call Rate', rows: r.TopEffectiveCallRate, format: 'percent' },
    { key: 'bottomEff', title: 'Bottom Effective Call Rate', rows: r.BottomEffectiveCallRate, format: 'percent' },
    { key: 'topOmzet', title: 'Top Order Value', rows: r.TopOmzet, format: 'currency' },
    { key: 'topOrders', title: 'Top Orders', rows: r.TopOrders, format: 'number' },
    { key: 'missed', title: 'Most Missed Visits', rows: r.MostMissedVisits, format: 'number' },
    { key: 'unplanned', title: 'Most Unplanned Visits', rows: r.MostUnplannedVisits, format: 'number' },
  ]
})

function sliceRows(rows: FieldActivityRankingEntry[]): FieldActivityRankingEntry[] {
  return rows.slice(0, limit.value)
}

function formatValue(value: number | null, format: RankingCard['format']): string {
  if (value == null) return '—'
  if (format === 'percent') return formatPercent(value)
  if (format === 'currency') return formatCurrency(value)
  return formatNumber(value)
}

function onRowClick(entry: FieldActivityRankingEntry): void {
  emit('rowClick', entry)
}
</script>

<template>
  <section class="field-activity-ranking-grid">
    <div class="field-activity-ranking-grid__toolbar">
      <h2>Performance Rankings</h2>
      <SelectButton v-model="limit" :options="limitOptions" option-label="label" option-value="value" />
    </div>

    <div class="field-activity-ranking-grid__cards">
      <Card v-for="card in cards" :key="card.key" class="field-activity-ranking-grid__card">
        <template #title>{{ card.title }}</template>
        <template #content>
          <DataTable
            :value="sliceRows(card.rows)"
            :loading="loading"
            class="field-activity-ranking-grid__table"
            @row-click="(e) => onRowClick(e.data)"
          >
            <Column field="Rank" header="#" style="width: 3rem" />
            <Column field="SalesPersonCode" header="Code" />
            <Column field="SalesPersonName" header="Name" />
            <Column header="Value">
              <template #body="{ data }">
                {{ formatValue(data.PrimaryValue, card.format) }}
              </template>
            </Column>
          </DataTable>
        </template>
      </Card>
    </div>
  </section>
</template>

<style scoped>
.field-activity-ranking-grid__toolbar {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
  margin-bottom: 0.75rem;
}

.field-activity-ranking-grid__toolbar h2 {
  margin: 0;
  font-size: 1.125rem;
}

.field-activity-ranking-grid__cards {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(18rem, 1fr));
  gap: 0.75rem;
}

:deep(.field-activity-ranking-grid__table .p-datatable-tbody > tr) {
  cursor: pointer;
}
</style>

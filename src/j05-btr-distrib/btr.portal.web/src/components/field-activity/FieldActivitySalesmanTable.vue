<script setup lang="ts">
import { computed, ref } from 'vue'
import Card from 'primevue/card'
import Column from 'primevue/column'
import ColumnGroup from 'primevue/columngroup'
import DataTable from 'primevue/datatable'
import InputText from 'primevue/inputtext'
import Row from 'primevue/row'
import Select from 'primevue/select'
import Tag from 'primevue/tag'
import type { DashboardCollectionResponse } from '@/models/dashboard'
import type { FieldActivitySalesmanOverviewRow } from '@/models/fieldActivity'
import {
  COMMERCIAL_SIGNAL_LABELS,
  COMMERCIAL_SIGNAL_RATIONALES,
  type CommercialSignalKey,
} from '@/services/commercialSignalRules'
import {
  bandClass,
  effectiveCallBand,
  executionBand,
  gpsValidBand,
  statusLabel,
  statusSeverity,
} from '@/services/fieldActivityKpiBands'
import {
  DEFAULT_SCOREBOARD_RANK_MODE,
  SCOREBOARD_ACTION_SIGNAL_KEYS,
  SCOREBOARD_RANK_OPTIONS,
  barWidthPercent,
  classifyScoreboardAttention,
  columnBarMax,
  displayRank,
  rankScoreboardRows,
  type ScoreboardRankMode,
} from '@/services/fieldActivityScoreboard'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'

const props = defineProps<{
  rows: FieldActivitySalesmanOverviewRow[]
  collection?: DashboardCollectionResponse | null
  loading?: boolean
}>()

const emit = defineEmits<{
  rowClick: [row: FieldActivitySalesmanOverviewRow]
}>()

const globalFilter = ref('')
const rankMode = ref<ScoreboardRankMode>(DEFAULT_SCOREBOARD_RANK_MODE)
const rankOptions = [...SCOREBOARD_RANK_OPTIONS]

const filteredRows = computed(() => {
  const query = globalFilter.value.trim().toLowerCase()
  if (!query) return props.rows

  return props.rows.filter((row) =>
    `${row.SalesPersonCode} ${row.SalesPersonName}`.toLowerCase().includes(query),
  )
})

const attentionBySalesman = computed(() =>
  classifyScoreboardAttention(props.rows, props.collection?.TopOverdueSalesmen ?? []),
)

const attentionUnavailable = computed(
  () => !props.loading && (props.collection == null || !props.collection.IsAvailable),
)

const rankedRows = computed(() =>
  rankScoreboardRows(filteredRows.value, rankMode.value, attentionBySalesman.value),
)

const rankBySalesPersonId = computed(() => displayRank(rankedRows.value))

const executionBarMax = computed(() =>
  columnBarMax(filteredRows.value, (row) => row.VisitExecutionPercent),
)
const effectiveCallRateBarMax = computed(() =>
  columnBarMax(filteredRows.value, (row) => row.EffectiveCallRate),
)
const ordersBarMax = computed(() => columnBarMax(filteredRows.value, (row) => row.OrdersCount))
const orderValueBarMax = computed(() => columnBarMax(filteredRows.value, (row) => row.OmzetAmount))

function displayRankFor(row: FieldActivitySalesmanOverviewRow): number | undefined {
  return rankBySalesPersonId.value.get(row.SalesPersonId)
}

function barWidth(value: number | null, max: number): string {
  return `${barWidthPercent(value, max)}%`
}

interface AttentionTag {
  key: CommercialSignalKey
  label: string
  rationale: string
}

function attentionTags(row: FieldActivitySalesmanOverviewRow): AttentionTag[] {
  const attention = attentionBySalesman.value.get(row.SalesPersonId)
  if (!attention) return []

  return attention.classifications
    .filter((entry) => SCOREBOARD_ACTION_SIGNAL_KEYS.includes(entry.key))
    .map((entry) => ({
      key: entry.key,
      label: COMMERCIAL_SIGNAL_LABELS[entry.key],
      rationale: COMMERCIAL_SIGNAL_RATIONALES[entry.key],
    }))
}

function onRowClick(event: { data: FieldActivitySalesmanOverviewRow }): void {
  emit('rowClick', event.data)
}
</script>

<template>
  <Card class="field-activity-salesman-table">
    <template #title>
      <div class="field-activity-salesman-table__header">
        <span>Salesman Scoreboard</span>
        <div class="field-activity-salesman-table__controls">
          <label class="field-activity-salesman-table__rank-field" for="salesman-scoreboard-rank">
            Rank by
          </label>
          <Select
            input-id="salesman-scoreboard-rank"
            v-model="rankMode"
            :options="rankOptions"
            option-label="label"
            option-value="mode"
            class="field-activity-salesman-table__rank-select"
          />
          <span class="p-input-icon-left field-activity-salesman-table__search">
            <i class="pi pi-search" />
            <InputText v-model="globalFilter" placeholder="Search code or name" />
          </span>
        </div>
      </div>
    </template>

    <template #content>
      <div class="field-activity-salesman-table__table-panel">
        <DataTable
          :value="rankedRows"
          :loading="loading"
          striped-rows
          scrollable
          scroll-height="flex"
          class="field-activity-salesman-table__grid"
          @row-click="onRowClick"
        >
          <ColumnGroup type="header">
            <Row>
              <Column header="#" field="Rank" :rowspan="2" style="width: 4rem" />
              <Column header="Code" field="SalesPersonCode" :rowspan="2" />
              <Column header="Name" field="SalesPersonName" :rowspan="2" />
              <Column header="ACTIVITY" :colspan="5" />
              <Column header="PRODUCTIVITY" :colspan="2" />
              <Column header="OUTCOME" :colspan="2" />
              <Column header="SIGNAL" :colspan="3" />
            </Row>
            <Row>
              <Column header="Planned" field="PlannedVisits" />
              <Column header="Actual" field="ActualVisits" />
              <Column header="Execution %" field="VisitExecutionPercent" />
              <Column header="Missed" field="MissedVisits" />
              <Column header="Unplanned" field="UnplannedVisits" />
              <Column header="Effective" field="EffectiveCalls" />
              <Column header="Eff. Rate" field="EffectiveCallRate" />
              <Column header="Orders" field="OrdersCount" />
              <Column header="Order Value" field="OmzetAmount" />
              <Column header="Attention" field="Attention" />
              <Column header="GPS Valid %" field="GpsValidPercent" />
              <Column header="Status" field="StatusCode" />
            </Row>
          </ColumnGroup>

          <Column field="Rank" style="width: 4rem">
            <template #body="{ data }">{{ displayRankFor(data) }}</template>
          </Column>
          <Column field="SalesPersonCode" />
          <Column field="SalesPersonName" />
          <Column field="PlannedVisits">
            <template #body="{ data }">{{ formatNumber(data.PlannedVisits) }}</template>
          </Column>
          <Column field="ActualVisits">
            <template #body="{ data }">{{ formatNumber(data.ActualVisits) }}</template>
          </Column>
          <Column field="VisitExecutionPercent">
            <template #body="{ data }">
              <div class="field-activity-salesman-table__bar-cell">
                <span
                  class="field-activity-salesman-table__bar"
                  :style="{ width: barWidth(data.VisitExecutionPercent, executionBarMax) }"
                  aria-hidden="true"
                />
                <span
                  class="field-activity-salesman-table__bar-value"
                  :class="bandClass(executionBand(data.VisitExecutionPercent))"
                >
                  {{ formatPercent(data.VisitExecutionPercent) }}
                </span>
              </div>
            </template>
          </Column>
          <Column field="MissedVisits" />
          <Column field="UnplannedVisits" />
          <Column field="EffectiveCalls">
            <template #body="{ data }">{{ formatNumber(data.EffectiveCalls) }}</template>
          </Column>
          <Column field="EffectiveCallRate">
            <template #body="{ data }">
              <div class="field-activity-salesman-table__bar-cell">
                <span
                  class="field-activity-salesman-table__bar"
                  :style="{ width: barWidth(data.EffectiveCallRate, effectiveCallRateBarMax) }"
                  aria-hidden="true"
                />
                <span
                  class="field-activity-salesman-table__bar-value"
                  :class="bandClass(effectiveCallBand(data.EffectiveCallRate))"
                >
                  {{ formatPercent(data.EffectiveCallRate) }}
                </span>
              </div>
            </template>
          </Column>
          <Column field="OrdersCount">
            <template #body="{ data }">
              <div class="field-activity-salesman-table__bar-cell">
                <span
                  class="field-activity-salesman-table__bar"
                  :style="{ width: barWidth(data.OrdersCount, ordersBarMax) }"
                  aria-hidden="true"
                />
                <span class="field-activity-salesman-table__bar-value">
                  {{ data.OrdersCount }}
                </span>
              </div>
            </template>
          </Column>
          <Column field="OmzetAmount">
            <template #body="{ data }">
              <div class="field-activity-salesman-table__bar-cell">
                <span
                  class="field-activity-salesman-table__bar"
                  :style="{ width: barWidth(data.OmzetAmount, orderValueBarMax) }"
                  aria-hidden="true"
                />
                <span class="field-activity-salesman-table__bar-value">
                  {{ formatCurrency(data.OmzetAmount) }}
                </span>
              </div>
            </template>
          </Column>
          <Column field="Attention">
            <template #body="{ data }">
              <div
                v-if="attentionTags(data).length"
                class="field-activity-salesman-table__attention"
              >
                <Tag
                  v-for="tag in attentionTags(data)"
                  :key="tag.key"
                  class="field-activity-salesman-table__attention-tag"
                  severity="danger"
                  :value="tag.label"
                  :title="tag.rationale"
                />
              </div>
              <span
                v-else
                class="field-activity-salesman-table__attention-empty"
                :title="
                  attentionUnavailable ? 'Collection data is unavailable for this date.' : undefined
                "
              >
                —
              </span>
            </template>
          </Column>
          <Column field="GpsValidPercent">
            <template #body="{ data }">
              <span :class="bandClass(gpsValidBand(data.GpsValidPercent))">
                {{ formatPercent(data.GpsValidPercent) }}
              </span>
            </template>
          </Column>
          <Column field="StatusCode">
            <template #body="{ data }">
              <Tag
                :value="statusLabel(data.StatusCode)"
                :severity="statusSeverity(data.StatusCode)"
              />
            </template>
          </Column>
        </DataTable>
      </div>
    </template>
  </Card>
</template>

<style scoped>
.field-activity-salesman-table__header {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  justify-content: space-between;
  gap: 0.75rem;
}

.field-activity-salesman-table__controls {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 0.75rem;
}

.field-activity-salesman-table__rank-field {
  font-size: 0.875rem;
  color: var(--p-text-muted-color, #64748b);
}

.field-activity-salesman-table__rank-select {
  min-width: 12rem;
}

.field-activity-salesman-table__search {
  min-width: 14rem;
}

.field-activity-salesman-table__table-panel {
  max-height: 32rem;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

:deep(.field-activity-salesman-table__grid) {
  flex: 1;
  min-height: 0;
}

:deep(.field-activity-salesman-table__grid .p-datatable-tbody > tr) {
  cursor: pointer;
}

.field-activity-salesman-table__bar-cell {
  position: relative;
  display: flex;
  align-items: center;
  min-height: 1.5rem;
}

.field-activity-salesman-table__bar {
  position: absolute;
  left: 0;
  top: 50%;
  transform: translateY(-50%);
  height: 1.25rem;
  max-width: 100%;
  border-radius: var(--dashboard-radius-sm, 0.25rem);
  background: color-mix(in srgb, var(--domain-sales-color, #2563eb) 18%, transparent);
  transition: width var(--dashboard-transition, 175ms ease);
  pointer-events: none;
}

.field-activity-salesman-table__bar-value {
  position: relative;
  font-variant-numeric: tabular-nums;
}

.field-activity-salesman-table__attention {
  display: flex;
  flex-wrap: wrap;
  gap: 0.25rem;
}

.field-activity-salesman-table__attention-tag {
  font-size: 0.6875rem;
}

.field-activity-salesman-table__attention-empty {
  color: var(--p-text-muted-color, #94a3b8);
}

:deep(.field-activity-kpi-band) {
  font-weight: 600;
}

:deep(.field-activity-kpi-band--good) {
  color: var(--p-green-600, #16a34a);
}

:deep(.field-activity-kpi-band--warn) {
  color: var(--p-yellow-600, #ca8a04);
}

:deep(.field-activity-kpi-band--bad) {
  color: var(--p-red-600, #dc2626);
}

:deep(.field-activity-kpi-band--neutral) {
  color: var(--p-text-muted-color, #64748b);
}
</style>

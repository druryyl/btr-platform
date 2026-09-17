<script setup lang="ts">
import { computed } from 'vue'
import ProgressSpinner from 'primevue/progressspinner'
import type {
  FieldActivityRankingEntry,
  FieldActivityRankingSection,
  FieldActivitySalesmanOverviewRow,
} from '@/models/fieldActivity'
import type { DashboardCollectionResponse } from '@/models/dashboard'
import type { InvestigationMetadata } from '@/models/investigation'
import { formatCurrency, formatNumber, formatPercent } from '@/services/formatters'
import {
  needsCreditReview,
  recognitionCandidates,
  type CommercialSignalRow,
} from '@/services/commercialSignalRules'
import {
  collectionActionInputs,
  joinSalesmenToCollection,
  type NeedsEscalationInputs,
} from '@/services/fieldActivityCollectionComposition'

const props = withDefaults(
  defineProps<{
    salesmen?: FieldActivitySalesmanOverviewRow[]
    rankings?: FieldActivityRankingSection | null
    collection?: DashboardCollectionResponse | null
    loading?: boolean
  }>(),
  {
    salesmen: () => [],
    rankings: null,
    collection: null,
    loading: false,
  },
)

export interface CommercialRiskClickPayload {
  signalKey: string
  salesPersonId: string | null
  investigation: InvestigationMetadata | null
}

const emit = defineEmits<{
  salesmanClick: [salesPersonId: string]
  commercialRiskClick: [payload: CommercialRiskClickPayload]
  recognitionClick: [salesPersonId: string]
}>()

const salesmanById = computed(() => {
  const map = new Map<string, FieldActivitySalesmanOverviewRow>()
  for (const row of props.salesmen) map.set(row.SalesPersonId, row)
  return map
})

interface FieldExecutionItem {
  key: string
  salesPersonId: string
  name: string
  code: string
  valueText: string
  contextText: string | null
}

interface FieldExecutionCategory {
  key: string
  label: string
  action: string
  items: FieldExecutionItem[]
}

function fromRankingEntry(
  entry: FieldActivityRankingEntry,
  valueKind: 'percent' | 'number',
): FieldExecutionItem {
  const primary = entry.PrimaryValue
  return {
    key: `${entry.SalesPersonId}-${entry.Rank}`,
    salesPersonId: entry.SalesPersonId,
    name: entry.SalesPersonName,
    code: entry.SalesPersonCode,
    valueText:
      primary == null
        ? '—'
        : valueKind === 'percent'
          ? formatPercent(primary)
          : formatNumber(primary),
    contextText: null,
  }
}

const investigationItems = computed<FieldExecutionItem[]>(() =>
  (props.rankings?.MostUnplannedVisits ?? []).map((entry) => {
    const salesman = salesmanById.value.get(entry.SalesPersonId)
    const item = fromRankingEntry(entry, 'number')
    return {
      ...item,
      valueText: entry.PrimaryValue == null ? '—' : `${formatNumber(entry.PrimaryValue)} unplanned`,
      contextText:
        salesman?.GpsValidPercent == null ? null : `GPS ${formatPercent(salesman.GpsValidPercent)} valid`,
    }
  }),
)

const zeroActivityItems = computed<FieldExecutionItem[]>(() =>
  props.salesmen
    .filter((row) => row.ActualVisits === 0)
    .map((row) => ({
      key: row.SalesPersonId,
      salesPersonId: row.SalesPersonId,
      name: row.SalesPersonName,
      code: row.SalesPersonCode,
      valueText: '0 visits today',
      contextText: row.WilayahName?.trim() ? row.WilayahName : null,
    })),
)

const fieldExecutionCategories = computed<FieldExecutionCategory[]>(() => [
  {
    key: 'needsCoaching',
    label: 'Needs Coaching',
    action: 'Lowest order conversion — coach on customer engagement and visit approach.',
    items: (props.rankings?.BottomEffectiveCallRate ?? []).map((entry) =>
      fromRankingEntry(entry, 'percent'),
    ),
  },
  {
    key: 'needsPlanReview',
    label: 'Needs Plan Review',
    action: 'Lowest visit execution — review plan realism and discuss with the salesman.',
    items: (props.rankings?.BottomVisitExecution ?? []).map((entry) =>
      fromRankingEntry(entry, 'percent'),
    ),
  },
  {
    key: 'needsInvestigation',
    label: 'Needs Investigation',
    action: 'Highest unplanned visits plus GPS anomalies — verify context before acting.',
    items: investigationItems.value,
  },
  {
    key: 'needsImmediateAttention',
    label: 'Needs Immediate Attention',
    action: 'Zero activity today — contact the salesman now.',
    items: zeroActivityItems.value,
  },
])

interface CommercialRiskRow {
  salesPersonId: string
  name: string
  code: string
  revenue: number
  effectiveCallRate: number | null
  overdueExposure: number | null
  investigation: InvestigationMetadata | null
}

const commercialRows = computed<CommercialRiskRow[]>(() =>
  joinSalesmenToCollection(props.salesmen, props.collection?.TopOverdueSalesmen ?? []).map(
    ({ salesman, overdue }) => ({
      salesPersonId: salesman.SalesPersonId,
      name: salesman.SalesPersonName,
      code: salesman.SalesPersonCode,
      revenue: salesman.OmzetAmount,
      effectiveCallRate: salesman.EffectiveCallRate,
      overdueExposure: overdue?.Amount ?? null,
      investigation: overdue?.Investigation ?? null,
    }),
  ),
)

const signalRows = computed<CommercialSignalRow[]>(() =>
  commercialRows.value.map((row) => ({
    SalesPersonId: row.salesPersonId,
    Revenue: row.revenue,
    EffectiveCallRate: row.effectiveCallRate,
    OverdueExposure: row.overdueExposure,
  })),
)

const creditReviewRows = computed<CommercialRiskRow[]>(() => {
  const rows = signalRows.value
  return commercialRows.value.filter((_row, index) => needsCreditReview(rows[index], rows))
})

const recognitionRows = computed<CommercialRiskRow[]>(() => {
  const ids = new Set(recognitionCandidates(signalRows.value).map((row) => row.SalesPersonId))
  return commercialRows.value.filter((row) => ids.has(row.salesPersonId))
})

const collectionActions = computed(() =>
  collectionActionInputs(
    props.collection?.TopOverdueSalesmen ?? [],
    props.collection?.AttentionCards ?? null,
  ),
)

const needsEscalation = computed<NeedsEscalationInputs>(() => collectionActions.value.needsEscalation)

const collectionUnavailable = computed(
  () => !props.loading && (props.collection == null || !props.collection.IsAvailable),
)

function onSalesmanClick(salesPersonId: string): void {
  emit('salesmanClick', salesPersonId)
}

function onCommercialRiskClick(payload: CommercialRiskClickPayload): void {
  emit('commercialRiskClick', payload)
}
</script>

<template>
  <section class="field-activity-action-center">
    <header class="field-activity-action-center__header">
      <h2 class="field-activity-action-center__title">Action Center</h2>
      <p class="field-activity-action-center__subtitle">What should I do right now?</p>
    </header>

    <div v-if="loading" class="field-activity-action-center__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <template v-else>
      <div class="field-activity-action-center__group field-activity-action-center__group--execution">
        <h3 class="field-activity-action-center__group-title">Field Execution</h3>
        <div class="field-activity-action-center__categories">
          <article
            v-for="category in fieldExecutionCategories"
            :key="category.key"
            class="field-activity-action-center__category"
          >
            <h4 class="field-activity-action-center__category-label">{{ category.label }}</h4>
            <p class="field-activity-action-center__category-action">{{ category.action }}</p>

            <ul v-if="category.items.length" class="field-activity-action-center__list">
              <li
                v-for="item in category.items"
                :key="item.key"
                class="field-activity-action-center__item"
              >
                <button
                  type="button"
                  class="field-activity-action-center__item-name"
                  @click="onSalesmanClick(item.salesPersonId)"
                >
                  {{ item.name }}
                  <span class="field-activity-action-center__item-code">{{ item.code }}</span>
                </button>
                <span class="field-activity-action-center__item-value">
                  {{ item.valueText }}
                  <span v-if="item.contextText" class="field-activity-action-center__item-context">
                    {{ item.contextText }}
                  </span>
                </span>
              </li>
            </ul>
            <p v-else class="field-activity-action-center__empty">No salesmen in this category.</p>
          </article>
        </div>
      </div>

      <div class="field-activity-action-center__group field-activity-action-center__group--risk">
        <h3 class="field-activity-action-center__group-title">Commercial Risk</h3>
        <p v-if="collectionUnavailable" class="field-activity-action-center__unavailable">
          Collection data is unavailable. Field execution actions are unaffected.
        </p>

        <div class="field-activity-action-center__categories">
          <article class="field-activity-action-center__category">
            <h4 class="field-activity-action-center__category-label">Needs Collection Action</h4>
            <p class="field-activity-action-center__category-action">
              Highest overdue exposure by salesman — start cash-recovery follow-up.
            </p>

            <ul
              v-if="collectionActions.needsCollectionAction.length"
              class="field-activity-action-center__list"
            >
              <li
                v-for="(row, index) in collectionActions.needsCollectionAction"
                :key="`${row.salesPersonId ?? row.salesPersonName}-${index}`"
                class="field-activity-action-center__item"
              >
                <button
                  type="button"
                  class="field-activity-action-center__item-name"
                  @click="
                    onCommercialRiskClick({
                      signalKey: 'NeedsCollectionAction',
                      salesPersonId: row.salesPersonId,
                      investigation: row.investigation,
                    })
                  "
                >
                  {{ row.salesPersonName }}
                  <span class="field-activity-action-center__item-code">{{ row.salesPersonCode }}</span>
                </button>
                <span class="field-activity-action-center__item-value">
                  {{ formatCurrency(row.overdueAmount) }}
                  <span
                    v-if="row.percentOfTotal != null"
                    class="field-activity-action-center__item-context"
                  >
                    {{ formatPercent(row.percentOfTotal) }} of overdue
                  </span>
                </span>
              </li>
            </ul>
            <p v-else class="field-activity-action-center__empty">
              No salesmen require collection action.
            </p>
          </article>

          <article class="field-activity-action-center__category">
            <h4 class="field-activity-action-center__category-label">Needs Credit Review</h4>
            <p class="field-activity-action-center__category-action">
              High revenue plus high overdue — review credit terms with the salesman.
            </p>

            <ul v-if="creditReviewRows.length" class="field-activity-action-center__list">
              <li
                v-for="row in creditReviewRows"
                :key="`credit-${row.salesPersonId}`"
                class="field-activity-action-center__item"
              >
                <button
                  type="button"
                  class="field-activity-action-center__item-name"
                  @click="
                    onCommercialRiskClick({
                      signalKey: 'NeedsCreditReview',
                      salesPersonId: row.salesPersonId,
                      investigation: row.investigation,
                    })
                  "
                >
                  {{ row.name }}
                  <span class="field-activity-action-center__item-code">{{ row.code }}</span>
                </button>
                <span class="field-activity-action-center__item-value">
                  {{ row.overdueExposure == null ? '—' : formatCurrency(row.overdueExposure) }}
                  <span class="field-activity-action-center__item-context">
                    Revenue {{ formatCurrency(row.revenue) }}
                  </span>
                </span>
              </li>
            </ul>
            <p v-else class="field-activity-action-center__empty">
              No salesmen require credit review.
            </p>
          </article>

          <article class="field-activity-action-center__category">
            <h4 class="field-activity-action-center__category-label">Needs Escalation</h4>
            <p class="field-activity-action-center__category-action">
              Aging over 90 and legacy debt — escalate structural risk to management.
            </p>

            <ul v-if="!collectionUnavailable" class="field-activity-action-center__list">
              <li class="field-activity-action-center__item">
                <button
                  type="button"
                  class="field-activity-action-center__item-name"
                  @click="
                    onCommercialRiskClick({
                      signalKey: 'NeedsEscalation',
                      salesPersonId: null,
                      investigation: null,
                    })
                  "
                >
                  Portfolio aging risk
                </button>
                <span class="field-activity-action-center__item-value">
                  {{ formatCurrency(needsEscalation.agingOver90Exposure) }}
                  <span class="field-activity-action-center__item-context">
                    {{ formatNumber(needsEscalation.legacyDebtCount) }} legacy debt
                  </span>
                </span>
              </li>
            </ul>
            <p v-else class="field-activity-action-center__empty">
              Collection data is unavailable.
            </p>
          </article>
        </div>
      </div>

      <div
        class="field-activity-action-center__group field-activity-action-center__group--recognition"
      >
        <h3 class="field-activity-action-center__group-title">Recognition</h3>
        <template v-if="recognitionRows.length">
          <p class="field-activity-action-center__category-action">
            Balanced performers — high sales, high effective calls, and a healthy portfolio.
          </p>
          <div class="field-activity-action-center__strip">
            <button
              v-for="row in recognitionRows"
              :key="`recognition-${row.salesPersonId}`"
              type="button"
              class="field-activity-action-center__chip"
              @click="emit('recognitionClick', row.salesPersonId)"
            >
              {{ row.name }}
              <span class="field-activity-action-center__chip-code">{{ row.code }}</span>
            </button>
          </div>
        </template>
        <p v-else class="field-activity-action-center__empty">No recognition candidates.</p>
      </div>
    </template>
  </section>
</template>

<style scoped>
.field-activity-action-center {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.field-activity-action-center__header {
  display: flex;
  flex-wrap: wrap;
  align-items: baseline;
  gap: 0.5rem;
}

.field-activity-action-center__title {
  margin: 0;
  font-size: 1.125rem;
}

.field-activity-action-center__subtitle {
  margin: 0;
  font-size: 0.8125rem;
  color: var(--p-text-muted-color, #64748b);
}

.field-activity-action-center__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.field-activity-action-center__group {
  display: flex;
  flex-direction: column;
  gap: 0.5rem;
  padding: 0.875rem;
  border-radius: var(--dashboard-radius);
  border-top: 3px solid var(--p-content-border-color);
  background: var(--p-content-background, #ffffff);
  box-shadow: var(--dashboard-shadow-idle);
}

.field-activity-action-center__group--execution {
  border-top-color: var(--domain-sales-color, #2563eb);
}

.field-activity-action-center__group--risk {
  border-top-color: var(--domain-collection-color, #ea580c);
}

.field-activity-action-center__group--recognition {
  border-top-color: var(--kpi-status-healthy-color, #0f766e);
}

.field-activity-action-center__group-title {
  margin: 0;
  font-size: 0.9375rem;
  text-transform: uppercase;
  letter-spacing: 0.06em;
  color: var(--p-text-muted-color, #64748b);
}

.field-activity-action-center__categories {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(16rem, 1fr));
  gap: 0.75rem;
}

.field-activity-action-center__category {
  display: flex;
  flex-direction: column;
  gap: 0.375rem;
  min-width: 0;
}

.field-activity-action-center__category-label {
  margin: 0;
  font-size: 0.875rem;
}

.field-activity-action-center__category-action {
  margin: 0;
  font-size: 0.75rem;
  color: var(--p-text-muted-color, #64748b);
}

.field-activity-action-center__unavailable {
  margin: 0;
  padding: 0.5rem 0.75rem;
  border-radius: var(--dashboard-radius-sm);
  background: var(--kpi-status-unknown-bg, #f1f5f9);
  color: var(--kpi-status-unknown-color, #64748b);
  font-size: 0.75rem;
}

.field-activity-action-center__list {
  margin: 0.25rem 0 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  max-height: 14rem;
  overflow-y: auto;
}

.field-activity-action-center__item {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.5rem;
  font-size: 0.8125rem;
}

.field-activity-action-center__item-name {
  padding: 0;
  border: none;
  background: none;
  color: var(--p-primary-color);
  font: inherit;
  font-weight: 600;
  cursor: pointer;
  text-align: left;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.field-activity-action-center__item-name:hover {
  text-decoration: underline;
}

.field-activity-action-center__item-code {
  margin-left: 0.375rem;
  font-weight: 500;
  font-size: 0.6875rem;
  color: var(--p-text-muted-color, #94a3b8);
}

.field-activity-action-center__item-value {
  flex: none;
  font-variant-numeric: tabular-nums;
  color: var(--p-text-color);
}

.field-activity-action-center__item-context {
  display: block;
  font-size: 0.625rem;
  color: var(--p-text-muted-color, #94a3b8);
  text-align: right;
}

.field-activity-action-center__empty {
  margin: 0.25rem 0 0;
  font-size: 0.75rem;
  color: var(--p-text-muted-color, #94a3b8);
}

.field-activity-action-center__strip {
  display: flex;
  flex-wrap: wrap;
  gap: 0.375rem;
  margin-top: 0.25rem;
}

.field-activity-action-center__chip {
  display: inline-flex;
  align-items: center;
  gap: 0.375rem;
  padding: 0.25rem 0.625rem;
  border: none;
  border-radius: var(--dashboard-radius-chip, 999px);
  background: var(--kpi-status-healthy-bg, #ccfbf1);
  color: var(--kpi-status-healthy-color, #0f766e);
  font-size: 0.75rem;
  font-weight: 700;
  cursor: pointer;
}

.field-activity-action-center__chip-code {
  font-weight: 500;
  font-size: 0.625rem;
  opacity: 0.75;
}
</style>

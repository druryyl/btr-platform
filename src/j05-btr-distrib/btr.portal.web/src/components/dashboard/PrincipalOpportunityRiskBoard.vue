<script setup lang="ts">
import { computed } from 'vue'
import ProgressSpinner from 'primevue/progressspinner'
import type {
  PrincipalPerformanceRankingItem,
  PrincipalSalesmanContributionItem,
} from '@/models/dashboard'
import type {
  OpportunityRiskEntry,
  OpportunityRiskRuleId,
} from '@/services/principalOpportunityRisk'
import { opportunityRiskBoard } from '@/services/principalOpportunityRisk'
import { principalDependencies } from '@/services/principalContribution'
import { formatNumber, formatPercent } from '@/services/formatters'

const MAX_CARDS = 5

const props = defineProps<{
  ranking: PrincipalPerformanceRankingItem[]
  contributions: PrincipalSalesmanContributionItem[]
  loading: boolean
}>()

interface RulePresentation {
  categoryLabel: string
  supportLabel: string
  formatSupport: (value: number) => string
  secondaryLabel?: string
  formatSecondary?: (value: number) => string
}

interface BoardCard {
  key: string
  ruleId: OpportunityRiskRuleId
  categoryLabel: string
  supplierId: string
  principalName: string
  supportLabel: string
  supportValue: string
  percentileLabel: string
  secondaryLabel: string | null
  secondaryValue: string | null
  secondaryPercentileLabel: string | null
  interpretation: string
  populationSize: number
}

function percentFromRatio(value: number): string {
  return formatPercent(value * 100)
}

function percentFromPercent(value: number): string {
  return formatPercent(value)
}

const RULE_PRESENTATION: Record<OpportunityRiskRuleId, RulePresentation> = {
  Opportunity: {
    categoryLabel: 'Opportunity',
    supportLabel: 'Coverage %',
    formatSupport: percentFromRatio,
    secondaryLabel: 'Achievement %',
    formatSecondary: percentFromRatio,
  },
  ReturnRisk: {
    categoryLabel: 'Return Risk',
    supportLabel: 'Return %',
    formatSupport: percentFromRatio,
  },
  DependencyRisk: {
    categoryLabel: 'Dependency Risk',
    supportLabel: 'Dependency Ratio',
    formatSupport: percentFromPercent,
  },
  Underperforming: {
    categoryLabel: 'Underperforming',
    supportLabel: 'Achievement %',
    formatSupport: percentFromRatio,
  },
}

function percentileLabel(value: number): string {
  return `P${value.toFixed(1)}`
}

function interpret(entry: OpportunityRiskEntry): string {
  switch (entry.ruleId) {
    case 'Opportunity':
      return (
        `Strong market reach (Coverage ${percentileLabel(entry.percentile)}) ` +
        `but underperforming sales (Achievement ${percentileLabel(entry.secondaryPercentile ?? 0)}).`
      )
    case 'ReturnRisk':
      return `Return level significantly above portfolio peers (Return ${percentileLabel(entry.percentile)}).`
    case 'DependencyRisk':
      return (
        `Sales concentrated on a small number of salesmen ` +
        `(Dependency ${percentileLabel(entry.percentile)}).`
      )
    case 'Underperforming':
      return `Lagging the portfolio on achievement (Achievement ${percentileLabel(entry.percentile)}).`
    default:
      return 'Prioritization signal.'
  }
}

function severity(entry: OpportunityRiskEntry): number {
  return entry.ruleId === 'Underperforming' ? 100 - entry.percentile : entry.percentile
}

function orderCards(entries: ReadonlyArray<OpportunityRiskEntry>): OpportunityRiskEntry[] {
  return [...entries].sort((a, b) => {
    const bySeverity = severity(b) - severity(a)
    if (bySeverity !== 0) return bySeverity
    const byName = a.principalName.localeCompare(b.principalName, undefined, {
      sensitivity: 'accent',
    })
    if (byName !== 0) return byName
    return a.supplierId.localeCompare(b.supplierId)
  })
}

function toCard(entry: OpportunityRiskEntry): BoardCard {
  const presentation = RULE_PRESENTATION[entry.ruleId]
  return {
    key: `${entry.ruleId}:${entry.supplierId}`,
    ruleId: entry.ruleId,
    categoryLabel: presentation.categoryLabel,
    supplierId: entry.supplierId,
    principalName: entry.principalName || entry.supplierId,
    supportLabel: presentation.supportLabel,
    supportValue: presentation.formatSupport(entry.supportingMetric),
    percentileLabel: percentileLabel(entry.percentile),
    secondaryLabel: presentation.secondaryLabel ?? null,
    secondaryValue:
      entry.secondaryMetric != null && presentation.formatSecondary
        ? presentation.formatSecondary(entry.secondaryMetric)
        : null,
    secondaryPercentileLabel:
      entry.secondaryPercentile != null ? percentileLabel(entry.secondaryPercentile) : null,
    interpretation: interpret(entry),
    populationSize: entry.populationSize,
  }
}

const board = computed(() => {
  const dependencies = principalDependencies(props.contributions, props.ranking)
  return opportunityRiskBoard(props.ranking, dependencies)
})

const opportunityCards = computed(() =>
  orderCards(board.value.opportunities).slice(0, MAX_CARDS).map(toCard),
)
const riskCards = computed(() => orderCards(board.value.risks).slice(0, MAX_CARDS).map(toCard))
const opportunityTotal = computed(() => board.value.opportunities.length)
const riskTotal = computed(() => board.value.risks.length)
</script>

<template>
  <section
    class="principal-opportunity-risk"
    aria-label="Opportunity and risk board"
    data-testid="opportunity-risk-board"
  >
    <h2 class="principal-opportunity-risk__title">Opportunity &amp; Risk Board</h2>
    <p class="principal-opportunity-risk__note">
      Prioritization only. Relative percentile rules over the ranked Principal
      population. This board raises no alerts and introduces no independent
      thresholds; EX01/EX02 remain the authoritative alerting mechanism.
    </p>

    <div v-if="loading" class="principal-opportunity-risk__loading">
      <ProgressSpinner style="width: 2.5rem; height: 2.5rem" stroke-width="4" />
    </div>

    <div v-else class="principal-opportunity-risk__columns">
      <div
        class="principal-opportunity-risk__column principal-opportunity-risk__column--opportunities"
        data-testid="opportunity-column"
      >
        <h3 class="principal-opportunity-risk__column-title">
          Opportunities
          <span class="principal-opportunity-risk__column-count">
            {{ formatNumber(opportunityTotal) }}
          </span>
        </h3>

        <p v-if="opportunityCards.length === 0" class="principal-opportunity-risk__empty">
          No opportunities for the current period.
        </p>
        <template v-else>
          <p class="principal-opportunity-risk__showing">
            Showing {{ opportunityCards.length }} of {{ formatNumber(opportunityTotal) }}
          </p>
          <ul class="principal-opportunity-risk__list">
            <li
              v-for="card in opportunityCards"
              :key="card.key"
              class="principal-opportunity-risk__card"
              :data-supplier-id="card.supplierId"
              :data-rule-id="card.ruleId"
            >
              <div class="principal-opportunity-risk__card-head">
                <span class="principal-opportunity-risk__principal">{{ card.principalName }}</span>
                <span class="principal-opportunity-risk__category">{{ card.categoryLabel }}</span>
              </div>
              <dl class="principal-opportunity-risk__metrics">
                <div class="principal-opportunity-risk__metric">
                  <dt>{{ card.supportLabel }}</dt>
                  <dd>{{ card.supportValue }}</dd>
                  <dd class="principal-opportunity-risk__percentile">
                    Percentile {{ card.percentileLabel }}
                  </dd>
                </div>
                <div v-if="card.secondaryLabel" class="principal-opportunity-risk__metric">
                  <dt>{{ card.secondaryLabel }}</dt>
                  <dd>{{ card.secondaryValue }}</dd>
                  <dd
                    v-if="card.secondaryPercentileLabel"
                    class="principal-opportunity-risk__percentile"
                  >
                    Percentile {{ card.secondaryPercentileLabel }}
                  </dd>
                </div>
                <div class="principal-opportunity-risk__metric">
                  <dt>Population</dt>
                  <dd>{{ formatNumber(card.populationSize) }}</dd>
                </div>
              </dl>
              <p class="principal-opportunity-risk__interpretation">{{ card.interpretation }}</p>
            </li>
          </ul>
        </template>
      </div>

      <div
        class="principal-opportunity-risk__column principal-opportunity-risk__column--risks"
        data-testid="risk-column"
      >
        <h3 class="principal-opportunity-risk__column-title">
          Risks
          <span class="principal-opportunity-risk__column-count">{{ formatNumber(riskTotal) }}</span>
        </h3>

        <p v-if="riskCards.length === 0" class="principal-opportunity-risk__empty">
          No risks for the current period.
        </p>
        <template v-else>
          <p class="principal-opportunity-risk__showing">
            Showing {{ riskCards.length }} of {{ formatNumber(riskTotal) }}
          </p>
          <ul class="principal-opportunity-risk__list">
            <li
              v-for="card in riskCards"
              :key="card.key"
              class="principal-opportunity-risk__card"
              :data-supplier-id="card.supplierId"
              :data-rule-id="card.ruleId"
            >
              <div class="principal-opportunity-risk__card-head">
                <span class="principal-opportunity-risk__principal">{{ card.principalName }}</span>
                <span class="principal-opportunity-risk__category">{{ card.categoryLabel }}</span>
              </div>
              <dl class="principal-opportunity-risk__metrics">
                <div class="principal-opportunity-risk__metric">
                  <dt>{{ card.supportLabel }}</dt>
                  <dd>{{ card.supportValue }}</dd>
                  <dd class="principal-opportunity-risk__percentile">
                    Percentile {{ card.percentileLabel }}
                  </dd>
                </div>
                <div class="principal-opportunity-risk__metric">
                  <dt>Population</dt>
                  <dd>{{ formatNumber(card.populationSize) }}</dd>
                </div>
              </dl>
              <p class="principal-opportunity-risk__interpretation">{{ card.interpretation }}</p>
            </li>
          </ul>
        </template>
      </div>
    </div>
  </section>
</template>

<style scoped>
.principal-opportunity-risk {
  margin-top: 1rem;
  padding: 1rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  box-shadow: var(--dashboard-shadow-idle);
}

.principal-opportunity-risk__title {
  margin: 0 0 0.5rem;
  font-size: 1rem;
}

.principal-opportunity-risk__note {
  margin: 0 0 1rem;
  color: var(--p-text-muted-color);
  font-size: 0.875rem;
}

.principal-opportunity-risk__loading {
  display: flex;
  justify-content: center;
  padding: 2rem 0;
}

.principal-opportunity-risk__columns {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 1rem;
  align-items: start;
}

.principal-opportunity-risk__column {
  padding: 0.75rem;
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
  border-top: 3px solid var(--p-primary-color, #2563eb);
  min-width: 0;
}

.principal-opportunity-risk__column--risks {
  border-top-color: #dc2626;
  background: color-mix(in srgb, #dc2626 3%, white);
}

.principal-opportunity-risk__column-title {
  margin: 0 0 0.5rem;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 0.5rem;
  font-size: 0.9375rem;
}

.principal-opportunity-risk__column-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 1.5rem;
  padding: 0 0.4rem;
  border-radius: 999px;
  background: var(--p-surface-100, #f1f5f9);
  color: var(--p-text-color);
  font-size: 0.75rem;
  font-variant-numeric: tabular-nums;
}

.principal-opportunity-risk__showing {
  margin: 0 0 0.5rem;
  color: var(--p-text-muted-color);
  font-size: 0.75rem;
}

.principal-opportunity-risk__empty {
  margin: 0;
  padding: 0.5rem 0;
  color: var(--p-text-muted-color);
}

.principal-opportunity-risk__list {
  margin: 0;
  padding: 0;
  list-style: none;
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.principal-opportunity-risk__card {
  padding: 0.75rem;
  background: var(--p-surface-0);
  border: 1px solid var(--p-surface-200);
  border-radius: var(--dashboard-radius-sm);
}

.principal-opportunity-risk__card-head {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 0.5rem;
  margin-bottom: 0.5rem;
}

.principal-opportunity-risk__principal {
  font-weight: 600;
  color: var(--p-text-color);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.principal-opportunity-risk__category {
  flex: 0 0 auto;
  padding: 0.1rem 0.4rem;
  border-radius: 999px;
  background: color-mix(in srgb, var(--p-primary-color, #2563eb) 12%, white);
  color: var(--p-text-color);
  font-size: 0.6875rem;
  font-weight: 600;
}

.principal-opportunity-risk__column--risks .principal-opportunity-risk__category {
  background: color-mix(in srgb, #dc2626 12%, white);
}

.principal-opportunity-risk__metrics {
  margin: 0 0 0.5rem;
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 0.5rem 0.75rem;
}

.principal-opportunity-risk__metric {
  display: flex;
  flex-direction: column;
  gap: 0.1rem;
  min-width: 0;
}

.principal-opportunity-risk__metric dt {
  font-size: 0.6875rem;
  font-weight: 600;
  color: var(--p-text-muted-color, #64748b);
}

.principal-opportunity-risk__metric dd {
  margin: 0;
  font-variant-numeric: tabular-nums;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.principal-opportunity-risk__percentile {
  color: var(--p-text-muted-color);
  font-size: 0.75rem;
}

.principal-opportunity-risk__interpretation {
  margin: 0;
  color: var(--p-text-muted-color);
  font-size: 0.8125rem;
}

@media (max-width: 900px) {
  .principal-opportunity-risk__columns {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 600px) {
  .principal-opportunity-risk__metrics {
    grid-template-columns: 1fr;
  }
}
</style>

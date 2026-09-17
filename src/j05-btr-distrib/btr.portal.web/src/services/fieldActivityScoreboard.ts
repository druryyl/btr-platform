import type { DashboardCollectionRankingRow } from '@/models/dashboard'
import type { FieldActivitySalesmanOverviewRow } from '@/models/fieldActivity'
import {
  classifyCommercialSignals,
  type CommercialSignalClassification,
  type CommercialSignalKey,
  type CommercialSignalRow,
} from './commercialSignalRules'
import { joinSalesmenToCollection } from './fieldActivityCollectionComposition'

export type ScoreboardRankMode =
  | 'orderValue'
  | 'orders'
  | 'effectiveCallRate'
  | 'visitExecutionPercent'
  | 'attention'

export interface ScoreboardRankOption {
  mode: ScoreboardRankMode
  label: string
}

export const DEFAULT_SCOREBOARD_RANK_MODE: ScoreboardRankMode = 'orderValue'

export const SCOREBOARD_RANK_OPTIONS: readonly ScoreboardRankOption[] = [
  { mode: 'orderValue', label: 'Order Value' },
  { mode: 'orders', label: 'Orders' },
  { mode: 'effectiveCallRate', label: 'Effective Call Rate' },
  { mode: 'visitExecutionPercent', label: 'Visit Execution %' },
  { mode: 'attention', label: 'Attention (worst first)' },
]

export const SCOREBOARD_ACTION_SIGNAL_KEYS: readonly CommercialSignalKey[] = [
  'NeedsCollectionAction',
  'NeedsCreditReview',
]

export interface ScoreboardAttention {
  salesPersonId: string
  classifications: CommercialSignalClassification[]
  overdueExposure: number | null
}

export type ScoreboardAttentionMap = Map<string, ScoreboardAttention>

export function classifyScoreboardAttention(
  salesmen: ReadonlyArray<FieldActivitySalesmanOverviewRow>,
  topOverdueSalesmen: ReadonlyArray<DashboardCollectionRankingRow>,
): ScoreboardAttentionMap {
  const joined = joinSalesmenToCollection(salesmen, topOverdueSalesmen)
  const rows: CommercialSignalRow[] = joined.map(({ salesman, overdue }) => ({
    SalesPersonId: salesman.SalesPersonId,
    Revenue: salesman.OmzetAmount,
    EffectiveCallRate: salesman.EffectiveCallRate,
    OverdueExposure: overdue?.Amount ?? null,
  }))

  const attentionBySalesman: ScoreboardAttentionMap = new Map()
  joined.forEach(({ salesman, overdue }, index) => {
    attentionBySalesman.set(salesman.SalesPersonId, {
      salesPersonId: salesman.SalesPersonId,
      classifications: classifyCommercialSignals(rows[index], rows),
      overdueExposure: overdue?.Amount ?? null,
    })
  })

  return attentionBySalesman
}

function compareSalesPersonCode(a: string, b: string): number {
  if (a === b) return 0
  return a < b ? -1 : 1
}

function compareDescNullLast(a: number | null, b: number | null): number {
  if (a == null && b == null) return 0
  if (a == null) return 1
  if (b == null) return -1
  if (a === b) return 0
  return b - a
}

function measureAccessor(
  mode: Exclude<ScoreboardRankMode, 'attention'>,
): (row: FieldActivitySalesmanOverviewRow) => number | null {
  const accessors: Record<
    Exclude<ScoreboardRankMode, 'attention'>,
    (row: FieldActivitySalesmanOverviewRow) => number | null
  > = {
    orderValue: (row) => row.OmzetAmount,
    orders: (row) => row.OrdersCount,
    effectiveCallRate: (row) => row.EffectiveCallRate,
    visitExecutionPercent: (row) => row.VisitExecutionPercent,
  }
  return accessors[mode]
}

function isActionSignal(attention: ScoreboardAttention | undefined): boolean {
  if (!attention) return false
  return attention.classifications.some((entry) =>
    SCOREBOARD_ACTION_SIGNAL_KEYS.includes(entry.key),
  )
}

export function rankScoreboardRows(
  rows: ReadonlyArray<FieldActivitySalesmanOverviewRow>,
  mode: ScoreboardRankMode,
  attentionBySalesman: ReadonlyMap<string, ScoreboardAttention> = new Map(),
): FieldActivitySalesmanOverviewRow[] {
  const sorted = [...rows]

  if (mode === 'attention') {
    sorted.sort((a, b) => {
      const aAttention = attentionBySalesman.get(a.SalesPersonId)
      const bAttention = attentionBySalesman.get(b.SalesPersonId)
      const aAction = isActionSignal(aAttention)
      const bAction = isActionSignal(bAttention)

      if (aAction !== bAction) return aAction ? -1 : 1

      if (aAction) {
        const overdueComparison = compareDescNullLast(
          aAttention?.overdueExposure ?? null,
          bAttention?.overdueExposure ?? null,
        )
        if (overdueComparison !== 0) return overdueComparison
      } else {
        const orderValueComparison = compareDescNullLast(a.OmzetAmount, b.OmzetAmount)
        if (orderValueComparison !== 0) return orderValueComparison
      }

      return compareSalesPersonCode(a.SalesPersonCode, b.SalesPersonCode)
    })

    return sorted
  }

  const accessor = measureAccessor(mode)
  sorted.sort((a, b) => {
    const measureComparison = compareDescNullLast(accessor(a), accessor(b))
    if (measureComparison !== 0) return measureComparison
    return compareSalesPersonCode(a.SalesPersonCode, b.SalesPersonCode)
  })

  return sorted
}

export function displayRank(
  orderedRows: ReadonlyArray<FieldActivitySalesmanOverviewRow>,
): Map<string, number> {
  const ranks = new Map<string, number>()
  orderedRows.forEach((row, index) => {
    if (!ranks.has(row.SalesPersonId)) {
      ranks.set(row.SalesPersonId, index + 1)
    }
  })
  return ranks
}

export function columnBarMax(
  rows: ReadonlyArray<FieldActivitySalesmanOverviewRow>,
  accessor: (row: FieldActivitySalesmanOverviewRow) => number | null,
): number {
  let max = 0
  for (const row of rows) {
    const value = accessor(row)
    if (value == null) continue
    if (value > max) max = value
  }
  return max
}

export function barWidthPercent(value: number | null, max: number): number {
  if (value == null || max <= 0) return 0
  const width = (value / max) * 100
  return Number.isFinite(width) ? width : 0
}

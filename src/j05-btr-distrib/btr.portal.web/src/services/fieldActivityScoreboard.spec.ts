import { describe, expect, it } from 'vitest'
import type { DashboardCollectionRankingRow } from '@/models/dashboard'
import type { FieldActivitySalesmanOverviewRow } from '@/models/fieldActivity'
import type { InvestigationMetadata } from '@/models/investigation'
import {
  classifyCommercialSignals,
  needsCollectionAction,
  needsCreditReview,
  type CommercialSignalRow,
} from './commercialSignalRules'
import {
  DEFAULT_SCOREBOARD_RANK_MODE,
  SCOREBOARD_ACTION_SIGNAL_KEYS,
  SCOREBOARD_RANK_OPTIONS,
  barWidthPercent,
  classifyScoreboardAttention,
  columnBarMax,
  displayRank,
  rankScoreboardRows,
} from './fieldActivityScoreboard'

function row(
  code: string,
  overrides: Partial<FieldActivitySalesmanOverviewRow> = {},
): FieldActivitySalesmanOverviewRow {
  return {
    SalesPersonId: code,
    SalesPersonCode: code,
    SalesPersonName: `Name ${code}`,
    WilayahName: 'Wilayah',
    HasEmail: false,
    Rank: 1,
    PlannedVisits: 0,
    ActualVisits: 0,
    VisitExecutionPercent: null,
    EffectiveCalls: 0,
    EffectiveCallRate: null,
    MissedVisits: 0,
    UnplannedVisits: 0,
    GpsValidPercent: null,
    OrdersCount: 0,
    OmzetAmount: 0,
    StatusCode: 'NoActivity',
    ...overrides,
  }
}

function investigation(entityId: string): InvestigationMetadata {
  return {
    SignalKey: 'Overdue',
    SignalLabel: 'Overdue',
    EntityType: 'Salesman',
    EntityId: entityId,
    EntityName: `Entity ${entityId}`,
  }
}

function overdueRow(entityId: string, amount: number): DashboardCollectionRankingRow {
  return {
    Rank: 1,
    EntityCode: entityId,
    EntityName: `Entity ${entityId}`,
    Amount: amount,
    PercentOfTotal: null,
    ReportRoute: null,
    Investigation: investigation(entityId),
  }
}

function codes(rows: ReadonlyArray<FieldActivitySalesmanOverviewRow>): string[] {
  return rows.map((entry) => entry.SalesPersonCode)
}

describe('fieldActivityScoreboard', () => {
  describe('SCOREBOARD_RANK_OPTIONS', () => {
    it('exposes exactly the five approved ranking options', () => {
      expect(SCOREBOARD_RANK_OPTIONS.map((option) => option.mode)).toEqual([
        'orderValue',
        'orders',
        'effectiveCallRate',
        'visitExecutionPercent',
        'attention',
      ])
      expect(SCOREBOARD_RANK_OPTIONS.map((option) => option.label)).toEqual([
        'Order Value',
        'Orders',
        'Effective Call Rate',
        'Visit Execution %',
        'Attention (worst first)',
      ])
    })

    it('defaults to Order Value descending (outcome-first)', () => {
      expect(DEFAULT_SCOREBOARD_RANK_MODE).toBe('orderValue')
    })

    it('limits attention signals to the two action rules', () => {
      expect(SCOREBOARD_ACTION_SIGNAL_KEYS).toEqual([
        'NeedsCollectionAction',
        'NeedsCreditReview',
      ])
    })
  })

  describe('rankScoreboardRows — value modes', () => {
    it('orders by Order Value descending', () => {
      const rows = [
        row('S1', { OmzetAmount: 30 }),
        row('S2', { OmzetAmount: 90 }),
        row('S3', { OmzetAmount: 60 }),
      ]

      expect(codes(rankScoreboardRows(rows, 'orderValue'))).toEqual(['S2', 'S3', 'S1'])
    })

    it('orders by Orders descending', () => {
      const rows = [
        row('S1', { OrdersCount: 2 }),
        row('S2', { OrdersCount: 9 }),
        row('S3', { OrdersCount: 5 }),
      ]

      expect(codes(rankScoreboardRows(rows, 'orders'))).toEqual(['S2', 'S3', 'S1'])
    })

    it('orders by Effective Call Rate descending with null last', () => {
      const rows = [
        row('S1', { EffectiveCallRate: 20 }),
        row('S2', { EffectiveCallRate: null }),
        row('S3', { EffectiveCallRate: 80 }),
      ]

      expect(codes(rankScoreboardRows(rows, 'effectiveCallRate'))).toEqual(['S3', 'S1', 'S2'])
    })

    it('orders by Visit Execution % descending with null last', () => {
      const rows = [
        row('S1', { VisitExecutionPercent: null }),
        row('S2', { VisitExecutionPercent: 40 }),
        row('S3', { VisitExecutionPercent: 95 }),
      ]

      expect(codes(rankScoreboardRows(rows, 'visitExecutionPercent'))).toEqual(['S3', 'S2', 'S1'])
    })

    it('breaks ties by SalesPersonCode ascending', () => {
      const rows = [
        row('S3', { OmzetAmount: 100 }),
        row('S1', { OmzetAmount: 100 }),
        row('S2', { OmzetAmount: 100 }),
      ]

      expect(codes(rankScoreboardRows(rows, 'orderValue'))).toEqual(['S1', 'S2', 'S3'])
    })

    it('keeps null-measure rows in ascending code order at the end', () => {
      const rows = [
        row('S2', { VisitExecutionPercent: null }),
        row('S1', { VisitExecutionPercent: null }),
        row('S3', { VisitExecutionPercent: 10 }),
      ]

      expect(codes(rankScoreboardRows(rows, 'visitExecutionPercent'))).toEqual(['S3', 'S1', 'S2'])
    })

    it('returns an empty list for empty rows', () => {
      expect(rankScoreboardRows([], 'orderValue')).toEqual([])
    })

    it('orders an all-zero population by code ascending', () => {
      const rows = [row('S2'), row('S3'), row('S1')]

      expect(codes(rankScoreboardRows(rows, 'orderValue'))).toEqual(['S1', 'S2', 'S3'])
      expect(codes(rankScoreboardRows(rows, 'orders'))).toEqual(['S1', 'S2', 'S3'])
      expect(codes(rankScoreboardRows(rows, 'effectiveCallRate'))).toEqual(['S1', 'S2', 'S3'])
    })

    it('does not mutate the input order', () => {
      const rows = [row('S1', { OmzetAmount: 10 }), row('S2', { OmzetAmount: 20 })]
      const original = [...rows]

      rankScoreboardRows(rows, 'orderValue')

      expect(rows).toEqual(original)
    })
  })

  describe('rankScoreboardRows — attention mode', () => {
    const salesmen = [
      row('S1', { OmzetAmount: 10, EffectiveCallRate: 10 }),
      row('S2', { OmzetAmount: 20, EffectiveCallRate: 20 }),
      row('S3', { OmzetAmount: 30, EffectiveCallRate: 30 }),
      row('S4', { OmzetAmount: 40, EffectiveCallRate: 40 }),
      row('S5', { OmzetAmount: 50, EffectiveCallRate: 50 }),
      row('S6', { OmzetAmount: 60, EffectiveCallRate: 60 }),
      row('S7', { OmzetAmount: 70, EffectiveCallRate: 70 }),
      row('S8', { OmzetAmount: 80, EffectiveCallRate: 80 }),
    ]
    const topOverdue = [
      overdueRow('S1', 5),
      overdueRow('S2', 10),
      overdueRow('S3', 15),
      overdueRow('S4', 20),
      overdueRow('S5', 25),
      overdueRow('S6', 30),
      overdueRow('S7', 90),
      overdueRow('S8', 80),
    ]
    const attention = classifyScoreboardAttention(salesmen, topOverdue)

    it('places action salesmen first by Overdue Exposure descending, then the rest by Order Value descending', () => {
      expect(codes(rankScoreboardRows(salesmen, 'attention', attention))).toEqual([
        'S7',
        'S8',
        'S6',
        'S5',
        'S4',
        'S3',
        'S2',
        'S1',
      ])
    })

    it('falls back to Order Value descending for all rows when attention is unavailable', () => {
      expect(codes(rankScoreboardRows(salesmen, 'attention'))).toEqual([
        'S8',
        'S7',
        'S6',
        'S5',
        'S4',
        'S3',
        'S2',
        'S1',
      ])
    })

    it('breaks ties by SalesPersonCode ascending', () => {
      const ties = [
        row('S2', { OmzetAmount: 50, EffectiveCallRate: 50 }),
        row('S1', { OmzetAmount: 50, EffectiveCallRate: 50 }),
        row('S3', { OmzetAmount: 50, EffectiveCallRate: 50 }),
        row('S4', { OmzetAmount: 50, EffectiveCallRate: 50 }),
      ]

      expect(codes(rankScoreboardRows(ties, 'attention'))).toEqual(['S1', 'S2', 'S3', 'S4'])
    })

    it('returns an empty list for an empty population', () => {
      expect(rankScoreboardRows([], 'attention', attention)).toEqual([])
    })
  })

  describe('classifyScoreboardAttention', () => {
    const salesmen = [
      row('S1', { OmzetAmount: 10, EffectiveCallRate: 10 }),
      row('S2', { OmzetAmount: 20, EffectiveCallRate: 20 }),
      row('S3', { OmzetAmount: 30, EffectiveCallRate: 30 }),
      row('S4', { OmzetAmount: 40, EffectiveCallRate: 40 }),
      row('S5', { OmzetAmount: 50, EffectiveCallRate: 50 }),
      row('S6', { OmzetAmount: 60, EffectiveCallRate: 60 }),
      row('S7', { OmzetAmount: 70, EffectiveCallRate: 70 }),
      row('S8', { OmzetAmount: 80, EffectiveCallRate: 80 }),
    ]
    const topOverdue = [
      overdueRow('S1', 5),
      overdueRow('S2', 10),
      overdueRow('S3', 15),
      overdueRow('S4', 20),
      overdueRow('S5', 25),
      overdueRow('S6', 30),
      overdueRow('S7', 90),
      overdueRow('S8', 80),
    ]

    it('returns one entry per salesman keyed by SalesPersonId', () => {
      const attention = classifyScoreboardAttention(salesmen, topOverdue)

      expect(attention.size).toBe(8)
      expect(attention.get('S1')?.salesPersonId).toBe('S1')
    })

    it('surfaces the action classifications on the high-overdue salesmen', () => {
      const attention = classifyScoreboardAttention(salesmen, topOverdue)
      const keys = (id: string): string[] =>
        attention.get(id)?.classifications.map((entry) => entry.key) ?? []

      expect(keys('S7')).toContain('NeedsCollectionAction')
      expect(keys('S7')).toContain('NeedsCreditReview')
      expect(keys('S8')).toContain('NeedsCollectionAction')
      expect(keys('S1')).not.toContain('NeedsCollectionAction')
      expect(keys('S1')).not.toContain('NeedsCreditReview')
    })

    it('reports the joined Overdue Exposure and null when no collection row exists', () => {
      const attention = classifyScoreboardAttention(salesmen, topOverdue)

      expect(attention.get('S7')?.overdueExposure).toBe(90)
      expect(attention.get('S1')?.overdueExposure).toBe(5)
    })

    it('classifies identically to a direct Action Center rule run over the same population', () => {
      const attention = classifyScoreboardAttention(salesmen, topOverdue)
      const overdueBySalesman = new Map(topOverdue.map((item) => [item.EntityCode, item.Amount]))
      const signalRows: CommercialSignalRow[] = salesmen.map((salesman) => ({
        SalesPersonId: salesman.SalesPersonId,
        Revenue: salesman.OmzetAmount,
        EffectiveCallRate: salesman.EffectiveCallRate,
        OverdueExposure: overdueBySalesman.get(salesman.SalesPersonCode) ?? null,
      }))

      salesmen.forEach((salesman, index) => {
        const signalRow = signalRows[index]
        expect(attention.get(salesman.SalesPersonId)?.classifications).toEqual(
          classifyCommercialSignals(signalRow, signalRows),
        )
        const attentionKeys =
          attention
            .get(salesman.SalesPersonId)
            ?.classifications.map((entry) => entry.key) ?? []
        expect(attentionKeys.includes('NeedsCollectionAction')).toBe(
          needsCollectionAction(signalRow, signalRows),
        )
        expect(attentionKeys.includes('NeedsCreditReview')).toBe(
          needsCreditReview(signalRow, signalRows),
        )
      })
    })

    it('degrades to empty classification without overdue data', () => {
      const attention = classifyScoreboardAttention(salesmen, [])
      const keys =
        attention.get('S7')?.classifications.map((entry) => entry.key) ?? []

      expect(attention.size).toBe(8)
      expect(attention.get('S7')?.overdueExposure).toBeNull()
      expect(keys).not.toContain('NeedsCollectionAction')
      expect(keys).not.toContain('NeedsCreditReview')
    })

    it('returns an empty map for an empty population', () => {
      expect(classifyScoreboardAttention([], topOverdue).size).toBe(0)
    })
  })

  describe('displayRank', () => {
    it('assigns 1-based positions in the supplied display order', () => {
      const ordered = [row('S3'), row('S1'), row('S2')]

      const ranks = displayRank(ordered)

      expect(ranks.get('S3')).toBe(1)
      expect(ranks.get('S1')).toBe(2)
      expect(ranks.get('S2')).toBe(3)
    })

    it('returns an empty map for empty rows', () => {
      expect(displayRank([]).size).toBe(0)
    })

    it('numbers positions from the current order, not the backend Rank field', () => {
      const ordered = [row('S1', { Rank: 99 }), row('S2', { Rank: 1 })]

      const ranks = displayRank(ordered)

      expect(ranks.get('S1')).toBe(1)
      expect(ranks.get('S2')).toBe(2)
    })
  })

  describe('columnBarMax', () => {
    it('returns the maximum non-null value over the current rows', () => {
      const rows = [
        row('S1', { OmzetAmount: 30 }),
        row('S2', { OmzetAmount: 90 }),
        row('S3', { OmzetAmount: 60 }),
      ]

      expect(columnBarMax(rows, (entry) => entry.OmzetAmount)).toBe(90)
    })

    it('ignores null values', () => {
      const rows = [
        row('S1', { EffectiveCallRate: null }),
        row('S2', { EffectiveCallRate: 40 }),
        row('S3', { EffectiveCallRate: null }),
      ]

      expect(columnBarMax(rows, (entry) => entry.EffectiveCallRate)).toBe(40)
    })

    it('returns 0 for empty rows and for all-null or all-zero rows', () => {
      expect(columnBarMax([], (entry) => entry.OmzetAmount)).toBe(0)
      expect(
        columnBarMax([row('S1'), row('S2')], (entry) => entry.OrdersCount),
      ).toBe(0)
      expect(
        columnBarMax([row('S1'), row('S2')], (entry) => entry.EffectiveCallRate),
      ).toBe(0)
    })
  })

  describe('barWidthPercent', () => {
    it('scales the value as value ÷ max × 100%', () => {
      expect(barWidthPercent(50, 100)).toBe(50)
      expect(barWidthPercent(90, 90)).toBe(100)
    })

    it('returns 0 for a null value or a non-positive maximum', () => {
      expect(barWidthPercent(null, 100)).toBe(0)
      expect(barWidthPercent(50, 0)).toBe(0)
    })

    it('never produces NaN or Infinity', () => {
      expect(Number.isFinite(barWidthPercent(10, 3))).toBe(true)
      expect(Number.isFinite(barWidthPercent(0, 0))).toBe(true)
    })
  })
})

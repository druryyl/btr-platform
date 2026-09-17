import { describe, expect, it } from 'vitest'
import type {
  DashboardCollectionAgingBucket,
  DashboardCollectionAttentionCards,
  DashboardCollectionRankingRow,
} from '@/models/dashboard'
import type { InvestigationMetadata } from '@/models/investigation'
import {
  collectionActionInputs,
  joinSalesmenToCollection,
  overdueDistribution,
  territoryFinancialHealth,
} from './fieldActivityCollectionComposition'

function investigation(entityId: string, entityName = 'Entity'): InvestigationMetadata {
  return {
    SignalKey: 'Overdue',
    SignalLabel: 'Overdue',
    EntityType: 'Salesman',
    EntityId: entityId,
    EntityName: entityName,
  }
}

function rankingRow(
  overrides: Partial<DashboardCollectionRankingRow> = {},
): DashboardCollectionRankingRow {
  return {
    Rank: 1,
    EntityCode: 'C001',
    EntityName: 'Entity',
    Amount: 0,
    PercentOfTotal: null,
    ReportRoute: null,
    Investigation: null,
    ...overrides,
  }
}

function attentionCards(
  overrides: Partial<DashboardCollectionAttentionCards> = {},
): DashboardCollectionAttentionCards {
  return {
    OverdueExposure: 0,
    AgingOver90Exposure: 0,
    OverdueConcentrationPercent: null,
    ExposureRequiresAttention: false,
    CashCollectedMtd: 0,
    RecoveryVsBillingPercent: null,
    RecoveryRequiresAttention: false,
    LegacyDebtCount: 0,
    PortfolioRequiresAttention: false,
    ...overrides,
  }
}

function salesman(salesPersonId: string, salesPersonName = 'Salesman') {
  return { SalesPersonId: salesPersonId, SalesPersonName: salesPersonName }
}

describe('fieldActivityCollectionComposition', () => {
  describe('joinSalesmenToCollection', () => {
    it('joins on Investigation.EntityId (canonical SalesPersonId)', () => {
      const salesmen = [salesman('S1'), salesman('S2'), salesman('S3')]
      const overdue = rankingRow({
        EntityName: 'Salesman Two',
        Amount: 500,
        Investigation: investigation('S2', 'Salesman Two'),
      })

      const joined = joinSalesmenToCollection(salesmen, [overdue])

      expect(joined).toHaveLength(3)
      expect(joined[0].salesman.SalesPersonId).toBe('S1')
      expect(joined[0].overdue).toBeNull()
      expect(joined[1].salesman.SalesPersonId).toBe('S2')
      expect(joined[1].overdue).toBe(overdue)
      expect(joined[2].overdue).toBeNull()
    })

    it('never uses salesman name as the join key', () => {
      const salesmen = [salesman('S1', 'Alice')]
      const nameOnlyMatch = rankingRow({
        EntityCode: 'S1',
        EntityName: 'Alice',
        Amount: 900,
        Investigation: investigation('OTHER', 'Alice'),
      })

      const joined = joinSalesmenToCollection(salesmen, [nameOnlyMatch])

      expect(joined[0].overdue).toBeNull()
    })

    it('treats a salesman without an overdue row as having no collection signal', () => {
      const salesmen = [salesman('S1'), salesman('S2')]
      const overdue = rankingRow({ Investigation: investigation('S2') })

      const joined = joinSalesmenToCollection(salesmen, [overdue])

      expect(joined[0].overdue).toBeNull()
      expect(joined[1].overdue).not.toBeNull()
    })

    it('ignores overdue rows without an Investigation entity id', () => {
      const salesmen = [salesman('S1')]
      const noEntityId = rankingRow({ EntityCode: 'S1', EntityName: 'Salesman' })

      const joined = joinSalesmenToCollection(salesmen, [noEntityId])

      expect(joined[0].overdue).toBeNull()
    })

    it('resolves duplicate EntityId matches deterministically to the first row', () => {
      const salesmen = [salesman('S1')]
      const first = rankingRow({ Amount: 100, Investigation: investigation('S1') })
      const second = rankingRow({ Amount: 200, Investigation: investigation('S1') })

      const joined = joinSalesmenToCollection(salesmen, [first, second])

      expect(joined[0].overdue).toBe(first)
    })
  })

  describe('territoryFinancialHealth', () => {
    it('maps Overdue Exposure and Overdue Concentration per Wilayah', () => {
      const rows = [
        rankingRow({ EntityName: 'Yogyakarta', Amount: 1000, PercentOfTotal: 40 }),
        rankingRow({ EntityName: 'Solo', Amount: 500, PercentOfTotal: 20 }),
      ]

      const result = territoryFinancialHealth(rows, attentionCards())

      expect(result.territories).toEqual([
        { wilayahName: 'Yogyakarta', overdueExposure: 1000, overdueConcentrationPercent: 40 },
        { wilayahName: 'Solo', overdueExposure: 500, overdueConcentrationPercent: 20 },
      ])
    })

    it('returns an empty territory list for an empty TopOverdueWilayah', () => {
      const result = territoryFinancialHealth([], attentionCards())

      expect(result.territories).toEqual([])
    })

    it('preserves null PercentOfTotal instead of coercing it to zero', () => {
      const rows = [rankingRow({ EntityName: 'Klaten', Amount: 300, PercentOfTotal: null })]

      const result = territoryFinancialHealth(rows, attentionCards())

      expect(result.territories[0].overdueConcentrationPercent).toBeNull()
    })

    it('surfaces the page-level Aging Risk summary from AttentionCards and AgingRiskSummary', () => {
      const buckets: DashboardCollectionAgingBucket[] = [
        { BucketKey: 'Over90', BucketLabel: 'Over 90 days', Amount: 250, SortOrder: 4 },
      ]

      const result = territoryFinancialHealth(
        [],
        attentionCards({ AgingOver90Exposure: 1234 }),
        buckets,
      )

      expect(result.agingRisk.agingOver90Exposure).toBe(1234)
      expect(result.agingRisk.buckets).toEqual(buckets)
    })

    it('does not compute a Collection Amount by Territory measure (GAP-002)', () => {
      const rows = [rankingRow({ EntityName: 'Magelang', Amount: 750, PercentOfTotal: 30 })]

      const result = territoryFinancialHealth(rows, attentionCards())

      expect(Object.keys(result.territories[0]).sort()).toEqual(
        ['overdueConcentrationPercent', 'overdueExposure', 'wilayahName'].sort(),
      )
    })

    it('degrades the Aging Risk summary when AttentionCards is unavailable', () => {
      const result = territoryFinancialHealth([], null)

      expect(result.agingRisk.agingOver90Exposure).toBe(0)
      expect(result.agingRisk.buckets).toEqual([])
    })
  })

  describe('overdueDistribution', () => {
    it('returns a contribution-style list with name, amount, and PercentOfTotal', () => {
      const rows = [
        rankingRow({ EntityName: 'Yogyakarta', Amount: 1000, PercentOfTotal: 40 }),
        rankingRow({ EntityName: 'Solo', Amount: 500, PercentOfTotal: 20 }),
      ]

      expect(overdueDistribution(rows)).toEqual([
        { wilayahName: 'Yogyakarta', amount: 1000, percentOfTotal: 40 },
        { wilayahName: 'Solo', amount: 500, percentOfTotal: 20 },
      ])
    })

    it('returns an empty list for an empty TopOverdueWilayah', () => {
      expect(overdueDistribution([])).toEqual([])
    })

    it('preserves null PercentOfTotal', () => {
      const rows = [rankingRow({ EntityName: 'Klaten', Amount: 300, PercentOfTotal: null })]

      expect(overdueDistribution(rows)[0].percentOfTotal).toBeNull()
    })
  })

  describe('collectionActionInputs', () => {
    it('surfaces Needs Collection Action as the highest overdue by salesman', () => {
      const rows = [
        rankingRow({ EntityName: 'Low', Amount: 100, Investigation: investigation('S1', 'Low') }),
        rankingRow({ EntityName: 'High', Amount: 900, Investigation: investigation('S2', 'High') }),
        rankingRow({ EntityName: 'Mid', Amount: 400, Investigation: investigation('S3', 'Mid') }),
      ]

      const result = collectionActionInputs(rows, attentionCards())

      expect(result.needsCollectionAction.map((entry) => entry.salesPersonName)).toEqual([
        'High',
        'Mid',
        'Low',
      ])
      expect(result.needsCollectionAction[0].overdueAmount).toBe(900)
      expect(result.needsCollectionAction[0].salesPersonId).toBe('S2')
    })

    it('preserves null PercentOfTotal and surfaces Investigation metadata', () => {
      const row = rankingRow({
        EntityName: 'Salesman',
        EntityCode: 'S1',
        Amount: 100,
        PercentOfTotal: null,
        Investigation: investigation('S1', 'Salesman'),
      })

      const result = collectionActionInputs([row], attentionCards())

      expect(result.needsCollectionAction[0].percentOfTotal).toBeNull()
      expect(result.needsCollectionAction[0].investigation).toBe(row.Investigation)
      expect(result.needsCollectionAction[0].salesPersonCode).toBe('S1')
    })

    it('surfaces Needs Escalation inputs from AttentionCards', () => {
      const result = collectionActionInputs(
        [],
        attentionCards({ AgingOver90Exposure: 4321, LegacyDebtCount: 7 }),
      )

      expect(result.needsEscalation).toEqual({
        agingOver90Exposure: 4321,
        legacyDebtCount: 7,
      })
    })

    it('degrades Needs Escalation inputs when AttentionCards is unavailable', () => {
      const result = collectionActionInputs([], null)

      expect(result.needsEscalation).toEqual({ agingOver90Exposure: 0, legacyDebtCount: 0 })
    })
  })
})

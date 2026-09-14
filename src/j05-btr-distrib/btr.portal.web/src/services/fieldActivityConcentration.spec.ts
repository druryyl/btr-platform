import { describe, expect, it } from 'vitest'
import type { FieldActivitySalesmanOverviewRow } from '@/models/fieldActivity'
import {
  buildRevenueConcentration,
  CONCENTRATION_OTHERS_KEY,
} from './fieldActivityConcentration'

function salesman(
  overrides: Partial<FieldActivitySalesmanOverviewRow> = {},
): FieldActivitySalesmanOverviewRow {
  const salesPersonId = overrides.SalesPersonId ?? 'sp-1'
  const salesPersonCode = overrides.SalesPersonCode ?? 'S001'
  return {
    SalesPersonId: salesPersonId,
    SalesPersonCode: salesPersonCode,
    SalesPersonName: overrides.SalesPersonName ?? `Salesman ${salesPersonCode}`,
    WilayahName: 'Jakarta',
    HasEmail: true,
    Rank: 1,
    PlannedVisits: 10,
    ActualVisits: 9,
    VisitExecutionPercent: 90,
    EffectiveCalls: 5,
    EffectiveCallRate: 55.5,
    MissedVisits: 1,
    UnplannedVisits: 0,
    GpsValidPercent: 100,
    OrdersCount: 3,
    OmzetAmount: 0,
    StatusCode: 'OnTrack',
    ...overrides,
  }
}

describe('fieldActivityConcentration', () => {
  describe('buildRevenueConcentration', () => {
    it('returns total, top-1, top-3 shares and ordered segments for a normal team', () => {
      const salesmen = [
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 500 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 300 }),
        salesman({ SalesPersonId: 'c', SalesPersonCode: 'S003', OmzetAmount: 100 }),
        salesman({ SalesPersonId: 'd', SalesPersonCode: 'S004', OmzetAmount: 60 }),
        salesman({ SalesPersonId: 'e', SalesPersonCode: 'S005', OmzetAmount: 40 }),
      ]

      const result = buildRevenueConcentration(salesmen)

      expect(result.total).toBe(1000)
      expect(result.top1Share).toBe(50)
      expect(result.top3Share).toBe(90)
      expect(result.isEmpty).toBe(false)
      expect(result.segments.map((segment) => segment.amount)).toEqual([500, 300, 100, 100])
      expect(result.segments.map((segment) => segment.percent)).toEqual([50, 30, 10, 10])
    })

    it('names the top three salesmen and labels the remainder Others', () => {
      const salesmen = [
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', SalesPersonName: 'Ana', OmzetAmount: 500 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', SalesPersonName: 'Budi', OmzetAmount: 300 }),
        salesman({ SalesPersonId: 'c', SalesPersonCode: 'S003', SalesPersonName: 'Cici', OmzetAmount: 100 }),
        salesman({ SalesPersonId: 'd', SalesPersonCode: 'S004', SalesPersonName: 'Dedi', OmzetAmount: 100 }),
      ]

      const result = buildRevenueConcentration(salesmen)

      expect(result.segments).toHaveLength(4)
      expect(result.segments.slice(0, 3)).toEqual([
        { key: 'a', label: 'Ana', code: 'S001', amount: 500, percent: 50 },
        { key: 'b', label: 'Budi', code: 'S002', amount: 300, percent: 30 },
        { key: 'c', label: 'Cici', code: 'S003', amount: 100, percent: 10 },
      ])
      expect(result.segments[3]).toEqual({
        key: CONCENTRATION_OTHERS_KEY,
        label: 'Others',
        code: null,
        amount: 100,
        percent: 10,
      })
    })

    it('computes the Others remainder as total minus the top-three amount', () => {
      const salesmen = [
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 700 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 150 }),
        salesman({ SalesPersonId: 'c', SalesPersonCode: 'S003', OmzetAmount: 100 }),
        salesman({ SalesPersonId: 'd', SalesPersonCode: 'S004', OmzetAmount: 40 }),
        salesman({ SalesPersonId: 'e', SalesPersonCode: 'S005', OmzetAmount: 10 }),
      ]

      const result = buildRevenueConcentration(salesmen)
      const others = result.segments.find((segment) => segment.key === CONCENTRATION_OTHERS_KEY)
      const namedAmount = result.segments
        .filter((segment) => segment.key !== CONCENTRATION_OTHERS_KEY)
        .reduce((sum, segment) => sum + segment.amount, 0)

      expect(others?.amount).toBe(50)
      expect(namedAmount + (others?.amount ?? 0)).toBe(result.total)
    })

    it('returns exact totals without floating point drift', () => {
      const salesmen = [
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 12_500_000 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 7_250_000 }),
        salesman({ SalesPersonId: 'c', SalesPersonCode: 'S003', OmzetAmount: 250_000 }),
      ]

      const result = buildRevenueConcentration(salesmen)

      expect(result.total).toBe(20_000_000)
      expect(result.segments.reduce((sum, segment) => sum + segment.amount, 0)).toBe(20_000_000)
    })

    it('handles a single salesman as 100% concentration with no Others segment', () => {
      const result = buildRevenueConcentration([
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 400 }),
      ])

      expect(result.total).toBe(400)
      expect(result.top1Share).toBe(100)
      expect(result.top3Share).toBe(100)
      expect(result.isEmpty).toBe(false)
      expect(result.segments).toEqual([
        { key: 'a', label: 'Salesman S001', code: 'S001', amount: 400, percent: 100 },
      ])
      expect(result.segments.some((segment) => segment.key === CONCENTRATION_OTHERS_KEY)).toBe(false)
    })

    it('handles two salesmen without a phantom Others segment', () => {
      const result = buildRevenueConcentration([
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 300 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 100 }),
      ])

      expect(result.total).toBe(400)
      expect(result.top1Share).toBe(75)
      expect(result.top3Share).toBe(100)
      expect(result.segments).toHaveLength(2)
      expect(result.segments.some((segment) => segment.key === CONCENTRATION_OTHERS_KEY)).toBe(false)
    })

    it('sorts deterministic ties by SalesPersonCode ascending', () => {
      const result = buildRevenueConcentration([
        salesman({ SalesPersonId: 'z', SalesPersonCode: 'S009', OmzetAmount: 100 }),
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 100 }),
        salesman({ SalesPersonId: 'm', SalesPersonCode: 'S005', OmzetAmount: 100 }),
        salesman({ SalesPersonId: 'x', SalesPersonCode: 'S004', OmzetAmount: 100 }),
      ])

      expect(result.segments.map((segment) => segment.code)).toEqual(['S001', 'S004', 'S005', null])
      expect(result.top3Share).toBe(75)
    })

    it('does not let zero-amount salesmen outrank positive ones', () => {
      const result = buildRevenueConcentration([
        salesman({ SalesPersonId: 'zero', SalesPersonCode: 'S000', OmzetAmount: 0 }),
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 10 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 5 }),
      ])

      expect(result.segments.map((segment) => segment.key)).toEqual(['a', 'b'])
      expect(result.segments.some((segment) => segment.key === 'zero')).toBe(false)
    })

    it('omits zero-width segments and does not invent an Others segment of zero', () => {
      const result = buildRevenueConcentration([
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 80 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 20 }),
        salesman({ SalesPersonId: 'c', SalesPersonCode: 'S003', OmzetAmount: 0 }),
        salesman({ SalesPersonId: 'd', SalesPersonCode: 'S004', OmzetAmount: 0 }),
      ])

      expect(result.segments).toHaveLength(2)
      expect(result.segments.every((segment) => segment.amount > 0)).toBe(true)
      expect(result.top3Share).toBe(100)
    })

    it('returns the empty state when there are no salesmen', () => {
      const result = buildRevenueConcentration([])

      expect(result).toEqual({
        total: 0,
        top1Share: null,
        top3Share: null,
        segments: [],
        isEmpty: true,
      })
    })

    it('returns the empty state when total order value is zero', () => {
      const result = buildRevenueConcentration([
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 0 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 0 }),
      ])

      expect(result.total).toBe(0)
      expect(result.top1Share).toBeNull()
      expect(result.top3Share).toBeNull()
      expect(result.segments).toEqual([])
      expect(result.isEmpty).toBe(true)
    })

    it('never produces NaN or Infinity across shares and segments', () => {
      const result = buildRevenueConcentration([
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 1 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 2 }),
        salesman({ SalesPersonId: 'c', SalesPersonCode: 'S003', OmzetAmount: 0 }),
      ])

      expect(Number.isFinite(result.total)).toBe(true)
      expect(Number.isFinite(result.top1Share)).toBe(true)
      expect(Number.isFinite(result.top3Share)).toBe(true)
      for (const segment of result.segments) {
        expect(Number.isFinite(segment.amount)).toBe(true)
        expect(Number.isFinite(segment.percent)).toBe(true)
      }
    })

    it('renders shares to at most one decimal place', () => {
      const result = buildRevenueConcentration([
        salesman({ SalesPersonId: 'a', SalesPersonCode: 'S001', OmzetAmount: 1 }),
        salesman({ SalesPersonId: 'b', SalesPersonCode: 'S002', OmzetAmount: 1 }),
        salesman({ SalesPersonId: 'c', SalesPersonCode: 'S003', OmzetAmount: 1 }),
      ])

      expect(result.top1Share).toBe(33.3)
      expect(result.top3Share).toBe(100)
      for (const segment of result.segments) {
        expect(Math.round(segment.percent * 10)).toBe(segment.percent * 10)
      }
    })
  })
})

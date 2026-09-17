import { describe, expect, it } from 'vitest'
import {
  COMMERCIAL_SIGNAL_KEYS,
  classifyCommercialSignals,
  isHealthyPortfolio,
  isHighOverdue,
  isHighRevenue,
  isRecognitionCandidate,
  needsCollectionAction,
  needsCreditReview,
  recognitionCandidates,
  type CommercialSignalRow,
} from './commercialSignalRules'

function row(
  SalesPersonId: string,
  values: Partial<Omit<CommercialSignalRow, 'SalesPersonId'>> = {},
): CommercialSignalRow {
  return {
    SalesPersonId,
    Revenue: null,
    EffectiveCallRate: null,
    OverdueExposure: null,
    ...values,
  }
}

describe('commercialSignalRules', () => {
  describe('empty population', () => {
    const empty: CommercialSignalRow[] = []
    const subject = row('S1', { Revenue: 100, EffectiveCallRate: 50, OverdueExposure: 10 })

    it('classifies every rule as false without throwing', () => {
      expect(isHighRevenue(subject, empty)).toBe(false)
      expect(isHighOverdue(subject, empty)).toBe(false)
      expect(isHealthyPortfolio(subject, empty)).toBe(false)
      expect(needsCreditReview(subject, empty)).toBe(false)
      expect(needsCollectionAction(subject, empty)).toBe(false)
      expect(isRecognitionCandidate(subject, empty)).toBe(false)
    })

    it('returns no recognition candidates or classifications', () => {
      expect(recognitionCandidates(empty)).toEqual([])
      expect(classifyCommercialSignals(subject, empty)).toEqual([])
    })
  })

  describe('small population', () => {
    it('does not throw for a single-salesman population and degrades to the defined rule', () => {
      const single = row('S1', { Revenue: 100, EffectiveCallRate: 50, OverdueExposure: 10 })
      expect(() => isHighRevenue(single, [single])).not.toThrow()
      expect(isHighRevenue(single, [single])).toBe(false)
      expect(isHealthyPortfolio(single, [single])).toBe(true)
      expect(isRecognitionCandidate(single, [single])).toBe(false)
    })

    it('does not throw for populations of two and three salesmen', () => {
      const two = [row('S1', { Revenue: 20, OverdueExposure: 5 }), row('S2', { Revenue: 10, OverdueExposure: 15 })]
      const three = [...two, row('S3', { Revenue: 30, OverdueExposure: 25 })]
      expect(() => recognitionCandidates(two)).not.toThrow()
      expect(() => recognitionCandidates(three)).not.toThrow()
      expect(recognitionCandidates(two)).toEqual([])
      expect(recognitionCandidates(three)).toEqual([])
    })
  })

  describe('missing values', () => {
    it('excludes null revenue from the high-revenue population instead of treating it as zero', () => {
      const target = row('S1', { Revenue: 40 })
      const rows = [target, row('S2', { Revenue: null }), row('S3', { Revenue: 30 }), row('S4', { Revenue: 20 })]
      expect(isHighRevenue(target, rows)).toBe(false)
    })

    it('excludes null overdue from the high-overdue population instead of treating it as zero', () => {
      const target = row('S1', { OverdueExposure: 30 })
      const rows = [target, row('S2', { OverdueExposure: null }), row('S3', { OverdueExposure: 20 }), row('S4', { OverdueExposure: 10 })]
      expect(isHighOverdue(target, rows)).toBe(false)
    })

    it('excludes null overdue from the healthy-portfolio population instead of treating it as zero', () => {
      const target = row('S1', { OverdueExposure: 20 })
      const rows = [
        target,
        row('S2', { OverdueExposure: null }),
        row('S3', { OverdueExposure: 10 }),
        row('S4', { OverdueExposure: 30 }),
        row('S5', { OverdueExposure: 40 }),
      ]
      expect(isHealthyPortfolio(target, rows)).toBe(true)
    })
  })

  describe('ties', () => {
    it('resolves equal values deterministically with the same percentile', () => {
      const a = row('S1', { Revenue: 10, OverdueExposure: 10 })
      const b = row('S2', { Revenue: 10, OverdueExposure: 10 })
      const c = row('S3', { Revenue: 20, OverdueExposure: 20 })
      const d = row('S4', { Revenue: 30, OverdueExposure: 30 })
      const rows = [a, b, c, d]
      expect(isHighRevenue(a, rows)).toBe(isHighRevenue(b, rows))
      expect(isHealthyPortfolio(a, rows)).toBe(true)
      expect(isHealthyPortfolio(b, rows)).toBe(true)
      expect(isHighRevenue(a, rows)).toBe(false)
    })

    it('does not inflate the rank for values tied at the top', () => {
      const target = row('S4', { OverdueExposure: 40 })
      const rows = [
        row('S1', { OverdueExposure: 10 }),
        row('S2', { OverdueExposure: 20 }),
        row('S3', { OverdueExposure: 30 }),
        target,
        row('S5', { OverdueExposure: 40 }),
      ]
      expect(isHighOverdue(target, rows)).toBe(false)
    })
  })

  describe('rule boundaries at the 25% quartile', () => {
    const rows = [
      row('S1', { Revenue: 10, OverdueExposure: 5 }),
      row('S2', { Revenue: 20, OverdueExposure: 15 }),
      row('S3', { Revenue: 30, OverdueExposure: 25 }),
      row('S4', { Revenue: 40, OverdueExposure: 35 }),
    ]

    it('isHighRevenue includes the top-quartile boundary and excludes below it', () => {
      expect(isHighRevenue(rows[3], rows)).toBe(true)
      expect(isHighRevenue(rows[2], rows)).toBe(false)
    })

    it('isHighOverdue includes the top-quartile boundary and excludes below it', () => {
      expect(isHighOverdue(rows[3], rows)).toBe(true)
      expect(isHighOverdue(rows[2], rows)).toBe(false)
    })

    it('isHealthyPortfolio includes the bottom-quartile boundary and excludes above it', () => {
      expect(isHealthyPortfolio(rows[0], rows)).toBe(true)
      expect(isHealthyPortfolio(rows[1], rows)).toBe(true)
      expect(isHealthyPortfolio(rows[2], rows)).toBe(false)
      expect(isHealthyPortfolio(rows[3], rows)).toBe(false)
    })

    it('needsCreditReview requires high revenue and high overdue', () => {
      const highBoth = row('S1', { Revenue: 100, OverdueExposure: 100 })
      const highRevenueOnly = row('S2', { Revenue: 90, OverdueExposure: 5 })
      const highOverdueOnly = row('S3', { Revenue: 10, OverdueExposure: 95 })
      const population = [
        highBoth,
        highRevenueOnly,
        highOverdueOnly,
        row('S4', { Revenue: 80, OverdueExposure: 60 }),
        row('S5', { Revenue: 70, OverdueExposure: 70 }),
        row('S6', { Revenue: 60, OverdueExposure: 80 }),
        row('S7', { Revenue: 50, OverdueExposure: 40 }),
        row('S8', { Revenue: 40, OverdueExposure: 30 }),
      ]
      expect(needsCreditReview(highBoth, population)).toBe(true)
      expect(needsCreditReview(highRevenueOnly, population)).toBe(false)
      expect(needsCreditReview(highOverdueOnly, population)).toBe(false)
    })

    it('needsCollectionAction mirrors high overdue', () => {
      expect(needsCollectionAction(rows[3], rows)).toBe(true)
      expect(needsCollectionAction(rows[2], rows)).toBe(false)
    })

    it('recognitionCandidates requires high revenue, high effective call rate, and healthy portfolio', () => {
      const candidate = row('R1', { Revenue: 100, EffectiveCallRate: 100, OverdueExposure: 5 })
      const lowCallRate = row('R2', { Revenue: 99, EffectiveCallRate: 5, OverdueExposure: 6 })
      const unhealthy = row('R3', { Revenue: 98, EffectiveCallRate: 99, OverdueExposure: 100 })
      const population = [
        candidate,
        lowCallRate,
        unhealthy,
        row('R4', { Revenue: 70, EffectiveCallRate: 80, OverdueExposure: 50 }),
        row('R5', { Revenue: 65, EffectiveCallRate: 75, OverdueExposure: 45 }),
        row('R6', { Revenue: 60, EffectiveCallRate: 70, OverdueExposure: 40 }),
        row('R7', { Revenue: 55, EffectiveCallRate: 65, OverdueExposure: 35 }),
        row('R8', { Revenue: 50, EffectiveCallRate: 60, OverdueExposure: 30 }),
        row('R9', { Revenue: 45, EffectiveCallRate: 55, OverdueExposure: 25 }),
        row('R10', { Revenue: 40, EffectiveCallRate: 50, OverdueExposure: 20 }),
        row('R11', { Revenue: 35, EffectiveCallRate: 45, OverdueExposure: 15 }),
        row('R12', { Revenue: 30, EffectiveCallRate: 10, OverdueExposure: 10 }),
      ]
      expect(recognitionCandidates(population)).toEqual([candidate])
    })
  })

  describe('classifyCommercialSignals', () => {
    const rows = [
      row('R1', { Revenue: 40, EffectiveCallRate: 40, OverdueExposure: 5 }),
      row('R2', { Revenue: 30, EffectiveCallRate: 30, OverdueExposure: 15 }),
      row('R3', { Revenue: 20, EffectiveCallRate: 20, OverdueExposure: 25 }),
      row('R4', { Revenue: 10, EffectiveCallRate: 10, OverdueExposure: 35 }),
    ]

    it('returns a classification structure with key, label, and rationale', () => {
      const classifications = classifyCommercialSignals(rows[0], rows)
      expect(classifications.map((entry) => entry.key)).toEqual([
        'HighRevenue',
        'HealthyPortfolio',
        'RecognitionCandidate',
      ])
      for (const entry of classifications) {
        expect(COMMERCIAL_SIGNAL_KEYS).toContain(entry.key)
        expect(typeof entry.label).toBe('string')
        expect(entry.label.length).toBeGreaterThan(0)
        expect(typeof entry.rationale).toBe('string')
        expect(entry.rationale.length).toBeGreaterThan(0)
      }
    })

    it('surfaces commercial risk classifications for high-overdue salesmen', () => {
      expect(classifyCommercialSignals(rows[3], rows).map((entry) => entry.key)).toEqual([
        'HighOverdue',
        'NeedsCollectionAction',
      ])
    })

    it('returns an empty classification list when no rule matches', () => {
      expect(classifyCommercialSignals(rows[2], rows)).toEqual([])
    })
  })
})
